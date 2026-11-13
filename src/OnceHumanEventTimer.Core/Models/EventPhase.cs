// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Models;

/// <summary>
/// Represents the phase of the Starfall Inversion event cycle.
/// </summary>
public enum EventPhase
{
    /// <summary>
    /// Event is currently inactive, counting down to the next window.
    /// </summary>
    Inactive,

    /// <summary>
    /// Approaching the start of the event (within threshold, e.g. 2 minutes).
    /// </summary>
    StartingSoon,

    /// <summary>
    /// The Starfall Inversion event is active in-game (low gravity tide, crystals active).
    /// </summary>
    Active,

    /// <summary>
    /// The event is active and nearing its conclusion.
    /// </summary>
    EndingSoon
}
