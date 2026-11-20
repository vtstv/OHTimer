// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Models;

/// <summary>
/// Immutable snapshot representing the current real-time state of the Boss Respawn Timer.
/// </summary>
public sealed record BossTimerSnapshot(
    BossTimerState State,
    TimeSpan RemainingTime,
    TimeSpan TotalDuration,
    double ProgressRatio,
    string FormattedRemaining,
    string BossName,
    string ColorHex,
    bool IsActive,
    bool IsWarning,
    bool IsSpawned,
    bool IsFlashing);
