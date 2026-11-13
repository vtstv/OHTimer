// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Services;
using OnceHumanEventTimer.UI.ViewModels;
using OnceHumanEventTimer.UI.Views;

namespace OnceHumanEventTimer.UI;

public partial class App
{
    private bool _userManuallyHidden;

    private void RegisterHotkeys(
        IHotkeyService hotkeyService,
        UserSettings settings,
        OverlayWindow overlayWindow,
        OverlayViewModel overlayVm)
    {
        hotkeyService.UnregisterAll();

        if (!string.IsNullOrWhiteSpace(settings.ToggleVisibilityHotkey))
        {
            hotkeyService.RegisterHotkey(settings.ToggleVisibilityHotkey, () =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (overlayWindow.IsVisible)
                    {
                        _userManuallyHidden = true;
                        overlayWindow.Hide();
                    }
                    else
                    {
                        _userManuallyHidden = false;
                        overlayWindow.Show();
                    }
                });
            });
        }

        if (!string.IsNullOrWhiteSpace(settings.ToggleClickThroughHotkey))
        {
            hotkeyService.RegisterHotkey(settings.ToggleClickThroughHotkey, () =>
            {
                Dispatcher.Invoke(() =>
                {
                    overlayVm.ToggleClickThroughCommand.Execute(null);
                });
            });
        }
    }
}
