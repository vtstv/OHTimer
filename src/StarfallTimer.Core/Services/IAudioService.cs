// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StarfallTimer.Core.Services;

/// <summary>
/// Service responsible for playing alert chimes and custom notifications.
/// </summary>
public interface IAudioService : IDisposable
{
    /// <summary>
    /// Plays an urgent warning chime (approaching event start).
    /// </summary>
    void PlayWarning(double volume, string? customFilePath = null);

    /// <summary>
    /// Plays a celebratory event start chime.
    /// </summary>
    void PlayEventStart(double volume);

    /// <summary>
    /// Plays an event end warning chime.
    /// </summary>
    void PlayEventEnding(double volume);

    /// <summary>
    /// Plays a test sound at specified volume.
    /// </summary>
    void PlayTestSound(double volume);
}
