// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using OnceHumanEventTimer.Core.Services;
using OnceHumanEventTimer.Infrastructure.Notifications;
using OnceHumanEventTimer.Infrastructure.Win32;
using OnceHumanEventTimer.UI.ViewModels;
using OnceHumanEventTimer.UI.Views;

namespace OnceHumanEventTimer.UI;

public partial class App
{
    private void SetupSystemTray(
        ILocalizationService loc,
        OverlayWindow overlayWindow,
        OverlayViewModel overlayVm,
        ISettingsService settingsService,
        INotificationService notifications)
    {
        _trayMenuWindow = new Tray.TrayMenuWindow(
            loc,
            toggleVisibilityAction: () => ToggleOverlayVisibility(overlayWindow),
            toggleLockAction: () => Dispatcher.Invoke(() => overlayVm.ToggleLockCommand.Execute(null)),
            openSettingsAction: OpenSettingsWindow,
            exitAction: ShutdownApp,
            togglePauseAlertsAction: () => Dispatcher.Invoke(() => overlayVm.TogglePauseAlertsCommand.Execute(null)),
            areAlertsPausedFunc: () => settingsService.Settings.AreAlertsPaused,
            isOverlayVisibleFunc: () => overlayWindow.IsVisible);

        _trayService = new Win32TrayService(
            toggleVisibilityAction: () => ToggleOverlayVisibility(overlayWindow),
            showContextMenuAction: () => Dispatcher.Invoke(_trayMenuWindow.ShowMenu));

        _trayService.Initialize();
        if (notifications is WindowsNotificationService winNotif)
            winNotif.OnShowNotification = (title, msg) => _trayService.ShowNotification(title, msg);
    }

    private void SetupFocusMonitor(
        IGameFocusMonitor focusMonitor,
        ISettingsService settingsService,
        OverlayWindow overlayWindow)
    {
        focusMonitor.GameFocusChanged += (s, isFocused) =>
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!settingsService.Settings.AutoHideWhenGameOutOfFocus) return;

                if (isFocused)
                {
                    if (!_userManuallyHidden && !overlayWindow.IsVisible)
                        overlayWindow.Show();
                }
                else
                {
                    if (overlayWindow.IsVisible)
                        overlayWindow.Hide();
                }
            }), System.Windows.Threading.DispatcherPriority.Normal);
        };
        focusMonitor.Start();
    }
}
