// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Models;

namespace OnceHumanEventTimer.Core.Services;

/// <summary>
/// Service managing real-time countdown, state transitions, and alert triggers for boss respawn tracking.
/// </summary>
public interface IBossTimerTracker : IDisposable
{
    /// <summary>Gets the latest state snapshot.</summary>
    BossTimerSnapshot CurrentSnapshot { get; }

    /// <summary>Gets the current state of the boss timer.</summary>
    BossTimerState State { get; }

    /// <summary>Gets whether the timer is currently active (running, warning, or spawned).</summary>
    bool IsActive { get; }

    /// <summary>Fired when timer state, remaining time, or progress updates.</summary>
    event EventHandler<BossTimerSnapshot>? SnapshotUpdated;

    /// <summary>Fired when hotkey is triggered to play confirmation chime.</summary>
    event EventHandler? HotkeyTriggered;

    /// <summary>Fired when remaining time enters early warning threshold.</summary>
    event EventHandler? WarningTriggered;

    /// <summary>Fired when timer hits 00:00 (boss spawn).</summary>
    event EventHandler? SpawnTriggered;

    /// <summary>Fired on each configured repeat alert interval after spawn.</summary>
    event EventHandler? RepeatAlertTriggered;

    /// <summary>Updates tracker with new configuration settings.</summary>
    void Configure(BossTimerSettings settings);

    /// <summary>Starts or restarts the boss respawn countdown, optionally anchored at a reference time.</summary>
    void Start(DateTimeOffset? startTime = null);

    /// <summary>Stops the countdown immediately.</summary>
    void Stop();

    /// <summary>Toggles timer execution: stops running timer or starts inactive timer.</summary>
    void Toggle();

    /// <summary>Handles hotkey press according to configured repress behavior.</summary>
    void HandleHotkeyPress();

    /// <summary>Advances the timer using a reference point in time (deterministic for testing).</summary>
    void Tick(DateTimeOffset referenceTime);
}
