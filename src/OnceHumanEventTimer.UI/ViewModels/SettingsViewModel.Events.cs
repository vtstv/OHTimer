// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Infrastructure.Win32;

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
    public int SelectedEventStartHour
    {
        get => SelectedEvent?.StartHour ?? 0;
        set { if (SelectedEvent != null && SelectedEvent.StartHour != value) { SelectedEvent.StartHour = Math.Max(0, value); OnPropertyChanged(); } }
    }
    public int SelectedEventStartMinute
    {
        get => SelectedEvent?.StartMinute ?? 0;
        set { if (SelectedEvent != null && SelectedEvent.StartMinute != value) { SelectedEvent.StartMinute = Math.Max(0, value); OnPropertyChanged(); } }
    }
    public int SelectedEventStartSecond
    {
        get => SelectedEvent?.StartSecond ?? 0;
        set { if (SelectedEvent != null && SelectedEvent.StartSecond != value) { SelectedEvent.StartSecond = Math.Clamp(value, 0, 59); OnPropertyChanged(); } }
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

    public bool SelectedEventShowOnlyWhenUpcoming
    {
        get => SelectedEvent?.ShowOnlyWhenUpcoming ?? false;
        set { if (SelectedEvent != null && SelectedEvent.ShowOnlyWhenUpcoming != value) { SelectedEvent.ShowOnlyWhenUpcoming = value; OnPropertyChanged(); } }
    }

    public int SelectedEventUpcomingThresholdMinutes
    {
        get => SelectedEvent?.UpcomingThresholdMinutes ?? 10;
        set { if (SelectedEvent != null && SelectedEvent.UpcomingThresholdMinutes != value) { SelectedEvent.UpcomingThresholdMinutes = Math.Max(1, value); OnPropertyChanged(); } }
    }

    public SoundPreset SelectedEventSoundPreset
    {
        get => SelectedEvent?.SoundPreset ?? SoundPreset.CosmicPulse;
        set
        {
            if (SelectedEvent != null && SelectedEvent.SoundPreset != value)
            {
                SelectedEvent.SoundPreset = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSelectedEventCustomSound));
            }
        }
    }

    public bool IsSelectedEventCustomSound => SelectedEventSoundPreset == SoundPreset.Custom;

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
        OnPropertyChanged(nameof(SelectedEventStartHour));
        OnPropertyChanged(nameof(SelectedEventStartMinute));
        OnPropertyChanged(nameof(SelectedEventStartSecond));
        OnPropertyChanged(nameof(SelectedEventDurationMinutes));
        OnPropertyChanged(nameof(SelectedEventTimeOffsetSeconds));
        OnPropertyChanged(nameof(SelectedEventColorHex));
        OnPropertyChanged(nameof(SelectedEventDescription));
        OnPropertyChanged(nameof(SelectedEventIsEnabled));
        OnPropertyChanged(nameof(SelectedEventShowOnlyWhenUpcoming));
        OnPropertyChanged(nameof(SelectedEventUpcomingThresholdMinutes));
        OnPropertyChanged(nameof(SelectedEventSoundPreset));
        OnPropertyChanged(nameof(IsSelectedEventCustomSound));
        OnPropertyChanged(nameof(SelectedEventCustomSoundPath));
        OnPropertyChanged(nameof(IsSelectedEventActive));
        NotifyReorderState();
    }

    [RelayCommand]
    private void PickEventColor()
    {
        var activeWindow = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                           ?? Application.Current?.MainWindow;
        nint hwnd = activeWindow != null ? new WindowInteropHelper(activeWindow).Handle : 0;
        string? newColor = NativeColorDialog.ShowDialog(hwnd, SelectedEventColorHex);
        if (!string.IsNullOrWhiteSpace(newColor))
        {
            SelectedEventColorHex = newColor;
        }
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
}
