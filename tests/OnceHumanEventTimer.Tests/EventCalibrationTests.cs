// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Scheduling;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class EventCalibrationTests
{
    [Fact]
    public void CalculateCycleStartTime_DriftGreenhouseUserCase_SetsAccurateStart()
    {
        // Reference local time: 14:31:31 CET (UTC+1)
        var now = new DateTimeOffset(2026, 11, 14, 14, 31, 31, TimeSpan.FromHours(1));
        // User's in-game countdown: "Opens at 1 h 3 m 29 s"
        var remaining = new TimeSpan(1, 3, 29);
        var targetStart = now.Add(remaining); // 15:35:00

        int cycleMinutes = 240; // 4 hours
        var (h, m, s) = EventTimeCalculator.CalculateCycleStartTime(targetStart, cycleMinutes);

        // Target 15:35:00 in a 4-hour cycle anchored at 00:00:
        // Anchors are 00:00, 04:00, 08:00, 12:00, 16:00, 20:00.
        // Cycle anchor for 15:35:00 is 12:00:00.
        // Offset from 12:00:00 is 3 hours, 35 minutes, 0 seconds.
        Assert.Equal(3, h);
        Assert.Equal(35, m);
        Assert.Equal(0, s);

        // Verify with CalculateSnapshot
        var ev = new EventDefinition
        {
            Id = "drift-greenhouse",
            Name = "Drift Greenhouse",
            CycleMinutes = 240,
            StartHour = h,
            StartMinute = m,
            StartSecond = s,
            DurationMinutes = 30,
            TimeOffsetSeconds = 0
        };

        var snapshot = EventTimeCalculator.CalculateSnapshot(now, ev);
        Assert.Equal(EventPhase.Inactive, snapshot.Phase);
        Assert.Equal(targetStart, snapshot.NextEventStart);
        Assert.Equal(remaining, snapshot.TimeRemaining);
    }

    [Fact]
    public void CalculateCycleStartTime_ThreeHoursFiftyFiveMinutes_SetsAccurateStart()
    {
        // Reference local time: 15:00:00 CET
        var now = new DateTimeOffset(2026, 11, 14, 15, 0, 0, TimeSpan.FromHours(1));
        var remaining = new TimeSpan(3, 55, 0);
        var targetStart = now.Add(remaining); // 18:55:00

        int cycleMinutes = 240; // 4-hour cycle (anchors: 16:00, 20:00)
        var (h, m, s) = EventTimeCalculator.CalculateCycleStartTime(targetStart, cycleMinutes);

        // 18:55:00 is 2h 55m past 16:00:00
        Assert.Equal(2, h);
        Assert.Equal(55, m);
        Assert.Equal(0, s);

        // In Once Human, world bosses like Drift Greenhouse are defeated within 3-5 minutes,
        // so duration after kill is <= 5m and the event is inactive waiting for next start.
        var ev = new EventDefinition
        {
            Id = "drift-greenhouse",
            Name = "Drift Greenhouse",
            CycleMinutes = 240,
            StartHour = h,
            StartMinute = m,
            StartSecond = s,
            DurationMinutes = 4,
            TimeOffsetSeconds = 0
        };

        var snapshot = EventTimeCalculator.CalculateSnapshot(now, ev);
        Assert.Equal(EventPhase.Inactive, snapshot.Phase);
        Assert.Equal(targetStart, snapshot.NextEventStart);
        Assert.Equal(remaining, snapshot.TimeRemaining);
    }

    [Fact]
    public void CalculateCycleStartTime_OneHourCycle_CalibratesMinuteAndSecond()
    {
        var now = new DateTimeOffset(2026, 11, 14, 12, 10, 0, TimeSpan.FromHours(1));
        var remaining = new TimeSpan(0, 12, 29); // 12m 29s
        var targetStart = now.Add(remaining); // 12:22:29

        int cycleMinutes = 60; // 1-hour cycle
        var (h, m, s) = EventTimeCalculator.CalculateCycleStartTime(targetStart, cycleMinutes);

        Assert.Equal(0, h);
        Assert.Equal(22, m);
        Assert.Equal(29, s);

        var ev = new EventDefinition
        {
            Id = "gravity-abyss",
            Name = "Gravity Abyss",
            CycleMinutes = 60,
            StartHour = h,
            StartMinute = m,
            StartSecond = s,
            DurationMinutes = 30,
            TimeOffsetSeconds = 0
        };

        var snapshot = EventTimeCalculator.CalculateSnapshot(now, ev);
        Assert.Equal(EventPhase.Inactive, snapshot.Phase);
        Assert.Equal(targetStart, snapshot.NextEventStart);
        Assert.Equal(remaining, snapshot.TimeRemaining);
    }

    [Fact]
    public void CalculateCycleStartTime_CrossesMidnight_CalibratesCorrectly()
    {
        // 23:45:00 on Day 1
        var now = new DateTimeOffset(2026, 11, 14, 23, 45, 0, TimeSpan.FromHours(1));
        var remaining = new TimeSpan(0, 45, 0); // 45m
        var targetStart = now.Add(remaining); // 00:30:00 on Day 2

        int cycleMinutes = 120; // 2-hour cycle (anchors: 00:00, 02:00, ...)
        var (h, m, s) = EventTimeCalculator.CalculateCycleStartTime(targetStart, cycleMinutes);

        Assert.Equal(0, h);
        Assert.Equal(30, m);
        Assert.Equal(0, s);

        var ev = new EventDefinition
        {
            Id = "two-hour-event",
            Name = "Two Hour Event",
            CycleMinutes = 120,
            StartHour = h,
            StartMinute = m,
            StartSecond = s,
            DurationMinutes = 20,
            TimeOffsetSeconds = 0
        };

        var snapshot = EventTimeCalculator.CalculateSnapshot(now, ev);
        Assert.Equal(EventPhase.Inactive, snapshot.Phase);
        Assert.Equal(targetStart, snapshot.NextEventStart);
        Assert.Equal(remaining, snapshot.TimeRemaining);
    }
}
