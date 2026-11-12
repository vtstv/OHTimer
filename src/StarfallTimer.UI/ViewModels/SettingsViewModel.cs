// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using StarfallTimer.Core.Models;
using StarfallTimer.Core.Services;

namespace StarfallTimer.UI.ViewModels;

/// <summary>
/// ViewModel driving the settings configuration dialog.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IEventScheduler _scheduler;
    private readonly IAudioService _audio;
    private readonly ILocalizationService _loc;

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
    private LanguageInfo _selectedLanguage;

    [ObservableProperty]
    private string _toggleVisibilityHotkey;

    [ObservableProperty]
    private string _toggleClickThroughHotkey;

    [ObservableProperty]
    private bool _showSavedNotification;

    public IReadOnlyList<LanguageInfo> SupportedLanguages => _loc.SupportedLanguages;

    public Action? RequestResetPosition { get; set; }
    public Action? RequestCloseSettings { get; set; }

    public SettingsViewModel(
        ISettingsService settingsService,
        IEventScheduler scheduler,
        IAudioService audio,
        ILocalizationService loc)
    {
        _settingsService = settingsService;
        _scheduler = scheduler;
        _audio = audio;
        _loc = loc;

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
        _toggleVisibilityHotkey = current.ToggleVisibilityHotkey;
        _toggleClickThroughHotkey = current.ToggleClickThroughHotkey;

        _selectedLanguage = SupportedLanguages.FirstOrDefault(l => l.Code == current.Language)
                            ?? SupportedLanguages.First();

        InitializeEvents(current);
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
        s.Language = SelectedLanguage.Code;
        s.ToggleVisibilityHotkey = ToggleVisibilityHotkey;
        s.ToggleClickThroughHotkey = ToggleClickThroughHotkey;

        s.Events = Events.ToList();
        s.ActiveEventId = ActiveEventId;
        s.GlobalTimeOffsetSeconds = GlobalTimeOffsetSeconds;

        var activeEvent = s.GetActiveEvent();
        s.StartMinute = activeEvent.StartMinute;
        s.DurationMinutes = activeEvent.DurationMinutes;

        _loc.SetLanguage(SelectedLanguage.Code);
        _scheduler.SetActiveEvent(activeEvent, GlobalTimeOffsetSeconds);
        _scheduler.ConfigureSchedule(activeEvent.StartMinute, activeEvent.DurationMinutes, AlertMinutesBefore, AlertMinutesBeforeEnd);

        await _settingsService.SaveAsync();

        ShowSavedNotification = true;
        await Task.Delay(2500);
        ShowSavedNotification = false;
    }
}
