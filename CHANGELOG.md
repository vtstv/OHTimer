# Changelog

All notable changes to **Once Human: Event Timer** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.5.2] - 2026-11-14

### Fixed
- **NVIDIA Overlay Global Hotkey Conflict (Error 1409)**: Switched the default Pause/Resume Alerts hotkey from `Ctrl+Alt+M` to `Ctrl+Alt+P`. NVIDIA App / GeForce Experience exclusively captures `Ctrl+Alt+M` system-wide for microphone muting, which prevented Once Human Event Timer from registering the hotkey (Win32 Error 1409: `ERROR_HOTKEY_ALREADY_REGISTERED`). Added automatic fallback to `Ctrl+Alt+P` for existing configurations with `Ctrl+Alt+M` and added fallback logic for `MOD_NOREPEAT` support.
- **WPF ScrollViewer & ListBox Dark Theme Integration**: Added application-wide implicit `ScrollViewer` template and dark `ListBox` ControlTemplate ensuring the sleek dark cyberpunk scrollbar (`Styles/ScrollBars.xaml`) consistently applies across all list boxes, dropdowns, and scrollable panels, eliminating bright default Windows Aero chrome.
- **Event Presets Column Width & Button Spacing**: Expanded presets list column width to 300px to guarantee ample breathing room for button labels across all languages (English, German, Russian).

### Added
- **Mini-HUD Miniature Mute Indicator**: Added a dedicated glowing muted indicator (`🔕`) in both Single-Timer and Multi-Timer Mini-HUD views. Even when Mini-HUD header controls are hidden (`ShowMiniControls: false`), players can immediately see when alerts and notifications are temporarily muted.

---

## [1.5.1] - 2026-11-14

### Performance & Optimizations
- **Hardware DirectFlip & MPO Compatibility (`WS_EX_NOACTIVATE`)**: Added the `WS_EX_NOACTIVATE` Win32 extended window style to the HUD overlay. Informs Windows DWM and GPU drivers that the overlay will never steal input focus, allowing DirectX 11/12 hardware presentation models (Independent Flip / DirectFlip / Multi-Plane Overlays) to stay fully engaged in *Once Human*, preventing in-game frame rate drops, G-Sync/FreeSync stutter, and flip queue interruptions.
- **Zero-CPU Foreground Window Detection in `GameFocusMonitor`**: Eliminated recurring 350ms process inspections (`Process.GetProcessById`). Implemented fast-path foreground window handle caching (`_lastForegroundHwnd`) and lightweight Win32 title matching, reducing background polling CPU usage and garbage collection allocations to virtually 0% during gameplay.

### Added
- **Global Hotkey to Pause/Resume Alerts (`Ctrl+Alt+M`)**: Added configurable hotkey (`TogglePauseAlertsHotkey`), overlay quick button, right-click context menu option, and system tray toggle allowing players to immediately mute or resume audio alerts and desktop notifications on demand without leaving the game.
- **Confirmation Dialogs for Destructive & Reset Actions**: Added modal confirmation prompts to "Delete Event" and "Restore Presets" in the Event Manager to protect against accidental clicks, fully localized in English, German, and Russian.
- **Custom Cyberpunk ScrollBar Styling**: Implemented a sleek dark slate track with neon cyan hover glow (`Styles/ScrollBars.xaml`) replacing standard bright Windows scrollbars across all scrollable panels.
- **Responsive 2-Row Event Manager Button Layout**: Reorganized Event actions (`Add New Event`, `Delete Event`, `Restore Presets`) into a clean 2-row layout, completely eliminating button text truncation across all languages.
- **Overlay Visual Showcase in Documentation**: Updated `README.md` to showcase actual in-game HUD overlay and floating Mini-HUD screenshots.

---

## [1.5.0] - 2026-11-13

