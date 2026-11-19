// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Localization;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Scheduling;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class ExtraAlertAndDuplicateTests
{
    private readonly LocalizationService _loc = new();

    [Fact]
    public void ExtraAlert_TriggersAtConfiguredElapsedMinutes_AndSuppressesStartAlert()
    {
        var evaluator = new NotificationEvaluator(_loc);

        var eventDef = new EventDefinition
        {
            Id = "dissonant-resonance",
            Name = "Visional Wheel: The Echo",
            CycleMinutes = 60,
            StartMinute = 0,
            StartSecond = 0,
            DurationMinutes = 30,
            AlertOnEventStart = false,
            EnableExtraAlert = true,
            ExtraAlertMinutes = 15
        };

        var baseTime = new DateTimeOffset(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);

        // 1. At T+0 (Event start): Active phase, but AlertOnEventStart is false -> returns null
        var snapStart = EventTimeCalculator.CalculateSnapshot(baseTime, eventDef);
        Assert.Equal(EventPhase.Active, snapStart.Phase);
        var alertStart = evaluator.Evaluate(snapStart);
        Assert.Null(alertStart);

        // 2. At T+10m: Elapsed is 10 min < 15 min -> returns null
        var snap10m = EventTimeCalculator.CalculateSnapshot(baseTime.AddMinutes(10), eventDef);
        Assert.Equal(EventPhase.Active, snap10m.Phase);
        var alert10m = evaluator.Evaluate(snap10m);
        Assert.Null(alert10m);

        // 3. At T+15m: Elapsed is 15 min -> Triggers Extra Alert (Boss Portal Open!)
        var snap15m = EventTimeCalculator.CalculateSnapshot(baseTime.AddMinutes(15), eventDef);
        Assert.Equal(EventPhase.Active, snap15m.Phase);
        var alert15m = evaluator.Evaluate(snap15m);
        Assert.NotNull(alert15m);
        Assert.True(alert15m.IsExtraAlert);
        Assert.Contains("Boss Portal", alert15m.Title);
        Assert.True(alert15m.PlaySound);
        Assert.True(alert15m.ShowToast);

        // 4. At T+16m: Already fired for this cycle -> returns null (deduplicated)
        var snap16m = EventTimeCalculator.CalculateSnapshot(baseTime.AddMinutes(16), eventDef);
        var alert16m = evaluator.Evaluate(snap16m);
        Assert.Null(alert16m);
    }

    [Fact]
    public void NormalEvent_WithAlertOnStart_TriggersNormalStartAlert()
    {
        var evaluator = new NotificationEvaluator(_loc);

        var eventDef = new EventDefinition
        {
            Id = "gravity-abyss",
            Name = "Gravity Abyss",
            CycleMinutes = 60,
            StartMinute = 0,
            DurationMinutes = 30,
            AlertOnEventStart = true,
            EnableExtraAlert = false
        };

        var baseTime = new DateTimeOffset(2026, 11, 19, 14, 0, 0, TimeSpan.Zero);
        var snap = EventTimeCalculator.CalculateSnapshot(baseTime, eventDef);
        var alert = evaluator.Evaluate(snap);

        Assert.NotNull(alert);
        Assert.False(alert.IsExtraAlert);
        Assert.Equal(EventPhase.Active, alert.Phase);
        Assert.Contains("Event Active", alert.Title);
    }

    [Fact]
    public void EventDefinition_Clone_PreservesAlertProperties()
    {
        var original = new EventDefinition
        {
            Id = "echo-test",
            Name = "The Echo",
            CycleMinutes = 60,
            StartMinute = 10,
            DurationMinutes = 30,
            AlertOnEventStart = false,
            EnableExtraAlert = true,
            ExtraAlertMinutes = 15,
            ColorHex = "#D946EF"
        };

        var clone = original.Clone();

        Assert.Equal(original.Id, clone.Id);
        Assert.Equal(original.Name, clone.Name);
        Assert.False(clone.AlertOnEventStart);
        Assert.True(clone.EnableExtraAlert);
        Assert.Equal(15, clone.ExtraAlertMinutes);
        Assert.Equal("#D946EF", clone.ColorHex);
    }

    [Fact]
    public void CountdownToBoss_CalculatesCountdownDirectlyToBossPortal()
    {
        var eventDef = new EventDefinition
        {
            Id = "dissonant-resonance",
            Name = "Visional Wheel: The Echo",
            CycleMinutes = 60,
            StartMinute = 15,
            DurationMinutes = 30,
            EnableExtraAlert = true,
            ExtraAlertMinutes = 15,
            CountdownToBoss = true
        };

        var baseTime = new DateTimeOffset(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);

        // At 12:00: Event starts at 12:15, Boss portal opens at 12:30
        // Countdown to boss should be exactly 30 minutes!
        var snap1200 = EventTimeCalculator.CalculateSnapshot(baseTime, eventDef);
        Assert.True(snap1200.IsBossCountdown);
        Assert.False(snap1200.IsBossOpen);
        Assert.Equal(EventPhase.Inactive, snap1200.Phase);
        Assert.Equal(TimeSpan.FromMinutes(30), snap1200.TimeRemaining);

        // At 12:15: Event starts in-game, but Boss is in 15 minutes
        var snap1215 = EventTimeCalculator.CalculateSnapshot(baseTime.AddMinutes(15), eventDef);
        Assert.True(snap1215.IsBossCountdown);
        Assert.False(snap1215.IsBossOpen);
        Assert.Equal(EventPhase.Inactive, snap1215.Phase);
        Assert.Equal(TimeSpan.FromMinutes(15), snap1215.TimeRemaining);

        // At 12:30: Boss portal opens!
        var snap1230 = EventTimeCalculator.CalculateSnapshot(baseTime.AddMinutes(30), eventDef);
        Assert.True(snap1230.IsBossCountdown);
        Assert.True(snap1230.IsBossOpen);
        Assert.Equal(EventPhase.Active, snap1230.Phase);
        Assert.Equal(TimeSpan.FromMinutes(15), snap1230.TimeRemaining);
    }

    [Fact]
    public void EnsureDefaultPresets_MigratesDisabledActiveEventToEnabledS5()
    {
        var settings = new UserSettings
        {
            ActiveEventId = "gravity-abyss",
            Events = new List<EventDefinition>
            {
                new() { Id = "dissonant-resonance", Name = "Dissonant Resonance", IsEnabled = true },
                new() { Id = "gravity-abyss", Name = "Gravity Abyss", IsEnabled = false }
            }
        };

        settings.EnsureDefaultPresets();

        Assert.Equal("dissonant-resonance", settings.ActiveEventId);
        var active = settings.GetActiveEvent();
        Assert.Equal("dissonant-resonance", active.Id);
        Assert.Equal("S5: The Echo", active.Name);
        Assert.True(active.IsEnabled);
    }

    [Fact]
    public void EventScheduler_SetTrackedEvents_PicksEnabledEventOverDisabledActiveEvent()
    {
        var loc = new LocalizationService();
        var scheduler = new EventScheduler(loc);

        var events = new List<EventDefinition>
        {
            new() { Id = "dissonant-resonance", Name = "S5: The Echo", IsEnabled = true, StartMinute = 15 },
            new() { Id = "gravity-abyss", Name = "Gravity Abyss", IsEnabled = false, StartMinute = 22 }
        };

        scheduler.SetTrackedEvents(events, activeEventId: "gravity-abyss");

        Assert.Equal("dissonant-resonance", scheduler.ActiveEvent.Id);
        Assert.True(scheduler.ActiveEvent.IsEnabled);
    }

    [Fact]
    public void UserSettings_OnboardingFlag_DefaultsToFalseAndPersists()
    {
        var settings = new UserSettings();
        Assert.False(settings.HasCompletedOnboarding);

        settings.HasCompletedOnboarding = true;
        string json = System.Text.Json.JsonSerializer.Serialize(settings);
        var restored = System.Text.Json.JsonSerializer.Deserialize<UserSettings>(json);
        Assert.NotNull(restored);
        Assert.True(restored.HasCompletedOnboarding);
    }

    [Fact]
    public void EnsureDefaultPresets_DisablesGravityAbyssAndMigratesToMinute16()
    {
        var settings = new UserSettings
        {
            Events = new List<EventDefinition>
            {
                new() { Id = "dissonant-resonance", Name = "Dissonant Resonance", StartMinute = 22, IsEnabled = true },
                new() { Id = "gravity-abyss", Name = "Gravity Abyss", IsEnabled = true }
            }
        };

        settings.EnsureDefaultPresets();

        var resonance = settings.Events.Single(e => e.Id == "dissonant-resonance");
        Assert.Equal(16, resonance.StartMinute);
        Assert.True(resonance.IsEnabled);

        var abyss = settings.Events.Single(e => e.Id == "gravity-abyss");
        Assert.False(abyss.IsEnabled);
    }
}
