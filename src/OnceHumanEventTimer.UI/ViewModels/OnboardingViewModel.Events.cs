// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OnceHumanEventTimer.Core.Models;

namespace OnceHumanEventTimer.UI.ViewModels;

public partial class OnboardingViewModel
{
    [ObservableProperty]
    private ObservableCollection<OnboardingEventItemViewModel> _events = new();

    private void InitializeEvents()
    {
        _settingsService.Settings.EnsureDefaultPresets();

        // On initial onboarding, ensure only S5: The Echo is enabled by default
        if (!_settingsService.Settings.HasCompletedOnboarding)
        {
            foreach (var ev in _settingsService.Settings.Events)
            {
                ev.IsEnabled = (ev.Id == "dissonant-resonance");
            }
            _settingsService.Settings.ActiveEventId = "dissonant-resonance";
        }

        var items = _settingsService.Settings.Events
            .Select(e => new OnboardingEventItemViewModel(e, () => _ = SaveEventSelectionsAsync(ensureAtLeastOne: false)))
            .ToList();

        Events = new ObservableCollection<OnboardingEventItemViewModel>(items);
    }

    public async Task SaveEventSelectionsAsync(bool ensureAtLeastOne = false)
    {
        // Guarantee at least one event remains enabled when navigating away or finalizing
        if (ensureAtLeastOne && !Events.Any(e => e.IsEnabled))
        {
            var echoItem = Events.FirstOrDefault(e => e.Id == "dissonant-resonance") ?? Events.FirstOrDefault();
            if (echoItem != null)
            {
                echoItem.IsEnabled = true;
            }
        }

        var s = _settingsService.Settings;
        s.Events = Events.Select(e => e.Event).ToList();

        if (s.Events.Any(e => e.Id == "dissonant-resonance" && e.IsEnabled))
        {
            s.ActiveEventId = "dissonant-resonance";
        }
        else
        {
            var firstEnabled = s.Events.FirstOrDefault(e => e.IsEnabled);
            if (firstEnabled != null)
            {
                s.ActiveEventId = firstEnabled.Id;
            }
        }

        _scheduler?.SetTrackedEvents(s.Events, s.ActiveEventId, s.GlobalTimeOffsetSeconds);
        await _settingsService.SaveAsync();
    }
}

/// <summary>
/// Selectable event preset item wrapper for interactive onboarding list.
/// </summary>
public partial class OnboardingEventItemViewModel : ObservableObject
{
    private readonly Action _onToggle;

    public EventDefinition Event { get; }

    [ObservableProperty]
    private bool _isEnabled;

    public string Id => Event.Id;
    public string Name => Event.Name;
    public string Description => Event.Description;
    public string ColorHex => !string.IsNullOrWhiteSpace(Event.ColorHex) ? Event.ColorHex : "#00F0FF";

    public string TimingSummary => Event.Id switch
    {
        "dissonant-resonance" => "Cycle: 60m | Starts: XX:15:00 | Boss: XX:30:00",
        "cargo-scramble" => "Cycle: 60m | Starts: XX:00:00 | Duration: 20m",
        "drift-greenhouse" => "Cycle: 240m | World 1–12 dynamic spawn",
        _ => $"Cycle: {Event.CycleMinutes}m | Starts: XX:{Event.StartMinute:D2}:{Event.StartSecond:D2}"
    };

    public string BadgeText => Event.Id switch
    {
        "dissonant-resonance" => "#1 SEASON 5",
        "cargo-scramble" => "#2 TOP META",
        "drift-greenhouse" => "MANUAL SYNC",
        "gravity-abyss" => "LEGACY",
        _ => string.Empty
    };

    public bool HasBadge => !string.IsNullOrEmpty(BadgeText);

    public OnboardingEventItemViewModel(EventDefinition ev, Action onToggle)
    {
        Event = ev;
        _isEnabled = ev.IsEnabled;
        _onToggle = onToggle;
    }

    partial void OnIsEnabledChanged(bool value)
    {
        Event.IsEnabled = value;
        _onToggle?.Invoke();
    }
}