### Added
- **Second-Precision Event Scheduling Engine (`StartSecond`)**: Extended `EventDefinition` and `EventTimeCalculator` with sub-minute resolution. Configured `Gravity Abyss` to start at exact in-game server cycle **XX:22:29** with **0s default offset**, providing drift-free zero-calibration countdown accuracy immediately out-of-the-box matching Once Human server ticks.
- **Dedicated System Settings Tab**: Added a clean cyberpunk-styled **System** tab to the Settings window organizing OS-level and application lifecycle preferences.
- **Windows Autostart Integration**: Added user-configurable toggle "Start with Windows" in System settings using `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, allowing the timer to seamlessly launch on computer boot.
- **Auto-Hide Overlay when Game is Out of Focus (Default: ON)**: Added real-time foreground window detection via Win32 `GetForegroundWindow` and process checking. When the player Alt-Tabs away from *Once Human* to a browser or desktop app, the overlay automatically hides itself to keep the screen clear, and instantly reappears when switching back to Once Human.
- **Expanded Once Human Event Presets**: Expanded built-in event templates library to 14 authentic Once Human events (Gravity Abyss, Skyripper Anomaly, Cargo Scramble, Prime War, Public Crisis, Chiron the Centaur, Extraction Workshop Defense, Blackfell Stronghold Loot Run, Lunar Oracle, Lunar Revelry, Aberrant Progeny, Dissonant Resonance, Crate Loot Reset, Custom Wheel Event).
- **Multi-Package Distribution Channel**: Introduced 3 distribution formats for releases:
  1. *Portable (Framework-Dependent)*: Ultra-lightweight (~0.38 MB ZIP) for users with .NET 8 Desktop Runtime.
  2. *Standalone (Self-Contained)*: Plug-and-play (~63 MB ZIP) with bundled runtime, zero dependencies required.
  3. *Full Windows Setup Installer*: Setup wizard (`OnceHumanTimer-v1.5.0-Setup.exe`) with Start Menu & Desktop shortcuts, clean uninstaller in Windows Settings, and multilingual support (EN/DE/RU).
- **Multilingual Support**: Fully localized all new System settings, descriptions, and second-precision inputs across English (`en-US`), German (`de-DE`), and Russian (`ru-RU`).

### Fixed
- **Persistent Red Border on Sound Selection ComboBox**: Fixed WPF data binding validation error where clicking "Save & Apply" caused the sound preset ComboBoxes to turn red. Converted `SoundPresetOption` into an observable model and updated display titles in-place without clearing the item collection, preventing null coercion and type conversion validation faults.
- **GitHub Updater Repository Discovery**: Fixed in-app updater handling when checking for releases on repositories before first release publication, displaying a clear status message instead of throwing an unhandled request error. Differentiated update targets so standalone builds download standalone updates.

---

## [1.4.0] - 2026-11-13

### Added
- **Full-Spectrum Native Color Picker**: Replaced predefined color swatch buttons with a sleek interactive Color Picker button displaying the live color swatch and HEX code. Clicking opens the modal Windows color palette dialog (`ChooseColor`), enabling selection of any color across the entire RGB/HSV color space with persistent custom color slots.
- **Mini-HUD Maximum Minimization**: Added "Show Mini-HUD Header & Controls" appearance option (`ShowMiniControls`). When unchecked, completely collapses the header bar (`MINI HUD` label and control buttons `[🗖] [⚙] [✕]`), yielding an ultra-clean floating widget showing strictly the event status badge and countdown timer.
- **HUD Context Menu & Double-Click Gesture**: Added high-tech dark styled right-click ContextMenu on the HUD overlay (Settings, Toggle Mini/Full HUD, Lock Position, Exit) and double-click to toggle between Mini and Full HUD, ensuring effortless widget control even when header buttons are hidden.
- **Procedural Synthesized Sound Library**: Expanded alert audio options with 7 procedural in-memory sound presets (`Celestial Bell`, `Cosmic Pulse`, `Stardust Ping`, `Quantum Chord`, `Deep Resonance`, `Cyber Warning`, `Emergency Siren`) plus custom WAV file support.
- **Audio Sound Selectors & Instant Preview**: Added dropdown selectors and instant "Preview" test buttons for both default audio alerts and individual event presets.
- **Multilingual Support**: Fully localized new options, presets, and menus across English (`en-US`), German (`de-DE`), and Russian (`ru-RU`).

### Fixed
- **Transparent Dragging Issue**: Resolved bug where unchecking "Show Overlay Background" and "Show Outer Border" caused drag-and-drop to fail. Replaced fully transparent hit-test drop (`Transparent` / alpha=0) with `#01000000` (invisible alpha=1) on container borders, guaranteeing 100% solid hit-testing to the WPF engine while remaining completely invisible to the human eye.
- **Draggable Cursor Feedback**: Added `SizeAll` cursor when overlay is unlocked to provide immediate visual feedback that the widget is draggable.

---

## [1.3.0] - 2026-11-13

### Added
- **Custom Dark Titlebar**: Replaced standard OS title bar on Settings dialog with a sleek dark title bar (`WindowChrome`), matching the Once Human theme with custom minimize, maximize, and close buttons.
- **Dynamic Event Ordering**: Multi-timer list automatically reorders events dynamically so that active events appear first, followed by events closest to starting.
- **Visual Accent Color Picker**: Added neon palette swatch buttons (Cyan, Purple, Amber, Emerald, Rose, Blue, Gold, Coral) with live preview box and custom HEX input in Events & Presets tab.
- **HUD Minimization & Appearance Options**: Added toggles for outer border, background transparency, circular progress ring, and next event footer, enabling ultra-minimal floating badges.
- **Per-Event Custom Audio**: Added custom WAV audio alert assignment for individual events with browse, test, and clear controls.
- **Clickable About Links**: Made author and repository links in About tab clickable to open directly in the default browser.

