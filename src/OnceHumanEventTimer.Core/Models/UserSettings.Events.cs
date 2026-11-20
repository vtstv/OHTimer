// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Models;

/// <summary>
/// Event and preset collection management for UserSettings.
/// </summary>
public partial class UserSettings
{
    /// <summary>
    /// Identifier of the currently active event definition.
    /// </summary>
    public string ActiveEventId { get; set; } = "dissonant-resonance";

    /// <summary>
    /// List of configured event schedules and presets.
    /// </summary>
    public List<EventDefinition> Events { get; set; } = GetDefaultEvents();

    /// <summary>
    /// Retrieves the currently active event definition, falling back to default if not found.
    /// </summary>
    public EventDefinition GetActiveEvent()
    {
        if (Events == null || Events.Count == 0)
        {
            Events = GetDefaultEvents();
        }

        var active = Events.FirstOrDefault(e => string.Equals(e.Id, ActiveEventId, StringComparison.OrdinalIgnoreCase));
        if (active == null || !active.IsEnabled)
        {
            var firstEnabled = Events.FirstOrDefault(e => e.IsEnabled);
            active = firstEnabled ?? active ?? Events[0];
            ActiveEventId = active.Id;
        }

        return active;
    }

    /// <summary>
    /// Returns all events that are currently enabled for HUD tracking.
    /// </summary>
    public List<EventDefinition> GetEnabledEvents()
    {
        if (Events == null || Events.Count == 0)
        {
            Events = GetDefaultEvents();
        }

        var enabled = Events.Where(e => e.IsEnabled).ToList();
        if (enabled.Count == 0)
        {
            var active = GetActiveEvent();
            active.IsEnabled = true;
            enabled.Add(active);
        }

        return enabled;
    }

    /// <summary>
    /// Generates the default event presets for Once Human.
    /// </summary>
    public static List<EventDefinition> GetDefaultEvents() => DefaultEventPresets.CreateDefaultEvents();

