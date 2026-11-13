// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Services;

/// <summary>
/// Service monitoring whether the Once Human game client or application window has foreground focus.
/// </summary>
public interface IGameFocusMonitor : IDisposable
{
    /// <summary>
    /// Whether Once Human (or this application) currently has active foreground window focus.
    /// </summary>
    bool IsGameFocused { get; }

    /// <summary>
    /// Event triggered when game client focus changes.
    /// </summary>
    event EventHandler<bool>? GameFocusChanged;

    /// <summary>
    /// Starts background polling for foreground window focus.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops background polling.
    /// </summary>
    void Stop();
}
