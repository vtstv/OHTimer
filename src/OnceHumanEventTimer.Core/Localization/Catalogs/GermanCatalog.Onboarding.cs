// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Localization.Catalogs;

internal static partial class GermanCatalog
{
    static partial void RegisterOnboardingEntries(Dictionary<string, string> dict)
    {
        dict["Onboarding_WindowTitle"] = "Schnellstart-Anleitung — Once Human: Event Timer";
        dict["Onboarding_Header"] = "INTERAKTIVE ANLEITUNG";
        dict["Onboarding_StepIndicator"] = "Schritt {0} von {1}";
        dict["Onboarding_Skip"] = "Überspringen";
        dict["Onboarding_Back"] = "Zurück";
        dict["Onboarding_Next"] = "Weiter";
        dict["Onboarding_Finish"] = "Fertigstellen & Loslegen";

        dict["Onboarding_Step1_Title"] = "Rahmenloses HUD-Overlay & Steuerung";
        dict["Onboarding_Step1_Subtitle"] = "Interaktives Desktop-Widget ohne FPS-Verlust";
        dict["Onboarding_Step1_Desc"] = "Once Human: Event Timer läuft direkt über dem Spiel ohne FPS-Verlust (Hardware DirectFlip/MPO). Ziehen Sie das Overlay im entsperrten Zustand an eine beliebige Stelle. Verwenden Sie die 🗖-Schaltfläche zum Wechseln zwischen Voll-HUD und kompaktem Mini-HUD.";
        dict["Onboarding_Step1_Tip"] = "Aktivieren Sie Durchklicken (Strg+Alt+L), damit Mausklicks direkt ins Spiel durchgehen, ohne das Overlay zu aktivieren.";
        dict["Onboarding_Step2_Title"] = "Multi-Timer & Prioritäts-Presets";
        dict["Onboarding_Step2_Subtitle"] = "Wählen Sie Events direkt für das HUD-Overlay aus";
        dict["Onboarding_Step2_Desc"] = "Wählen Sie, welche Events auf dem HUD-Overlay angezeigt werden sollen. Standardmäßig ist S5: Das Echo aktiviert. Markieren Sie weitere gewünschte Events:\n• S5: Das Echo (Dissonante Resonanz)\n• Frachtgerangel (Spross-LKW)\n• Driften-Gewächshaus (Manueller Sync)\nAktivieren Sie mehrere Kontrollkästchen für den gleichzeitigen Multi-Timer-Modus!";
        dict["Onboarding_Step2_Tip"] = "Nutzen Sie 🔝, ▲, ▼ in den Einstellungen zum Umsortieren oder klicken Sie auf ⧉ Duplizieren für Varianten.";
        dict["Onboarding_Step3_Title"] = "Saison 5: Das Echo & Boss-Portal";
        dict["Onboarding_Step3_Subtitle"] = "Stündlich ab XX:15:00 mit Boss-Portal-Alarm nach 15 Minuten";
        dict["Onboarding_Step3_Desc"] = "Das Echo-Event startet stündlich um XX:15:00. Exakt 15 Minuten nach Beginn (XX:30:00) öffnet sich das 4-Spieler-Portal zu 'The Echo War'!\nAktivieren Sie 'Nur Countdown zum Boss-Portal', um direkt die Zeit bis zum Boss zu sehen — mit Sirenen-Alarm in Minute 15.";
        dict["Onboarding_Step3_Tip"] = "Der Start-Gong kann stummgeschaltet werden, sodass nur der Boss-Alarm auf Minute 15 ertönt.";
        dict["Onboarding_Step4_Title"] = "Null-Drift-Uhr-Kalibrierung";
        dict["Onboarding_Step4_Subtitle"] = "Perfekte Synchronisation mit dem Once Human Server";
        dict["Onboarding_Step4_Desc"] = "Server-Uhren können leicht von Ihrer PC-Zeit abweichen. Schauen Sie einfach auf den Countdown auf Ihrer Karte (z. B. '1h 3m 29s' oder '01:03:29'), geben Sie ihn ein und klicken Sie auf Kalibrieren. Der Timer berechnet sofort den Zyklusanker und gleicht Drift aus!";
        dict["Onboarding_Step4_Tip"] = "Unterstützt Eingaben auf Deutsch, Englisch und Russisch ('1 Std 3 Min 29 Sek', '1h 3m 29s', '1ч 3м 29с').";
        dict["Onboarding_Step5_Title"] = "Globale Hotkeys & System-Tray";
        dict["Onboarding_Step5_Subtitle"] = "Steuerung während des Spiels und Tray im Hintergrund";
        dict["Onboarding_Step5_Desc"] = "Steuern Sie das Overlay mitten im Kampf per Tastenkombination:\n• Strg+Alt+S — Overlay ein-/ausblenden.\n• Strg+Alt+L — Position sperren & Durchklicken umschalten (Klicks gehen ins Spiel).\n• Strg+Alt+P — Audio-Warnungen pausieren/fortsetzen (Anzeige: 🔕).\nLäuft unauffällig im Windows-Infobereich mit < 0,1% CPU-Auslastung.";
        dict["Onboarding_Step5_Tip"] = "Tastenkombinationen können im Reiter 'Hotkeys' angepasst werden. Diese Anleitung ist jederzeit unter 'Über' aufrufbar.";
        dict["Settings_TutorialTitle"] = "Interaktive Schnellstart-Anleitung";
        dict["Settings_TutorialDesc"] = "Schritt-für-Schritt-Anleitung zu Overlay-Steuerung, Hotkeys, S5-Boss-Timer und Server-Kalibrierung erneut starten.";
        dict["Settings_LaunchTutorial"] = "Anleitung starten";
        dict["Onboarding_SelectEvents"] = "Zu überwachende Events wählen";
        dict["Onboarding_MultiTimerHint"] = "✓ HUD zeigt alle markierten Events an, wenn sie aktiv oder bevorstehend sind";
    }
}
