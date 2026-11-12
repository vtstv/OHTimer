// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StarfallTimer.Core.Models;

/// <summary>
/// Defines a recurring in-game event schedule and display profile.
/// </summary>
public class EventDefinition
{
    /// <summary>
    /// Unique identifier for the event (e.g., "gravity-abyss", "cargo-scramble").
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the event.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Total duration of one recurrence cycle in minutes (default: 60 for hourly events).
    /// </summary>
    public int CycleMinutes { get; set; } = 60;

    /// <summary>
    /// Minute past the cycle anchor when the event begins (0 to CycleMinutes - 1).
    /// </summary>
    public int StartMinute { get; set; } = 22;

    /// <summary>
    /// Active duration of the event window in minutes.
    /// </summary>
    public int DurationMinutes { get; set; } = 30;

    /// <summary>
    /// Offset in seconds applied to synchronize with game server time drift.
    /// </summary>
    public int TimeOffsetSeconds { get; set; } = 0;

    /// <summary>
    /// Hex color code used for accents and HUD badges (e.g., "#00E5FF", "#A855F7", "#F59E0B").
    /// </summary>
    public string ColorHex { get; set; } = "#00E5FF";

    /// <summary>
    /// Brief description of the event, objectives, and mechanics.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this event is enabled for multi-timer tracking in the overlay.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Creates an exact clone of the event definition.
    /// </summary>
    public EventDefinition Clone()
    {
        return new EventDefinition
        {
            Id = Id,
            Name = Name,
            CycleMinutes = CycleMinutes,
            StartMinute = StartMinute,
            DurationMinutes = DurationMinutes,
            TimeOffsetSeconds = TimeOffsetSeconds,
            ColorHex = ColorHex,
            Description = Description,
            IsEnabled = IsEnabled
        };
    }
}
