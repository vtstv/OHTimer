// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Interop;
using Microsoft.Extensions.DependencyInjection;
using OnceHumanEventTimer.Infrastructure.Win32;
using OnceHumanEventTimer.UI.ViewModels;
using OnceHumanEventTimer.UI.Views;

namespace OnceHumanEventTimer.UI;

public partial class App
{
    private SettingsWindow? _settingsWindow;
    private OnboardingWindow? _onboardingWindow;

    private void OpenSettingsWindow()
    {
        if (_settingsWindow != null && _settingsWindow.IsLoaded)
        {
            if (_settingsWindow.WindowState == WindowState.Minimized)
            {
                _settingsWindow.WindowState = WindowState.Normal;
            }
            _settingsWindow.Topmost = true;
            _settingsWindow.Activate();
            _settingsWindow.Focus();
            WindowStyleManager.BringToForeground(new WindowInteropHelper(_settingsWindow).Handle);
            return;
        }

        var vm = _serviceProvider!.GetRequiredService<SettingsViewModel>();
        vm.RequestResetPosition = () => { if (MainWindow != null) { MainWindow.Left = 40; MainWindow.Top = 40; } };
        vm.RequestRestartOnboarding = OpenOnboardingWizard;

        _settingsWindow = new SettingsWindow(vm)
        {
            Topmost = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        _settingsWindow.Closed += (s, e) => _settingsWindow = null;
        _settingsWindow.Show();
        _settingsWindow.Activate();
        _settingsWindow.Focus();
        WindowStyleManager.BringToForeground(new WindowInteropHelper(_settingsWindow).Handle);
    }

    public void ToggleSettingsWindow()
    {
        Dispatcher.Invoke(() =>
        {
            if (_settingsWindow != null && _settingsWindow.IsLoaded && _settingsWindow.IsVisible)
            {
                if (_settingsWindow.WindowState == WindowState.Minimized)
                {
                    _settingsWindow.WindowState = WindowState.Normal;
                    _settingsWindow.Topmost = true;
                    _settingsWindow.Activate();
                    _settingsWindow.Focus();
                    WindowStyleManager.BringToForeground(new WindowInteropHelper(_settingsWindow).Handle);
                    return;
                }

                if (_settingsWindow.DataContext is SettingsViewModel vm)
                {
                    _ = vm.SaveCommand.ExecuteAsync(null);
                }

                _settingsWindow.Close();
                _settingsWindow = null;
            }
            else
            {
                OpenSettingsWindow();
            }
        });
    }

    public void OpenOnboardingWizard()
    {
        if (_onboardingWindow != null && _onboardingWindow.IsLoaded)
        {
            _onboardingWindow.Topmost = true;
            _onboardingWindow.Activate();
            _onboardingWindow.Focus();
            return;
        }

        var vm = _serviceProvider!.GetRequiredService<OnboardingViewModel>();
        _onboardingWindow = new OnboardingWindow(vm)
        {
            Topmost = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        _onboardingWindow.Closed += (s, e) => _onboardingWindow = null;
        _onboardingWindow.Show();
        _onboardingWindow.Activate();
        _onboardingWindow.Focus();
    }
}
