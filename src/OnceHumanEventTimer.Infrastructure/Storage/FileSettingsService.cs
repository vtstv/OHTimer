// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Text.Json;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.Infrastructure.Storage;

/// <summary>
/// Persists user settings to local AppData storage in JSON format.
/// </summary>
public sealed class FileSettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly object _lock = new();

    /// <summary>
    /// Full path to the active settings.json file.
    /// </summary>
    public string SettingsFilePath => _settingsFilePath;

    /// <summary>
    /// When true, settings are stored locally alongside the executable (portable mode).
    /// </summary>
    public bool IsPortableMode { get; }

    public UserSettings Settings { get; private set; } = new();

    public event EventHandler<UserSettings>? SettingsChanged;

    public FileSettingsService(string? customFolder = null)
    {
        string modernFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OnceHumanEventTimer");

        string appDir = AppContext.BaseDirectory;
        string localSettingsFile = Path.Combine(appDir, "settings.json");
        string localMarker = Path.Combine(appDir, "portable");
        string localMarkerTxt = Path.Combine(appDir, "portable.txt");
        string localMarkerDot = Path.Combine(appDir, ".portable");

        bool wantsPortable = File.Exists(localSettingsFile) ||
                             File.Exists(localMarker) ||
                             File.Exists(localMarkerTxt) ||
                             File.Exists(localMarkerDot);

        if (customFolder != null)
        {
            Directory.CreateDirectory(customFolder);
            _settingsFilePath = Path.Combine(customFolder, "settings.json");
            IsPortableMode = false;
        }
        else if (wantsPortable && IsDirectoryWritable(appDir))
        {
            IsPortableMode = true;
            _settingsFilePath = localSettingsFile;

            // If local settings.json does not exist yet (e.g. portable marker file created), copy from AppData
            if (!File.Exists(localSettingsFile))
            {
                string appDataFile = Path.Combine(modernFolder, "settings.json");
                if (File.Exists(appDataFile))
                {
                    try { File.Copy(appDataFile, localSettingsFile, overwrite: false); } catch { }
                }
            }
        }
        else
        {
            IsPortableMode = false;
            string legacyFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "OnceHumanOnceHumanEventTimer");

            // Automatically migrate legacy settings folder if present
            if (!Directory.Exists(modernFolder) && Directory.Exists(legacyFolder))
            {
                try
                {
                    string legacyFile = Path.Combine(legacyFolder, "settings.json");
                    if (File.Exists(legacyFile))
                    {
                        Directory.CreateDirectory(modernFolder);
                        File.Copy(legacyFile, Path.Combine(modernFolder, "settings.json"), overwrite: false);
                    }
                }
                catch
                {
                    // Non-critical fallback
                }
            }

            Directory.CreateDirectory(modernFolder);
            _settingsFilePath = Path.Combine(modernFolder, "settings.json");
        }

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    private static bool IsDirectoryWritable(string directoryPath)
    {
        try
        {
            string probePath = Path.Combine(directoryPath, $".probe_{Guid.NewGuid():N}.tmp");
            File.WriteAllText(probePath, "probe");
            File.Delete(probePath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task LoadAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                using var stream = File.OpenRead(_settingsFilePath);
                var loaded = await JsonSerializer.DeserializeAsync<UserSettings>(stream, _jsonOptions);
                if (loaded != null)
                {
                    loaded.EnsureDefaultPresets();
                    lock (_lock)
                    {
                        Settings = loaded;
                    }
                    SettingsChanged?.Invoke(this, Settings);
                    return;
                }
            }
        }
        catch
        {
            // Fall back to default settings on read error or corrupted json
        }

        lock (_lock)
        {
            Settings = new UserSettings();
        }
        await SaveAsync();
    }

    public async Task SaveAsync()
    {
        try
        {
            byte[] utf8Bytes;
            lock (_lock)
            {
                utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(Settings, _jsonOptions);
            }

            await File.WriteAllBytesAsync(_settingsFilePath, utf8Bytes);
            SettingsChanged?.Invoke(this, Settings);
        }
        catch
        {
            // Ignore non-fatal disk write errors
        }
    }

    public void NotifyChanged()
    {
        SettingsChanged?.Invoke(this, Settings);
    }
}
