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
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
}
