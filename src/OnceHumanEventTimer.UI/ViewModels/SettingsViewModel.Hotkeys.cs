// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnceHumanEventTimer.Core.Models;

namespace OnceHumanEventTimer.UI.ViewModels;

public partial class SettingsViewModel
{
    [ObservableProperty]
    private string _toggleVisibilityHotkey = "Ctrl+Alt+S";

    [ObservableProperty]
    private string _toggleClickThroughHotkey = "Ctrl+Alt+L";

    [ObservableProperty]
    private string _togglePauseAlertsHotkey = "Ctrl+Alt+P";

    [ObservableProperty]
    private string _toggleSettingsHotkey = "Ctrl+Alt+O";

    private void InitializeHotkeys(UserSettings current)
    {
        ToggleVisibilityHotkey = current.ToggleVisibilityHotkey;
        ToggleClickThroughHotkey = current.ToggleClickThroughHotkey;
        TogglePauseAlertsHotkey = current.TogglePauseAlertsHotkey;
        ToggleSettingsHotkey = current.ToggleSettingsHotkey;
    }

    [RelayCommand]
    private void ResetToggleVisibilityHotkey()
    {
        ToggleVisibilityHotkey = "Ctrl+Alt+S";
    }

    [RelayCommand]
    private void ResetToggleClickThroughHotkey()
    {
        ToggleClickThroughHotkey = "Ctrl+Alt+L";
    }

    [RelayCommand]
    private void ResetTogglePauseAlertsHotkey()
    {
        TogglePauseAlertsHotkey = "Ctrl+Alt+P";
    }

    [RelayCommand]
    private void ResetToggleSettingsHotkey()
    {
        ToggleSettingsHotkey = "Ctrl+Alt+O";
    }
}
