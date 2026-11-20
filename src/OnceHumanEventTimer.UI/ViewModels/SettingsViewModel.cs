// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.UI.ViewModels;

/// <summary>
/// ViewModel driving the settings configuration dialog.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IEventScheduler _scheduler;
    private readonly IAudioService _audio;
    private readonly ILocalizationService _loc;
    private readonly IUpdateService? _updateService;

    [ObservableProperty]
    private int _startMinute;

    [ObservableProperty]
    private int _durationMinutes;

    [ObservableProperty]
    private int _alertMinutesBefore;

    [ObservableProperty]
    private int _alertMinutesBeforeEnd;

    [ObservableProperty]
    private double _volume;

    [ObservableProperty]
    private bool _playAlertSound;

    [ObservableProperty]
    private bool _playOnEventStart;

    [ObservableProperty]
    private bool _showToastAlert;

    [ObservableProperty]
    private bool _areAlertsPaused;

    [ObservableProperty]
    private string? _customSoundPath;

    [ObservableProperty]
    private double _opacity;

    [ObservableProperty]
    private double _scale;

    [ObservableProperty]
    private bool _alwaysOnTop;

    [ObservableProperty]
    private bool _isClickThrough;

    [ObservableProperty]
    private bool _isLocked;

    [ObservableProperty]
    private bool _compactMode;

    [ObservableProperty]
    private bool _showBorder;

    [ObservableProperty]
    private bool _showBackground;

    [ObservableProperty]
    private bool _showProgressRing;

    [ObservableProperty]
    private bool _showNextEventTime;

    [ObservableProperty]
    private bool _showMiniControls;

    [ObservableProperty]
    private LanguageInfo _selectedLanguage;

    partial void OnOpacityChanged(double value) { _settingsService.Settings.Opacity = value; _settingsService.NotifyChanged(); }
    partial void OnScaleChanged(double value) { _settingsService.Settings.Scale = value; _settingsService.NotifyChanged(); }
    partial void OnCompactModeChanged(bool value) { _settingsService.Settings.CompactMode = value; _settingsService.NotifyChanged(); }
    partial void OnShowBorderChanged(bool value) { _settingsService.Settings.ShowBorder = value; _settingsService.NotifyChanged(); }
    partial void OnShowBackgroundChanged(bool value) { _settingsService.Settings.ShowBackground = value; _settingsService.NotifyChanged(); }
    partial void OnShowProgressRingChanged(bool value) { _settingsService.Settings.ShowProgressRing = value; _settingsService.NotifyChanged(); }
    partial void OnShowNextEventTimeChanged(bool value) { _settingsService.Settings.ShowNextEventTime = value; _settingsService.NotifyChanged(); }
    partial void OnShowMiniControlsChanged(bool value) { _settingsService.Settings.ShowMiniControls = value; _settingsService.NotifyChanged(); }

    [ObservableProperty]
    private bool _showSavedNotification;

    public IReadOnlyList<LanguageInfo> SupportedLanguages => _loc.SupportedLanguages;

    public Action? RequestResetPosition { get; set; }
    public Action? RequestCloseSettings { get; set; }
    public Action? RequestRestartOnboarding { get; set; }

    [RelayCommand]
    private void RestartOnboarding() => RequestRestartOnboarding?.Invoke();

    public SettingsViewModel(
        ISettingsService settingsService,
        IEventScheduler scheduler,
        IAudioService audio,
        ILocalizationService loc,
        IUpdateService? updateService = null,
        IAutostartService? autostartService = null)
    {
        _settingsService = settingsService;
        _scheduler = scheduler;
        _audio = audio;
        _loc = loc;
        _updateService = updateService;
        _autostartService = autostartService;

        var current = _settingsService.Settings;
        _startMinute = current.StartMinute;
        _durationMinutes = current.DurationMinutes;
        _alertMinutesBefore = current.AlertMinutesBefore;
        _alertMinutesBeforeEnd = current.AlertMinutesBeforeEnd;
        _volume = current.Volume;
        _playAlertSound = current.PlayAlertSound;
        _playOnEventStart = current.PlayOnEventStart;
        _showToastAlert = current.ShowToastAlert;
        _areAlertsPaused = current.AreAlertsPaused;
        _customSoundPath = current.CustomSoundPath;
        _opacity = current.Opacity;
        _scale = current.Scale;
        _alwaysOnTop = current.AlwaysOnTop;
        _isClickThrough = current.IsClickThrough;
        _isLocked = current.IsLocked;
        _compactMode = current.CompactMode;
        _showBorder = current.ShowBorder;
        _showBackground = current.ShowBackground;
        _showProgressRing = current.ShowProgressRing;
        _showNextEventTime = current.ShowNextEventTime;
        _showMiniControls = current.ShowMiniControls;

        _selectedLanguage = SupportedLanguages.FirstOrDefault(l => l.Code == current.Language)
                            ?? SupportedLanguages.First();

        InitializeEvents(current);
        InitializeAudio(current);
        InitializeSystem(current, autostartService);
        InitializeHotkeys(current);
        InitializeBossTimer(current);
    }

    [RelayCommand]
    private void TestAudio()
    {
        _audio.PlayWarning(Volume, CustomSoundPath);
    }

    [RelayCommand]
    private void BrowseSound()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "WAV Audio Files (*.wav)|*.wav|All Files (*.*)|*.*",
            Title = "Select Custom Alert Sound"
        };

        if (dialog.ShowDialog() == true)
        {
            CustomSoundPath = dialog.FileName;
        }
    }

    /// <summary>
    /// Gets the application display version string formatted as "Version X.Y.Z".
    /// Dynamically reflects the assembly version centrally defined in Directory.Build.props.
    /// </summary>
    public string AppVersionText => $"Version {typeof(App).Assembly.GetName().Version?.ToString(3) ?? "1.6.1"}";
}
