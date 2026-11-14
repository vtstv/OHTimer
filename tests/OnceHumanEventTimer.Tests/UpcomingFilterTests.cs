// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Scheduling;
using Xunit;

namespace OnceHumanEventTimer.Tests;

/// <summary>
/// Unit tests for ShowOnlyWhenUpcoming filter logic on long-cycle events.
/// </summary>
public class UpcomingFilterTests
{
    [Fact]
    public void UpcomingFilter_HidesLongCycleEvent_WhenFarAway()
    {
        var greenhouse = new EventDefinition
        {
            Id = "drift-greenhouse",
            Name = "Drift Greenhouse",
            CycleMinutes = 240,
            StartMinute = 4,
            StartSecond = 4,
            DurationMinutes = 30,
            ShowOnlyWhenUpcoming = true,
            UpcomingThresholdMinutes = 10,
            IsEnabled = true
        };

        var abyss = new EventDefinition
        {
            Id = "gravity-abyss",
            Name = "Gravity Abyss",
            CycleMinutes = 60,
            StartMinute = 22,
            DurationMinutes = 30,
            ShowOnlyWhenUpcoming = false,
            IsEnabled = true
        };

        // At 13:10:00:
        // Greenhouse starts at 16:04:04 (2h 54m away) -> Should be filtered out
        // Gravity Abyss starts at 13:22:00 (12m away) -> Should be visible
        var testTime = new DateTimeOffset(2026, 11, 14, 13, 10, 0, TimeSpan.FromHours(1));

        var snapGreenhouse = EventTimeCalculator.CalculateSnapshot(testTime, greenhouse);
        var snapAbyss = EventTimeCalculator.CalculateSnapshot(testTime, abyss);

        var list = new List<EventSnapshot> { snapGreenhouse, snapAbyss };

        var visible = list.Where(s =>
        {
            if (s.Event == null) return true;
            if (!s.Event.ShowOnlyWhenUpcoming) return true;
            if (s.IsActive) return true;
            int threshold = s.Event.UpcomingThresholdMinutes > 0 ? s.Event.UpcomingThresholdMinutes : 10;
            return s.TimeRemaining <= TimeSpan.FromMinutes(threshold);
        }).ToList();

        Assert.Single(visible);
        Assert.Equal("gravity-abyss", visible[0].Event?.Id);
    }

    [Fact]
    public void UpcomingFilter_ShowsLongCycleEvent_WhenWithinThreshold()
    {
        var greenhouse = new EventDefinition
        {
            Id = "drift-greenhouse",
            Name = "Drift Greenhouse",
            CycleMinutes = 240,
            StartMinute = 4,
            StartSecond = 4,
            DurationMinutes = 30,
            ShowOnlyWhenUpcoming = true,
            UpcomingThresholdMinutes = 10,
            IsEnabled = true
        };

        // At 15:56:00 (8 minutes before start at 16:04:04) -> <= 10m threshold -> Should be visible
        var testTime = new DateTimeOffset(2026, 11, 14, 15, 56, 0, TimeSpan.FromHours(1));
        var snapGreenhouse = EventTimeCalculator.CalculateSnapshot(testTime, greenhouse);

        var list = new List<EventSnapshot> { snapGreenhouse };

        var visible = list.Where(s =>
        {
            if (s.Event == null) return true;
            if (!s.Event.ShowOnlyWhenUpcoming) return true;
            if (s.IsActive) return true;
            int threshold = s.Event.UpcomingThresholdMinutes > 0 ? s.Event.UpcomingThresholdMinutes : 10;
            return s.TimeRemaining <= TimeSpan.FromMinutes(threshold);
        }).ToList();

        Assert.Single(visible);
        Assert.Equal("drift-greenhouse", visible[0].Event?.Id);
    }

    [Fact]
    public void UpcomingFilter_ShowsLongCycleEvent_WhenActive()
    {
        var greenhouse = new EventDefinition
        {
            Id = "drift-greenhouse",
            Name = "Drift Greenhouse",
            CycleMinutes = 240,
            StartMinute = 4,
            StartSecond = 4,
            DurationMinutes = 30,
            ShowOnlyWhenUpcoming = true,
            UpcomingThresholdMinutes = 10,
            IsEnabled = true
        };

        // At 16:15:00 (event is actively running) -> Should be visible
        var testTime = new DateTimeOffset(2026, 11, 14, 16, 15, 0, TimeSpan.FromHours(1));
        var snapGreenhouse = EventTimeCalculator.CalculateSnapshot(testTime, greenhouse);

        Assert.True(snapGreenhouse.IsActive);

        var list = new List<EventSnapshot> { snapGreenhouse };

        var visible = list.Where(s =>
        {
            if (s.Event == null) return true;
            if (!s.Event.ShowOnlyWhenUpcoming) return true;
            if (s.IsActive) return true;
            int threshold = s.Event.UpcomingThresholdMinutes > 0 ? s.Event.UpcomingThresholdMinutes : 10;
            return s.TimeRemaining <= TimeSpan.FromMinutes(threshold);
        }).ToList();

        Assert.Single(visible);
        Assert.Equal("drift-greenhouse", visible[0].Event?.Id);
    }
}
