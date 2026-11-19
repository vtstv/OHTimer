// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.Input;

namespace OnceHumanEventTimer.UI.ViewModels;

public partial class SettingsViewModel
{
    [RelayCommand]
    private void ResetPosition()
    {
        RequestResetPosition?.Invoke();
    }

    [RelayCommand]
    private void LaunchTutorial()
    {
        RequestRestartOnboarding?.Invoke();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var s = _settingsService.Settings;
        s.AlertMinutesBefore = AlertMinutesBefore;
        s.AlertMinutesBeforeEnd = AlertMinutesBeforeEnd;
        s.Volume = Volume;
        s.PlayAlertSound = PlayAlertSound;
        s.PlayOnEventStart = PlayOnEventStart;
        s.ShowToastAlert = ShowToastAlert;
        s.AreAlertsPaused = AreAlertsPaused;
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
        s.TogglePauseAlertsHotkey = TogglePauseAlertsHotkey;
        s.StartWithWindows = StartWithWindows;
        s.AutoHideWhenGameOutOfFocus = AutoHideWhenGameOutOfFocus;

        _autostartService?.SetAutostart(StartWithWindows);

        s.Events = Events.ToList();

        if (!Events.Any(e => e.Id == ActiveEventId && e.IsEnabled))
        {
            if (SelectedEvent != null && SelectedEvent.IsEnabled)
            {
                ActiveEventId = SelectedEvent.Id;
            }
            else
            {
                var firstEnabled = Events.FirstOrDefault(e => e.IsEnabled);
                if (firstEnabled != null) ActiveEventId = firstEnabled.Id;
            }
        }

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
