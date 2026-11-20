// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Localization.Catalogs;

internal static partial class GermanCatalog
{
    static partial void RegisterKillCounterEntries(Dictionary<string, string> dict)
    {
        dict["Settings_TabKillCounter"] = "Kill-Zähler";
        dict["KillCounter_CardTarget"] = "Ziel & Lauf-Ziel";
        dict["KillCounter_TargetName"] = "Ziel- / Bossname:";
        dict["KillCounter_QuickPresets"] = "Schnellvorlagen:";
        dict["KillCounter_PresetGiant"] = "Giant";
        dict["KillCounter_PresetManibus"] = "Manibus";
        dict["KillCounter_CurrentCount"] = "Aktuelle Kills / Läufe:";
        dict["KillCounter_EnableGoal"] = "Lauf- / Kill-Ziel aktivieren";
        dict["KillCounter_GoalCount"] = "Zielanzahl:";
        dict["KillCounter_TotalAllTime"] = "Kills insgesamt (Allzeit):";
        dict["KillCounter_ResetSession"] = "↺ Sitzung zurücksetzen";

        dict["KillCounter_CardActivation"] = "Aktivierung & Hotkeys";
        dict["KillCounter_Enable"] = "Farm- & Kill-Zähler aktivieren";
        dict["KillCounter_IncrementHotkey"] = "Kill erhöhen (+1):";
        dict["KillCounter_DecrementHotkey"] = "Verringern / Rückgängig (-1):";
        dict["KillCounter_HotkeyResetTooltip"] = "Auf Standard-Hotkey zurücksetzen";
        dict["KillCounter_PlaySound"] = "Akustische Bestätigung bei Kill / Rückgängig";
        dict["KillCounter_AutoStartBossTimer"] = "Boss-Respawn-Timer bei Kill automatisch starten";
        dict["KillCounter_ShowInMiniHud"] = "Zähler-Badge im Mini-HUD anzeigen";
        dict["KillCounter_FlashOnGoalReached"] = "HUD-Karte blinken & feiern bei Zielerreichung";
        dict["KillCounter_Color"] = "Akzentfarbe:";
        dict["KillCounter_ColorTooltip"] = "Zähler-Akzentfarbe aus Palette wählen";

        dict["KillCounter_Status_GoalReached"] = "ZIEL ERREICHT! 🎉";
        dict["KillCounter_GoalReachedTitle"] = "Kill-Ziel erreicht!";
        dict["KillCounter_GoalReachedMsg"] = "Glückwunsch! Du hast {0} Läufe für {1} abgeschlossen!";
        dict["Settings_HotkeyKillIncrement"] = "Kill-Zähler erhöhen (+1)";
        dict["Settings_HotkeyKillIncrementDesc"] = "Erhöht den aktiven Kill-Zähler um 1 und startet optional den Boss-Timer.";
        dict["Settings_HotkeyKillDecrement"] = "Kill-Zähler verringern / Rückgängig (-1)";
        dict["Settings_HotkeyKillDecrementDesc"] = "Macht die letzte Zählung bei versehentlichem Drücken rückgängig.";
        dict["KillCounter_BadgeKills"] = "KILLS";
        dict["KillCounter_BadgeGoal"] = "ZIEL";
        dict["KillCounter_ResetPrompt"] = "Aktuellen Sitzungs-Kill-Zähler auf 0 zurücksetzen? Gesamtkills bleiben erhalten.";
    }
}
