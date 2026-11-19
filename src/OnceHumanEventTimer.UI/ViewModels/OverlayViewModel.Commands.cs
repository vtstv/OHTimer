// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.Input;

namespace OnceHumanEventTimer.UI.ViewModels;

public partial class OverlayViewModel
{
    [RelayCommand]
    private async Task ToggleLockAsync()
    {
        IsLocked = !IsLocked;
        _settingsService.Settings.IsLocked = IsLocked;
        await _settingsService.SaveAsync();
    }

    [RelayCommand]
    private async Task ToggleClickThroughAsync()
    {
        IsClickThrough = !IsClickThrough;
        _settingsService.Settings.IsClickThrough = IsClickThrough;
        RequestClickThroughChanged?.Invoke(IsClickThrough);
        await _settingsService.SaveAsync();
    }

    [RelayCommand]
    private async Task ToggleCompactModeAsync()
    {
        CompactMode = !CompactMode;
        _settingsService.Settings.CompactMode = CompactMode;
        await _settingsService.SaveAsync();
    }

    [RelayCommand]
    private async Task TogglePauseAlertsAsync()
    {
        AreAlertsPaused = !AreAlertsPaused;
        _settingsService.Settings.AreAlertsPaused = AreAlertsPaused;
        await _settingsService.SaveAsync();
    }

    [RelayCommand]
    private void OpenSettings() => RequestOpenSettings?.Invoke();

    [RelayCommand]
    private void HideOverlay() => RequestHideOverlay?.Invoke();

    [RelayCommand]
    private void ExitApp() => RequestExitApp?.Invoke();
}
