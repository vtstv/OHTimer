// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Models;

/// <summary>
/// Configuration settings for the dedicated Farm and Kill Tracker.
/// </summary>
[Serializable]
public class KillCounterSettings
{
    /// <summary>Whether the Kill Counter feature is active on the HUD overlay.</summary>
    public bool IsEnabled { get; set; } = false;

    private string _bossName = "Shadow Hound";
    /// <summary>Target boss or objective name displayed on the HUD.</summary>
    public string BossName
    {
        get => string.IsNullOrWhiteSpace(_bossName) ? "Shadow Hound" : _bossName;
        set => _bossName = string.IsNullOrWhiteSpace(value) ? "Shadow Hound" : value;
    }

    private int _currentCount = 0;
    /// <summary>Current session kill / run count.</summary>
    public int CurrentCount
    {
        get => Math.Max(0, _currentCount);
        set => _currentCount = Math.Max(0, value);
    }

    /// <summary>Whether a target run / kill goal is enabled.</summary>
    public bool EnableGoal { get; set; } = false;

    private int _goalCount = 20;
    /// <summary>Target run / kill goal count (e.g. 20 runs).</summary>
    public int GoalCount
    {
        get => _goalCount <= 0 ? 1 : _goalCount;
        set => _goalCount = value <= 0 ? 1 : value;
    }

    private int _totalAllTimeCount = 0;
    /// <summary>Accumulated all-time total kills across sessions.</summary>
    public int TotalAllTimeCount
    {
        get => Math.Max(0, _totalAllTimeCount);
        set => _totalAllTimeCount = Math.Max(0, value);
    }

    private string _incrementHotkey = "NumPad +";
    /// <summary>Global hotkey string to increment kill count (+1).</summary>
    public string IncrementHotkey
    {
        get => string.IsNullOrWhiteSpace(_incrementHotkey) ? "NumPad +" : _incrementHotkey;
        set => _incrementHotkey = string.IsNullOrWhiteSpace(value) ? "NumPad +" : value;
    }

    private string _decrementHotkey = "NumPad -";
    /// <summary>Global hotkey string to decrement / undo kill count (-1).</summary>
    public string DecrementHotkey
    {
        get => string.IsNullOrWhiteSpace(_decrementHotkey) ? "NumPad -" : _decrementHotkey;
        set => _decrementHotkey = string.IsNullOrWhiteSpace(value) ? "NumPad -" : value;
    }

    /// <summary>Whether to play an audible sound chime on count increment or decrement.</summary>
    public bool PlaySound { get; set; } = true;

    /// <summary>Whether to automatically start the active Boss Respawn Timer on kill increment.</summary>
    public bool AutoStartBossTimer { get; set; } = false;

    private string _colorHex = "#EF4444";
    /// <summary>Accent hex color used for the kill counter card and badges.</summary>
    public string ColorHex
    {
        get => string.IsNullOrWhiteSpace(_colorHex) ? "#EF4444" : _colorHex;
        set => _colorHex = string.IsNullOrWhiteSpace(value) ? "#EF4444" : value;
    }

    /// <summary>Whether to render the kill counter badge in compact Mini-HUD mode.</summary>
    public bool ShowInMiniHud { get; set; } = true;

    /// <summary>Whether to visually flash the HUD card when the goal is reached.</summary>
    public bool FlashOnGoalReached { get; set; } = true;

    /// <summary>Creates a shallow copy of the settings.</summary>
    public KillCounterSettings Clone() => (KillCounterSettings)MemberwiseClone();
}
