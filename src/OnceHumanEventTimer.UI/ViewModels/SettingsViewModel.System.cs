// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.UI.ViewModels;

public partial class SettingsViewModel
{
    private readonly IAutostartService? _autostartService;

    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private bool _autoHideWhenGameOutOfFocus;

    public bool IsPortableMode => _settingsService.IsPortableMode;

    public string SettingsFilePath => _settingsService.SettingsFilePath;

    public string StorageModeDescription => IsPortableMode
        ? _loc.GetString("Settings_StoragePortable")
        : _loc.GetString("Settings_StorageAppData");

    private void InitializeSystem(UserSettings current, IAutostartService? autostartService)
    {
        StartWithWindows = autostartService?.IsAutostartEnabled() ?? current.StartWithWindows;
        AutoHideWhenGameOutOfFocus = current.AutoHideWhenGameOutOfFocus;
    }

    [RelayCommand]
    private void OpenSettingsFolder()
    {
        try
        {
            string? folder = Path.GetDirectoryName(SettingsFilePath);
            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                });
            }
        }
        catch
        {
            // Ignore failure to open folder
        }
    }
}
