// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OnceHumanEventTimer.Core.Models;

namespace OnceHumanEventTimer.UI.ViewModels;

public partial class SettingsViewModel
{
    [ObservableProperty]
    private ObservableCollection<EventDefinition> _events = new();

    [ObservableProperty]
    private EventDefinition? _selectedEvent;

    [ObservableProperty]
    private string _activeEventId = "gravity-abyss";

    [ObservableProperty]
    private int _globalTimeOffsetSeconds;

    [ObservableProperty]
    private string _gameRemainingInput = "08:23";

    [ObservableProperty]
    private string? _syncFeedbackMessage;

    [ObservableProperty]
    private bool _showSyncFeedback;

    public bool IsSelectedEventActive => SelectedEvent != null && SelectedEvent.Id == ActiveEventId;

    public string SelectedEventName
    {
        get => SelectedEvent?.Name ?? string.Empty;
        set { if (SelectedEvent != null && SelectedEvent.Name != value) { SelectedEvent.Name = value; OnPropertyChanged(); } }
    }

    public int SelectedEventCycleMinutes
    {
        get => SelectedEvent?.CycleMinutes ?? 60;
        set { if (SelectedEvent != null && SelectedEvent.CycleMinutes != value) { SelectedEvent.CycleMinutes = Math.Max(1, value); OnPropertyChanged(); } }
    }

    public int SelectedEventStartMinute
    {
        get => SelectedEvent?.StartMinute ?? 0;
        set { if (SelectedEvent != null && SelectedEvent.StartMinute != value) { SelectedEvent.StartMinute = Math.Max(0, value); OnPropertyChanged(); } }
    }

    public int SelectedEventDurationMinutes
    {
        get => SelectedEvent?.DurationMinutes ?? 15;
        set { if (SelectedEvent != null && SelectedEvent.DurationMinutes != value) { SelectedEvent.DurationMinutes = Math.Max(1, value); OnPropertyChanged(); } }
    }

    public int SelectedEventTimeOffsetSeconds
    {
        get => SelectedEvent?.TimeOffsetSeconds ?? 0;
        set { if (SelectedEvent != null && SelectedEvent.TimeOffsetSeconds != value) { SelectedEvent.TimeOffsetSeconds = value; OnPropertyChanged(); } }
    }

    public string SelectedEventColorHex
    {
        get => SelectedEvent?.ColorHex ?? "#00E5FF";
        set { if (SelectedEvent != null && SelectedEvent.ColorHex != value) { SelectedEvent.ColorHex = value; OnPropertyChanged(); } }
    }

    public string SelectedEventDescription
    {
        get => SelectedEvent?.Description ?? string.Empty;
        set { if (SelectedEvent != null && SelectedEvent.Description != value) { SelectedEvent.Description = value; OnPropertyChanged(); } }
    }

    public bool SelectedEventIsEnabled
    {
        get => SelectedEvent?.IsEnabled ?? false;
        set { if (SelectedEvent != null && SelectedEvent.IsEnabled != value) { SelectedEvent.IsEnabled = value; OnPropertyChanged(); } }
    }

    public string? SelectedEventCustomSoundPath
    {
        get => SelectedEvent?.CustomSoundPath;
        set { if (SelectedEvent != null && SelectedEvent.CustomSoundPath != value) { SelectedEvent.CustomSoundPath = value; OnPropertyChanged(); } }
    }

    private void InitializeEvents(UserSettings current)
    {
        GlobalTimeOffsetSeconds = current.GlobalTimeOffsetSeconds;
        ActiveEventId = current.ActiveEventId;
        Events = new ObservableCollection<EventDefinition>(current.Events.Select(e => e.Clone()));
        SelectedEvent = Events.FirstOrDefault(e => e.Id == ActiveEventId) ?? Events.FirstOrDefault();
    }

    partial void OnSelectedEventChanged(EventDefinition? value)
    {
        OnPropertyChanged(nameof(SelectedEventName));
        OnPropertyChanged(nameof(SelectedEventCycleMinutes));
        OnPropertyChanged(nameof(SelectedEventStartMinute));
        OnPropertyChanged(nameof(SelectedEventDurationMinutes));
        OnPropertyChanged(nameof(SelectedEventTimeOffsetSeconds));
        OnPropertyChanged(nameof(SelectedEventColorHex));
        OnPropertyChanged(nameof(SelectedEventDescription));
        OnPropertyChanged(nameof(SelectedEventIsEnabled));
        OnPropertyChanged(nameof(SelectedEventCustomSoundPath));
        OnPropertyChanged(nameof(IsSelectedEventActive));
    }

    [RelayCommand]
    private void SelectEventColor(string hex)
    {
        if (!string.IsNullOrWhiteSpace(hex)) SelectedEventColorHex = hex.Trim();
    }

    [RelayCommand]
    private void BrowseEventSound()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "WAV Audio Files (*.wav)|*.wav|All Files (*.*)|*.*",
            Title = "Select Event Alert Sound"
        };
        if (dlg.ShowDialog() == true && SelectedEvent != null)
        {
            SelectedEventCustomSoundPath = dlg.FileName;
        }
    }

    [RelayCommand]
    private void TestEventAudio()
    {
        _audio.PlayWarning(Volume, SelectedEventCustomSoundPath ?? CustomSoundPath);
    }

    [RelayCommand]
    private void ClearEventSound()
    {
        SelectedEventCustomSoundPath = null;
    }

    partial void OnActiveEventIdChanged(string value) => OnPropertyChanged(nameof(IsSelectedEventActive));

    [RelayCommand]
    private void SetActiveEvent()
    {
        if (SelectedEvent != null)
        {
            ActiveEventId = SelectedEvent.Id;
            OnPropertyChanged(nameof(IsSelectedEventActive));
        }
    }

    [RelayCommand]
    private void AddEvent()
    {
        var newEvent = new EventDefinition
        {
            Id = $"custom-{Guid.NewGuid():N}"[..15],
            Name = "New Custom Event",
            CycleMinutes = 60,
            StartMinute = 0,
            DurationMinutes = 15,
            TimeOffsetSeconds = 0,
            ColorHex = "#00E5FF",
            Description = "Custom scheduled Once Human in-game event."
        };
        Events.Add(newEvent);
        SelectedEvent = newEvent;
    }

    [RelayCommand]
    private void DeleteEvent()
    {
        if (SelectedEvent == null || Events.Count <= 1) return;
        var toRemove = SelectedEvent;
        int index = Events.IndexOf(toRemove);
        Events.Remove(toRemove);
        if (ActiveEventId == toRemove.Id)
        {
            ActiveEventId = Events[0].Id;
        }
        SelectedEvent = Events[Math.Min(index, Events.Count - 1)];
    }

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
        else if (TimeSpan.TryParse(raw, out var parsed))
        {
            remaining = parsed;
        }
        else
        {
            SyncFeedbackMessage = "Invalid format. Use mm:ss (e.g. 08:23)";
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
