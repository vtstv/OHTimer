# Changelog

All notable changes to **Once Human: Event Timer** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