### Fixed
- **Graceful GitHub Update Handling**: Handled 404 response on empty or newly created GitHub repositories gracefully with informative status message instead of failing with a generic error.

### Changed
- **Simplified User Documentation**: Replaced technical architectural documentation in `README.md` with a clean, user-oriented gamer guide.
- **Repository Hygiene**: Untracked `AGENTS.md` and added to `.gitignore` to prevent committing internal agent files.

---

## [1.2.1] - 2026-11-13

### Fixed
- **Startup Crash Fix (`0xE0434352` / `XamlParseException`)**: Removed direct `Icon` pack references in XAML for frameless `OverlayWindow` and added safe runtime icon loader with fallback in `SettingsWindow` to eliminate single-file publish crash.

### Changed
- **Solution & Namespace Rebranding**: Renamed solution, projects, assemblies, and root namespaces from `StarfallTimer` to `OnceHumanEventTimer`.
- **Primary Executable**: Published binary is now `OnceHumanEventTimer.exe`.
- **Application Storage**: Updated default local state storage directory to `%APPDATA%\OnceHumanEventTimer` with seamless backwards-compatible configuration migration.
- **Repository & Release Integration**: Updated self-updater API and git remote origin to `https://github.com/vtstv/OHTimer.git`.

---

## [1.2.0] - 2026-11-12

### Added
- **Multi-Timer HUD**: Simultaneously monitor multiple active in-game events with stacked cards in Full Mode or clean rows in Compact Mini-HUD mode.
- **Visional Wheel Event Presets**: Added 8 pre-configured event schedules:
  - *Gravity Abyss (Starfall Inversion)*: 60m cycle, XX:22 start, 30m duration, -31s calibrated offset.
  - *Cargo Scramble (Sproutlet Truck)*: 60m cycle, XX:00 start, 20m duration.
  - *Lunar Oracle (Blood Moon)*: 60m cycle, XX:45 start, 15m duration.
  - *Lunar Revelry (Lord of Moonlight)*: 60m cycle, XX:40 start, 20m duration.
  - *Aberrant Progeny (Spore Tide)*: 60m cycle, XX:10 start, 25m duration.
  - *Dissonant Resonance*: 60m cycle, XX:15 start, 30m duration.
  - *Crate Loot Reset (4-Hour)*: 240m cycle, XX:00 start, 5m duration.
  - *Custom Wheel Event*: 60m cycle, XX:00 start, 15m duration.
- **Compact Mini-HUD Mode**: Minimalist floating pill display mode showing only event badges and active countdowns.
- **Seamless GitHub Self-Updater**: Integrated update detection against GitHub Releases API, changelog viewer, download progress tracking, and detached Windows in-place executable replacement with automatic restart.
- **Dark ComboBox Theme**: Custom high-contrast cyberpunk dark ControlTemplate for dropdown selectors.
- **Live Opacity Adjustment**: Overlay transparency changes update immediately in real-time.

### Changed
- Re-calibrated default time offset for *Gravity Abyss* to -31 seconds to achieve zero drift with live Once Human game servers.
- Expanded Settings dialog dimensions to 780x660 for improved usability.
- Streamlined main overlay title bar: Moved version credits to the About tab in Settings.
- Full localization parity for English (`en-US`), German (`de-DE`), and Russian (`ru-RU`).

### Fixed
- System tray icon now reliably extracts and renders the application icon from the embedded PE resource.
- Resolved overlay lock state visual rendering issues.

---

## [1.1.0] - 2026-11-12

### Added
- **Universal Event Scheduling Engine**: Configurable cycle period, start minute, duration, and accent colors.
- **Dynamic Game Time Sync**: One-click sync matching in-game timer readouts to eliminate clock drift.
- **German (`de-DE`) Localization**: Added full German language catalog.
- **Stardust Cosmic Diamond Icon**: Multi-resolution application icon assets.

### Changed
- Refactored UI controls to custom dark Cyberpunk / Stardust theme.
- Removed legacy Chinese (`zh-CN`) translation catalog.

---

## [1.0.0] - 2026-11-11

### Added
- Initial release of Starfall Inversion Timer for Once Human.
- Hourly Gravity Abyss countdown and phase transitions.
- Frameless transparent overlay with click-through and position locking.
- Audio synthesis alerts and Windows toast notifications.
- Win32 tray menu and customizable global hotkeys.

---

*Authored by [Murr](https://github.com/vtstv). Distributed under the MIT License.*
