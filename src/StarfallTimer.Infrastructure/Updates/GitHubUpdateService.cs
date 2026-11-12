// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using StarfallTimer.Core.Models;
using StarfallTimer.Core.Services;

namespace StarfallTimer.Infrastructure.Updates;

/// <summary>
/// Checks and downloads updates from GitHub Releases and executes seamless in-place replacement on Windows.
/// </summary>
public sealed class GitHubUpdateService : IUpdateService, IDisposable
{
    private const string PrimaryRepoApi = "https://api.github.com/repos/vtstv/OHStarfarllTimer/releases/latest";
    private const string FallbackRepoApi = "https://api.github.com/repos/vtstv/OnceHumanEventTimer/releases/latest";

    private readonly HttpClient _httpClient;
    private readonly bool _disposeClient;

    public GitHubUpdateService(HttpClient? httpClient = null)
    {
        _disposeClient = httpClient == null;
        _httpClient = httpClient ?? new HttpClient();

        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("OnceHumanEventTimer-Updater", GetCurrentVersionString()));
        }

        if (!_httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/vnd.github.v3+json"))
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        }
    }

    public async Task<UpdateInfo> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        string currentVersion = GetCurrentVersionString();
        string json = await FetchLatestReleaseJsonAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        string tagName = root.TryGetProperty("tag_name", out var tagProp) ? tagProp.GetString() ?? "" : "";
        string cleanVersion = tagName.Trim().TrimStart('v', 'V');
        string releaseNotes = root.TryGetProperty("body", out var bodyProp) ? bodyProp.GetString() ?? "" : "";
        string publishedAt = root.TryGetProperty("published_at", out var pubProp) ? pubProp.GetString() ?? "" : "";

        string? downloadUrl = ExtractBestDownloadUrl(root);

        bool hasUpdate = IsNewerVersion(cleanVersion, currentVersion);

        return new UpdateInfo(
            HasUpdate: hasUpdate,
            CurrentVersion: currentVersion,
            LatestVersion: cleanVersion,
            ReleaseNotes: releaseNotes,
            DownloadUrl: downloadUrl,
            PublishedAt: publishedAt);
    }

    public async Task<bool> DownloadAndApplyUpdateAsync(
        string downloadUrl,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(downloadUrl)) return false;

        string? currentExePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(currentExePath) || !File.Exists(currentExePath))
        {
            currentExePath = Process.GetCurrentProcess().MainModule?.FileName;
        }

        if (string.IsNullOrEmpty(currentExePath) || !File.Exists(currentExePath)) return false;

        string tempDir = Path.Combine(Path.GetTempPath(), "OnceHumanTimer_Update");
        Directory.CreateDirectory(tempDir);

        string downloadedPayload = Path.Combine(tempDir, "update_payload.bin");
        if (File.Exists(downloadedPayload)) File.Delete(downloadedPayload);

        using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
        {
            response.EnsureSuccessStatusCode();
            long totalBytes = response.Content.Headers.ContentLength ?? -1L;

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(downloadedPayload, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

            var buffer = new byte[81920];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalRead += bytesRead;

                if (totalBytes > 0 && progress != null)
                {
                    progress.Report((double)totalRead / totalBytes);
                }
            }
        }

        string stagedExePath = downloadedPayload;
        if (downloadUrl.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || IsZipArchive(downloadedPayload))
        {
            string extractDir = Path.Combine(tempDir, "extracted");
            if (Directory.Exists(extractDir)) Directory.Delete(extractDir, true);
            Directory.CreateDirectory(extractDir);

            ZipFile.ExtractToDirectory(downloadedPayload, extractDir, true);
            var exeFiles = Directory.GetFiles(extractDir, "*.exe", SearchOption.AllDirectories);
            var matchedExe = exeFiles.FirstOrDefault(f => Path.GetFileName(f).Contains("OnceHuman", StringComparison.OrdinalIgnoreCase))
                             ?? exeFiles.FirstOrDefault();

            if (matchedExe != null) stagedExePath = matchedExe;
        }

        int currentPid = Environment.ProcessId;
        string scriptPath = Path.Combine(tempDir, "apply_update.cmd");
        string cmdScript =
$@"@echo off
chcp 65001 >nul
set PID={currentPid}
set TARGET=""{currentExePath}""
set SOURCE=""{stagedExePath}""

:wait_loop
tasklist /fi ""PID eq %PID%"" 2>nul | findstr /i ""%PID%"" >nul
if %ERRORLEVEL% EQU 0 (
    timeout /t 1 /nobreak >nul
    goto wait_loop
)

copy /y %SOURCE% %TARGET% >nul
if %ERRORLEVEL% NEQ 0 (
    timeout /t 1 /nobreak >nul
    copy /y %SOURCE% %TARGET% >nul
)

start """" %TARGET%
del ""%~f0"" >nul 2>&1
";
        await File.WriteAllTextAsync(scriptPath, cmdScript, cancellationToken);

        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c \"\"{scriptPath}\"\"",
            CreateNoWindow = true,
            UseShellExecute = false,
            WorkingDirectory = tempDir
        };

        Process.Start(psi);

        _ = Task.Run(async () =>
        {
            await Task.Delay(400);
            Environment.Exit(0);
        });

        return true;
    }

    private async Task<string> FetchLatestReleaseJsonAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(PrimaryRepoApi, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
        }
        catch
        {
            // Fallback to secondary repo endpoint below
        }

        using var fallbackResponse = await _httpClient.GetAsync(FallbackRepoApi, cancellationToken);
        fallbackResponse.EnsureSuccessStatusCode();
        return await fallbackResponse.Content.ReadAsStringAsync(cancellationToken);
    }

    private static string? ExtractBestDownloadUrl(JsonElement root)
    {
        if (root.TryGetProperty("assets", out var assets) && assets.ValueKind == JsonValueKind.Array)
        {
            string? zipUrl = null;
            foreach (var asset in assets.EnumerateArray())
            {
                string name = asset.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "";
                string url = asset.TryGetProperty("browser_download_url", out var urlProp) ? urlProp.GetString() ?? "" : "";

                if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) return url;
                if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) && zipUrl == null) zipUrl = url;
            }

            if (!string.IsNullOrEmpty(zipUrl)) return zipUrl;
        }

        if (root.TryGetProperty("zipball_url", out var zipball)) return zipball.GetString();
        return null;
    }

    private static bool IsZipArchive(string filePath)
    {
        try
        {
            using var fs = File.OpenRead(filePath);
            var header = new byte[2];
            return fs.Read(header, 0, 2) == 2 && header[0] == 'P' && header[1] == 'K';
        }
        catch
        {
            return false;
        }
    }

    private static bool IsNewerVersion(string remoteVersion, string currentVersion)
    {
        if (Version.TryParse(remoteVersion, out var remoteVer) && Version.TryParse(currentVersion, out var curVer))
        {
            return remoteVer > curVer;
        }

        return !string.IsNullOrWhiteSpace(remoteVersion) &&
               !string.Equals(remoteVersion, currentVersion, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetCurrentVersionString()
    {
        var entry = Assembly.GetEntryAssembly() ?? typeof(GitHubUpdateService).Assembly;
        var infoVer = entry.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(infoVer))
        {
            return infoVer.Split('+')[0];
        }

        var ver = entry.GetName().Version;
        return ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "1.2.0";
    }

    public void Dispose()
    {
        if (_disposeClient) _httpClient.Dispose();
    }
}
