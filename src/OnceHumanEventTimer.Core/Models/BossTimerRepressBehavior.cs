// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Models;

/// <summary>
/// Defines the behavior when the Boss Timer hotkey is pressed while a timer is already active.
/// </summary>
public enum BossTimerRepressBehavior
{
    /// <summary>Restarts the countdown from full duration (default).</summary>
    Restart = 0,

    /// <summary>Toggles timer execution: stops running timer or starts inactive timer.</summary>
    Toggle = 1,

    /// <summary>Stops the countdown immediately.</summary>
    Stop = 2
}
