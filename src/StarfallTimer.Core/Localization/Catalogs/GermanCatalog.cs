// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StarfallTimer.Core.Localization.Catalogs;

internal static class GermanCatalog
{
    public static Dictionary<string, string> GetEntries() => new(StringComparer.OrdinalIgnoreCase)
    {
        ["App_Title"] = "Once Human — Event-Timer",
        ["Status_Waiting"] = "WARTEN",
        ["Status_StartingSoon"] = "BEGINNT BALD",
        ["Status_Active"] = "EVENT AKTIV",
        ["Status_EndingSoon"] = "ENDET BALD",
        ["HUD_UntilStart"] = "EVENT BEGINNT IN",
        ["HUD_UntilEnd"] = "EVENT ENDET IN",
        ["HUD_ClickThrough"] = "Durchklicken",
        ["HUD_Locked"] = "Gesperrt",
        ["HUD_Unlock"] = "Entsperren",
        ["HUD_Settings"] = "Einstellungen",
        ["HUD_Close"] = "HUD ausblenden",
        ["HUD_About"] = "Über",

        ["Notification_StartingSoon_Title"] = "Event-Benachrichtigung",
        ["Notification_StartingSoon_Body"] = "Event beginnt in {0} Minuten! Mach dich bereit.",
        ["Notification_Started_Title"] = "Event aktiv!",
        ["Notification_Started_Body"] = "Das Event ist jetzt in Once Human aktiv!",
        ["Notification_EndingSoon_Title"] = "Event endet bald",
        ["Notification_EndingSoon_Body"] = "Event endet in {0} Minuten. Beende das Einsammeln deiner Beute!",

        ["Settings_Title"] = "Once Human Event-Timer — Einstellungen",
        ["Settings_TabGeneral"] = "Allgemein",
        ["Settings_TabSchedule"] = "Zeitplan",
        ["Settings_TabEvents"] = "Events & Vorlagen",
        ["Settings_TabNotifications"] = "Warnungen & Audio",
        ["Settings_TabAppearance"] = "Erscheinungsbild",
        ["Settings_TabHotkeys"] = "Tastenkombinationen",
        ["Settings_TabAbout"] = "Über",

        ["Settings_ActiveEvent"] = "Aktives Event:",
        ["Settings_CycleMinutes"] = "Zyklusdauer (Minuten):",
        ["Settings_StartMinute"] = "Startminute des Zyklus (nach voller Stunde):",
        ["Settings_DurationMinutes"] = "Event-Dauer (Minuten):",
        ["Settings_TimeOffset"] = "Zeitversatz (Sekunden):",
        ["Settings_SyncToGame"] = "Mit Spielzeit synchronisieren",
        ["Settings_GameRemaining"] = "Spiel-Timer (mm:ss):",
        ["Settings_SyncButton"] = "Synchronisieren",
        ["Settings_SyncSuccess"] = "Mit Spielzeit synchronisiert! Versatz auf {0}s gesetzt.",
        ["Settings_AddEvent"] = "Neues Event hinzufügen",
        ["Settings_DeleteEvent"] = "Event löschen",
        ["Settings_EventName"] = "Event-Name:",
        ["Settings_EventColor"] = "Akzentfarbe:",
        ["Settings_EventDesc"] = "Beschreibung:",

        ["Settings_AlertBeforeStart"] = "Warnung vor Beginn (Minuten):",
        ["Settings_AlertBeforeEnd"] = "Warnung vor Ende (Minuten):",
        ["Settings_Volume"] = "Benachrichtigungslautstärke:",
        ["Settings_PlaySound"] = "Audiowarnungen aktivieren",
        ["Settings_PlayOnStart"] = "Sound abspielen, wenn Event beginnt",
        ["Settings_ShowToast"] = "Windows-Desktopbenachrichtigungen anzeigen",
        ["Settings_CustomSound"] = "Benutzerdefinierte Audiodatei (.wav):",
        ["Settings_Browse"] = "Durchsuchen...",
        ["Settings_TestAudio"] = "Audio testen",
        ["Settings_Opacity"] = "Overlay-Deckkraft:",
        ["Settings_Scale"] = "UI-Skalierung:",
        ["Settings_AlwaysOnTop"] = "Immer im Vordergrund",
        ["Settings_ClickThrough"] = "Durchklicken aktivieren (Klicks gehen an das Spiel)",
        ["Settings_LockPosition"] = "Overlay-Position sperren",
        ["Settings_CompactMode"] = "Kompakter Mini-HUD-Modus",
        ["Settings_Language"] = "Sprache:",
        ["Settings_ResetPosition"] = "HUD-Position oben links zurücksetzen",
        ["Settings_Save"] = "Speichern & Anwenden",
        ["Settings_SavedSuccess"] = "Einstellungen erfolgreich gespeichert!",

        ["Settings_HotkeyToggle"] = "Hotkey für Overlay-Sichtbarkeit:",
        ["Settings_HotkeyLock"] = "Hotkey für Durchklicken / Sperren:",

        ["Tray_OpenHUD"] = "Overlay anzeigen",
        ["Tray_HideHUD"] = "Overlay ausblenden",
        ["Tray_Settings"] = "Einstellungen...",
        ["Tray_ToggleLock"] = "Sperren / Entsperren",
        ["Tray_ToggleClickThrough"] = "Durchklicken umschalten",
        ["Tray_Exit"] = "Beenden",

        ["About_Title"] = "Über Once Human Event-Timer",
        ["About_AppName"] = "Once Human — Event-Timer",
        ["About_Version"] = "Version {0}",
        ["About_Desc"] = "Ein eleganter, latenzarmer HUD-Overlay-Timer für Once Human In-Game-Events wie Gravitationsabgrund (Sternenfall-Inversion), Frachtgerangel und benutzerdefinierte Zyklen.",
        ["About_Author"] = "Erstellt von Murr (https://github.com/vtstv) für die Once Human Community.",
        ["About_GitHub"] = "GitHub-Repository",
        ["About_License"] = "MIT-Lizenz"
    };
}
