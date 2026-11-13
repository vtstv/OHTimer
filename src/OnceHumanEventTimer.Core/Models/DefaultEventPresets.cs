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
            Name = "Gravity Abyss (Starfall Inversion)",
            CycleMinutes = 60,
            StartMinute = 22,
            StartSecond = 29,
            DurationMinutes = 30,
            TimeOffsetSeconds = 0,
            ColorHex = "#00F0FF",
            IsEnabled = true,
            SoundPreset = SoundPreset.CelestialBell,
            Description = "Once Human Starfall Inversion / Gravity Abyss hourly event with low-gravity tide and Stardust Crystals."
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
