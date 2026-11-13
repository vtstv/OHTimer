// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using OnceHumanEventTimer.Core.Localization;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Utils;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class SettingsAndPresetsTests
{
    private readonly LocalizationService _loc = new();

    [Fact]
    public void UserSettings_DefaultValues_AreCorrect()
    {
        var settings = new UserSettings();

        Assert.True(settings.ShowMiniControls);
        Assert.Equal(SoundPreset.CosmicPulse, settings.AlertSoundPreset);
        Assert.True(settings.ShowBorder);
        Assert.True(settings.ShowBackground);
        Assert.False(settings.IsLocked);
        Assert.False(settings.IsClickThrough);
    }

    [Fact]
    public void EventDefinition_Clone_CopiesSoundPresetCorrectly()
    {
        var original = new EventDefinition
        {
            Id = "test-event",
            Name = "Test Event",
            CycleMinutes = 45,
            StartMinute = 10,
            DurationMinutes = 15,
            TimeOffsetSeconds = 12,
            ColorHex = "#A855F7",
            IsEnabled = true,
            SoundPreset = SoundPreset.EmergencySiren,
            CustomSoundPath = "C:\\test.wav"
        };

        var clone = original.Clone();

        Assert.Equal(original.Id, clone.Id);
        Assert.Equal(original.Name, clone.Name);
        Assert.Equal(original.CycleMinutes, clone.CycleMinutes);
        Assert.Equal(original.StartMinute, clone.StartMinute);
        Assert.Equal(original.DurationMinutes, clone.DurationMinutes);
        Assert.Equal(original.TimeOffsetSeconds, clone.TimeOffsetSeconds);
        Assert.Equal(original.ColorHex, clone.ColorHex);
        Assert.Equal(original.IsEnabled, clone.IsEnabled);
        Assert.Equal(SoundPreset.EmergencySiren, clone.SoundPreset);
        Assert.Equal("C:\\test.wav", clone.CustomSoundPath);
    }

    [Fact]
    public void DefaultEvents_HaveConfiguredSoundPresets()
    {
        var events = DefaultEventPresets.CreateDefaultEvents();

        Assert.NotEmpty(events);
        var gravityAbyss = events.FirstOrDefault(e => e.Id == "gravity-abyss");
        Assert.NotNull(gravityAbyss);
        Assert.Equal(SoundPreset.CelestialBell, gravityAbyss.SoundPreset);

        var cargoScramble = events.FirstOrDefault(e => e.Id == "cargo-scramble");
        Assert.NotNull(cargoScramble);
        Assert.Equal(SoundPreset.CyberWarning, cargoScramble.SoundPreset);

        var lunarOracle = events.FirstOrDefault(e => e.Id == "lunar-oracle");
        Assert.NotNull(lunarOracle);
        Assert.Equal(SoundPreset.EmergencySiren, lunarOracle.SoundPreset);

        foreach (var ev in events)
        {
            Assert.True(Enum.IsDefined(typeof(SoundPreset), ev.SoundPreset));
        }
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("ru-RU")]
    public void SoundPresets_AreTranslatedAcrossAllLanguages(string langCode)
    {
        _loc.SetLanguage(langCode);

        foreach (SoundPreset preset in Enum.GetValues<SoundPreset>())
        {
            string key = $"SoundPreset_{preset}";
            string translated = _loc.GetString(key);

            Assert.False(string.IsNullOrWhiteSpace(translated), $"Missing translation for {key} in {langCode}");
            Assert.DoesNotContain("[", translated);
        }
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("ru-RU")]
    public void NewKeys_AreTranslatedAcrossAllLanguages(string langCode)
    {
        _loc.SetLanguage(langCode);

        string[] keys =
        [
            "Settings_ShowMiniControls",
            "Settings_DefaultSound",
            "Settings_PreviewSound",
            "HUD_ToggleMini"
        ];

        foreach (var key in keys)
        {
            string translated = _loc.GetString(key);
            Assert.False(string.IsNullOrWhiteSpace(translated), $"Missing translation for {key} in {langCode}");
            Assert.DoesNotContain("[", translated);
        }
    }

    [Theory]
    [InlineData("#00F0FF", "#00F0FF")]
    [InlineData("00f0ff", "#00F0FF")]
    [InlineData("#FFF", "#FFFFFF")]
    [InlineData("#FF00F0FF", "#00F0FF")]
    [InlineData("", "#00F0FF")]
    [InlineData("invalid", "#00F0FF")]
    public void ColorUtility_NormalizeHex_HandlesVariousFormats(string input, string expected)
    {
        string actual = ColorUtility.NormalizeHex(input);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ColorUtility_TryParseRgb_ParsesComponentsCorrectly()
    {
        bool success = ColorUtility.TryParseRgb("#37FBEC", out byte r, out byte g, out byte b);
        Assert.True(success);
        Assert.Equal(0x37, r);
        Assert.Equal(0xFB, g);
        Assert.Equal(0xEC, b);

        string hex = ColorUtility.ToHex(r, g, b);
        Assert.Equal("#37FBEC", hex);
    }

    [Fact]
    public void UserSettings_JsonSerialization_PreservesAllNewProperties()
    {
        var settings = new UserSettings
        {
            ShowMiniControls = false,
            AlertSoundPreset = SoundPreset.EmergencySiren,
            CompactMode = true,
            ShowBackground = false,
            ShowBorder = false
        };

        string json = JsonSerializer.Serialize(settings);
        var deserialized = JsonSerializer.Deserialize<UserSettings>(json);

        Assert.NotNull(deserialized);
        Assert.False(deserialized.ShowMiniControls);
        Assert.Equal(SoundPreset.EmergencySiren, deserialized.AlertSoundPreset);
        Assert.True(deserialized.CompactMode);
        Assert.False(deserialized.ShowBackground);
        Assert.False(deserialized.ShowBorder);
    }
}
