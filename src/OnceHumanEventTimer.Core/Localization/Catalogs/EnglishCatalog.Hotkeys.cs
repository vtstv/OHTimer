// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Localization.Catalogs;

internal static partial class EnglishCatalog
{
    static partial void RegisterHotkeyEntries(Dictionary<string, string> dict)
    {
        dict["Settings_HotkeysCardTitle"] = "Global Shortcut Hotkeys";
        dict["Settings_HotkeysHint"] = "Click any hotkey to record a new key or combination. Press Esc to cancel.";
        dict["Settings_HotkeyToggle"] = "Toggle Overlay Visibility Hotkey:";
        dict["Settings_HotkeyToggleDesc"] = "Instantly show or hide the floating HUD overlay during gameplay.";
        dict["Settings_HotkeyLock"] = "Toggle Click-Through / Lock Hotkey:";
        dict["Settings_HotkeyLockDesc"] = "Lock overlay position and allow mouse clicks to pass through to the game.";
        dict["Settings_HotkeyPauseAlerts"] = "Toggle Pause Alerts Hotkey:";
        dict["Settings_HotkeyPauseAlertsDesc"] = "Temporarily mute or resume all audio alerts and toast notifications.";
        dict["Settings_HotkeySettings"] = "Open / Close Settings Window:";
        dict["Settings_HotkeySettingsDesc"] = "Global hotkey to toggle the Settings dialog on and off (default: Ctrl+Alt+O).";
        dict["Settings_HotkeyBossTimer"] = "Boss Respawn Timer Hotkey:";
        dict["Settings_HotkeyBossTimerDesc"] = "Start, restart, or toggle the dedicated boss respawn countdown.";
        dict["Settings_HotkeyResetTooltip"] = "Reset to default hotkey";
    }
}
