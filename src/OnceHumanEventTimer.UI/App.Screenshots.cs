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
            settingsService.Settings.ShowBackground = true;
            settingsService.Settings.ShowBorder = true;
            settingsService.Settings.Opacity = 1.0;
            settingsService.Settings.ShowMiniControls = true;
            settingsService.Settings.Scale = 1.0;

            var gEvent = settingsService.Settings.Events.FirstOrDefault(e => e.Id == "gravity-abyss");
            if (gEvent != null) gEvent.IsEnabled = true;
            var cEvent = settingsService.Settings.Events.FirstOrDefault(e => e.Id == "cargo-scramble");
            if (cEvent != null) cEvent.IsEnabled = true;
            var dEvent = settingsService.Settings.Events.FirstOrDefault(e => e.Id == "drift-greenhouse");
            if (dEvent != null)
            {
                dEvent.IsEnabled = true;
                dEvent.ShowOnlyWhenUpcoming = false;
            }

            var scheduler = _serviceProvider!.GetRequiredService<IEventScheduler>();
            scheduler.SetTrackedEvents(settingsService.Settings.Events, settingsService.Settings.ActiveEventId, 0);

            // 1. Setup & Capture Settings Window Tabs
            var settingsVm = _serviceProvider!.GetRequiredService<SettingsViewModel>();
            settingsVm.ConfirmAction = (_, _) => true;
            var settingsWindow = new SettingsWindow(settingsVm)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Width = 860,
                Height = 670
            };
            settingsWindow.Show();
            settingsWindow.UpdateLayout();
            await Task.Delay(400);

            var tabControl = FindVisualChild<TabControl>(settingsWindow);
            if (tabControl != null)
            {
                // Tab 0: Events & Presets (select Drift Greenhouse to showcase manual sync & calibration card)
                var dg = settingsVm.Events.FirstOrDefault(e => e.Id == "drift-greenhouse");
                if (dg != null) settingsVm.SelectedEvent = dg;

                tabControl.SelectedIndex = 0;
                settingsWindow.UpdateLayout();
                await Task.Delay(250);
                CaptureVisual(settingsWindow, Path.Combine(docsDir, "settings-events.jpg"), 860, 670, isJpeg: true);
                File.AppendAllText(logFile, "Captured settings-events.jpg\n");

                // Tab 1: Alerts & Audio
                tabControl.SelectedIndex = 1;
                settingsWindow.UpdateLayout();
                await Task.Delay(250);
                CaptureVisual(settingsWindow, Path.Combine(docsDir, "settings-audio.png"), 860, 670);
                File.AppendAllText(logFile, "Captured settings-audio.png\n");

                // Tab 2: Appearance
                tabControl.SelectedIndex = 2;
                settingsWindow.UpdateLayout();
                await Task.Delay(250);
                CaptureVisual(settingsWindow, Path.Combine(docsDir, "hud-appearance.jpg"), 860, 670, isJpeg: true);
                File.AppendAllText(logFile, "Captured hud-appearance.jpg\n");

                // Tab 3: System
                tabControl.SelectedIndex = 3;
                settingsWindow.UpdateLayout();
                await Task.Delay(250);
                CaptureVisual(settingsWindow, Path.Combine(docsDir, "settings-system.png"), 860, 670);
                File.AppendAllText(logFile, "Captured settings-system.png\n");
            }
            settingsWindow.Close();

            // 2. Capture HUD Overlay (Vibrant Atmospheric Stardust Backdrop)
            var overlayWindow = Current.Windows.OfType<OverlayWindow>().FirstOrDefault() ?? (OverlayWindow)MainWindow;
            var overlayVm = _serviceProvider!.GetRequiredService<OverlayViewModel>();
            var rootBorder = (FrameworkElement)overlayWindow.Content;

            overlayVm.ShowBackground = true;
            overlayVm.ShowBorder = true;
            overlayVm.ShowMiniControls = true;
            overlayVm.Opacity = 1.0;
            overlayVm.Scale = 1.0;

            var hudBackdrop = new RadialGradientBrush(
                Color.FromRgb(0x13, 0x1D, 0x30),
                Color.FromRgb(0x06, 0x09, 0x12))
            {
                Center = new Point(0.5, 0.45),
                GradientOrigin = new Point(0.5, 0.45),
                RadiusX = 0.85,
                RadiusY = 0.85
            };

            // 2A. Capture Full Multi-Timer HUD (shows Cargo Scramble, Gravity Abyss, Drift Greenhouse)
            overlayVm.CompactMode = false;
            overlayWindow.Width = double.NaN;
            overlayWindow.Height = double.NaN;
            overlayWindow.SizeToContent = SizeToContent.WidthAndHeight;
            overlayWindow.Show();
            overlayWindow.UpdateLayout();
            await Task.Delay(350);
            CaptureVisual(overlayWindow, Path.Combine(docsDir, "hud-multitimer.png"), 520, 360, backdrop: hudBackdrop, drawBorderFrame: true);
            File.AppendAllText(logFile, $"Captured hud-multitimer.png (size: {overlayWindow.ActualWidth}x{overlayWindow.ActualHeight})\n");

            // 2B. Capture Mini-HUD (showcase Single-Timer Gravity Abyss floating pill)
            var singleList = settingsService.Settings.Events.Select(e => e.Clone()).ToList();
            foreach (var ev in singleList) ev.IsEnabled = false;
            var gMini = singleList.FirstOrDefault(e => e.Id == "gravity-abyss");
            if (gMini != null) gMini.IsEnabled = true;

            scheduler.SetTrackedEvents(singleList, "gravity-abyss", 0);
            overlayVm.CompactMode = true;
            overlayWindow.Width = 250;
            overlayWindow.Height = double.NaN;
            overlayWindow.SizeToContent = SizeToContent.Height;
            overlayWindow.UpdateLayout();
            await Task.Delay(350);
            CaptureVisual(overlayWindow, Path.Combine(docsDir, "hud-mini.png"), 380, 180, backdrop: hudBackdrop, drawBorderFrame: true);
            File.AppendAllText(logFile, $"Captured hud-mini.png (size: {overlayWindow.ActualWidth}x{overlayWindow.ActualHeight})\n");

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

    private static void CaptureVisual(FrameworkElement visual, string outputPath, int defaultW = 0, int defaultH = 0, bool isJpeg = false, Brush? backdrop = null, bool drawBorderFrame = false)
    {
        double rawW = !double.IsNaN(visual.ActualWidth) && visual.ActualWidth > 0 ? visual.ActualWidth : (!double.IsNaN(visual.Width) ? visual.Width : 0);
        double rawH = !double.IsNaN(visual.ActualHeight) && visual.ActualHeight > 0 ? visual.ActualHeight : (!double.IsNaN(visual.Height) ? visual.Height : 0);

        int outW = defaultW > 0 ? defaultW : (int)rawW;
        int outH = defaultH > 0 ? defaultH : (int)rawH;
        if (outW <= 0) outW = 400;
        if (outH <= 0) outH = 300;

        int visualW = (int)Math.Max(1, rawW > 0 ? rawW : outW);
        int visualH = (int)Math.Max(1, rawH > 0 ? rawH : outH);

        var rtb = new RenderTargetBitmap(outW, outH, 96, 96, PixelFormats.Pbgra32);
        var dv = new DrawingVisual();
        using (var dc = dv.RenderOpen())
        {
            if (backdrop != null)
            {
                var borderPen = drawBorderFrame ? new Pen(new SolidColorBrush(Color.FromArgb(0x35, 0x00, 0xF0, 0xFF)), 1.5) : null;
                dc.DrawRoundedRectangle(backdrop, borderPen, new Rect(0.75, 0.75, Math.Max(1, outW - 1.5), Math.Max(1, outH - 1.5)), 10, 10);
            }

            int offsetX = outW > visualW ? (outW - visualW) / 2 : 0;
            int offsetY = outH > visualH ? (outH - visualH) / 2 : 0;

            var brush = new VisualBrush(visual)
            {
                Stretch = Stretch.None,
                Viewbox = new Rect(0, 0, visualW, visualH),
                ViewboxUnits = BrushMappingMode.Absolute
            };
            dc.DrawRectangle(brush, null, new Rect(offsetX, offsetY, visualW, visualH));
        }
        rtb.Render(dv);

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
