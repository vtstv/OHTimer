// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

        string raw = GameRemainingInput.Trim();
        TimeSpan remaining;
        var parts = raw.Split(':');
        if (parts.Length == 2 && int.TryParse(parts[0], out int mins) && int.TryParse(parts[1], out int secs))
        {
            remaining = new TimeSpan(0, mins, secs);
        }
        else if (parts.Length == 3 && int.TryParse(parts[0], out int hrs) && int.TryParse(parts[1], out int m) && int.TryParse(parts[2], out int s))
        {
            remaining = new TimeSpan(hrs, m, s);
        }
        else if (TryParseGameTimeString(raw, out var parsedGameTime))
        {
            remaining = parsedGameTime;
        }
        else if (TimeSpan.TryParse(raw, out var parsed))
        {
            remaining = parsed;
        }
        else
        {
            SyncFeedbackMessage = "Invalid format. Use mm:ss (e.g. 08:23) or 2h 29m 4s";
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

    private static bool TryParseGameTimeString(string input, out TimeSpan result)
    {
        result = TimeSpan.Zero;
        var match = System.Text.RegularExpressions.Regex.Match(
            input,
            @"^(?:(?<h>\d+)\s*h(?:ours?|ч)?)?\s*(?:(?<m>\d+)\s*m(?:in(?:utes?)?|м(?:ин)?)?)?\s*(?:(?<s>\d+)\s*s(?:ec(?:onds?)?|с(?:ек)?)?)?$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (!match.Success || (!match.Groups["h"].Success && !match.Groups["m"].Success && !match.Groups["s"].Success))
        {
            return false;
        }

        int hours = match.Groups["h"].Success ? int.Parse(match.Groups["h"].Value) : 0;
        int minutes = match.Groups["m"].Success ? int.Parse(match.Groups["m"].Value) : 0;
        int seconds = match.Groups["s"].Success ? int.Parse(match.Groups["s"].Value) : 0;

        result = new TimeSpan(hours, minutes, seconds);
        return result > TimeSpan.Zero;
    }
}
