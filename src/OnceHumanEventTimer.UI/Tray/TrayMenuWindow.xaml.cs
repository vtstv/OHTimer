// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using OnceHumanEventTimer.Core.Services;
using OnceHumanEventTimer.Infrastructure.Win32;

namespace OnceHumanEventTimer.UI.Tray;

/// <summary>
/// Minimalist dark tray menu window matching Once Human Stardust cyberpunk aesthetic.
/// Pure text items without emojis/hotkeys, reliable outside-click auto-dismissal,
/// and smooth instant opening on the very first right click.
/// </summary>
public partial class TrayMenuWindow : Window
{
    private readonly ILocalizationService _loc;
    private readonly Action _toggleVisibilityAction;
    private readonly Action _toggleLockAction;
    private readonly Action _openSettingsAction;
    private readonly Action _exitAction;
    private readonly Action? _togglePauseAlertsAction;
    private readonly Func<bool>? _areAlertsPausedFunc;
    private readonly Func<bool>? _isOverlayVisibleFunc;
    private readonly TrayMenuOutsideClickTracker _outsideClickTracker;

    private DateTime _openedAtUtc = DateTime.MinValue;
    private DateTime _lastOutsideClickDismissUtc = DateTime.MinValue;
    private bool _isExplicitShutdown;

    public TrayMenuWindow(
        ILocalizationService loc,
        Action toggleVisibilityAction,
        Action toggleLockAction,
        Action openSettingsAction,
        Action exitAction,
        Action? togglePauseAlertsAction = null,
        Func<bool>? areAlertsPausedFunc = null,
        Func<bool>? isOverlayVisibleFunc = null)
    {
        _loc = loc;
        _toggleVisibilityAction = toggleVisibilityAction;
        _toggleLockAction = toggleLockAction;
        _openSettingsAction = openSettingsAction;
        _exitAction = exitAction;
        _togglePauseAlertsAction = togglePauseAlertsAction;
        _areAlertsPausedFunc = areAlertsPausedFunc;
        _isOverlayVisibleFunc = isOverlayVisibleFunc;

        InitializeComponent();

        _outsideClickTracker = new TrayMenuOutsideClickTracker(
            () => new WindowInteropHelper(this).Handle,
            () =>
            {
                _lastOutsideClickDismissUtc = DateTime.UtcNow;
                Dispatcher.BeginInvoke(HideMenu);
            });

        SourceInitialized += OnSourceInitialized;
        Deactivated += OnDeactivated;
        KeyDown += OnKeyDown;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        WindowStyleManager.SetToolWindowOnly(helper.Handle, true);
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        if ((DateTime.UtcNow - _openedAtUtc).TotalMilliseconds < 350)
        {
            return;
        }

        HideMenu();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            HideMenu();
            e.Handled = true;
        }
    }

    public void ShowMenu()
    {
        if ((DateTime.UtcNow - _lastOutsideClickDismissUtc).TotalMilliseconds < 250)
        {
            return;
        }

        if (IsVisible)
        {
            if ((DateTime.UtcNow - _openedAtUtc).TotalMilliseconds < 350)
            {
                return;
            }
            HideMenu();
            return;
        }

        _openedAtUtc = DateTime.UtcNow;
        UpdateMenuLabels();

        Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double menuWidth = DesiredSize.Width > 0 ? DesiredSize.Width : 200;
        double menuHeight = DesiredSize.Height > 0 ? DesiredSize.Height : 210;

        var (cursorX, cursorY) = WindowStyleManager.GetCursorPosition();
        var (workLeftPx, workTopPx, workRightPx, workBottomPx) = WindowStyleManager.GetMonitorWorkArea(cursorX, cursorY);

        var dpi = VisualTreeHelper.GetDpi(this);
        double dpiX = dpi.DpiScaleX > 0 ? dpi.DpiScaleX : 1.0;
        double dpiY = dpi.DpiScaleY > 0 ? dpi.DpiScaleY : 1.0;

        double cursorDipX = cursorX / dpiX;
        double cursorDipY = cursorY / dpiY;
        double workLeft = workLeftPx / dpiX;
        double workTop = workTopPx / dpiY;
        double workRight = workRightPx / dpiX;
        double workBottom = workBottomPx / dpiY;

        double targetLeft = cursorDipX - 10;
        double targetTop = cursorDipY - menuHeight;

        if (targetTop < workTop) targetTop = cursorDipY;
        if (targetLeft + menuWidth > workRight) targetLeft = workRight - menuWidth;
        if (targetLeft < workLeft) targetLeft = workLeft;
        if (targetTop + menuHeight > workBottom) targetTop = workBottom - menuHeight;

        Left = targetLeft;
        Top = targetTop;

        Show();
        Activate();

        var helper = new WindowInteropHelper(this);
        if (helper.Handle != 0)
        {
            WindowStyleManager.BringToForeground(helper.Handle);
        }

        BtnToggleVisibility.Focus();
        _outsideClickTracker.Start();
    }

    public void HideMenu()
    {
        if (IsVisible)
        {
            _outsideClickTracker.Stop();
            Hide();

            var helper = new WindowInteropHelper(this);
            if (helper.Handle != 0)
            {
                WindowStyleManager.PostNullMessage(helper.Handle);
            }
        }
    }

    private void UpdateMenuLabels()
    {
        bool isVisible = _isOverlayVisibleFunc != null && _isOverlayVisibleFunc();
        BtnToggleVisibility.Content = isVisible
            ? _loc.GetString("Tray_HideHUD")
            : _loc.GetString("Tray_OpenHUD");

        BtnToggleLock.Content = _loc.GetString("Tray_ToggleLock");

        if (_togglePauseAlertsAction != null && _areAlertsPausedFunc != null)
        {
            BtnPauseAlerts.Visibility = Visibility.Visible;
            bool isPaused = _areAlertsPausedFunc();
            BtnPauseAlerts.Content = isPaused
                ? _loc.GetString("Tray_ResumeAlerts")
                : _loc.GetString("Tray_PauseAlerts");
        }
        else
        {
            BtnPauseAlerts.Visibility = Visibility.Collapsed;
        }

        BtnSettings.Content = _loc.GetString("Tray_Settings");
        BtnExit.Content = _loc.GetString("Tray_Exit");
    }

    private void OnToggleVisibilityClick(object sender, RoutedEventArgs e) { HideMenu(); _toggleVisibilityAction.Invoke(); }
    private void OnToggleLockClick(object sender, RoutedEventArgs e) { HideMenu(); _toggleLockAction.Invoke(); }
    private void OnPauseAlertsClick(object sender, RoutedEventArgs e) { HideMenu(); _togglePauseAlertsAction?.Invoke(); }
    private void OnSettingsClick(object sender, RoutedEventArgs e) { HideMenu(); _openSettingsAction.Invoke(); }
    private void OnExitClick(object sender, RoutedEventArgs e) { HideMenu(); _exitAction.Invoke(); }

    public void CloseForShutdown()
    {
        _isExplicitShutdown = true;
        _outsideClickTracker.Dispose();
        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isExplicitShutdown)
        {
            e.Cancel = true;
            HideMenu();
        }
        base.OnClosing(e);
    }
}
