# Once Human: Event Timer

[![CI](https://github.com/vtstv/OHTimer/actions/workflows/ci.yml/badge.svg)](https://github.com/vtstv/OHTimer/actions)
![.NET 8](https://img.shields.io/badge/.NET-8.0_LTS-512BD4?logo=dotnet&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows_10%20%2F%2011-0078D6?logo=windows&logoColor=white)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
![Version](https://img.shields.io/badge/Release-v1.5.0-00F0FF)

A sleek, lightweight desktop HUD overlay for **Once Human** that tracks recurring in-game events in real time. Never miss **Gravity Abyss**, **Skyripper**, **Cargo Scramble**, or global crate resets again!

---

## ✨ Features

- **🎯 Real-Time Event Tracking & Auto-Sorting**:
  - Track single or multiple events simultaneously.
  - Automatically sorts active events first, followed by events closest to starting.

- **⏱️ Second-Precision Zero-Drift Synchronization**:
  - Exact second-level cycle anchor: Gravity Abyss starts at **XX:22:29** with **0s default offset**, matching game servers out of the box.
  - One-click sync: enter your in-game timer (e.g. `08:23`) to calibrate immediately.
  - Quick nudge buttons (`-5s`, `-1s`, `+1s`, `+5s`, `0s`) for instant micro-adjustments.

- **🎨 Sleek Dark Cyberpunk HUD & Minimization Options**:
  - Frameless dark theme matching Once Human's aesthetic.
  - **Full HUD** with countdown rings and progress bars, or **Mini HUD** floating pill.
  - Full appearance customization: toggle outer borders, background transparency, progress rings, and footer times for a pure minimalist floating badge setup.
  - Option to collapse Mini-HUD controls and headers for maximum screen clarity.
  - Custom dark titlebar in Settings.

- **🌈 Full-Spectrum Native Color Picker**:
  - Interactive Color Picker button with live swatch and HEX display.
  - Full Windows palette dialog (`ChooseColor`) supporting custom color presets.

- **🔊 Procedural Synthesizer & Per-Event Audio**:
  - Built-in library of 7 synthesized procedural audio alerts (`Celestial Bell`, `Cosmic Pulse`, `Stardust Ping`, `Quantum Chord`, `Deep Resonance`, `Cyber Warning`, `Emergency Siren`).
  - Assign custom `.wav` sound files to individual events or globally.

- **🛡️ Auto-Hide on Alt-Tab & Click-Through Mode**:
  - Automatically hides the overlay when Once Human is not the active window, and reappears when switching back.
  - Mouse clicks pass straight through to the game (`WS_EX_TRANSPARENT`) so the overlay never disrupts combat or aiming.
  - Lock position (`🔒`) to prevent accidental dragging during gameplay.
  - Configurable Windows Autostart in System settings.

- **🚀 Built-in Auto-Updater**:
  - Checks GitHub releases for new versions with changelog viewer and seamless one-click update.

- **🌍 Multilingual**:
  - Full support for English (`en-US`), German (`de-DE`), and Russian (`ru-RU`).

---

## 📅 Supported Event Presets

| Event | Cycle | Start | Duration | Default Accent | Description |
| :--- | :---: | :---: | :---: | :---: | :--- |
| **Gravity Abyss** | 60 min | XX:22:29 | 30 min | `#00F0FF` (Cyan) | Hourly low-gravity tide with Stardust Crystals. |
| **Skyripper Anomaly** | 60 min | XX:37:29 | 20 min | `#8B5CF6` (Violet) | Aerial floating rift anomaly event. |
| **Cargo Scramble (Sproutlet Truck)** | 60 min | XX:00:00 | 20 min | `#F59E0B` (Amber) | PvP convoy escort and sproutlet gathering. |
| **Prime War (Nightmare Defense)** | 120 min | XX:00:00 | 30 min | `#DC2626` (Crimson) | Massive cooperative defense against Manibus. |
| **Public Crisis (Harrowing)** | 30 min | XX:00:00 | 15 min | `#06B6D4` (Teal) | Fast bi-hourly open-world crisis for weapon mods. |
| **Chiron the Centaur** | 60 min | XX:30:00 | 15 min | `#F97316` (Orange) | Centaur boss dynamic encounter in Red Sands. |
| **Extraction Workshop Defense** | 15 min | XX:00:00 | 10 min | `#14B8A6` (Jade) | Refinery wave defense for Stardust fuel. |
| **Blackfell Stronghold Loot Run** | 240 min | XX:00:00 | 15 min | `#6366F1` (Indigo) | 4-hour weapon and gear crate looting route. |
| **Lunar Oracle (Blood Moon)** | 60 min | XX:45:00 | 15 min | `#EF4444` (Coral) | Blood moon aberration anomaly event. |
| **Lunar Revelry (Lord of Moonlight)** | 60 min | XX:40:00 | 20 min | `#A855F7` (Purple) | Moonlight festival world gathering. |
| **Aberrant Progeny (Spore Tide)** | 60 min | XX:10:00 | 25 min | `#10B981` (Emerald) | Spore tide aberration defense wave. |
| **Dissonant Resonance** | 60 min | XX:15:00 | 30 min | `#EC4899` (Rose) | Dynamic soundwave anomaly zone. |
| **Crate Loot Reset (4-Hour)** | 240 min | XX:00:00 | 5 min | `#3B82F6` (Blue) | Global mystical crate refresh window. |
| **Custom Wheel Event** | Custom | Custom | Custom | `#EAB308` (Gold) | User-defined cyclic interval and duration. |

---

## 🎮 How to Use

1. **Download**: Grab the latest `OnceHumanTimer-v1.3.0.zip` from [Releases](https://github.com/vtstv/OHTimer/releases).
2. **Launch**: Extract the folder and run `OnceHumanEventTimer.exe`.
3. **Move & Lock**: Drag the HUD overlay anywhere on your screen. Click `🔓` to lock it in place.
4. **Settings (`⚙`)**: Click the gear icon or right-click the system tray icon to enable multiple timers, pick accent colors, or configure audio alerts.
5. **Sync with Game**: In Settings under **Events & Presets**, type the remaining time shown in your game and click **Sync**.

---

## ⌨️ Default Hotkeys

| Shortcut | Action |
| :--- | :--- |
| <kbd>Ctrl</kbd> + <kbd>Alt</kbd> + <kbd>S</kbd> | Toggle HUD overlay visibility |
| <kbd>Ctrl</kbd> + <kbd>Alt</kbd> + <kbd>L</kbd> | Toggle click-through mode & position lock |

---

## 📦 Requirements

- Windows 10 (version 19041+) or Windows 11
- [.NET Desktop Runtime 8.0](https://dotnet.microsoft.com/download/dotnet/8.0/runtime) (if using framework-dependent build)

---

## 👤 Author & License

- **Author**: **Murr** ([https://github.com/vtstv](https://github.com/vtstv))
- **Repository**: [https://github.com/vtstv/OHTimer](https://github.com/vtstv/OHTimer)
- **License**: [MIT License](LICENSE)

*Disclaimer: Once Human is a registered trademark of Starry Studio / NetEase Games. This utility is an unofficial companion tool.*
