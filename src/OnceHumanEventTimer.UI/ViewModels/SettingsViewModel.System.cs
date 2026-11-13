// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
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

    private void InitializeSystem(UserSettings current, IAutostartService? autostartService)
    {
        StartWithWindows = autostartService?.IsAutostartEnabled() ?? current.StartWithWindows;
        AutoHideWhenGameOutOfFocus = current.AutoHideWhenGameOutOfFocus;
    }
}
