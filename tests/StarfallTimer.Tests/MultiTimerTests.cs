// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using StarfallTimer.Core.Localization;
using StarfallTimer.Core.Models;
using StarfallTimer.Core.Scheduling;
using Xunit;

namespace StarfallTimer.Tests;

/// <summary>
/// Unit tests for multi-timer concurrent scheduling, Visional Wheel presets, and -31s default drift offset.
/// </summary>
public class MultiTimerTests
{
    private readonly LocalizationService _loc = new();

    [Fact]
    public void GravityAbyss_DefaultTimeOffset_IsMinus31Seconds()
    {
        var scheduler = new EventScheduler(_loc);

        Assert.Equal("gravity-abyss", scheduler.ActiveEvent.Id);
        Assert.Equal(-31, scheduler.ActiveEvent.TimeOffsetSeconds);
        Assert.Equal(-31, scheduler.TimeOffsetSeconds);
        Assert.Equal("#00F0FF", scheduler.ActiveEvent.ColorHex);
        Assert.True(scheduler.ActiveEvent.IsEnabled);
    }

    [Fact]
    public void AllEightVisionalWheelPresets_AreConfiguredProperly()
    {
        var presets = DefaultEventPresets.CreateDefaultEvents();
        Assert.Equal(8, presets.Count);

        var abyss = presets.Single(e => e.Id == "gravity-abyss");
        Assert.Equal("Gravity Abyss (Starfall Inversion)", abyss.Name);
        Assert.Equal(60, abyss.CycleMinutes);
        Assert.Equal(22, abyss.StartMinute);
        Assert.Equal(30, abyss.DurationMinutes);
        Assert.Equal(-31, abyss.TimeOffsetSeconds);
        Assert.Equal("#00F0FF", abyss.ColorHex);
        Assert.True(abyss.IsEnabled);

        var cargo = presets.Single(e => e.Id == "cargo-scramble");
        Assert.Equal("Cargo Scramble (Sproutlet Truck)", cargo.Name);
        Assert.Equal(60, cargo.CycleMinutes);
        Assert.Equal(0, cargo.StartMinute);
        Assert.Equal(20, cargo.DurationMinutes);
        Assert.Equal(0, cargo.TimeOffsetSeconds);
        Assert.Equal("#F59E0B", cargo.ColorHex);
        Assert.False(cargo.IsEnabled);

        var lunarOracle = presets.Single(e => e.Id == "lunar-oracle");
        Assert.Equal("Lunar Oracle (Blood Moon)", lunarOracle.Name);
        Assert.Equal(60, lunarOracle.CycleMinutes);
        Assert.Equal(45, lunarOracle.StartMinute);
        Assert.Equal(15, lunarOracle.DurationMinutes);
        Assert.Equal("#EF4444", lunarOracle.ColorHex);

        var lunarRevelry = presets.Single(e => e.Id == "lunar-revelry");
        Assert.Equal("Lunar Revelry (Lord of Moonlight)", lunarRevelry.Name);
        Assert.Equal(60, lunarRevelry.CycleMinutes);
        Assert.Equal(40, lunarRevelry.StartMinute);
        Assert.Equal(20, lunarRevelry.DurationMinutes);
        Assert.Equal("#A855F7", lunarRevelry.ColorHex);

        var spore = presets.Single(e => e.Id == "aberrant-progeny");
        Assert.Equal("Aberrant Progeny (Spore Tide)", spore.Name);
        Assert.Equal(60, spore.CycleMinutes);
        Assert.Equal(10, spore.StartMinute);
        Assert.Equal(25, spore.DurationMinutes);
        Assert.Equal("#10B981", spore.ColorHex);

        var resonance = presets.Single(e => e.Id == "dissonant-resonance");
        Assert.Equal("Dissonant Resonance", resonance.Name);
        Assert.Equal(60, resonance.CycleMinutes);
        Assert.Equal(15, resonance.StartMinute);
        Assert.Equal(30, resonance.DurationMinutes);
        Assert.Equal("#EC4899", resonance.ColorHex);

        var crateReset = presets.Single(e => e.Id == "crate-loot-reset");
        Assert.Equal("Crate Loot Reset (4-Hour)", crateReset.Name);
        Assert.Equal(240, crateReset.CycleMinutes);
        Assert.Equal(0, crateReset.StartMinute);
        Assert.Equal(5, crateReset.DurationMinutes);
        Assert.Equal("#3B82F6", crateReset.ColorHex);

        var custom = presets.Single(e => e.Id == "custom-wheel");
        Assert.Equal("Custom Wheel Event", custom.Name);
        Assert.Equal(60, custom.CycleMinutes);
        Assert.Equal(0, custom.StartMinute);
        Assert.Equal(15, custom.DurationMinutes);
        Assert.Equal("#EAB308", custom.ColorHex);
    }

