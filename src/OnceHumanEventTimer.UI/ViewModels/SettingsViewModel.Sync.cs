// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnceHumanEventTimer.Core.Scheduling;

namespace OnceHumanEventTimer.UI.ViewModels;

public partial class SettingsViewModel
{
    [ObservableProperty]
    private string _eventRemainingInput = "1h 3m 29s";

    [ObservableProperty]
    private string? _calibrateFeedbackMessage;

    [ObservableProperty]
    private bool _showCalibrateFeedback;

    [ObservableProperty]
    private string _gameRemainingInput = "08:23";

    [ObservableProperty]
    private string? _syncFeedbackMessage;

    [ObservableProperty]
    private bool _showSyncFeedback;

    [RelayCommand]
    private void CalibrateEventStart()
    {
        if (SelectedEvent == null)
        {
            CalibrateFeedbackMessage = _loc.GetString("Settings_NoEventSelected");
            ShowCalibrateFeedback = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(EventRemainingInput))
        {
            CalibrateFeedbackMessage = _loc.GetString("Settings_CalibrateEmptyPrompt");
            ShowCalibrateFeedback = true;
            return;
        }

        if (!GameTimeParser.TryParse(EventRemainingInput, out var remaining, allowZero: false))
        {
            CalibrateFeedbackMessage = _loc.GetString("Settings_CalibrateInvalidFormat");
            ShowCalibrateFeedback = true;
            return;
        }

        var now = DateTimeOffset.Now;
        var targetStartTime = now.Add(remaining);

        var (h, m, s) = EventTimeCalculator.CalculateCycleStartTime(targetStartTime, SelectedEvent.CycleMinutes);
        SelectedEvent.StartHour = h;
        SelectedEvent.StartMinute = m;
        SelectedEvent.StartSecond = s;
        // Reset any accidental or extreme time offset back to zero
        SelectedEvent.TimeOffsetSeconds = 0;

        // If the remaining time indicates the previous cycle would still overlap the active duration,
        // adjust duration so the event is recognized as concluded and waiting for the next start.
        double elapsedMinutes = SelectedEvent.CycleMinutes - remaining.TotalMinutes;
        if (elapsedMinutes >= 0 && elapsedMinutes < SelectedEvent.DurationMinutes)
        {
            SelectedEvent.DurationMinutes = Math.Max(1, (int)Math.Floor(elapsedMinutes));
            OnPropertyChanged(nameof(SelectedEventDurationMinutes));
        }

        OnPropertyChanged(nameof(SelectedEventStartHour));
        OnPropertyChanged(nameof(SelectedEventStartMinute));
        OnPropertyChanged(nameof(SelectedEventStartSecond));
        OnPropertyChanged(nameof(SelectedEventTimeOffsetSeconds));

        // Immediately update scheduler with newly calibrated timings
        _scheduler.SetTrackedEvents(Events, ActiveEventId, GlobalTimeOffsetSeconds);

        string template = _loc.GetString("Settings_CalibrateSuccess");
        string countdownStr = remaining.Hours > 0
            ? $"{remaining.Hours}h {remaining.Minutes}m {remaining.Seconds}s"
            : $"{remaining.Minutes}m {remaining.Seconds}s";
        CalibrateFeedbackMessage = string.Format(template, h, m, s, countdownStr);
        ShowCalibrateFeedback = true;
    }

    [RelayCommand]
    private void SyncToGame()
    {
        if (string.IsNullOrWhiteSpace(GameRemainingInput))
        {
            SyncFeedbackMessage = "Please enter remaining countdown (e.g. 08:23) or 0s";
            ShowSyncFeedback = true;
            return;
        }

        if (!GameTimeParser.TryParse(GameRemainingInput, out var remaining, allowZero: true))
        {
            SyncFeedbackMessage = "Invalid format. Use hh:mm:ss, mm:ss, or 0s (reset)";
            ShowSyncFeedback = true;
            return;
        }

        if (remaining == TimeSpan.Zero)
        {
            if (SelectedEvent != null)
            {
                SelectedEvent.TimeOffsetSeconds = 0;
                OnPropertyChanged(nameof(SelectedEventTimeOffsetSeconds));
            }
            _scheduler.SetTimeOffset(GlobalTimeOffsetSeconds);
            SyncFeedbackMessage = string.Format(_loc.GetString("Settings_SyncSuccess"), 0);
            ShowSyncFeedback = true;
            return;
        }

        var targetEvent = SelectedEvent ?? _scheduler.ActiveEvent;
        _scheduler.SetActiveEvent(targetEvent, GlobalTimeOffsetSeconds);

        // Pre-check: prevent huge offsets if user pasted an event countdown into drift box
        var now = DateTimeOffset.Now;
        int currentEffectiveOffset = targetEvent.TimeOffsetSeconds + GlobalTimeOffsetSeconds;
        var currentSnap = _scheduler.CalculateSnapshot(now, targetEvent, currentEffectiveOffset);
        DateTimeOffset targetTime = currentSnap.IsActive ? currentSnap.NextEventEnd : currentSnap.NextEventStart;
        TimeSpan rawRemainingWithoutOffset = targetTime - now;
        int potentialOffset = (int)Math.Round((rawRemainingWithoutOffset - remaining).TotalSeconds);

        if (Math.Abs(potentialOffset) > 600) // > 10 minutes skew
        {
            SyncFeedbackMessage = _loc.GetString("Settings_SyncDriftTooLarge");
            ShowSyncFeedback = true;
            return;
        }

        int calculatedOffset = _scheduler.SyncToGameTime(remaining);
        if (SelectedEvent != null)
        {
            SelectedEvent.TimeOffsetSeconds = _scheduler.ActiveEvent.TimeOffsetSeconds;
            OnPropertyChanged(nameof(SelectedEventTimeOffsetSeconds));
        }

        string template = _loc.GetString("Settings_SyncSuccess");
        SyncFeedbackMessage = string.Format(template, calculatedOffset);
        ShowSyncFeedback = true;
    }

    [RelayCommand]
    private void NudgeOffset(string deltaString)
    {
        if (int.TryParse(deltaString, out int delta) && SelectedEvent != null)
        {
            SelectedEvent.TimeOffsetSeconds = delta == 0 ? 0 : SelectedEvent.TimeOffsetSeconds + delta;
            OnPropertyChanged(nameof(SelectedEventTimeOffsetSeconds));
            _scheduler.SetTimeOffset(SelectedEvent.TimeOffsetSeconds + GlobalTimeOffsetSeconds);
            string template = _loc.GetString("Settings_OffsetAdjusted");
            SyncFeedbackMessage = string.Format(template, SelectedEvent.TimeOffsetSeconds);
            ShowSyncFeedback = true;
        }
    }
}
