// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Models;

/// <summary>
/// Execution state of the Boss Respawn Timer.
/// </summary>
public enum BossTimerState
{
    /// <summary>Timer is stopped or not running.</summary>
    Inactive = 0,

    /// <summary>Timer is actively counting down to boss spawn.</summary>
    Running = 1,

    /// <summary>Remaining time is within warning threshold before spawn.</summary>
    Warning = 2,

    /// <summary>Boss has spawned (00:00 reached).</summary>
    Spawned = 3
}
