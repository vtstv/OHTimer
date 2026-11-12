// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Text.Json;
using StarfallTimer.Core.Models;
using StarfallTimer.Core.Services;

namespace StarfallTimer.Infrastructure.Storage;

/// <summary>
/// Persists user settings to local AppData storage in JSON format.
/// </summary>
public sealed class FileSettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly object _lock = new();

    public UserSettings Settings { get; private set; } = new();

    public event EventHandler<UserSettings>? SettingsChanged;

    public FileSettingsService(string? customFolder = null)
    {
        string baseFolder = customFolder ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OnceHumanStarfallTimer");

        Directory.CreateDirectory(baseFolder);
        _settingsFilePath = Path.Combine(baseFolder, "settings.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
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
}
