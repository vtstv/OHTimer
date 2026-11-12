// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StarfallTimer.Core.Services;

/// <summary>
/// Service managing system-wide keyboard shortcuts.
/// </summary>
public interface IHotkeyService : IDisposable
{
    /// <summary>
    /// Registers a global hotkey with an associated callback action.
    /// </summary>
    bool RegisterHotkey(string hotkeyString, Action callback);

    /// <summary>
    /// Unregisters all active hotkeys.
    /// </summary>
    void UnregisterAll();
}
