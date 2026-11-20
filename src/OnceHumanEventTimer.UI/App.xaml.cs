// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using OnceHumanEventTimer.Core.Localization;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Scheduling;
using OnceHumanEventTimer.Core.Services;
using OnceHumanEventTimer.Infrastructure.Audio;
using OnceHumanEventTimer.Infrastructure.Notifications;
using OnceHumanEventTimer.Infrastructure.Storage;
using OnceHumanEventTimer.Infrastructure.Updates;
using OnceHumanEventTimer.Infrastructure.Windows;
using OnceHumanEventTimer.Infrastructure.Win32;
using OnceHumanEventTimer.UI.Localization;
using OnceHumanEventTimer.UI.ViewModels;
using OnceHumanEventTimer.UI.Views;

namespace OnceHumanEventTimer.UI;

/// <summary>
/// Application entry point configuring Dependency Injection, single-instance mutex, and lifecycle.
/// </summary>
public partial class App : Application
{
    private static Mutex? _singleInstanceMutex;
    private ServiceProvider? _serviceProvider;
    private Win32TrayService? _trayService;
    private Tray.TrayMenuWindow? _trayMenuWindow;

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Enforce single instance
        var cmdArgs = Environment.GetCommandLineArgs();
        bool isCapture = cmdArgs.Any(a => a.StartsWith("--capture-screenshots", StringComparison.OrdinalIgnoreCase));
        _singleInstanceMutex = new Mutex(true, "OnceHuman_EventTimer_Mutex_V1", out bool isNewInstance);
        if (!isNewInstance && !isCapture)
        {
            MessageBox.Show("Once Human: Event Timer is already running. Check your system tray.", "Once Human: Event Timer", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        base.OnStartup(e);

        string? captureFolder = null;
        if (isCapture)
        {
            captureFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "OHTimer_Capture_" + Guid.NewGuid().ToString("N"));
        }

        var services = new ServiceCollection();
        ConfigureServices(services, captureFolder);
        _serviceProvider = services.BuildServiceProvider();

        // 1. Load Settings
        var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
        await settingsService.LoadAsync();

        // 2. Setup Localization
        var loc = _serviceProvider.GetRequiredService<ILocalizationService>();
        loc.SetLanguage(settingsService.Settings.Language);
        LocalizationProvider.Instance.Initialize(loc);

        // 3. Configure and Wire Scheduler Alerts
        var scheduler = _serviceProvider.GetRequiredService<IEventScheduler>();
        var audio = _serviceProvider.GetRequiredService<IAudioService>();
        var notifications = _serviceProvider.GetRequiredService<INotificationService>();

        var activeEvent = settingsService.Settings.GetActiveEvent();
        scheduler.SetTrackedEvents(settingsService.Settings.Events, settingsService.Settings.ActiveEventId, settingsService.Settings.GlobalTimeOffsetSeconds);
        scheduler.SetActiveEvent(activeEvent, settingsService.Settings.GlobalTimeOffsetSeconds);
        scheduler.ConfigureSchedule(
            activeEvent.StartMinute,
            activeEvent.DurationMinutes,
            settingsService.Settings.AlertMinutesBefore,
            settingsService.Settings.AlertMinutesBeforeEnd,
            activeEvent.StartSecond);

        scheduler.NotificationTriggered += (s, args) =>
        {
            if (settingsService.Settings.AreAlertsPaused) return;

            if (args.PlaySound && settingsService.Settings.PlayAlertSound)
            {
                string? soundPath = args.Event?.CustomSoundPath;
                if (string.IsNullOrWhiteSpace(soundPath))
                {
                    soundPath = settingsService.Settings.CustomSoundPath;
                }

                SoundPreset preset = args.Event?.SoundPreset ?? settingsService.Settings.AlertSoundPreset;

                if (args.Phase == EventPhase.Active)
                {
                    if (args.IsExtraAlert)
                    {
                        if (preset == SoundPreset.Custom && !string.IsNullOrWhiteSpace(soundPath))
                            audio.PlayPreset(SoundPreset.Custom, settingsService.Settings.Volume, soundPath);
                        else
                            audio.PlayPreset(preset, settingsService.Settings.Volume);
                    }
                    else if (settingsService.Settings.PlayOnEventStart && (args.Event == null || args.Event.AlertOnEventStart))
                    {
                        if (preset == SoundPreset.Custom && !string.IsNullOrWhiteSpace(soundPath))
                            audio.PlayPreset(SoundPreset.Custom, settingsService.Settings.Volume, soundPath);
                        else
                            audio.PlayEventStart(settingsService.Settings.Volume);
                    }
                }
                else if (args.Phase == EventPhase.EndingSoon)
                {
                    if (preset == SoundPreset.Custom && !string.IsNullOrWhiteSpace(soundPath))
                        audio.PlayPreset(SoundPreset.Custom, settingsService.Settings.Volume, soundPath);
                    else
                        audio.PlayEventEnding(settingsService.Settings.Volume);
                }
                else
                {
                    audio.PlayPreset(preset, settingsService.Settings.Volume, soundPath);
                }
            }

            if (args.ShowToast && settingsService.Settings.ShowToastAlert)
            {
                notifications.ShowNotification(args.Title, args.Message);
            }
        };

        // 4. Create Overlay Window
        var overlayVm = _serviceProvider.GetRequiredService<OverlayViewModel>();
        var overlayWindow = new OverlayWindow(overlayVm, settingsService);
        MainWindow = overlayWindow;
        overlayWindow.Show();

        // 5. Wire Overlay Actions
        overlayVm.RequestOpenSettings = OpenSettingsWindow;
        overlayVm.RequestHideOverlay = () => overlayWindow.Hide();
        overlayVm.RequestExitApp = ShutdownApp;

        // 6. Setup Hotkeys, Boss Timer & Kill Counter
        var bossTimer = _serviceProvider.GetRequiredService<IBossTimerTracker>();
        SetupBossTimerAlerts(bossTimer, settingsService, audio, loc, notifications);

        var killCounter = _serviceProvider.GetRequiredService<IKillCounterTracker>();
        SetupKillCounterAlerts(killCounter, bossTimer, settingsService, audio, loc, notifications);

        var hotkeyService = _serviceProvider.GetRequiredService<IHotkeyService>();
        if (hotkeyService is Win32HotkeyService winHotkey)
        {
            winHotkey.Initialize();
            RegisterHotkeys(winHotkey, settingsService.Settings, overlayWindow, overlayVm, loc, notifications, bossTimer, killCounter);
        }

        // 7. Setup System Tray & Game Focus Monitor
        SetupSystemTray(loc, overlayWindow, overlayVm, settingsService, notifications);
        var focusMonitor = _serviceProvider.GetRequiredService<IGameFocusMonitor>();
        SetupFocusMonitor(focusMonitor, settingsService, overlayWindow);

        settingsService.SettingsChanged += (s, updatedSettings) =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (hotkeyService is Win32HotkeyService winHotkey)
                {
                    RegisterHotkeys(winHotkey, updatedSettings, overlayWindow, overlayVm, loc, notifications, bossTimer, killCounter);
                }

                if (!updatedSettings.AutoHideWhenGameOutOfFocus && !_userManuallyHidden && !overlayWindow.IsVisible)
                {
                    overlayWindow.Show();
                }
            });
        };

        // 9. Start Timing Engine
        scheduler.Start();

        if (isCapture)
        {
            _ = HandleScreenshotGenerationAsync(cmdArgs);
        }
        else if (!settingsService.Settings.HasCompletedOnboarding)
        {
            OpenOnboardingWizard();
        }
    }

    private void PersistSettingsOnExit()
    {
        try
        {
            var ss = _serviceProvider?.GetService<ISettingsService>();
            if (ss is Infrastructure.Storage.FileSettingsService fss)
            {
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };
                byte[] bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(ss.Settings, options);
                System.IO.File.WriteAllBytes(fss.SettingsFilePath, bytes);
            }
        }
        catch { }
    }

    private void ShutdownApp()
    {
        PersistSettingsOnExit();
        _trayMenuWindow?.CloseForShutdown();
        _trayService?.Dispose();
        try { _singleInstanceMutex?.ReleaseMutex(); } catch { }
        _singleInstanceMutex?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        PersistSettingsOnExit();
        _trayMenuWindow?.CloseForShutdown();
        _trayService?.Dispose();
        _serviceProvider?.Dispose();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
