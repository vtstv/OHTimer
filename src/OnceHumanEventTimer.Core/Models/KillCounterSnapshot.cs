// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Models;

/// <summary>
/// Immutable snapshot representing the current state of the Kill Counter.
/// </summary>
public record KillCounterSnapshot(
    bool IsEnabled,
    string BossName,
    int CurrentCount,
    bool EnableGoal,
    int GoalCount,
    int TotalAllTimeCount,
    double ProgressRatio,
    bool IsGoalReached,
    string FormattedCount,
    string ColorHex,
    bool FlashOnGoalReached,
    bool ShowInMiniHud
);
