// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Models;

namespace OnceHumanEventTimer.Core.Services;

/// <summary>
/// Service managing real-time kill and farm run count tracking, goals, and hotkey actions.
/// </summary>
public interface IKillCounterTracker : IDisposable
{
    /// <summary>Gets the latest state snapshot.</summary>
    KillCounterSnapshot CurrentSnapshot { get; }

    /// <summary>Gets whether the tracker is currently enabled.</summary>
    bool IsEnabled { get; }

    /// <summary>Fired when kill counter state, count, or progress updates.</summary>
    event EventHandler<KillCounterSnapshot>? SnapshotUpdated;

    /// <summary>Fired when a kill or run count is incremented (+1).</summary>
    event EventHandler? CountIncremented;

    /// <summary>Fired when a kill count is decremented / undone (-1).</summary>
    event EventHandler? CountDecremented;

    /// <summary>Fired when session count is reset to 0.</summary>
    event EventHandler? SessionReset;

    /// <summary>Fired when target run goal is achieved.</summary>
    event EventHandler? GoalReached;

    /// <summary>Updates tracker with new configuration settings.</summary>
    void Configure(KillCounterSettings settings);

    /// <summary>Increments session and all-time counts by 1.</summary>
    void Increment();

    /// <summary>Decrements session and all-time counts by 1 (undo).</summary>
    void Decrement();

    /// <summary>Resets current session count to 0, preserving all-time total.</summary>
    void ResetSession();

    /// <summary>Resets all-time count to 0.</summary>
    void ResetAllTime();

    /// <summary>Directly sets current session count.</summary>
    void SetCurrentCount(int count);

    /// <summary>Updates goal target configuration.</summary>
    void SetGoal(bool enableGoal, int goalCount);

    /// <summary>Updates target boss / objective name.</summary>
    void SetBossName(string name);
}
