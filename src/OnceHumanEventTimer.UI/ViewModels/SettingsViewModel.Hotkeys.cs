// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using OnceHumanEventTimer.Core.Models;

namespace OnceHumanEventTimer.UI.ViewModels;

public partial class SettingsViewModel
{
    [ObservableProperty]
    private string _toggleVisibilityHotkey = "Ctrl+Alt+S";

    [ObservableProperty]
    private string _toggleClickThroughHotkey = "Ctrl+Alt+L";

    [ObservableProperty]
    private string _togglePauseAlertsHotkey = "Ctrl+Alt+M";

    private void InitializeHotkeys(UserSettings current)
    {
        ToggleVisibilityHotkey = current.ToggleVisibilityHotkey;
        ToggleClickThroughHotkey = current.ToggleClickThroughHotkey;
        TogglePauseAlertsHotkey = current.TogglePauseAlertsHotkey;
    }
}
