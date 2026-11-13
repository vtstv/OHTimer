// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using OnceHumanEventTimer.Core.Services;
using OnceHumanEventTimer.UI.ViewModels;
using OnceHumanEventTimer.UI.Views;

namespace OnceHumanEventTimer.UI;

public partial class App
{
    private async Task HandleScreenshotGenerationAsync(string[] args)
    {
        string? customOut = args.FirstOrDefault(a => a.StartsWith("--output=", StringComparison.OrdinalIgnoreCase))?[9..];
        string docsDir = !string.IsNullOrWhiteSpace(customOut)
            ? Path.GetFullPath(customOut)
            : Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "docs", "images"));

        if (!Directory.Exists(docsDir))
        {
            docsDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "docs", "images"));
        }
        Directory.CreateDirectory(docsDir);

        string logFile = Path.Combine(docsDir, "capture.log");
        File.WriteAllText(logFile, $"Starting capture at {DateTime.Now} into {docsDir}\n");

        try
        {
            // Temporarily disable auto-hide so overlay stays visible for capture
            var settingsService = _serviceProvider!.GetRequiredService<ISettingsService>();
            settingsService.Settings.AutoHideWhenGameOutOfFocus = false;

            // 1. Setup & Capture Settings Window Tabs
            var settingsVm = _serviceProvider!.GetRequiredService<SettingsViewModel>();
            var settingsWindow = new SettingsWindow(settingsVm)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Width = 860,
                Height = 600
            };
            settingsWindow.Show();
            settingsWindow.UpdateLayout();
            await Task.Delay(400);

            var tabControl = FindVisualChild<TabControl>(settingsWindow);
            if (tabControl != null)
            {
                // Tab 0: Events & Presets
                tabControl.SelectedIndex = 0;
                settingsWindow.UpdateLayout();
                await Task.Delay(250);
                CaptureVisual(settingsWindow, Path.Combine(docsDir, "settings-events.jpg"), 860, 600, isJpeg: true);
                File.AppendAllText(logFile, "Captured settings-events.jpg\n");

                // Tab 1: Alerts & Audio
                tabControl.SelectedIndex = 1;
                settingsWindow.UpdateLayout();
                await Task.Delay(250);
                CaptureVisual(settingsWindow, Path.Combine(docsDir, "settings-audio.png"), 860, 600);
                File.AppendAllText(logFile, "Captured settings-audio.png\n");

                // Tab 2: Appearance
                tabControl.SelectedIndex = 2;
                settingsWindow.UpdateLayout();
                await Task.Delay(250);
                CaptureVisual(settingsWindow, Path.Combine(docsDir, "hud-appearance.jpg"), 860, 600, isJpeg: true);
                File.AppendAllText(logFile, "Captured hud-appearance.jpg\n");

                // Tab 3: System
                tabControl.SelectedIndex = 3;
                settingsWindow.UpdateLayout();
                await Task.Delay(250);
                CaptureVisual(settingsWindow, Path.Combine(docsDir, "settings-system.png"), 860, 600);
                File.AppendAllText(logFile, "Captured settings-system.png\n");
            }
            settingsWindow.Close();

            // 2. Capture HUD Overlay
            var overlayWindow = Current.Windows.OfType<OverlayWindow>().FirstOrDefault() ?? (OverlayWindow)MainWindow;
            var overlayVm = _serviceProvider!.GetRequiredService<OverlayViewModel>();

            overlayVm.CompactMode = false;
            overlayWindow.Show();
            overlayWindow.UpdateLayout();
            await Task.Delay(300);
            CaptureVisual(overlayWindow, Path.Combine(docsDir, "hud-multitimer.png"));
            File.AppendAllText(logFile, "Captured hud-multitimer.png\n");

            overlayVm.CompactMode = true;
            overlayWindow.UpdateLayout();
            await Task.Delay(300);
            CaptureVisual(overlayWindow, Path.Combine(docsDir, "hud-mini.png"));
            File.AppendAllText(logFile, "Captured hud-mini.png\n");

            File.AppendAllText(logFile, "Capture completed successfully!\n");
        }
        catch (Exception ex)
        {
            File.AppendAllText(logFile, $"ERROR: {ex}\n");
        }
        finally
        {
            ShutdownApp();
            Environment.Exit(0);
        }
    }

    private static void CaptureVisual(FrameworkElement visual, string outputPath, int defaultW = 0, int defaultH = 0, bool isJpeg = false)
    {
        int width = (int)Math.Max(visual.ActualWidth, visual.Width);
        int height = (int)Math.Max(visual.ActualHeight, visual.Height);
        if (width <= 0) width = defaultW > 0 ? defaultW : 400;
        if (height <= 0) height = defaultH > 0 ? defaultH : 300;

        var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(visual);

        using var fs = File.Create(outputPath);
        if (isJpeg)
        {
            var encoder = new JpegBitmapEncoder { QualityLevel = 95 };
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(fs);
        }
        else
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(fs);
        }
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typed) return typed;
            var sub = FindVisualChild<T>(child);
            if (sub != null) return sub;
        }
        return null;
    }
}
