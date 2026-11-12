// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using StarfallTimer.Core.Localization;
using StarfallTimer.Core.Models;
using StarfallTimer.Core.Scheduling;
using StarfallTimer.Core.Services;
using StarfallTimer.Infrastructure.Audio;
using StarfallTimer.Infrastructure.Notifications;
using StarfallTimer.Infrastructure.Storage;
using StarfallTimer.Infrastructure.Updates;
using StarfallTimer.Infrastructure.Win32;
using StarfallTimer.UI.Localization;
using StarfallTimer.UI.ViewModels;
using StarfallTimer.UI.Views;

namespace StarfallTimer.UI;

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
        const string mutexName = "OnceHuman_EventTimer_Mutex_V1";
        _singleInstanceMutex = new Mutex(true, mutexName, out bool isNewInstance);
        if (!isNewInstance)
        {
            MessageBox.Show(
                "Once Human Starfall Timer is already running. Check your system tray.",
                "Starfall Timer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

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
            settingsService.Settings.AlertMinutesBeforeEnd);

        scheduler.NotificationTriggered += (s, args) =>
        {
            if (args.PlaySound && settingsService.Settings.PlayAlertSound)
            {
                if (args.Phase == EventPhase.Active)
                {
                    if (settingsService.Settings.PlayOnEventStart)
                    {
                        audio.PlayEventStart(settingsService.Settings.Volume);
                    }
                }
                else if (args.Phase == EventPhase.EndingSoon)
                {
                    audio.PlayEventEnding(settingsService.Settings.Volume);
                }
                else
                {
                    audio.PlayWarning(settingsService.Settings.Volume, settingsService.Settings.CustomSoundPath);
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
                    if (overlayWindow.IsVisible) overlayWindow.Hide();
                    else overlayWindow.Show();
                });
            },
            toggleLockAction: () =>
            {
                Dispatcher.Invoke(() => overlayVm.ToggleLockCommand.Execute(null));
            },
            openSettingsAction: OpenSettingsWindow,
            exitAction: ShutdownApp);

        _trayService.Initialize(overlayWindow.Handle);

        if (notifications is WindowsNotificationService winNotif)
        {
            winNotif.OnShowNotification = (title, msg) => _trayService.ShowNotification(title, msg);
        }

        // 8. Start Timing Engine
        scheduler.Start();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<ISettingsService, FileSettingsService>();
        services.AddSingleton<IAudioService, SynthesizerAudioService>();
        services.AddSingleton<INotificationService, WindowsNotificationService>();
        services.AddSingleton<IEventScheduler, StarfallEventScheduler>();
        services.AddSingleton<IHotkeyService, Win32HotkeyService>();
        services.AddSingleton<IUpdateService, GitHubUpdateService>();

        services.AddSingleton<OverlayViewModel>();
        services.AddTransient<SettingsViewModel>();
    }

    private void RegisterHotkeys(
        IHotkeyService hotkeyService,
        UserSettings settings,
        OverlayWindow overlayWindow,
        OverlayViewModel overlayVm)
    {
        hotkeyService.UnregisterAll();

        if (!string.IsNullOrWhiteSpace(settings.ToggleVisibilityHotkey))
        {
            hotkeyService.RegisterHotkey(settings.ToggleVisibilityHotkey, () =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (overlayWindow.IsVisible) overlayWindow.Hide();
                    else overlayWindow.Show();
                });
            });
        }

        if (!string.IsNullOrWhiteSpace(settings.ToggleClickThroughHotkey))
        {
            hotkeyService.RegisterHotkey(settings.ToggleClickThroughHotkey, () =>
            {
                Dispatcher.Invoke(() =>
                {
                    overlayVm.ToggleClickThroughCommand.Execute(null);
                });
            });
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
