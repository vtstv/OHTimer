// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StarfallTimer.Core.Models;

/// <summary>
/// User configuration settings for overlay appearance, timing, and alerts.
/// </summary>
public class UserSettings
{
    /// <summary>
    /// X coordinate of the overlay window.
    /// </summary>
    public double OverlayPositionX { get; set; } = 40;

    /// <summary>
    /// Y coordinate of the overlay window.
    /// </summary>
    public double OverlayPositionY { get; set; } = 40;

    /// <summary>
    /// Background opacity of the HUD overlay (0.2 to 1.0).
    /// </summary>
    public double Opacity { get; set; } = 0.92;

    /// <summary>
    /// HUD UI Scale factor (0.7 to 2.0).
    /// </summary>
    public double Scale { get; set; } = 1.0;

    /// <summary>
    /// When true, mouse clicks pass through the overlay to the game underneath.
    /// </summary>
    public bool IsClickThrough { get; set; } = false;

    /// <summary>
    /// When true, the overlay position is locked and cannot be dragged.
    /// </summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// When true, overlay stays above all other windows.
    /// </summary>
    public bool AlwaysOnTop { get; set; } = true;

    /// <summary>
    /// When true, compact HUD view is rendered (smaller footprint).
    /// </summary>
    public bool CompactMode { get; set; } = false;

    /// <summary>
    /// Master volume for sound notifications (0.0 to 1.0).
    /// </summary>
    public double Volume { get; set; } = 0.8;

    /// <summary>
    /// Selected UI language code (e.g., "en-US", "ru-RU", "zh-CN").
    /// </summary>
    public string Language { get; set; } = "en-US";

    /// <summary>
    /// The minute past the hour when Starfall Inversion starts (default: 22).
    /// </summary>
    public int StartMinute { get; set; } = 22;

    /// <summary>
    /// The duration of the Starfall Inversion event in minutes (default: 30).
    /// </summary>
    public int DurationMinutes { get; set; } = 30;

    /// <summary>
    /// Minutes before event start to trigger early notification (default: 2).
    /// </summary>
    public int AlertMinutesBefore { get; set; } = 2;

    /// <summary>
    /// Minutes before event end to trigger warning (default: 3).
    /// </summary>
    public int AlertMinutesBeforeEnd { get; set; } = 3;

    /// <summary>
    /// Whether sound alerts are played.
    /// </summary>
    public bool PlayAlertSound { get; set; } = true;

    /// <summary>
    /// Whether Windows toast notifications are shown.
    /// </summary>
    public bool ShowToastAlert { get; set; } = true;

    /// <summary>
    /// Play chime sound when event actually starts.
    /// </summary>
    public bool PlayOnEventStart { get; set; } = true;

    /// <summary>
    /// Optional path to custom sound file (.wav).
    /// </summary>
    public string? CustomSoundPath { get; set; }

    /// <summary>
    /// Global hotkey string to toggle overlay visibility.
    /// </summary>
    public string ToggleVisibilityHotkey { get; set; } = "Ctrl+Alt+S";

    /// <summary>
    /// Global hotkey string to toggle click-through lock.
    /// </summary>
    public string ToggleClickThroughHotkey { get; set; } = "Ctrl+Alt+L";
}
