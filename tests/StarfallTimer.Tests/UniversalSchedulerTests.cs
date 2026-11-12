// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using StarfallTimer.Core.Localization;
using StarfallTimer.Core.Models;
using StarfallTimer.Core.Scheduling;
using Xunit;

namespace StarfallTimer.Tests;

public class UniversalSchedulerTests
{
    private readonly StarfallEventScheduler _scheduler;

    public UniversalSchedulerTests()
    {
        var loc = new LocalizationService();
        _scheduler = new StarfallEventScheduler(loc);
    }

    [Fact]
    public void DefaultPresets_ArePopulatedInUserSettings()
    {
        var settings = new UserSettings();

        Assert.NotNull(settings.Events);
        Assert.Equal(3, settings.Events.Count);
        Assert.Equal("gravity-abyss", settings.ActiveEventId);

        var active = settings.GetActiveEvent();
        Assert.Equal("gravity-abyss", active.Id);
        Assert.Equal(60, active.CycleMinutes);
        Assert.Equal(22, active.StartMinute);
        Assert.Equal(30, active.DurationMinutes);
    }

    [Fact]
    public void CustomCycle_CalculatesWindowsCorrectly()
    {
        var customEvent = new EventDefinition
        {
            Id = "mini-cycle",
            Name = "Mini Cycle",
            CycleMinutes = 30,
            StartMinute = 5,
            DurationMinutes = 10,
            TimeOffsetSeconds = 0
        };

        // 10:02:00 -> 3 minutes before 10:05:00
        var testTime = new DateTimeOffset(2026, 11, 12, 10, 2, 0, TimeSpan.Zero);
        var snapshot = _scheduler.CalculateSnapshot(testTime, customEvent);

        Assert.Equal(EventPhase.Inactive, snapshot.Phase);
        Assert.Equal(TimeSpan.FromMinutes(3), snapshot.TimeRemaining);
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 10, 5, 0, TimeSpan.Zero), snapshot.NextEventStart);

        // 10:08:00 -> active with 7 minutes remaining until 10:15:00
        var activeTime = new DateTimeOffset(2026, 11, 12, 10, 8, 0, TimeSpan.Zero);
        var activeSnapshot = _scheduler.CalculateSnapshot(activeTime, customEvent);

        Assert.Equal(EventPhase.Active, activeSnapshot.Phase);
        Assert.Equal(TimeSpan.FromMinutes(7), activeSnapshot.TimeRemaining);
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 10, 15, 0, TimeSpan.Zero), activeSnapshot.NextEventEnd);
    }

    [Fact]
    public void ArbitraryCycle_45Minutes_CalculatesPhasesAccurately()
    {
        var eventDef = new EventDefinition
        {
            Id = "cargo-45",
            Name = "Cargo 45",
            CycleMinutes = 45,
            StartMinute = 10,
            DurationMinutes = 20,
            TimeOffsetSeconds = 0
        };

        // Anchor 00:00 -> 00:10 start, 00:30 end
        var inactiveTime = new DateTimeOffset(2026, 11, 12, 0, 2, 0, TimeSpan.Zero);
        var inactiveSnap = _scheduler.CalculateSnapshot(inactiveTime, eventDef);
        Assert.Equal(EventPhase.Inactive, inactiveSnap.Phase);
        Assert.Equal(TimeSpan.FromMinutes(8), inactiveSnap.TimeRemaining);
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 0, 10, 0, TimeSpan.Zero), inactiveSnap.NextEventStart);

        // 00:28:00 -> 2 minutes remaining of duration -> EndingSoon
        var endingSoonTime = new DateTimeOffset(2026, 11, 12, 0, 28, 0, TimeSpan.Zero);
        var endingSoonSnap = _scheduler.CalculateSnapshot(endingSoonTime, eventDef);
        Assert.Equal(EventPhase.EndingSoon, endingSoonSnap.Phase);
        Assert.Equal(TimeSpan.FromMinutes(2), endingSoonSnap.TimeRemaining);

        // 00:30:00 -> Ended, next cycle anchor is 00:45 -> start at 00:55
        var postEventTime = new DateTimeOffset(2026, 11, 12, 0, 30, 0, TimeSpan.Zero);
        var postEventSnap = _scheduler.CalculateSnapshot(postEventTime, eventDef);
        Assert.Equal(EventPhase.Inactive, postEventSnap.Phase);
        Assert.Equal(TimeSpan.FromMinutes(25), postEventSnap.TimeRemaining);
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 0, 55, 0, TimeSpan.Zero), postEventSnap.NextEventStart);
    }

    [Fact]
    public void ArbitraryCycle_120Minutes_CalculatesMultiHourCycle()
    {
        var eventDef = new EventDefinition
        {
            Id = "bi-hourly",
            Name = "Bi-Hourly Boss",
            CycleMinutes = 120,
            StartMinute = 30,
            DurationMinutes = 45,
            TimeOffsetSeconds = 0
        };

        // 02:00:00 -> Cycle anchor 02:00, starts at 02:30:00
        var testTime = new DateTimeOffset(2026, 11, 12, 2, 0, 0, TimeSpan.Zero);
        var snap = _scheduler.CalculateSnapshot(testTime, eventDef);

        Assert.Equal(EventPhase.Inactive, snap.Phase);
        Assert.Equal(TimeSpan.FromMinutes(30), snap.TimeRemaining);
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 2, 30, 0, TimeSpan.Zero), snap.NextEventStart);
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 3, 15, 0, TimeSpan.Zero), snap.NextEventEnd);
    }

    [Fact]
    public void TimeOffset_Positive_EliminatesPCClockDrift()
    {
        var eventDef = new EventDefinition
        {
            Id = "gravity-abyss",
            Name = "Gravity Abyss",
            CycleMinutes = 60,
            StartMinute = 22,
            DurationMinutes = 30,
            TimeOffsetSeconds = 31 // Game server is 31s ahead
        };

        // PC time: 14:21:00. Effective server time: 14:21:31.
        // Target: 14:22:00. Remaining: 29 seconds.
        var testTime = new DateTimeOffset(2026, 11, 12, 14, 21, 0, TimeSpan.Zero);
        var snapshot = _scheduler.CalculateSnapshot(testTime, eventDef, eventDef.TimeOffsetSeconds);

        Assert.Equal(TimeSpan.FromSeconds(29), snapshot.TimeRemaining);
        Assert.Equal("00:29", snapshot.FormattedRemaining);
        Assert.Equal(EventPhase.StartingSoon, snapshot.Phase);
    }

    [Fact]
    public void TimeOffset_Negative_AdjustsWhenPcAheadOfServer()
    {
        var eventDef = new EventDefinition
        {
            Id = "gravity-abyss",
            Name = "Gravity Abyss",
            CycleMinutes = 60,
            StartMinute = 22,
            DurationMinutes = 30,
            TimeOffsetSeconds = -45 // Game server is 45s behind PC
        };

        // PC time: 14:22:15. Effective server time: 14:21:30.
        // Target: 14:22:00. Remaining: 30 seconds.
        var testTime = new DateTimeOffset(2026, 11, 12, 14, 22, 15, TimeSpan.Zero);
        var snapshot = _scheduler.CalculateSnapshot(testTime, eventDef, eventDef.TimeOffsetSeconds);

        Assert.Equal(TimeSpan.FromSeconds(30), snapshot.TimeRemaining);
        Assert.Equal("00:30", snapshot.FormattedRemaining);
        Assert.Equal(EventPhase.StartingSoon, snapshot.Phase);
    }

    [Fact]
    public void SyncToGameTime_AdjustsOffsetToMatchTargetRemaining()
    {
        var eventDef = new EventDefinition
        {
            Id = "sync-test",
            Name = "Sync Test",
            CycleMinutes = 60,
            StartMinute = 22,
            DurationMinutes = 30,
            TimeOffsetSeconds = 0
        };

        _scheduler.SetActiveEvent(eventDef);

        bool eventFired = false;
        int firedOffset = 0;
        _scheduler.TimeOffsetChanged += (_, offset) =>
        {
            eventFired = true;
            firedOffset = offset;
        };

        // In-game countdown displays 08:23 remaining
        var desiredRemaining = new TimeSpan(0, 8, 23);
        int computedOffset = _scheduler.SyncToGameTime(desiredRemaining);

        Assert.True(eventFired);
        Assert.Equal(computedOffset, firedOffset);

        // Verify that scheduler snapshot reflects within 1 second of desired remaining
        var snap = _scheduler.CurrentSnapshot;
        Assert.True(Math.Abs((snap.TimeRemaining - desiredRemaining).TotalSeconds) <= 1.0);
    }

    [Fact]
    public void SetActiveEvent_SwitchesEventAndUpdatesSnapshotImmediately()
    {
        var custom = new EventDefinition
        {
            Id = "custom-wheel",
            Name = "Custom Wheel",
            CycleMinutes = 15,
            StartMinute = 0,
            DurationMinutes = 5,
            TimeOffsetSeconds = 10
        };

        _scheduler.SetActiveEvent(custom);

        Assert.Equal("custom-wheel", _scheduler.ActiveEvent.Id);
        Assert.Equal(10, _scheduler.TimeOffsetSeconds);
        Assert.Equal("Custom Wheel", _scheduler.CurrentSnapshot.EventName);
    }
}
