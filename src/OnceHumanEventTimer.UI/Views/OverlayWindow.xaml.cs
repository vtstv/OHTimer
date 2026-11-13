// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using OnceHumanEventTimer.Core.Services;
using OnceHumanEventTimer.Infrastructure.Win32;
using OnceHumanEventTimer.UI.ViewModels;

namespace OnceHumanEventTimer.UI.Views;

public partial class OverlayWindow : Window
{
    private readonly OverlayViewModel _viewModel;
    private readonly ISettingsService _settingsService;
    private nint _hwnd;

    public nint Handle => _hwnd;

    public OverlayWindow(OverlayViewModel viewModel, ISettingsService settingsService)
    {
        _viewModel = viewModel;
        _settingsService = settingsService;
        DataContext = _viewModel;

        InitializeComponent();

        Left = _settingsService.Settings.OverlayPositionX;
        Top = _settingsService.Settings.OverlayPositionY;

        SourceInitialized += OnSourceInitialized;
        LocationChanged += OnLocationChanged;

        _viewModel.RequestClickThroughChanged = SetClickThrough;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        _hwnd = new WindowInteropHelper(this).Handle;
        WindowStyleManager.SetToolWindow(_hwnd, true);

        if (_viewModel.IsClickThrough)
        {
            SetClickThrough(true);
        }
    }

    public void SetClickThrough(bool clickThrough)
    {
        if (_hwnd != 0)
        {
            WindowStyleManager.SetClickThrough(_hwnd, clickThrough);
        }
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_viewModel.IsLocked && !_viewModel.IsClickThrough && e.ButtonState == MouseButtonState.Pressed)
        {
            try
            {
                DragMove();
            }
            catch
            {
                // Ignore transient DragMove failures
            }
        }
    }

    private void OnLocationChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Normal && Left > -10000 && Top > -10000)
        {
            _settingsService.Settings.OverlayPositionX = Left;
            _settingsService.Settings.OverlayPositionY = Top;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _ = _settingsService.SaveAsync();
        base.OnClosed(e);
    }
}
