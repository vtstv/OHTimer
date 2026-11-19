// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Input;
using OnceHumanEventTimer.UI.ViewModels;

namespace OnceHumanEventTimer.UI.Views;

/// <summary>
/// Interaction logic for OnboardingWindow.xaml.
/// </summary>
public partial class OnboardingWindow : Window
{
    private DateTime _popupClosedTime = DateTime.MinValue;

    public OnboardingWindow(OnboardingViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose = Close;
        Loaded += (s, e) =>
        {
            Topmost = true;
            Activate();
            Focus();
        };

        LanguagePopup.Closed += (s, e) =>
        {
            _popupClosedTime = DateTime.UtcNow;
        };

        LocationChanged += (s, e) =>
        {
            if (LanguagePopup.IsOpen)
            {
                LanguagePopup.IsOpen = false;
            }
        };

        Deactivated += (s, e) =>
        {
            if (LanguagePopup.IsOpen)
            {
                LanguagePopup.IsOpen = false;
            }
        };

        PreviewKeyDown += (s, e) =>
        {
            if (e.Key == Key.Escape && LanguagePopup.IsOpen)
            {
                LanguagePopup.IsOpen = false;
                e.Handled = true;
            }
        };
    }

    private void LanguageToggle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (LanguagePopup.IsOpen || (DateTime.UtcNow - _popupClosedTime).TotalMilliseconds < 250)
        {
            LanguagePopup.IsOpen = false;
            LanguageToggle.IsChecked = false;
            e.Handled = true;
        }
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
}
