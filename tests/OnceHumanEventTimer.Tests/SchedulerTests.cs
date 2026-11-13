// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Localization;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Scheduling;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class SchedulerTests
{
    private readonly EventScheduler _scheduler;

    public SchedulerTests()
    {
        var loc = new LocalizationService();
        _scheduler = new EventScheduler(loc);
        _scheduler.SetTimeOffset(0);
        _scheduler.ConfigureSchedule(
            startMinute: 22,
            durationMinutes: 30,
            alertBeforeStart: 2,
            alertBeforeEnd: 3);
    }

    [Fact]
    public void CalculateSnapshot_BeforeEventWindow_ReturnsInactive()
    {
        // 14:10:00 UTC -> Event starts at 14:22:00
        var testTime = new DateTimeOffset(2026, 11, 12, 14, 10, 0, TimeSpan.Zero);
        var snapshot = _scheduler.CalculateSnapshot(testTime);

        Assert.Equal(EventPhase.Inactive, snapshot.Phase);
        Assert.False(snapshot.IsActive);
        Assert.Equal(TimeSpan.FromMinutes(12), snapshot.TimeRemaining);
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 14, 22, 0, TimeSpan.Zero), snapshot.NextEventStart);
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 14, 52, 0, TimeSpan.Zero), snapshot.NextEventEnd);
    }

    [Fact]
    public void CalculateSnapshot_WithinTwoMinutesBeforeEvent_ReturnsStartingSoon()
    {
        // 14:20:30 UTC -> 1 minute 30 seconds before 14:22:00
        var testTime = new DateTimeOffset(2026, 11, 12, 14, 20, 30, TimeSpan.Zero);
        var snapshot = _scheduler.CalculateSnapshot(testTime);

        Assert.Equal(EventPhase.StartingSoon, snapshot.Phase);
        Assert.False(snapshot.IsActive);
        Assert.Equal(TimeSpan.FromSeconds(90), snapshot.TimeRemaining);
        Assert.Equal("01:30", snapshot.FormattedRemaining);
    }

    [Fact]
    public void CalculateSnapshot_ExactEventStart_ReturnsActive()
    {
        // 14:22:00 UTC -> Event starts right now
        var testTime = new DateTimeOffset(2026, 11, 12, 14, 22, 0, TimeSpan.Zero);
        var snapshot = _scheduler.CalculateSnapshot(testTime);

        Assert.Equal(EventPhase.Active, snapshot.Phase);
        Assert.True(snapshot.IsActive);
        Assert.Equal(TimeSpan.FromMinutes(30), snapshot.TimeRemaining);
        Assert.Equal("30:00", snapshot.FormattedRemaining);
    }

    [Fact]
    public void CalculateSnapshot_MidwayThroughEvent_ReturnsActiveWithRemainingTime()
    {
        // 14:35:00 UTC -> 17 minutes remaining until 14:52:00
        var testTime = new DateTimeOffset(2026, 11, 12, 14, 35, 0, TimeSpan.Zero);
        var snapshot = _scheduler.CalculateSnapshot(testTime);

        Assert.Equal(EventPhase.Active, snapshot.Phase);
        Assert.True(snapshot.IsActive);
        Assert.Equal(TimeSpan.FromMinutes(17), snapshot.TimeRemaining);
        Assert.Equal("17:00", snapshot.FormattedRemaining);
    }

    [Fact]
    public void CalculateSnapshot_WithinEndWarningThreshold_ReturnsEndingSoon()
    {
        // 14:50:00 UTC -> 2 minutes remaining until 14:52:00 (threshold is 3 min)
        var testTime = new DateTimeOffset(2026, 11, 12, 14, 50, 0, TimeSpan.Zero);
        var snapshot = _scheduler.CalculateSnapshot(testTime);

        Assert.Equal(EventPhase.EndingSoon, snapshot.Phase);
        Assert.True(snapshot.IsActive);
        Assert.Equal(TimeSpan.FromMinutes(2), snapshot.TimeRemaining);
    }

    [Fact]
    public void CalculateSnapshot_ExactEventEnd_TransitionsToInactiveForNextHour()
    {
        // 14:52:00 UTC -> Event ended, next event starts at 15:22:00 (30 minutes remaining)
        var testTime = new DateTimeOffset(2026, 11, 12, 14, 52, 0, TimeSpan.Zero);
        var snapshot = _scheduler.CalculateSnapshot(testTime);

        Assert.Equal(EventPhase.Inactive, snapshot.Phase);
        Assert.False(snapshot.IsActive);
        Assert.Equal(TimeSpan.FromMinutes(30), snapshot.TimeRemaining);
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 15, 22, 0, TimeSpan.Zero), snapshot.NextEventStart);
    }

    [Fact]
    public void CalculateSnapshot_HourRolloverNearMidnight_RollsOverDayCorrectly()
    {
        // 23:55:00 UTC -> Next event is tomorrow at 00:22:00 (27 minutes remaining)
        var testTime = new DateTimeOffset(2026, 11, 12, 23, 55, 0, TimeSpan.Zero);
        var snapshot = _scheduler.CalculateSnapshot(testTime);

        Assert.Equal(EventPhase.Inactive, snapshot.Phase);
        Assert.Equal(TimeSpan.FromMinutes(27), snapshot.TimeRemaining);
        Assert.Equal(new DateTimeOffset(2026, 11, 13, 0, 22, 0, TimeSpan.Zero), snapshot.NextEventStart);
    }
}
