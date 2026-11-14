// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnceHumanEventTimer.Core.Scheduling;

namespace OnceHumanEventTimer.UI.ViewModels;

public partial class SettingsViewModel
{
    [ObservableProperty]
    private string _gameRemainingInput = "08:23";

    [ObservableProperty]
    private string? _syncFeedbackMessage;

    [ObservableProperty]
    private bool _showSyncFeedback;

    [RelayCommand]
    private void SyncToGame()
    {
        if (string.IsNullOrWhiteSpace(GameRemainingInput))
        {
            SyncFeedbackMessage = "Please enter remaining countdown (e.g. 08:23)";
            ShowSyncFeedback = true;
            return;
        }

        if (!GameTimeParser.TryParse(GameRemainingInput, out var remaining))
        {
            SyncFeedbackMessage = "Invalid format. Use hh:mm:ss, mm:ss, or 1h 3m 29s (1 Std 3 Min 29 Sek)";
            ShowSyncFeedback = true;
            return;
        }

        var targetEvent = SelectedEvent ?? _scheduler.ActiveEvent;
        _scheduler.SetActiveEvent(targetEvent, GlobalTimeOffsetSeconds);

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
            SyncFeedbackMessage = $"Offset adjusted to {SelectedEvent.TimeOffsetSeconds}s";
            ShowSyncFeedback = true;
        }
    }
}
