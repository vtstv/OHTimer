// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Localization.Catalogs;

internal static partial class EnglishCatalog
{
    static partial void RegisterOnboardingEntries(Dictionary<string, string> dict)
    {
        dict["Onboarding_WindowTitle"] = "Quick Start Guide — Once Human: Event Timer";
        dict["Onboarding_Header"] = "INTERACTIVE ONBOARDING GUIDE";
        dict["Onboarding_StepIndicator"] = "Step {0} of {1}";
        dict["Onboarding_Skip"] = "Skip Tutorial";
        dict["Onboarding_Back"] = "Back";
        dict["Onboarding_Next"] = "Next Step";
        dict["Onboarding_Finish"] = "Finish & Start Playing";

        dict["Onboarding_Step1_Title"] = "Frameless HUD Overlay & Controls";
        dict["Onboarding_Step1_Subtitle"] = "Interactive floating overlay with zero FPS loss";
        dict["Onboarding_Step1_Desc"] = "Once Human: Event Timer runs directly on top of the game with zero frame drops (hardware DirectFlip/MPO). Drag the overlay to any position when unlocked. Use the 🗖 button to toggle between full HUD and compact Mini-HUD.";
        dict["Onboarding_Step1_Tip"] = "Toggle Click-Through (Ctrl+Alt+L) to let mouse clicks pass directly into Once Human without activating the overlay.";
        dict["Onboarding_Step2_Title"] = "Multi-Timer & Priority Presets";
        dict["Onboarding_Step2_Subtitle"] = "Select events to track directly on your HUD overlay";
        dict["Onboarding_Step2_Desc"] = "Select which events to track on your HUD overlay. By default, S5: The Echo is enabled. Mark any additional events you want to monitor:\n• S5: The Echo (Visional Wheel Season 5)\n• Cargo Scramble (Sproutlet Truck)\n• Drift Greenhouse (Manual Sync)\nEnable multiple checkboxes to track active events simultaneously in Multi-Timer mode!";
        dict["Onboarding_Step2_Tip"] = "Use the 🔝, ▲, ▼ buttons in Settings to reorder presets, or click ⧉ Duplicate to create custom variants.";
        dict["Onboarding_Step3_Title"] = "Season 5: The Echo & Boss Portal";
        dict["Onboarding_Step3_Subtitle"] = "Hourly cycle at XX:15:00 with 15-minute boss portal alert";
        dict["Onboarding_Step3_Desc"] = "The Echo event descends hourly at XX:15:00. Exactly 15 minutes after start (XX:30:00), the 4-player 'Echo War' boss dungeon portal opens!\nTurn on 'Countdown to Boss Portal only' in event settings to track time directly until the boss opens, with a dedicated Emergency Siren alert at the 15th minute.";
        dict["Onboarding_Step3_Tip"] = "You can silence the event start chime and only receive the boss portal alert on the 15th minute.";
        dict["Onboarding_Step4_Title"] = "Zero-Drift Clock Calibration";
        dict["Onboarding_Step4_Subtitle"] = "Perfect synchronization with your Once Human server";
        dict["Onboarding_Step4_Desc"] = "Server clocks may drift by several seconds from your PC clock. Simply open your in-game map or event prompt, enter the countdown you see (e.g. '1h 3m 29s' or '01:03:29') into 'Calibrate Start from In-Game Timer', and press Calibrate. The timer instantaneously calculates exact cycle anchors and server skew!";
        dict["Onboarding_Step4_Tip"] = "Supports natural language inputs in English, German, and Russian ('1h 3m 29s', '1ч 3м 29с', '1 Std 3 Min 29 Sek').";
        dict["Onboarding_Step5_Title"] = "Global Hotkeys & System Tray";
        dict["Onboarding_Step5_Subtitle"] = "Hands-free in-game control and silent background tray";
        dict["Onboarding_Step5_Desc"] = "Control your overlay during intense gameplay with global hotkeys:\n• Ctrl+Alt+S — Toggle overlay visibility (hide/show).\n• Ctrl+Alt+L — Lock position & toggle click-through (mouse clicks pass directly to the game).\n• Ctrl+Alt+P — Temporarily pause/resume sound alerts (indicated by 🔕).\nRuns silently in the Windows system tray with < 0.1% CPU usage.";
        dict["Onboarding_Step5_Tip"] = "You can customize any hotkey combinations in the 'Hotkeys' tab, and re-launch this guide anytime from the 'About' tab.";
        dict["Settings_TutorialTitle"] = "Interactive Quick Start Guide";
        dict["Settings_TutorialDesc"] = "Re-launch the step-by-step tutorial explaining overlay controls, hotkeys, Season 5 boss timer, and server calibration.";
        dict["Settings_LaunchTutorial"] = "Launch Guide";
        dict["Onboarding_SelectEvents"] = "Select Events to Track";
        dict["Onboarding_MultiTimerHint"] = "✓ HUD displays all checked events when active or approaching";
    }
}