    [Fact]
    public void MultiTimer_ActiveSnapshots_TracksMultipleEnabledEventsConcurrently()
    {
        var scheduler = new EventScheduler(_loc);
        var presets = DefaultEventPresets.CreateDefaultEvents();

        // Enable Gravity Abyss and Cargo Scramble
        presets[0].IsEnabled = true;
        presets[1].IsEnabled = true;

        scheduler.SetTrackedEvents(presets, activeEventId: "gravity-abyss");

        Assert.Equal(2, scheduler.ActiveSnapshots.Count);
        Assert.Equal("gravity-abyss", scheduler.ActiveSnapshots[0].Event?.Id);
        Assert.Equal("cargo-scramble", scheduler.ActiveSnapshots[1].Event?.Id);

        // At 10:05:00 UTC:
        // Cargo Scramble (starts at 10:00, duration 20) -> Active with 15m remaining
        // Gravity Abyss (starts at 10:22, duration 30, offset -31s) -> Inactive
        var testTime = new DateTimeOffset(2026, 11, 12, 10, 5, 0, TimeSpan.Zero);
        var snapshots = scheduler.CalculateActiveSnapshots(testTime);

        Assert.Equal(2, snapshots.Count);
        var cargoSnap = snapshots.Single(s => s.Event?.Id == "cargo-scramble");
        Assert.Equal(EventPhase.Active, cargoSnap.Phase);
        Assert.Equal(TimeSpan.FromMinutes(15), cargoSnap.TimeRemaining);

        var abyssSnap = snapshots.Single(s => s.Event?.Id == "gravity-abyss");
        Assert.Equal(EventPhase.Inactive, abyssSnap.Phase);
    }

    [Fact]
    public void MultiTimer_DisabledEvents_AreExcludedFromActiveSnapshots()
    {
        var scheduler = new EventScheduler(_loc);
        var presets = DefaultEventPresets.CreateDefaultEvents();

        foreach (var p in presets) p.IsEnabled = false;
        presets[2].IsEnabled = true; // only Lunar Oracle enabled

        scheduler.SetTrackedEvents(presets);

        Assert.Single(scheduler.ActiveSnapshots);
        Assert.Equal("lunar-oracle", scheduler.ActiveSnapshots[0].Event?.Id);
    }

    [Fact]
    public void MultiTimer_NotificationEvaluator_FiresAlertsForMultipleEventsIndependently()
    {
        var evaluator = new NotificationEvaluator(_loc);

        var ev1 = new EventDefinition { Id = "ev1", Name = "Event One", StartMinute = 22, DurationMinutes = 30 };
        var ev2 = new EventDefinition { Id = "ev2", Name = "Event Two", StartMinute = 45, DurationMinutes = 15 };

        var snap1 = EventTimeCalculator.CalculateSnapshot(
            new DateTimeOffset(2026, 11, 12, 10, 20, 30, TimeSpan.Zero), ev1); // Starting soon
        var snap2 = EventTimeCalculator.CalculateSnapshot(
            new DateTimeOffset(2026, 11, 12, 10, 20, 30, TimeSpan.Zero), ev2); // Inactive

        var alert1 = evaluator.Evaluate(snap1);
        var alert2 = evaluator.Evaluate(snap2);

        Assert.NotNull(alert1);
        Assert.Null(alert2);
        Assert.Contains("Event One", alert1.Title);
        Assert.Equal(EventPhase.StartingSoon, alert1.Phase);

        // Subsequent evaluation of same cycle does not produce duplicate
        var duplicate = evaluator.Evaluate(snap1);
        Assert.Null(duplicate);
    }
}
