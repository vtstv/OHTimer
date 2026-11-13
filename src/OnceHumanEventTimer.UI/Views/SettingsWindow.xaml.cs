// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using OnceHumanEventTimer.UI.ViewModels;

namespace OnceHumanEventTimer.UI.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();

        viewModel.RequestCloseSettings = Close;
        LoadWindowIcon();
    }

    private void LoadWindowIcon()
    {
        try
        {
            var iconUri = new Uri("pack://application:,,,/Assets/app.ico", UriKind.RelativeOrAbsolute);
            Icon = BitmapFrame.Create(iconUri);
            return;
        }
        catch
        {
            // Fallback or ignore in single-file execution
        }

        try
        {
            string localIcon = Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico");
            if (File.Exists(localIcon))
            {
                Icon = new BitmapImage(new Uri(localIcon, UriKind.Absolute));
            }
        }
        catch
        {
            // Gracefully ignore icon load failure
        }
    }
}
