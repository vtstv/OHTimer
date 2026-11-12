// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarfallTimer.Core.Models;
using StarfallTimer.Core.Services;

namespace StarfallTimer.UI.ViewModels;

public partial class SettingsViewModel
{
    [ObservableProperty]
    private bool _isCheckingForUpdates;

    [ObservableProperty]
    private bool _isUpdating;

    [ObservableProperty]
    private double _updateDownloadProgress;

    [ObservableProperty]
    private string? _updateStatusMessage;

    [ObservableProperty]
    private bool _hasUpdate;

    [ObservableProperty]
    private string? _latestVersion;

    [ObservableProperty]
    private string? _releaseNotes;

    [ObservableProperty]
    private string? _downloadUrl;

    [ObservableProperty]
    private bool _showUpdateAvailablePanel;

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        if (_updateService == null || IsCheckingForUpdates || IsUpdating) return;

        IsCheckingForUpdates = true;
        ShowUpdateAvailablePanel = false;
        UpdateStatusMessage = _loc.GetString("Update_Checking");

        try
        {
            var info = await _updateService.CheckForUpdatesAsync();
            HasUpdate = info.HasUpdate;
            LatestVersion = info.LatestVersion;
            ReleaseNotes = info.ReleaseNotes;
            DownloadUrl = info.DownloadUrl;

            if (info.HasUpdate)
            {
                UpdateStatusMessage = _loc.Format("Update_Available", info.LatestVersion);
                ShowUpdateAvailablePanel = true;
            }
            else
            {
                UpdateStatusMessage = _loc.GetString("Update_UpToDate");
                ShowUpdateAvailablePanel = false;
            }
        }
        catch (Exception)
        {
            UpdateStatusMessage = _loc.GetString("Update_Error");
            ShowUpdateAvailablePanel = false;
        }
        finally
        {
            IsCheckingForUpdates = false;
        }
    }

    [RelayCommand]
    private async Task ApplyUpdateAsync()
    {
        if (_updateService == null || string.IsNullOrWhiteSpace(DownloadUrl) || IsUpdating) return;

        IsUpdating = true;
        UpdateDownloadProgress = 0;
        UpdateStatusMessage = _loc.Format("Update_Downloading", 0);

        try
        {
            var progress = new Progress<double>(p =>
            {
                int percent = (int)Math.Clamp(p * 100, 0, 100);
                UpdateDownloadProgress = percent;
                UpdateStatusMessage = _loc.Format("Update_Downloading", percent);
            });

            bool success = await _updateService.DownloadAndApplyUpdateAsync(DownloadUrl, progress);
            if (!success)
            {
                UpdateStatusMessage = _loc.GetString("Update_Error");
                IsUpdating = false;
            }
        }
        catch (Exception)
        {
            UpdateStatusMessage = _loc.GetString("Update_Error");
            IsUpdating = false;
        }
    }
}
