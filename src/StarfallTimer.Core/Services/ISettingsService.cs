// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using StarfallTimer.Core.Models;

namespace StarfallTimer.Core.Services;

/// <summary>
/// Service managing loading, persistence, and synchronization of user settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Current loaded user settings.
    /// </summary>
    UserSettings Settings { get; }

    /// <summary>
    /// Raised whenever settings are updated and saved.
    /// </summary>
    event EventHandler<UserSettings>? SettingsChanged;

    /// <summary>
    /// Loads settings from disk or initializes defaults.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Persists settings to disk.
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// Notifies listeners that in-memory settings have changed without writing to disk.
    /// </summary>
    void NotifyChanged();
}
