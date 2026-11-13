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

    [ObservableProperty]
    private string _toggleVisibilityHotkey;

    [ObservableProperty]
    private string _toggleClickThroughHotkey;

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
        _toggleVisibilityHotkey = current.ToggleVisibilityHotkey;
        _toggleClickThroughHotkey = current.ToggleClickThroughHotkey;

        _selectedLanguage = SupportedLanguages.FirstOrDefault(l => l.Code == current.Language)
                            ?? SupportedLanguages.First();

        InitializeEvents(current);
        InitializeAudio(current);
        InitializeSystem(current, autostartService);
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

    [RelayCommand]
    private void ResetPosition()
    {
        RequestResetPosition?.Invoke();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var s = _settingsService.Settings;
        s.StartMinute = StartMinute;
        s.DurationMinutes = DurationMinutes;
        s.AlertMinutesBefore = AlertMinutesBefore;
        s.AlertMinutesBeforeEnd = AlertMinutesBeforeEnd;
        s.Volume = Volume;
        s.PlayAlertSound = PlayAlertSound;
        s.PlayOnEventStart = PlayOnEventStart;
        s.ShowToastAlert = ShowToastAlert;
        s.CustomSoundPath = CustomSoundPath;
        s.Opacity = Opacity;
        s.Scale = Scale;
        s.AlwaysOnTop = AlwaysOnTop;
        s.IsClickThrough = IsClickThrough;
        s.IsLocked = IsLocked;
        s.CompactMode = CompactMode;
        s.ShowBorder = ShowBorder;
        s.ShowBackground = ShowBackground;
        s.ShowProgressRing = ShowProgressRing;
        s.ShowNextEventTime = ShowNextEventTime;
        s.ShowMiniControls = ShowMiniControls;
        s.AlertSoundPreset = AlertSoundPreset;
        s.Language = SelectedLanguage.Code;
        s.ToggleVisibilityHotkey = ToggleVisibilityHotkey;
        s.ToggleClickThroughHotkey = ToggleClickThroughHotkey;
        s.StartWithWindows = StartWithWindows;
        s.AutoHideWhenGameOutOfFocus = AutoHideWhenGameOutOfFocus;

        _autostartService?.SetAutostart(StartWithWindows);

        s.Events = Events.ToList();
        s.ActiveEventId = ActiveEventId;
        s.GlobalTimeOffsetSeconds = GlobalTimeOffsetSeconds;

        var activeEvent = s.GetActiveEvent();
        s.StartMinute = activeEvent.StartMinute;
        s.DurationMinutes = activeEvent.DurationMinutes;

        _loc.SetLanguage(SelectedLanguage.Code);
        _scheduler.SetTrackedEvents(s.Events, s.ActiveEventId, s.GlobalTimeOffsetSeconds);
        _scheduler.SetActiveEvent(activeEvent, GlobalTimeOffsetSeconds);
        _scheduler.ConfigureSchedule(activeEvent.StartMinute, activeEvent.DurationMinutes, AlertMinutesBefore, AlertMinutesBeforeEnd, activeEvent.StartSecond);

        await _settingsService.SaveAsync();

        ShowSavedNotification = true;
        await Task.Delay(2500);
        ShowSavedNotification = false;
    }
}
