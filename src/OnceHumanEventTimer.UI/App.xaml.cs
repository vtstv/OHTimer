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
    private SettingsWindow? _settingsWindow;

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

        var services = new ServiceCollection();
        ConfigureServices(services);
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

        scheduler.SetTrackedEvents(settingsService.Settings.Events, settingsService.Settings.ActiveEventId, settingsService.Settings.GlobalTimeOffsetSeconds);
        scheduler.SetActiveEvent(settingsService.Settings.GetActiveEvent(), settingsService.Settings.GlobalTimeOffsetSeconds);
        scheduler.ConfigureSchedule(
            settingsService.Settings.StartMinute,
            settingsService.Settings.DurationMinutes,
            settingsService.Settings.AlertMinutesBefore,
            settingsService.Settings.AlertMinutesBeforeEnd,
            settingsService.Settings.StartSecond);

        scheduler.NotificationTriggered += (s, args) =>
        {
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
                    if (settingsService.Settings.PlayOnEventStart)
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

        // 6. Setup Hotkeys
        var hotkeyService = _serviceProvider.GetRequiredService<IHotkeyService>();
        if (hotkeyService is Win32HotkeyService winHotkey)
        {
            winHotkey.Initialize(overlayWindow.Handle);
            RegisterHotkeys(winHotkey, settingsService.Settings, overlayWindow, overlayVm);
        }

        // 7. Setup System Tray
        _trayService = new Win32TrayService(
            loc,
            toggleVisibilityAction: () =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (overlayWindow.IsVisible)
                    {
                        _userManuallyHidden = true;
                        overlayWindow.Hide();
                    }
                    else
                    {
                        _userManuallyHidden = false;
                        overlayWindow.Show();
                    }
                });
            },
            toggleLockAction: () => Dispatcher.Invoke(() => overlayVm.ToggleLockCommand.Execute(null)),
            openSettingsAction: OpenSettingsWindow,
            exitAction: ShutdownApp);

        _trayService.Initialize(overlayWindow.Handle);
        if (notifications is WindowsNotificationService winNotif)
            winNotif.OnShowNotification = (title, msg) => _trayService.ShowNotification(title, msg);

        // 8. Setup Game Focus Monitor (Auto-hide when Once Human not in focus)
        var focusMonitor = _serviceProvider.GetRequiredService<IGameFocusMonitor>();
        focusMonitor.GameFocusChanged += (s, isFocused) =>
        {
            Dispatcher.Invoke(() =>
            {
                if (!settingsService.Settings.AutoHideWhenGameOutOfFocus) return;

                if (isFocused)
                {
                    if (!_userManuallyHidden && !overlayWindow.IsVisible)
                    {
                        overlayWindow.Show();
                    }
                }
                else
                {
                    if (overlayWindow.IsVisible)
                    {
                        overlayWindow.Hide();
                    }
                }
            });
        };
        focusMonitor.Start();

        settingsService.SettingsChanged += (s, updatedSettings) =>
        {
            Dispatcher.Invoke(() =>
            {
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
    }

    private void OpenSettingsWindow()
    {
        if (_settingsWindow != null && _settingsWindow.IsLoaded)
        {
            _settingsWindow.Activate();
            return;
        }

        var vm = _serviceProvider!.GetRequiredService<SettingsViewModel>();
        vm.RequestResetPosition = () =>
        {
            if (MainWindow != null)
            {
                MainWindow.Left = 40;
                MainWindow.Top = 40;
            }
        };

        _settingsWindow = new SettingsWindow(vm);
        _settingsWindow.Closed += (s, e) => _settingsWindow = null;
        _settingsWindow.Show();
    }

    private void ShutdownApp()
    {
        _trayService?.Dispose();
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayService?.Dispose();
        _serviceProvider?.Dispose();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
