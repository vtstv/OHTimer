// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using OnceHumanEventTimer.Core.Localization;
using OnceHumanEventTimer.Core.Scheduling;
using OnceHumanEventTimer.Core.Services;
using OnceHumanEventTimer.Infrastructure.Audio;
using OnceHumanEventTimer.Infrastructure.Notifications;
using OnceHumanEventTimer.Infrastructure.Storage;
using OnceHumanEventTimer.Infrastructure.Updates;
using OnceHumanEventTimer.Infrastructure.Windows;
using OnceHumanEventTimer.Infrastructure.Win32;
using OnceHumanEventTimer.UI.ViewModels;

namespace OnceHumanEventTimer.UI;

public partial class App
{
    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<ISettingsService, FileSettingsService>();
        services.AddSingleton<IAudioService, SynthesizerAudioService>();
        services.AddSingleton<INotificationService, WindowsNotificationService>();
        services.AddSingleton<IEventScheduler, EventScheduler>();
        services.AddSingleton<IHotkeyService, Win32HotkeyService>();
        services.AddSingleton<IUpdateService, GitHubUpdateService>();
        services.AddSingleton<IAutostartService, WindowsAutostartService>();
        services.AddSingleton<IGameFocusMonitor, GameFocusMonitor>();

        services.AddSingleton<OverlayViewModel>();
        services.AddTransient<SettingsViewModel>();
    }
}
