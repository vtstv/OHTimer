// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Localization.Catalogs;

internal static partial class EnglishCatalog
{
    static partial void RegisterKillCounterEntries(Dictionary<string, string> dict)
    {
        dict["Settings_TabKillCounter"] = "Kill Counter";
        dict["KillCounter_CardTarget"] = "Target & Run Goal";
        dict["KillCounter_TargetName"] = "Target / Boss Name:";
        dict["KillCounter_QuickPresets"] = "Quick Presets:";
        dict["KillCounter_CurrentCount"] = "Current Kills / Runs:";
        dict["KillCounter_EnableGoal"] = "Enable Run / Kill Goal";
        dict["KillCounter_GoalCount"] = "Goal Count:";
        dict["KillCounter_TotalAllTime"] = "Total All-Time Kills:";
        dict["KillCounter_ResetSession"] = "↺ Reset Session";

        dict["KillCounter_CardActivation"] = "Activation & Hotkeys";
        dict["KillCounter_Enable"] = "Enable Farm & Kill Counter";
        dict["KillCounter_IncrementHotkey"] = "Increment Kill (+1):";
        dict["KillCounter_DecrementHotkey"] = "Decrement / Undo (-1):";
        dict["KillCounter_HotkeyResetTooltip"] = "Reset to default hotkey";
        dict["KillCounter_PlaySound"] = "Sound confirmation on kill / undo";
        dict["KillCounter_AutoStartBossTimer"] = "Auto-start Boss Respawn Timer on kill";
        dict["KillCounter_ShowInMiniHud"] = "Show counter badge in Mini-HUD";
        dict["KillCounter_FlashOnGoalReached"] = "Flash HUD card & celebrate when goal reached";
        dict["KillCounter_Color"] = "Accent Color:";
        dict["KillCounter_ColorTooltip"] = "Choose counter accent color from palette";

        dict["KillCounter_Status_GoalReached"] = "GOAL REACHED! 🎉";
        dict["KillCounter_GoalReachedTitle"] = "Kill Goal Achieved!";
        dict["KillCounter_GoalReachedMsg"] = "Congratulations! You completed {0} runs on {1}!";
        dict["Settings_HotkeyKillIncrement"] = "Kill Counter Increment (+1)";
        dict["Settings_HotkeyKillIncrementDesc"] = "Add 1 to the active kill session counter and optionally start boss timer.";
        dict["Settings_HotkeyKillDecrement"] = "Kill Counter Decrement / Undo (-1)";
        dict["Settings_HotkeyKillDecrementDesc"] = "Undo the last kill count if accidentally pressed.";
        dict["KillCounter_BadgeKills"] = "KILLS";
        dict["KillCounter_BadgeGoal"] = "GOAL";
        dict["KillCounter_ResetPrompt"] = "Reset current session kill count to 0? All-time total will be preserved.";
    }
}
