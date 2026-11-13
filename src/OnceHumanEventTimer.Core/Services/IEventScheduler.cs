// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Models;

namespace OnceHumanEventTimer.Core.Services;

/// <summary>
/// Service responsible for calculating and updating event schedules, time drift offsets, and countdowns.
/// </summary>
public interface IEventScheduler : IDisposable
{
    /// <summary>
    /// Current calculated snapshot of the active event.
    /// </summary>
    EventSnapshot CurrentSnapshot { get; }

    /// <summary>
    /// Current calculated snapshots for all enabled events in multi-timer mode.
    /// </summary>
    IReadOnlyList<EventSnapshot> ActiveSnapshots { get; }

    /// <summary>
    /// Active event schedule definition being evaluated.
    /// </summary>
    EventDefinition ActiveEvent { get; }

    /// <summary>
    /// All configured events currently tracked by the scheduler.
    /// </summary>
    IReadOnlyList<EventDefinition> TrackedEvents { get; }

    /// <summary>
    /// Total combined time offset in seconds applied to reference timestamps.
    /// </summary>
    int TimeOffsetSeconds { get; set; }

    /// <summary>
    /// Raised every second when the snapshot is updated.
    /// </summary>
    event EventHandler<EventSnapshot>? SnapshotUpdated;

    /// <summary>
    /// Raised every second when all active snapshots are updated.
    /// </summary>
    event EventHandler<IReadOnlyList<EventSnapshot>>? SnapshotsUpdated;

    /// <summary>
    /// Raised when an alert condition is met (e.g. 2 minutes before event).
    /// </summary>
    event EventHandler<NotificationTriggerEventArgs>? NotificationTriggered;

    /// <summary>
    /// Raised when the time offset is updated or synchronized.
    /// </summary>
    event EventHandler<int>? TimeOffsetChanged;

    /// <summary>
    /// Starts the background evaluation timer.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the timer.
    /// </summary>
    void Stop();

    /// <summary>
    /// Sets the active event definition and optional global offset.
    /// </summary>
    void SetActiveEvent(EventDefinition eventDefinition, int globalTimeOffsetSeconds = 0);

    /// <summary>
    /// Sets the collection of tracked events, the active event, and optional global offset.
    /// </summary>
    void SetTrackedEvents(IEnumerable<EventDefinition> events, string? activeEventId = null, int globalTimeOffsetSeconds = 0);

    /// <summary>
    /// Updates event cycle parameters for legacy/quick configuration.
    /// </summary>
    void ConfigureSchedule(int startMinute, int durationMinutes, int alertBeforeStart, int alertBeforeEnd);

    /// <summary>
    /// Sets the drift time offset in seconds.
    /// </summary>
    void SetTimeOffset(int offsetSeconds);

    /// <summary>
    /// Synchronizes the scheduler to match in-game remaining countdown time to the second.
    /// Automatically calculates and applies the required TimeOffsetSeconds.
    /// </summary>
    /// <param name="remainingTime">The countdown time currently displayed in the game HUD.</param>
    /// <param name="isUntilEnd">
    /// Optional flag indicating if countdown is until event end (active) or until event start (inactive).
    /// If null, infers based on current phase and remaining duration.
    /// </param>
    /// <returns>The newly calculated TimeOffsetSeconds.</returns>
    int SyncToGameTime(TimeSpan remainingTime, bool? isUntilEnd = null);

    /// <summary>
    /// Pure function to calculate snapshot for an arbitrary reference timestamp using current active event.
    /// </summary>
    EventSnapshot CalculateSnapshot(DateTimeOffset referenceTime);

    /// <summary>
    /// Pure function to calculate snapshot for an arbitrary reference timestamp with a specific event definition and offset.
    /// </summary>
    EventSnapshot CalculateSnapshot(DateTimeOffset referenceTime, EventDefinition eventDefinition, int offsetSeconds = 0);

    /// <summary>
    /// Pure function to calculate snapshots for all currently enabled events at an arbitrary reference timestamp.
    /// </summary>
    IReadOnlyList<EventSnapshot> CalculateActiveSnapshots(DateTimeOffset referenceTime);
}
