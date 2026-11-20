// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Localization.Catalogs;

internal static partial class EnglishCatalog
{
    static partial void RegisterBossTimerEntries(Dictionary<string, string> dict)
    {
        dict["Settings_TabBossTimer"] = "Boss Timer";
        dict["BossTimer_CardActivation"] = "Activation & Hotkey";
        dict["BossTimer_Enable"] = "Enable Boss Respawn Timer";
        dict["BossTimer_Hotkey"] = "Trigger Hotkey:";
        dict["BossTimer_HotkeyRecorderPrompt"] = "[ Press a key... ]";
        dict["BossTimer_HotkeyResetTooltip"] = "Reset to default hotkey (NumPad 1)";
        dict["BossTimer_RepressBehavior"] = "On Hotkey Re-press:";
        dict["BossTimer_Repress_Restart"] = "Restart countdown (Default)";
        dict["BossTimer_Repress_Toggle"] = "Toggle Start / Stop";
        dict["BossTimer_Repress_Stop"] = "Stop countdown";
        dict["BossTimer_PlayHotkeySound"] = "Sound confirmation on hotkey press";

        dict["BossTimer_CardDuration"] = "Duration & Target Boss";
        dict["BossTimer_BossName"] = "Target / Boss Name:";
        dict["BossTimer_DurationMinutes"] = "Minutes:";
        dict["BossTimer_DurationSeconds"] = "Seconds:";
        dict["BossTimer_QuickPresets"] = "Quick Presets:";

        dict["BossTimer_CardAudio"] = "Audio & Repeat Alerts";
        dict["BossTimer_WarningAlert"] = "Warning alert before spawn";
        dict["BossTimer_WarningSeconds"] = "Seconds ahead:";
        dict["BossTimer_SpawnAlert"] = "Alert at 00:00 (Spawn)";
        dict["BossTimer_SoundPreset"] = "Spawn Sound Preset:";
        dict["BossTimer_Volume"] = "Alert Volume:";
        dict["BossTimer_TestSound"] = "Test";
        dict["BossTimer_EnableRepeatAlert"] = "Repeat alert after spawn";
        dict["BossTimer_RepeatCount"] = "Repeat count:";
        dict["BossTimer_RepeatInterval"] = "Interval (sec):";

        dict["BossTimer_CardVisuals"] = "Visuals & Appearance";
        dict["BossTimer_ShowToast"] = "Show Windows Toast notification";
        dict["BossTimer_FlashCard"] = "Flash HUD card on spawn";
        dict["BossTimer_AutoHide"] = "Auto-hide delay after spawn (sec):";
        dict["BossTimer_Color"] = "Accent Color:";
        dict["BossTimer_ColorTooltip"] = "Choose boss timer color from palette";

        dict["BossTimer_Status_Respawn"] = "RESPAWN";
        dict["BossTimer_Status_Warning"] = "APPROACHING";
        dict["BossTimer_Status_Spawned"] = "SPAWNED!";
        dict["BossTimer_SpawnToastTitle"] = "Boss Respawned!";
        dict["BossTimer_SpawnToastMsg"] = "has respawned! Get ready for combat.";
    }
}
