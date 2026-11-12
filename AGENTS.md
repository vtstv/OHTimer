# AGENTS.md — Development & Architecture Guidelines

This file provides architectural rules, coding standards, and operational guidelines for autonomous agents and human developers contributing to **Once Human: Starfall Inversion Timer**.

---

## 1. Project Overview

**Once Human: Starfall Inversion Timer** is a modern, lightweight Windows desktop HUD overlay designed for the game *Once Human*.
It tracks the recurring **Starfall Inversion** in-game event:
- **Cycle recurrence**: Hourly schedule (every 60 minutes).
- **Default Start Time**: 22 minutes past the hour (`XX:22:00`).
- **Default End Time**: 52 minutes past the hour (`XX:52:00`).
- **Active Duration**: 30 minutes.
- **Phase States**:
  - `Inactive` (waiting for the next event to begin).
  - `StartingSoon` (within configured threshold, e.g., 2 minutes before XX:22).
  - `Active` (event is in progress, low-gravity tide active).
  - `EndingSoon` (within threshold before XX:52).

---

## 2. Core Architectural Principles

The solution follows clean architectural separation across 4 distinct projects:

```
StarfallTimer.sln
├── src/
│   ├── StarfallTimer.Core            # Pure domain models, business logic & interfaces (zero UI dependencies)
│   ├── StarfallTimer.Infrastructure  # Win32 P/Invoke, Audio synthesis, File persistence, OS Toasts
│   └── StarfallTimer.UI              # WPF (MVVM), XAML Styles, Controls, Windows, Tray
└── tests/
    └── StarfallTimer.Tests           # xUnit Unit tests for scheduler & core models
```

### Key Rules:
1. **Zero Monoliths**: No class file should exceed ~250 lines. Decompose UI controls, models, and services into dedicated single-responsibility files.
2. **UI Independence of Core**: `StarfallTimer.Core` MUST NOT reference `System.Windows`, WPF, or any Windows-specific API. It must compile and run on pure `.NET 8`.
3. **Language & Localization Policy**:
   - **All source code, identifiers, comments, commits, and documentation MUST BE in English only.**
   - Default UI language is English (`en-US`).
   - Localization strings are stored in JSON dictionaries (`en-US.json`, `ru-RU.json`, `zh-CN.json`).
   - New strings must be added to all locale files (with English fallback if translation is pending).
4. **Dependency Injection**: Services are registered and resolved via `Microsoft.Extensions.DependencyInjection`. Avoid static singletons.

---

## 3. Technology & Coding Standards

- **Target Framework**: .NET 8 LTS (`net8.0` for Core/Tests, `net8.0-windows` for Infrastructure/UI).
- **C# Language Version**: C# 12 with:
  - File-scoped namespaces (`namespace StarfallTimer.Core.Models;`).
  - Strict Nullable Reference Types (`<Nullable>enable</Nullable>`). No unhandled null warnings.
  - Primary constructors where concise and readable.
  - Pattern matching and records for immutable snapshots.
- **MVVM Framework**: `CommunityToolkit.Mvvm`. Use source generators (`[ObservableProperty]`, `[RelayCommand]`).
- **P/Invoke & Win32 Safety**:
  - Always clean up registered hotkeys (`UnregisterHotKey`) on application exit or window closing.
  - Win32 calls must reside exclusively in `StarfallTimer.Infrastructure.Win32`.
  - Use `IntPtr` / `nint` correctly for 64-bit Windows compatibility.

---

## 4. UI/UX & Overlay Guidelines

- **Frameless Overlay Window**:
  - `WindowStyle="None"`, `AllowsTransparency="True"`, `Background="Transparent"`.
  - `Topmost="True"` with support to toggle through settings.
  - **Click-Through Mode**: Implemented via `WS_EX_TRANSPARENT` and `WS_EX_LAYERED`. When enabled, mouse clicks pass directly through to the game.
  - Position lock prevents accidental dragging during gameplay.
- **Aesthetic**:
  - Sleek dark cyberpunk / Once Human Stardust aesthetic.
  - Deep obsidian transparent card, neon cyan (`#00F0FF`), cosmic purple (`#A855F7`), amber warning (`#F59E0B`), and active gold (`#EAB308`).
  - Smooth visual animations with low CPU/GPU footprint.

---

## 5. Performance & Threading Rules

- The scheduler ticks every 1 second via a high-resolution timer.
- Zero heap allocations inside the 1-second tick loop where possible (reuse structs/records).
- Background operations (sound playback, disk I/O) MUST NOT block the UI thread.
- Any property updates bound to the UI must dispatch on the UI thread (`Application.Current.Dispatcher`).

---

## 6. Testing Requirements

- Any calculation regarding event time, phase transition, remaining duration, and alert triggers must be covered by xUnit tests in `StarfallTimer.Tests`.
- Edge cases to test:
  - Time boundary at `XX:21:59` to `XX:22:00`.
  - Time boundary at `XX:51:59` to `XX:52:00`.
  - Hour rollover from `XX:59:59` to `(XX+1):00:00`.
  - Custom event minute offset configurations.
