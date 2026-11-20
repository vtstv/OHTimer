# Once Human: Event Timer — Feature Guide

A modern, lightweight Windows desktop HUD overlay designed specifically for *Once Human*. Tracks cyclic world events, elite boss respawns, and farming milestones with zero frame drops, zero-drift game time synchronization, and out-of-process architecture.

---

## 1. Universal Event Scheduler & Presets

### Meta-Prioritized Built-in Presets
The timer library contains 15 authentic Once Human events, automatically prioritized for current gameplay meta:

| Event | Cycle | Schedule | Key Features |
| :--- | :--- | :--- | :--- |
| **S5: The Echo** *(Priority #0)* | 60 min | XX:15:00 (Duration: 30m) | Season 5 Dissonant Resonance. Triggers dedicated boss portal alert at XX:30:00 (15 min elapsed). |
| **Cargo Scramble** *(Priority #1)* | 60 min | XX:00:00 (Duration: 20m) | Sproutlet Truck PvP event across Red Sands and Iron River. |
| **Drift Greenhouse [Manual Sync]** | 240 min | XX:04:04 (Duration: 30m) | Dynamic per-world spawn (Worlds 1–12). Hidden until 10 min before start. |
| **Manibus (Prime War)** | 120 min | XX:00:00 (Duration: 30m) | Cooperative battlefield defense against Manibus. |
| **Skyripper Anomaly** | 60 min | XX:37:29 (Duration: 20m) | Aerial floating rift anomaly encounter. |
| **Public Crisis (Harrowing)** | 30 min | XX:00:00 (Duration: 15m) | Dynamic open-world crisis rewarding weapon mods and Stardust. |
| **Chiron the Centaur** | 60 min | XX:30:00 (Duration: 15m) | Dynamic desert boss encounter. |
| **Extraction Workshop Defense** | 15 min | XX:00:00 (Duration: 10m) | Stardust fuel wave defense. |
| **Other Presets** | Varies | Varies | Lunar Revelry, Lunar Oracle, Aberrant Progeny, Blackfell Loot Run, Crate Reset, Gravity Abyss (Legacy), Custom. |

### Mid-Event Checkpoint Alerts
Events with progressive mechanics (such as *The Echo War* boss portal opening 15 minutes after the Vision starts) trigger dedicated alerts at exact elapsed minute thresholds. Start alerts can be silenced on a per-event basis without suppressing mid-event portal chimes.

### Approaching Event Filter (`ShowOnlyWhenUpcoming`)
Long-cycle events (e.g. 4-hour Drift Greenhouse or Stronghold resets) can be hidden from the active HUD until approaching the start threshold (default: 10 minutes), eliminating HUD clutter while you farm.

### In-Game Time Calibration (Zero-Drift Sync)
Compensates for clock skew between local PC system time and Once Human game servers:
- Enter any remaining countdown visible on your in-game map or HUD (e.g., `1h 3m 29s`, `08:23`, `2h 54m`).
- Supports natural multilingual syntax (English, German, Russian) and automatically strips game prompt prefixes (e.g., *"Opens at..."*, *"Откроется через..."*).
- Click **Sync** to calculate exact server drift or cycle anchor timing instantaneously.
- Nudge buttons (`-5s`, `-1s`, `+1s`, `+5s`, `0s`) allow quick 1-second fine-tuning.

---

## 2. Dedicated Boss Respawn Timer

Track elite mob and world boss respawns on demand:
- **Global Hotkey**: Press <kbd>Alt+NumPad 1</kbd> anywhere in-game to start or restart the countdown.
- **Audio & Visual Alerts**:
  - Early warning chime at configurable seconds ahead (default: 30s).
  - Spawn alarm and celebratory chime at 00:00.
  - Optional repeating reminder alerts if the boss remains untouched.
  - Windows Desktop Toast notification on spawn.
- **HUD Card & Badges**: Displays countdown, visual progress bar, and dynamic status badges (`[🎯 RESPAWN]`, `[⚠️ APPROACHING]`, `[✨ SPAWNED!]`).
- **One-Click Dismiss**: Click the <kbd>✕</kbd> button on either Full HUD or Mini-HUD to dismiss an active timer early.
- **Customizable Cooldown**: Configure minutes and seconds, pick quick presets (3m, 5m, 10m, 15m, 20m, 30m), auto-hide delay, and neon accent color.

---

## 3. Smart Farm & Kill Counter

A dedicated run and kill tracker designed for dungeon grinding, weapon calibration farming, and silo runs:
- **Default State**: Disabled by default (`IsEnabled = false`) in base distribution to maintain a clean default overlay. Turn it on anytime under Settings > **Kill Counter** tab.
- **Global Hotkeys**:
  - <kbd>Alt+NumPad +</kbd>: Increment kill/run counter (+1) and optionally auto-start the Boss Respawn Timer.
  - <kbd>Alt+NumPad -</kbd>: Undo / decrement kill counter (-1) in case of accidental press.
- **Target Run Goal System**:
  - Set a run goal (e.g. 20 runs).
  - Real-time progress percentage badge and mini progress bar.
  - Triumphant audio chime and visual HUD flash upon goal completion.
- **Quick Target Presets**: One-click configuration buttons for *Treant*, *Hound*, *Giant*, *Silo*, and *Manibus*.
- **Chained Boss Timer Automation**: Enable *"Auto-start Boss Respawn Timer on kill"* to seamlessly loop farming sessions without touching the keyboard.
- **All-Time Total Tracking**: Session count can be reset to 0 while preserving lifetime total runs.

---

## 4. Global Keyboard Shortcuts

| Shortcut | Default Key | Action |
| :--- | :--- | :--- |
| **Toggle Settings Window** | <kbd>Ctrl</kbd> + <kbd>Alt</kbd> + <kbd>O</kbd> | Opens, focuses, or closes the Settings dialog instantly from in-game. |
| **Boss Respawn Timer** | <kbd>Alt</kbd> + <kbd>NumPad 1</kbd> | Starts or restarts the active boss respawn countdown. |
| **Kill Counter Increment** | <kbd>Alt</kbd> + <kbd>NumPad +</kbd> | Adds +1 kill/run and triggers optional chained boss timer. |
| **Kill Counter Decrement** | <kbd>Alt</kbd> + <kbd>NumPad -</kbd> | Undoes -1 kill/run. |
| **Toggle Overlay Visibility** | <kbd>Ctrl</kbd> + <kbd>Alt</kbd> + <kbd>S</kbd> | Temporarily hides or shows the floating HUD overlay. |
| **Toggle Click-Through / Lock** | <kbd>Ctrl</kbd> + <kbd>Alt</kbd> + <kbd>L</kbd> | Locks overlay position and lets mouse clicks pass directly into the game. |
| **Pause / Resume Alerts** | <kbd>Ctrl</kbd> + <kbd>Alt</kbd> + <kbd>P</kbd> | Temporarily mutes or resumes all sound chimes and toast notifications. |

### Interactive Hotkey Recording (`HotkeyBox`)
Click any hotkey field in Settings to record a new key combination. Supports modifier keys (<kbd>Ctrl</kbd>, <kbd>Alt</kbd>, <kbd>Shift</kbd>, <kbd>Win</kbd>) and single keys. Press <kbd>Esc</kbd> to cancel. Each shortcut includes a <kbd>↺</kbd> button to restore its default value.

---

## 5. Out-of-Process Windows Architecture & Performance

Once Human is protected by kernel-level anti-cheat (ACE / Anti-Cheat Expert) running with elevated Administrator privileges. This application is engineered with strict adherence to Windows security boundaries:
- **Zero Game Process Memory Touching**: Never reads, writes, hooks, or injects into Once Human process memory. Zero game process modification.
- **Windows UIPI Message Filter Bypass (`ChangeWindowMessageFilterEx`)**: Standard Windows OS API allows `WM_HOTKEY` messages to pass from elevated game windows to the non-elevated timer overlay without requiring the timer to run as Administrator.
- **Hardware DirectFlip & MPO Presentation (`WS_EX_NOACTIVATE`)**: Prevents the overlay from stealing keyboard or mouse focus from the game, preserving GPU Multi-Plane Overlay (MPO) and DirectFlip hardware presentation in Once Human for zero frame drops or input stutter.
- **Smart Game Focus Monitor**: Caches process IDs and queries process names via lightweight `PROCESS_QUERY_LIMITED_INFORMATION`. Fully compatible with Steam (`ONCE_HUMAN.exe`) and Loading Bay / NetEase (`AF_AP.exe`). Automatically hides overlay when Alt-Tabbing to desktop apps (with 400ms hysteresis debounce to prevent DirectX swapchain presentation stalls).

---

## 6. Overlay Views & Customization

- **Full HUD**: Displays active event, remaining countdown ring, multi-event badges, upcoming events list, and active boss/kill counter cards.
- **Compact Mini-HUD**: Ultra-minimalist horizontal pill showing countdown, next event name, and compact status badges.
- **Quick Switching**: Double-click anywhere on the overlay card to instantly toggle between Full HUD and Mini-HUD.
- **Visual Styling**:
  - Background opacity slider (20% – 100%).
  - UI scaling factor (70% – 200%).
  - Toggle background card, borders, circular progress rings, and mini-controls.
  - Neon accent color palette picker for each timer module.

---

## 7. Storage Modes (Portable vs AppData)

- **True Portable Mode**: Automatically enabled when `settings.json` or a `portable` / `portable.txt` file exists in the same folder as `OnceHumanEventTimer.exe`. Settings are saved locally alongside the executable.
- **AppData Mode**: Defaults to `%APPDATA%\OnceHumanEventTimer\settings.json`. If portable mode is enabled later, existing configuration is seamlessly migrated.
