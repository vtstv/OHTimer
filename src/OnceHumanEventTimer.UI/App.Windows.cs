// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using Microsoft.Extensions.DependencyInjection;
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
            _settingsWindow.Activate();
            return;
        }

        var vm = _serviceProvider!.GetRequiredService<SettingsViewModel>();
        vm.RequestResetPosition = () => { if (MainWindow != null) { MainWindow.Left = 40; MainWindow.Top = 40; } };
        vm.RequestRestartOnboarding = OpenOnboardingWizard;

        _settingsWindow = new SettingsWindow(vm);
        _settingsWindow.Closed += (s, e) => _settingsWindow = null;
        _settingsWindow.Show();
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
