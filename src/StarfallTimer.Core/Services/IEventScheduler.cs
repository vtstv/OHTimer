// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using StarfallTimer.Core.Models;

namespace StarfallTimer.Core.Services;

/// <summary>
/// Service responsible for calculating and updating Starfall Inversion event timing.
/// </summary>
public interface IEventScheduler : IDisposable
{
    /// <summary>
    /// Current calculated snapshot of the event.
    /// </summary>
    EventSnapshot CurrentSnapshot { get; }

    /// <summary>
    /// Raised every second when the snapshot is updated.
    /// </summary>
    event EventHandler<EventSnapshot>? SnapshotUpdated;

    /// <summary>
    /// Raised when an alert condition is met (e.g. 2 minutes before event).
    /// </summary>
    event EventHandler<NotificationTriggerEventArgs>? NotificationTriggered;

    /// <summary>
    /// Starts the background evaluation timer.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the timer.
    /// </summary>
    void Stop();

    /// <summary>
    /// Updates event cycle parameters.
    /// </summary>
    void ConfigureSchedule(int startMinute, int durationMinutes, int alertBeforeStart, int alertBeforeEnd);

    /// <summary>
    /// Pure function to calculate snapshot for an arbitrary reference timestamp.
    /// Useful for unit testing and future cycle projections.
    /// </summary>
    EventSnapshot CalculateSnapshot(DateTimeOffset referenceTime);
}
