# AGENTS.md — Development & Architecture Guidelines

This file provides architectural rules, coding standards, and operational guidelines for autonomous agents and human developers contributing to **Once Human: Event Timer** (formerly Starfall Inversion Timer).

---

## 1. Project Overview

**Once Human: Event Timer** is a modern, lightweight Windows desktop HUD overlay designed for the game *Once Human*.
It tracks recurring in-game events in real-time with zero-drift game time synchronization:
- **Universal Event Scheduling Engine**:
  - Supports multiple concurrent or selectable events via `EventDefinition` models.
  - Built-in presets:
    - **Gravity Abyss (Starfall Inversion)**: Hourly schedule (Cycle: 60m, Start: XX:22:00, Duration: 30m).
    - **Cargo Scramble**: Hourly schedule (Cycle: 60m, Start: XX:00:00, Duration: 20m).
    - **Custom Wheel Events**: Configurable cycle minutes (e.g., 15m, 45m, 120m), start minute, duration, and neon accent colors.
- **Phase States**:
  - `Inactive` (waiting for the next event to begin).
  - `StartingSoon` (within configured alert threshold, e.g., 2 minutes before start).
  - `Active` (event is in progress).
  - `EndingSoon` (within warning threshold before conclusion).
- **Game Time Synchronization (Zero Drift)**:
  - Compensates for clock skew between local PC system time and Once Human game servers via `TimeOffsetSeconds`.
  - Dynamic `SyncToGameTime(TimeSpan remainingTime)` computes and applies exact offsets instantaneously.
- **Author & Copyright**:
  - Developed and maintained by **Murr** ([https://github.com/vtstv](https://github.com/vtstv)).

---

## 2. Core Architectural Principles

The solution follows clean architectural separation across 4 distinct projects:

```
StarfallTimer.sln
├── src/
│   ├── StarfallTimer.Core            # Pure domain models, scheduling logic & localization (zero UI dependencies)
│   ├── StarfallTimer.Infrastructure  # Win32 P/Invoke, Audio synthesis, File persistence, OS Toasts
│   └── StarfallTimer.UI              # WPF (MVVM), XAML Styles, Controls, Windows, Tray
└── tests/
    └── StarfallTimer.Tests           # xUnit Unit tests for scheduler, sync & localization
```

### Key Rules:
1. **Zero Monoliths**: No class file should exceed ~250 lines. Decompose UI controls, models, viewmodels, and catalogs into dedicated single-responsibility files.
2. **UI Independence of Core**: `StarfallTimer.Core` MUST NOT reference `System.Windows`, WPF, or any Windows-specific API. It must compile and run on pure `.NET 8`.
3. **Language & Localization Policy**:
   - **All source code, identifiers, comments, commits, and documentation MUST BE in English only.**
   - Default UI language is English (`en-US`).
   - Supported languages:
     - 🇺🇸 English (`en-US`) — Default
     - 🇩🇪 German (`de-DE`)
     - 🇷🇺 Russian (`ru-RU`)
   - Catalogs are implemented in `StarfallTimer.Core.Localization.Catalogs` with strict key parity and English fallback.
4. **Dependency Injection**: Services are registered and resolved via `Microsoft.Extensions.DependencyInjection`. Avoid static singletons.

---

## 3. Technology & Coding Standards

- **Target Framework**: .NET 8 LTS (`net8.0` for Core/Tests, `net8.0-windows` for Infrastructure/UI).
- **C# Language Version**: C# 12 with:
  - File-scoped namespaces (`namespace StarfallTimer.Core.Models;`).
  - Strict Nullable Reference Types (`<Nullable>enable</Nullable>`). No unhandled null warnings.
  - Primary constructors where concise and readable.
  - Pattern matching and records for immutable snapshots (`EventSnapshot`).
- **MVVM Framework**: `CommunityToolkit.Mvvm`. Use source generators (`[ObservableProperty]`, `[RelayCommand]`). Partial classes for ViewModel decomposition.
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
  - Deep obsidian transparent card (`#0B0F17`), neon cyan (`#00F0FF`), cosmic purple (`#A855F7`), amber warning (`#F59E0B`), and active gold (`#EAB308`).
  - Dark custom ControlTemplates for all controls (TabControl, TabItem, ListBox, TextBox, Buttons). Standard white Windows controls are strictly prohibited.
  - Smooth visual animations with low CPU/GPU footprint (< 0.1% CPU).

---

## 5. Performance & Threading Rules

- The scheduler ticks every 1 second via a high-resolution timer.
- Zero heap allocations inside the 1-second tick loop where possible (reuse structs/records).
- Background operations (sound playback, disk I/O) MUST NOT block the UI thread.
- Any property updates bound to the UI must dispatch on the UI thread (`Application.Current.Dispatcher`).

---

## 6. Packaging & File Size Optimization

- Default releases are published as **Framework-Dependent single-file** executables (`--no-self-contained`).
- Package size must remain ultra-lightweight (< 5 MB, typically ~0.7 MB).
- Scripts:
  - `scripts/build-release.ps1`: Automated test execution, publishing, license bundling, and ZIP generation.
  - `scripts/build-release.cmd`: One-click Windows release launcher.

---

## 7. Testing Requirements

- Any calculation regarding event time, phase transition, remaining duration, arbitrary cycle minutes, drift offsets, and alert triggers must be covered by xUnit tests in `StarfallTimer.Tests`.
- Edge cases to test:
  - Time boundary at event start (`XX:21:59` to `XX:22:00`).
  - Time boundary at event end (`XX:51:59` to `XX:52:00`).
  - Hour rollover from `XX:59:59` to `(XX+1):00:00`.
  - Arbitrary cycle periods (e.g. 15m, 45m, 120m).
  - Positive and negative `TimeOffsetSeconds` clock drift adjustments.
  - `SyncToGameTime` calculation accuracy.
  - Localization key parity across English, German, and Russian catalogs.
