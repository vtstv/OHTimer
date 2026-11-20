// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Localization.Catalogs;

internal static partial class GermanCatalog
{
    static partial void RegisterBossTimerEntries(Dictionary<string, string> dict)
    {
        dict["Settings_TabBossTimer"] = "Boss-Timer";
        dict["BossTimer_CardActivation"] = "Aktivierung & Hotkey";
        dict["BossTimer_Enable"] = "Boss-Respawn-Timer aktivieren";
        dict["BossTimer_Hotkey"] = "Auslöser-Hotkey:";
        dict["BossTimer_HotkeyRecorderPrompt"] = "[ Taste drücken... ]";
        dict["BossTimer_HotkeyResetTooltip"] = "Auf Standard-Hotkey zurücksetzen (NumPad 1)";
        dict["BossTimer_RepressBehavior"] = "Beim erneuten Drücken:";
        dict["BossTimer_Repress_Restart"] = "Countdown neu starten";
        dict["BossTimer_Repress_Toggle"] = "Start / Stopp umschalten";
        dict["BossTimer_Repress_Stop"] = "Countdown stoppen";
        dict["BossTimer_PlayHotkeySound"] = "Tonsignal bei Tastendruck";

        dict["BossTimer_CardDuration"] = "Dauer & Ziel-Boss";
        dict["BossTimer_BossName"] = "Ziel- / Boss-Name:";
        dict["BossTimer_DurationMinutes"] = "Minuten:";
        dict["BossTimer_DurationSeconds"] = "Sekunden:";
        dict["BossTimer_QuickPresets"] = "Schnellvorlagen:";
        dict["BossTimer_StartAction"] = "▶ Countdown starten";
        dict["BossTimer_StopAction"] = "⏹ Countdown stoppen";

        dict["BossTimer_CardAudio"] = "Audio- & Wiederholungsalarme";
        dict["BossTimer_WarningAlert"] = "Warnalarm vor dem Erscheinen";
        dict["BossTimer_WarningSeconds"] = "Vorher (Sek.):";
        dict["BossTimer_SpawnAlert"] = "Alarm bei 00:00 (Erschienen)";
        dict["BossTimer_SoundPreset"] = "Respawn-Soundvorlage:";
        dict["BossTimer_Volume"] = "Alarmlautstärke:";
        dict["BossTimer_TestSound"] = "Testen";
        dict["BossTimer_EnableRepeatAlert"] = "Alarm nach dem Erscheinen wiederholen";
        dict["BossTimer_RepeatSettings"] = "Wiederholung:";
        dict["BossTimer_RepeatCount"] = "Wiederholungen:";
        dict["BossTimer_RepeatInterval"] = "Intervall (Sek.):";

        dict["BossTimer_CardVisuals"] = "Visuelles & Aussehen";
        dict["BossTimer_ShowToast"] = "Windows-Benachrichtigung anzeigen";
        dict["BossTimer_FlashCard"] = "HUD-Karte beim Erscheinen aufblinken lassen";
        dict["BossTimer_AutoHide"] = "Automatisches Ausblenden nach (Sek.):";
        dict["BossTimer_Color"] = "Akzentfarbe:";
        dict["BossTimer_ColorTooltip"] = "Wählen Sie eine Boss-Timer-Farbe aus der Palette";

        dict["BossTimer_Status_Respawn"] = "RESPAWN";
        dict["BossTimer_Status_Warning"] = "NÄHERT SICH";
        dict["BossTimer_Status_Spawned"] = "ERSCHIENEN!";
        dict["BossTimer_SpawnToastTitle"] = "Boss erschienen!";
        dict["BossTimer_SpawnToastMsg"] = "ist erschienen! Bereit machen zum Kampf.";
    }
}
