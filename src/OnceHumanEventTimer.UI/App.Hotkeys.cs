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

    private void ToggleOverlayVisibility(OverlayWindow overlayWindow)
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
    }

    private void RegisterHotkeys(
        IHotkeyService hotkeyService,
        UserSettings settings,
        OverlayWindow overlayWindow,
        OverlayViewModel overlayVm,
        ILocalizationService? loc = null,
        INotificationService? notifications = null)
    {
        hotkeyService.UnregisterAll();

        if (!string.IsNullOrWhiteSpace(settings.ToggleVisibilityHotkey))
        {
            hotkeyService.RegisterHotkey(settings.ToggleVisibilityHotkey, () => ToggleOverlayVisibility(overlayWindow));
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

        string pauseHotkey = !string.IsNullOrWhiteSpace(settings.TogglePauseAlertsHotkey)
            ? settings.TogglePauseAlertsHotkey
            : "Ctrl+Alt+P";

        void HandlePauseToggle()
        {
            Dispatcher.Invoke(() =>
            {
                overlayVm.TogglePauseAlertsCommand.Execute(null);
                if (settings.ShowToastAlert && notifications != null && loc != null)
                {
                    string title = loc.GetString("App_Title");
                    string msg = overlayVm.AreAlertsPaused
                        ? loc.GetString("HUD_AlertsPausedToast")
                        : loc.GetString("HUD_AlertsResumedToast");
                    notifications.ShowNotification(title, msg);
                }
            });
        }

        bool pauseOk = hotkeyService.RegisterHotkey(pauseHotkey, HandlePauseToggle);
        if (!pauseOk && pauseHotkey.Equals("Ctrl+Alt+M", StringComparison.OrdinalIgnoreCase))
        {
            settings.TogglePauseAlertsHotkey = "Ctrl+Alt+P";
            hotkeyService.RegisterHotkey("Ctrl+Alt+P", HandlePauseToggle);
        }
    }
}