    /// <summary>
    /// Merges any missing built-in default event presets into the configured event list,
    /// reorders them according to current meta (Cargo Scramble #0, S5 The Echo #1, Gravity Abyss last),
    /// and migrates legacy configurations.
    /// Returns true if any modifications or migrations occurred.
    /// </summary>
    public bool EnsureDefaultPresets()
    {
        bool wasMigrated = false;

        if (Events == null || Events.Count == 0)
        {
            Events = GetDefaultEvents();
            return true;
        }

        var defaults = DefaultEventPresets.CreateDefaultEvents();
        var existingIds = new HashSet<string>(Events.Select(e => e.Id), StringComparer.OrdinalIgnoreCase);

        foreach (var def in defaults)
        {
            if (!existingIds.Contains(def.Id))
            {
                if (def.Id != "dissonant-resonance")
                {
                    def.IsEnabled = false;
                }
                Events.Add(def);
                existingIds.Add(def.Id);
                wasMigrated = true;
            }
        }

        // Migrate provisional drift-greenhouse preset from 180m to verified 240m schedule
        var existingGreenhouse = Events.FirstOrDefault(e => e.Id == "drift-greenhouse");
        if (existingGreenhouse != null && (existingGreenhouse.CycleMinutes == 180 || existingGreenhouse.Name == "Drift Greenhouse"))
        {
            existingGreenhouse.Name = "Drift Greenhouse [Manual Sync]";
            existingGreenhouse.CycleMinutes = 240;
            existingGreenhouse.StartMinute = 4;
            existingGreenhouse.StartSecond = 4;
            existingGreenhouse.ShowOnlyWhenUpcoming = true;
            existingGreenhouse.UpcomingThresholdMinutes = 10;
            existingGreenhouse.Description = "Isles of Abyss per-world dynamic spawn (World 1–12). Requires calibration via Sync from in-game map.";
            wasMigrated = true;
        }

        // Migrate provisional dissonant-resonance preset to S5: The Echo
        var existingEcho = Events.FirstOrDefault(e => e.Id == "dissonant-resonance");
        if (existingEcho != null)
        {
            if (existingEcho.Name == "Dissonant Resonance" || existingEcho.Name == "Visional Wheel: The Echo" || existingEcho.StartMinute != 15 || existingEcho.StartSecond != 0)
            {
                bool preserveCountdownToBoss = existingEcho.Name != "Dissonant Resonance" && existingEcho.CountdownToBoss;
                existingEcho.Name = "S5: The Echo";
                existingEcho.CycleMinutes = 60;
                existingEcho.StartMinute = 15;
                existingEcho.StartSecond = 0;
                existingEcho.DurationMinutes = 30;
                existingEcho.ColorHex = "#D946EF";
                existingEcho.SoundPreset = SoundPreset.EmergencySiren;
                existingEcho.AlertOnEventStart = false;
                existingEcho.EnableExtraAlert = true;
                existingEcho.ExtraAlertMinutes = 15;
                existingEcho.CountdownToBoss = preserveCountdownToBoss;
                existingEcho.Description = "Season 5: Dissonant Resonance. Hourly schedule starting at XX:15:00. The Echo War boss portal opens at XX:30:00.";
                wasMigrated = true;
            }
        }

        // Disable concluded Gravity Abyss preset so it does not crowd HUD
        var existingAbyss = Events.FirstOrDefault(e => e.Id == "gravity-abyss");
        if (existingAbyss != null)
        {
            if (existingAbyss.IsEnabled)
            {
                existingAbyss.IsEnabled = false;
                wasMigrated = true;
            }
            if (existingAbyss.Name == "Gravity Abyss")
            {
                existingAbyss.Name = "Gravity Abyss (Legacy)";
                existingAbyss.Description = "Concluded Season event with low-gravity tide and Stardust Crystals.";
                wasMigrated = true;
            }
        }

        // Migrate prime-war preset name from legacy Prime War (Nightmare Defense) to Manibus (Prime War)
        var existingPrimeWar = Events.FirstOrDefault(e => e.Id == "prime-war");
        if (existingPrimeWar != null && (existingPrimeWar.Name == "Prime War (Nightmare Defense)" || existingPrimeWar.Name == "Prime War"))
        {
            existingPrimeWar.Name = "Manibus (Prime War)";
            wasMigrated = true;
        }

        // Reorder presets:
        // 1. S5: The Echo at index 0 (current active season event)
        var echo = Events.FirstOrDefault(e => e.Id == "dissonant-resonance");
        if (echo != null)
        {
            int idx = Events.IndexOf(echo);
            if (idx != 0)
            {
                Events.RemoveAt(idx);
                Events.Insert(0, echo);
                wasMigrated = true;
            }
        }

        // 2. Cargo Scramble (Sproutlet Truck) at index 1
        var cargo = Events.FirstOrDefault(e => e.Id == "cargo-scramble");
        if (cargo != null)
        {
            int idx = Events.IndexOf(cargo);
            int targetIdx = Events.Count > 1 ? 1 : 0;
            if (idx != targetIdx)
            {
                Events.RemoveAt(idx);
                Events.Insert(targetIdx, cargo);
                wasMigrated = true;
            }
        }

        // 3. Gravity Abyss at the very end of the list
        if (existingAbyss != null)
        {
            int idx = Events.IndexOf(existingAbyss);
            if (idx != Events.Count - 1)
            {
                Events.RemoveAt(idx);
                Events.Add(existingAbyss);
                wasMigrated = true;
            }
        }

        // Ensure at least one top event is enabled
        if (!Events.Any(e => e.IsEnabled))
        {
            if (echo != null) echo.IsEnabled = true;
            else if (cargo != null) cargo.IsEnabled = true;
            wasMigrated = true;
        }

        // Migrate active event to S5, Cargo Scramble, or first enabled event if active event is disabled or legacy gravity-abyss
        if (ActiveEventId == "gravity-abyss" || !Events.Any(e => e.Id == ActiveEventId && e.IsEnabled))
        {
            var preferred = Events.FirstOrDefault(e => e.Id == "dissonant-resonance" && e.IsEnabled)
                         ?? Events.FirstOrDefault(e => e.Id == "cargo-scramble" && e.IsEnabled)
                         ?? Events.FirstOrDefault(e => e.IsEnabled)
                         ?? Events[0];
            if (ActiveEventId != preferred.Id)
            {
                ActiveEventId = preferred.Id;
                wasMigrated = true;
            }
        }

        return wasMigrated;
    }
}
