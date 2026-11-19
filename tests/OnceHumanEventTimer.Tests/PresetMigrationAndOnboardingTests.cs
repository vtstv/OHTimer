// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Localization;
using OnceHumanEventTimer.Core.Models;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class PresetMigrationAndOnboardingTests
{
    private readonly LocalizationService _loc = new();

    [Fact]
    public void DefaultEventPresets_HasSproutletAtTop_EchoSecond_AndGravityAbyssAtBottom()
    {
        var presets = DefaultEventPresets.CreateDefaultEvents();
        Assert.Equal(15, presets.Count);

        // Index 0: Cargo Scramble (Sproutlet Truck)
        Assert.Equal("cargo-scramble", presets[0].Id);
        Assert.Equal("Cargo Scramble (Sproutlet Truck)", presets[0].Name);
        Assert.Equal(60, presets[0].CycleMinutes);
        Assert.Equal(0, presets[0].StartMinute);
        Assert.Equal(20, presets[0].DurationMinutes);
        Assert.Equal("#F59E0B", presets[0].ColorHex);

        // Index 1: S5: The Echo
        Assert.Equal("dissonant-resonance", presets[1].Id);
        Assert.Equal("S5: The Echo", presets[1].Name);
        Assert.Equal(60, presets[1].CycleMinutes);
        Assert.Equal(15, presets[1].StartMinute);
        Assert.Equal(0, presets[1].StartSecond);
        Assert.Equal(30, presets[1].DurationMinutes);
        Assert.Equal("#D946EF", presets[1].ColorHex);
        Assert.True(presets[1].IsEnabled);
        Assert.True(presets[1].EnableExtraAlert);
        Assert.Equal(15, presets[1].ExtraAlertMinutes);

        // Index 14 (Last): Gravity Abyss (Legacy)
        Assert.Equal("gravity-abyss", presets[^1].Id);
        Assert.Equal("Gravity Abyss (Legacy)", presets[^1].Name);
        Assert.False(presets[^1].IsEnabled);
    }

    [Fact]
    public void EnsureDefaultPresets_MigratesLegacyOrder_SproutletAndEchoMovedToTop_GravityAbyssMovedToBottom()
    {
        // Simulate existing user settings from earlier versions with Gravity Abyss at top,
        // Aberrant Progeny in middle, and S5 near bottom
        var settings = new UserSettings
        {
            ActiveEventId = "gravity-abyss",
            Events = new List<EventDefinition>
            {
                new() { Id = "gravity-abyss", Name = "Gravity Abyss", IsEnabled = true },
                new() { Id = "prime-war", Name = "Prime War", IsEnabled = false },
                new() { Id = "aberrant-progeny", Name = "Aberrant Progeny", IsEnabled = false },
                new() { Id = "cargo-scramble", Name = "Cargo Scramble", IsEnabled = false },
                new() { Id = "dissonant-resonance", Name = "Dissonant Resonance", StartMinute = 22, IsEnabled = true }
            }
        };

        bool wasMigrated = settings.EnsureDefaultPresets();
        Assert.True(wasMigrated);

        // Check list ordering: Cargo Scramble is index 0, S5 The Echo is index 1
        Assert.Equal("cargo-scramble", settings.Events[0].Id);
        Assert.Equal("dissonant-resonance", settings.Events[1].Id);

        // Gravity Abyss is moved to the very bottom and disabled
        Assert.Equal("gravity-abyss", settings.Events[^1].Id);
        Assert.Equal("Gravity Abyss (Legacy)", settings.Events[^1].Name);
        Assert.False(settings.Events[^1].IsEnabled);

        // S5 has updated schedule XX:15:00
        var echo = settings.Events.Single(e => e.Id == "dissonant-resonance");
        Assert.Equal("S5: The Echo", echo.Name);
        Assert.Equal(15, echo.StartMinute);
        Assert.Equal(0, echo.StartSecond);

        // ActiveEventId was migrated away from legacy gravity-abyss to S5
        Assert.Equal("dissonant-resonance", settings.ActiveEventId);
    }

    [Fact]
    public void EnsureDefaultPresets_ReturnsFalseWhenAlreadyMigrated()
    {
        var settings = new UserSettings();
        // First call migrates or verifies defaults
        settings.EnsureDefaultPresets();

        // Second call on already up-to-date settings returns false
        bool secondRun = settings.EnsureDefaultPresets();
        Assert.False(secondRun);
    }

    [Fact]
    public void EnsureDefaultPresets_EnsuresAtLeastOneTopEventEnabled_WhenAllDisabled()
    {
        var settings = new UserSettings
        {
            Events = new List<EventDefinition>
            {
                new() { Id = "cargo-scramble", Name = "Cargo Scramble", IsEnabled = false },
                new() { Id = "dissonant-resonance", Name = "S5: The Echo", IsEnabled = false },
                new() { Id = "gravity-abyss", Name = "Gravity Abyss", IsEnabled = false }
            }
        };

        settings.EnsureDefaultPresets();

        // One of the top events should be automatically enabled
        Assert.Contains(settings.Events, e => e.IsEnabled);
        var echo = settings.Events.Single(e => e.Id == "dissonant-resonance");
        Assert.True(echo.IsEnabled);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("ru-RU")]
    public void Onboarding_AllFiveStepsHaveCompleteLocalization(string langCode)
    {
        _loc.SetLanguage(langCode);

        string[] stepKeys = ["Title", "Subtitle", "Desc", "Tip"];
        for (int i = 1; i <= 5; i++)
        {
            foreach (var key in stepKeys)
            {
                string fullKey = $"Onboarding_Step{i}_{key}";
                string value = _loc.GetString(fullKey);
                Assert.False(string.IsNullOrWhiteSpace(value), $"Missing onboarding translation for '{fullKey}' in {langCode}");
                Assert.DoesNotContain("[", value);
            }
        }
    }

    [Fact]
    public void Onboarding_DefaultPresets_OnlyS5IsEnabled_AllOtherDisabled()
    {
        var settings = new UserSettings();
        settings.EnsureDefaultPresets();

        var enabled = settings.Events.Where(e => e.IsEnabled).ToList();
        Assert.Single(enabled);
        Assert.Equal("dissonant-resonance", enabled[0].Id);
        Assert.Equal(15, enabled[0].StartMinute);
        Assert.Equal(0, enabled[0].StartSecond);
        Assert.Equal(15, enabled[0].ExtraAlertMinutes);

        var disabled = settings.Events.Where(e => !e.IsEnabled).ToList();
        Assert.Equal(14, disabled.Count);
        Assert.Contains(disabled, e => e.Id == "cargo-scramble");
        Assert.Contains(disabled, e => e.Id == "gravity-abyss");
        Assert.Contains(disabled, e => e.Id == "drift-greenhouse");
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("ru-RU")]
    public void Onboarding_Step3_Mentions15And30Minutes(string langCode)
    {
        _loc.SetLanguage(langCode);

        string subtitle = _loc.GetString("Onboarding_Step3_Subtitle");
        string desc = _loc.GetString("Onboarding_Step3_Desc");

        Assert.Contains("15", subtitle);
        Assert.Contains("15", desc);
        Assert.Contains("30", desc);

        Assert.DoesNotContain("XX:16", subtitle);
        Assert.DoesNotContain("XX:16", desc);
        Assert.DoesNotContain("XX:31", desc);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("ru-RU")]
    public void Onboarding_SelectEvents_HeaderAndHintKeysExistInAllCatalogs(string langCode)
    {
        _loc.SetLanguage(langCode);

        string header = _loc.GetString("Onboarding_SelectEvents");
        string hint = _loc.GetString("Onboarding_MultiTimerHint");

        Assert.False(string.IsNullOrWhiteSpace(header));
        Assert.False(string.IsNullOrWhiteSpace(hint));
        Assert.DoesNotContain("[", header);
        Assert.DoesNotContain("[", hint);
    }

    [Fact]
    public void EnsureDefaultPresets_PreservesCountdownToBoss_ForS5()
    {
        var settings = new UserSettings
        {
            Events = new List<EventDefinition>
            {
                new()
                {
                    Id = "dissonant-resonance",
                    Name = "Visional Wheel: The Echo",
                    StartMinute = 16,
                    CountdownToBoss = true,
                    IsEnabled = true
                }
            }
        };

        settings.EnsureDefaultPresets();

        var echo = settings.Events.Single(e => e.Id == "dissonant-resonance");
        Assert.Equal(15, echo.StartMinute);
        Assert.Equal(0, echo.StartSecond);
        Assert.True(echo.CountdownToBoss);
    }
}
