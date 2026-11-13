// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Models;

/// <summary>
/// An immutable state snapshot of an in-game event at a specific timestamp.
/// </summary>
public record EventSnapshot
{
    /// <summary>
    /// Current cycle phase.
    /// </summary>
    public required EventPhase Phase { get; init; }

    /// <summary>
    /// Time remaining until the next phase change (until start if inactive, or until end if active).
    /// </summary>
    public required TimeSpan TimeRemaining { get; init; }

    /// <summary>
    /// Total duration of the current phase window.
    /// </summary>
    public required TimeSpan TotalPhaseDuration { get; init; }

    /// <summary>
    /// Progress ratio of the current phase (0.0 to 1.0).
    /// </summary>
    public required double ProgressRatio { get; init; }

    /// <summary>
    /// The timestamp when the next event will start.
    /// </summary>
    public required DateTimeOffset NextEventStart { get; init; }

    /// <summary>
    /// The timestamp when the current or next event will end.
    /// </summary>
    public required DateTimeOffset NextEventEnd { get; init; }

    /// <summary>
    /// Active event definition being tracked.
    /// </summary>
    public EventDefinition? Event { get; init; }

    /// <summary>
    /// Name of the active event.
    /// </summary>
    public string EventName { get; init; } = string.Empty;

    /// <summary>
    /// Effective time offset applied in seconds.
    /// </summary>
    public int EffectiveOffsetSeconds { get; init; } = 0;

    /// <summary>
    /// Indicates whether the event is actively taking place right now.
    /// </summary>
    public bool IsActive => Phase is EventPhase.Active or EventPhase.EndingSoon;

    /// <summary>
    /// Formatted countdown string (mm:ss or hh:mm:ss).
    /// </summary>
    public string FormattedRemaining => TimeRemaining.TotalHours >= 1
        ? $"{(int)TimeRemaining.TotalHours:D2}:{TimeRemaining.Minutes:D2}:{TimeRemaining.Seconds:D2}"
        : $"{TimeRemaining.Minutes:D2}:{TimeRemaining.Seconds:D2}";
}
