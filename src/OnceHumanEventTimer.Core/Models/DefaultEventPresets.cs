// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Models;

/// <summary>
/// Built-in event definitions and presets for Once Human: Event Timer.
/// </summary>
public static class DefaultEventPresets
{
    /// <summary>
    /// Creates the complete list of default Visional Wheel event presets.
    /// </summary>
    public static List<EventDefinition> CreateDefaultEvents() => new()
    {
        new EventDefinition
        {
            Id = "gravity-abyss",
            Name = "Gravity Abyss",
            CycleMinutes = 60,
            StartMinute = 22,
            StartSecond = 29,
            DurationMinutes = 30,
            TimeOffsetSeconds = 0,
            ColorHex = "#00F0FF",
            IsEnabled = true,
            SoundPreset = SoundPreset.CelestialBell,
            Description = "Once Human hourly event with low-gravity tide and Stardust Crystals."
        },
        new EventDefinition
        {
            Id = "cargo-scramble",
            Name = "Cargo Scramble (Sproutlet Truck)",
            CycleMinutes = 60,
            StartMinute = 0,
            DurationMinutes = 20,
            TimeOffsetSeconds = 0,
            ColorHex = "#F59E0B",
            IsEnabled = false,
            SoundPreset = SoundPreset.CyberWarning,
            Description = "Cargo Scramble PvP sproutlet truck dynamic encounter."
        },
        new EventDefinition
        {
            Id = "skyripper",
            Name = "Skyripper Anomaly",
            CycleMinutes = 60,
            StartMinute = 37,
            StartSecond = 29,
            DurationMinutes = 20,
            TimeOffsetSeconds = 0,
            ColorHex = "#8B5CF6",
            IsEnabled = false,
            SoundPreset = SoundPreset.EmergencySiren,
            Description = "Skyripper Rift anomaly encounter across aerial floating rift zones."
        },
        new EventDefinition
        {
            Id = "prime-war",
            Name = "Prime War (Nightmare Defense)",
            CycleMinutes = 120,
            StartMinute = 0,
            DurationMinutes = 30,
            TimeOffsetSeconds = 0,
            ColorHex = "#DC2626",
            IsEnabled = false,
            SoundPreset = SoundPreset.DeepResonance,
            Description = "Massive cooperative battlefield defending the Staroid Restraint device against Manibus."
        },
        new EventDefinition
        {
            Id = "public-crisis",
            Name = "Public Crisis (Harrowing)",
            CycleMinutes = 30,
            StartMinute = 0,
            DurationMinutes = 15,
            TimeOffsetSeconds = 0,
            ColorHex = "#06B6D4",
            IsEnabled = false,
            SoundPreset = SoundPreset.StardustPing,
            Description = "Rapid dynamic open-world public crisis event rewarding weapon mods and Stardust Source."
        },
        new EventDefinition
        {
            Id = "drift-greenhouse",
            Name = "Drift Greenhouse [Manual Sync]",
            CycleMinutes = 240,
            StartMinute = 4,
            StartSecond = 4,
            DurationMinutes = 30,
            TimeOffsetSeconds = 0,
            ColorHex = "#10B981",
            IsEnabled = false,
            SoundPreset = SoundPreset.CelestialBell,
            ShowOnlyWhenUpcoming = true,
            UpcomingThresholdMinutes = 10,
            Description = "Isles of Abyss per-world dynamic spawn (World 1–12). Requires calibration via Sync from in-game map."
        },
        new EventDefinition
        {
            Id = "chiron-centaur",
            Name = "Chiron the Centaur",
            CycleMinutes = 60,
            StartMinute = 30,
            DurationMinutes = 15,
            TimeOffsetSeconds = 0,
            ColorHex = "#F97316",
            IsEnabled = false,
            SoundPreset = SoundPreset.QuantumChord,
            Description = "Chiron centaur boss dynamic encounter in the Red Sands desert."
        },
        new EventDefinition
        {
            Id = "workshop-defense",
            Name = "Extraction Workshop Defense",
            CycleMinutes = 15,
            StartMinute = 0,
            DurationMinutes = 10,
            TimeOffsetSeconds = 0,
            ColorHex = "#14B8A6",
            IsEnabled = false,
            SoundPreset = SoundPreset.CyberWarning,
            Description = "Refinery and mining rig wave defense extracting Premium Stardust fuel."
        },
        new EventDefinition
        {
            Id = "blackfell-loot-run",
            Name = "Blackfell Stronghold Loot Run",
            CycleMinutes = 240,
            StartMinute = 0,
            DurationMinutes = 15,
            TimeOffsetSeconds = 0,
            ColorHex = "#6366F1",
            IsEnabled = false,
            SoundPreset = SoundPreset.CelestialBell,
            Description = "4-hour optimal high-yield weapon/gear crate looting route in Blackfell Fallen Zone."
        },
        new EventDefinition
        {
            Id = "lunar-oracle",
            Name = "Lunar Oracle (Blood Moon)",
            CycleMinutes = 60,
            StartMinute = 45,
            DurationMinutes = 15,
            TimeOffsetSeconds = 0,
            ColorHex = "#EF4444",
            IsEnabled = false,
            SoundPreset = SoundPreset.EmergencySiren,
            Description = "Lunar Oracle Blood Moon nocturnal event with high-tier aberrant mutations."
        },
        new EventDefinition
        {
            Id = "lunar-revelry",
            Name = "Lunar Revelry (Lord of Moonlight)",
            CycleMinutes = 60,
            StartMinute = 40,
            DurationMinutes = 20,
            TimeOffsetSeconds = 0,
            ColorHex = "#A855F7",
            IsEnabled = false,
            SoundPreset = SoundPreset.QuantumChord,
            Description = "Lord of Moonlight gathering festival with exclusive cosmic loot drops."
        },
        new EventDefinition
        {
            Id = "aberrant-progeny",
            Name = "Aberrant Progeny (Spore Tide)",
            CycleMinutes = 60,
            StartMinute = 10,
            DurationMinutes = 25,
            TimeOffsetSeconds = 0,
            ColorHex = "#10B981",
            IsEnabled = false,
            SoundPreset = SoundPreset.DeepResonance,
            Description = "Spore Tide invasion wave defending against aberrant hive monsters."
        },
        new EventDefinition
        {
            Id = "dissonant-resonance",
            Name = "Dissonant Resonance",
            CycleMinutes = 60,
            StartMinute = 15,
            DurationMinutes = 30,
            TimeOffsetSeconds = 0,
            ColorHex = "#EC4899",
            IsEnabled = false,
            SoundPreset = SoundPreset.StardustPing,
            Description = "Dissonant soundwave anomaly requiring acoustic tuning devices."
        },
        new EventDefinition
        {
            Id = "crate-loot-reset",
            Name = "Crate Loot Reset (4-Hour)",
            CycleMinutes = 240,
            StartMinute = 0,
            DurationMinutes = 5,
            TimeOffsetSeconds = 0,
            ColorHex = "#3B82F6",
            IsEnabled = false,
            SoundPreset = SoundPreset.CosmicPulse,
            Description = "4-hour global refresh cycle for high-tier mystical crates and stronghold loot."
        },
        new EventDefinition
        {
            Id = "custom-wheel",
            Name = "Custom Wheel Event",
            CycleMinutes = 60,
            StartMinute = 0,
            DurationMinutes = 15,
            TimeOffsetSeconds = 0,
            ColorHex = "#EAB308",
            IsEnabled = false,
            SoundPreset = SoundPreset.CosmicPulse,
            Description = "User-defined custom cyclic event schedule."
        }
    };
}
