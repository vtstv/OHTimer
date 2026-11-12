// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using StarfallTimer.Core.Models;

namespace StarfallTimer.Core.Scheduling;

/// <summary>
/// Pure calculation functions for event cycles, phase windows, and countdowns.
/// </summary>
public static class EventTimeCalculator
{
    /// <summary>
    /// Calculates an immutable event snapshot for an arbitrary reference timestamp.
    /// </summary>
    public static EventSnapshot CalculateSnapshot(
        DateTimeOffset referenceTime,
        EventDefinition eventDefinition,
        int offsetSeconds = 0,
        int alertBeforeStart = 2,
        int alertBeforeEnd = 3)
    {
        var effectiveTime = referenceTime.AddSeconds(offsetSeconds);

        int cycleMinutes = Math.Max(1, eventDefinition.CycleMinutes);
        int startMinute = Math.Clamp(eventDefinition.StartMinute, 0, cycleMinutes - 1);
        int durationMinutes = Math.Clamp(eventDefinition.DurationMinutes, 1, cycleMinutes);

        var baseDay = new DateTimeOffset(
            effectiveTime.Year,
            effectiveTime.Month,
            effectiveTime.Day,
            0,
            0,
            0,
            effectiveTime.Offset);

        double totalMinutesSinceDay = (effectiveTime - baseDay).TotalMinutes;
        long cycleIndex = (long)Math.Floor(totalMinutesSinceDay / cycleMinutes);
        var cycleAnchor = baseDay.AddMinutes(cycleIndex * cycleMinutes);

        var cycleStart = cycleAnchor.AddMinutes(startMinute);
        var cycleEnd = cycleStart.AddMinutes(durationMinutes);
        var cycleDuration = TimeSpan.FromMinutes(cycleMinutes);

        DateTimeOffset nextStart;
        DateTimeOffset nextEnd;
        EventPhase phase;
        TimeSpan timeRemaining;
        TimeSpan totalPhaseDuration;

        if (effectiveTime < cycleStart)
        {
            var prevCycleStart = (cycleAnchor - cycleDuration).AddMinutes(startMinute);
            var prevCycleEnd = prevCycleStart.AddMinutes(durationMinutes);
            totalPhaseDuration = cycleStart - prevCycleEnd;
            timeRemaining = cycleStart - effectiveTime;
            nextStart = cycleStart;
            nextEnd = cycleEnd;

            bool isStartingSoon = timeRemaining <= TimeSpan.FromMinutes(alertBeforeStart);
            phase = isStartingSoon ? EventPhase.StartingSoon : EventPhase.Inactive;
        }
        else if (effectiveTime < cycleEnd)
        {
            totalPhaseDuration = TimeSpan.FromMinutes(durationMinutes);
            timeRemaining = cycleEnd - effectiveTime;
            nextStart = cycleStart;
            nextEnd = cycleEnd;

            bool isEndingSoon = timeRemaining <= TimeSpan.FromMinutes(alertBeforeEnd);
            phase = isEndingSoon ? EventPhase.EndingSoon : EventPhase.Active;
        }
        else
        {
            var nextCycleAnchor = cycleAnchor + cycleDuration;
            nextStart = nextCycleAnchor.AddMinutes(startMinute);
            nextEnd = nextStart.AddMinutes(durationMinutes);

            totalPhaseDuration = nextStart - cycleEnd;
            timeRemaining = nextStart - effectiveTime;

            bool isStartingSoon = timeRemaining <= TimeSpan.FromMinutes(alertBeforeStart);
            phase = isStartingSoon ? EventPhase.StartingSoon : EventPhase.Inactive;
        }

        if (timeRemaining < TimeSpan.Zero)
        {
            timeRemaining = TimeSpan.Zero;
        }

        double elapsedSeconds = Math.Max(0, totalPhaseDuration.TotalSeconds - timeRemaining.TotalSeconds);
        double progress = totalPhaseDuration.TotalSeconds > 0
            ? Math.Clamp(elapsedSeconds / totalPhaseDuration.TotalSeconds, 0.0, 1.0)
            : 0.0;

        return new EventSnapshot
        {
            Phase = phase,
            TimeRemaining = timeRemaining,
            TotalPhaseDuration = totalPhaseDuration,
            ProgressRatio = progress,
            NextEventStart = nextStart,
            NextEventEnd = nextEnd,
            Event = eventDefinition,
            EventName = eventDefinition.Name,
            EffectiveOffsetSeconds = offsetSeconds
        };
    }
}
