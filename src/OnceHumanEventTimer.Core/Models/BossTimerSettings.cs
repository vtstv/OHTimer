// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Models;

/// <summary>
/// Configuration settings for the dedicated Boss Respawn Timer.
/// </summary>
public class BossTimerSettings
{
    /// <summary>Whether the Boss Timer feature and its global hotkey are enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    private string _hotkey = "Alt+NumPad 1";
    /// <summary>Interactive hotkey string to trigger the boss timer (default: Alt+NumPad 1).</summary>
    public string Hotkey
    {
        get => string.IsNullOrWhiteSpace(_hotkey) ? "Alt+NumPad 1" : _hotkey;
        set => _hotkey = string.IsNullOrWhiteSpace(value) ? "Alt+NumPad 1" : value;
    }

    /// <summary>Action to take when hotkey is re-pressed while timer is active.</summary>
    public BossTimerRepressBehavior RepressBehavior { get; set; } = BossTimerRepressBehavior.Restart;

    /// <summary>Whether to play an audible sound chime when the hotkey is pressed.</summary>
    public bool PlayHotkeySound { get; set; } = true;

    private string _bossName = "Boss Respawn";
    /// <summary>Target boss or objective name displayed on the HUD.</summary>
    public string BossName
    {
        get => string.IsNullOrWhiteSpace(_bossName) ? "Boss Respawn" : _bossName;
        set => _bossName = string.IsNullOrWhiteSpace(value) ? "Boss Respawn" : value;
    }

    /// <summary>Total countdown duration minutes component.</summary>
    public int DurationMinutes { get; set; } = 5;

    /// <summary>Total countdown duration seconds component.</summary>
    public int DurationSeconds { get; set; } = 0;

    /// <summary>Whether to play a warning sound before boss spawn.</summary>
    public bool EnableWarningAlert { get; set; } = true;

    /// <summary>Seconds before spawn to trigger early warning alert (default: 30s).</summary>
    public int WarningSecondsAhead { get; set; } = 30;

    /// <summary>Sound preset for early warning alert.</summary>
    public SoundPreset WarningSoundPreset { get; set; } = SoundPreset.CyberWarning;

    /// <summary>Whether to play an alert when the boss respawns at 00:00.</summary>
    public bool EnableSpawnAlert { get; set; } = true;

    /// <summary>Sound preset played upon boss respawn.</summary>
    public SoundPreset SpawnSoundPreset { get; set; } = SoundPreset.EmergencySiren;

    /// <summary>Volume level specifically for boss timer alerts (0.0 to 1.0).</summary>
    public double Volume { get; set; } = 0.8;

    /// <summary>Whether to trigger repeating alerts after boss spawns.</summary>
    public bool EnableRepeatAlert { get; set; } = true;

    /// <summary>How many times to repeat alert after spawn (default: 3).</summary>
    public int RepeatAlertCount { get; set; } = 3;

    /// <summary>Interval in seconds between repeat alerts (default: 10s).</summary>
    public int RepeatAlertIntervalSeconds { get; set; } = 10;

    /// <summary>Whether to dispatch a Windows OS Toast notification when boss spawns.</summary>
    public bool ShowToastOnSpawn { get; set; } = true;

    /// <summary>Whether to visually flash the HUD card when boss spawns.</summary>
    public bool FlashCardOnSpawn { get; set; } = true;

    /// <summary>Seconds to keep showing the card on HUD before auto-hiding after spawn.</summary>
    public int AutoHideSeconds { get; set; } = 15;

    private string _colorHex = "#EF4444";
    /// <summary>Accent hex color used for the boss timer card and badges.</summary>
    public string ColorHex
    {
        get => string.IsNullOrWhiteSpace(_colorHex) ? "#EF4444" : _colorHex;
        set => _colorHex = string.IsNullOrWhiteSpace(value) ? "#EF4444" : value;
    }

    /// <summary>Calculates total duration as a TimeSpan.</summary>
    public TimeSpan GetTotalDuration()
    {
        int totalSec = Math.Max(0, DurationMinutes) * 60 + Math.Max(0, DurationSeconds);
        return totalSec > 0 ? TimeSpan.FromSeconds(totalSec) : TimeSpan.FromMinutes(5);
    }

    /// <summary>Creates a shallow copy of the settings.</summary>
    public BossTimerSettings Clone() => (BossTimerSettings)MemberwiseClone();
}
