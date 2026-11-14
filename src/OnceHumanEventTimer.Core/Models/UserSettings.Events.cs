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
    public string ActiveEventId { get; set; } = "gravity-abyss";

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
        if (active == null)
        {
            active = Events[0];
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
    /// Merges any missing built-in default event presets into the configured event list.
    /// </summary>
    public void EnsureDefaultPresets()
    {
        if (Events == null || Events.Count == 0)
        {
            Events = GetDefaultEvents();
            return;
        }

        var defaults = DefaultEventPresets.CreateDefaultEvents();
        var existingIds = new HashSet<string>(Events.Select(e => e.Id), StringComparer.OrdinalIgnoreCase);

        foreach (var def in defaults)
        {
            if (!existingIds.Contains(def.Id))
            {
                def.IsEnabled = false;
                Events.Add(def);
                existingIds.Add(def.Id);
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
        }
    }
}
