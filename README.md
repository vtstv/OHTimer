# Once Human: Event Timer

[![CI](https://github.com/vtstv/OHTimer/actions/workflows/release.yml/badge.svg)](https://github.com/vtstv/OHTimer/actions)
![.NET 8](https://img.shields.io/badge/.NET-8.0_LTS-512BD4?logo=dotnet&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows_10%20%2F%2011-0078D6?logo=windows&logoColor=white)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
![Version](https://img.shields.io/badge/Release-v1.5.0-00F0FF)

A modern, high-performance desktop HUD overlay for **Once Human** that tracks recurring in-game events in real time. Designed with a dark cyberpunk aesthetic, sub-second server clock synchronization, customizable sound alerts, and zero-impact click-through gameplay support.

---

## Screenshots

<p align="center">
  <img src="docs/images/settings-events.jpg" alt="Event Management, Preset Library, and Server Time Calibration" width="760"/>
  <br/>
  <em>Universal event scheduling engine with 14 built-in Once Human presets, native full-spectrum color picker, and zero-drift server sync.</em>
</p>

<p align="center">
  <img src="docs/images/hud-appearance.jpg" alt="Appearance and HUD Customization" width="760"/>
  <br/>
  <em>Granular appearance settings for transparency, scale, borders, circular progress rings, and Mini-HUD header controls.</em>
</p>

<p align="center">
  <img src="docs/images/settings-system.png" alt="System Settings and Foreground Focus Detection" width="760"/>
  <br/>
  <em>System settings with Windows Autostart integration and automatic game foreground window detection.</em>
</p>

<p align="center">
  <img src="docs/images/settings-audio.png" alt="Procedural Audio Alerts and Synthesizer" width="760"/>
  <br/>
  <em>Procedural synthesizer audio presets, volume controls, and per-event sound assignment.</em>
</p>

---

## Features

- **Real-Time Event Tracking & Auto-Sorting**:
  - Track single or multiple concurrent event timers simultaneously on a clean desktop HUD.
  - Dynamic event auto-sorting prioritizes active events first, followed by events closest to starting.
  - Switch between Full HUD (circular progress rings, countdown bars) and Mini-HUD (minimal floating badge).

- **Second-Precision Zero-Drift Synchronization**:
  - Sub-second server cycle anchor: Gravity Abyss triggers at exact server second `XX:22:29` with `0s` offset, providing zero-drift accuracy out of the box.
  - Dynamic game clock synchronization: enter in-game remaining time (e.g. `08:23`) to calibrate clock skew immediately.
  - Quick-calibration nudge buttons (`-5s`, `-1s`, `+1s`, `+5s`, `0s`) for instant adjustment.

- **Game Focus Detection (Smart Auto-Hide)**:
  - Automatically conceals the overlay when *Once Human* is not the active window (e.g., when Alt-Tabbing to a web browser or another desktop app).
  - Instantly restores the overlay when returning to the game.

- **Click-Through Mode & Overlay Customization**:
  - Full click-through support (`WS_EX_TRANSPARENT`): mouse clicks pass directly into the game without interrupting aiming or combat.
  - Position lock to prevent accidental dragging during intense encounters.
  - Granular visual controls: toggle outer border glow, card background transparency, progress ring, next event footer, and Mini-HUD header controls.

- **Full-Spectrum Native Color Picker**:
  - Native Windows color palette dialog (`ChooseColor`) integration.
  - Assign any custom RGB/HEX accent color to individual events with live preview swatches.

- **Procedural Synthesizer & Per-Event Audio**:
  - Built-in audio library featuring 7 procedural synthesized alert presets (`Celestial Bell`, `Cosmic Pulse`, `Stardust Ping`, `Quantum Chord`, `Deep Resonance`, `Cyber Warning`, `Emergency Siren`).
  - Support for custom user `.wav` audio files per individual event or as global defaults.
  - Built-in instant preview audio test buttons.

- **System Integration & Automatic Updates**:
  - Optional Windows Autostart (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`) configured via the dedicated System settings tab.
  - Built-in GitHub update checker with changelog viewer and seamless in-place executable replacement.

- **Multilingual Support**:
  - Full native localization in English (`en-US`), German (`de-DE`), and Russian (`ru-RU`).

---

## Supported Event Presets

All built-in presets automatically populate in the event manager and can be enabled, adjusted, or restored at any time:

| Event | Cycle | Start | Duration | Default Color | Description |
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
| **Custom Wheel Event** | Custom | Custom | Custom | `#EAB308` (Gold) | User-defined cyclic interval, start time, and duration. |

---

## Distribution Formats

Releases are available in three distinct packages on the [GitHub Releases](https://github.com/vtstv/OHTimer/releases) page:

| Package | Filename | Size | Requirements |
| :--- | :--- | :---: | :--- |
| **Portable (Lightweight)** | `OnceHumanTimer-v1.5.0-portable.zip` | ~0.38 MB | Requires [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0). Ultra-compact single file. |
| **Standalone (Plug & Play)** | `OnceHumanTimer-v1.5.0-standalone.zip` | ~63 MB | Zero dependencies. Fully self-contained single executable. |
| **Windows Setup Installer** | `OnceHumanTimer-v1.5.0-Setup.exe` | ~64 MB | Inno Setup wizard with Start Menu / Desktop shortcuts and Windows uninstaller. |

---

## Keyboard Shortcuts

| Shortcut | Action |
| :--- | :--- |
| <kbd>Ctrl</kbd> + <kbd>Alt</kbd> + <kbd>S</kbd> | Toggle HUD overlay visibility |
| <kbd>Ctrl</kbd> + <kbd>Alt</kbd> + <kbd>L</kbd> | Toggle click-through mode and position lock |

---

## Installation & Usage

1. **Download**: Obtain your preferred release format from [GitHub Releases](https://github.com/vtstv/OHTimer/releases).
2. **Run**:
   - For portable editions, extract the ZIP file and run `OnceHumanEventTimer.exe`.
   - For the installer, run `OnceHumanTimer-v1.5.0-Setup.exe` and follow the setup wizard.
3. **Positioning**: Drag the HUD overlay to your preferred screen location. Use the lock button on the overlay or context menu to lock the position.
4. **Configuration**: Click the gear icon or right-click the system tray icon to access Settings. Select which events to track, customize colors, configure sound alerts, or calibrate time.
5. **Game Synchronization**: Under **Events & Presets**, compare the in-game event timer with the HUD. Enter the minutes and seconds remaining in-game and click **Sync** to calibrate.

---

## System Requirements

- **Operating System**: Windows 10 (64-bit, version 19041+) or Windows 11
- **Runtime**: [.NET Desktop Runtime 8.0](https://dotnet.microsoft.com/download/dotnet/8.0/runtime) (only for the lightweight portable package; standalone and installer packages bundle the runtime)

---

## Author & License

Developed and maintained by **Murr** ([GitHub](https://github.com/vtstv)).

Released under the [MIT License](LICENSE).
