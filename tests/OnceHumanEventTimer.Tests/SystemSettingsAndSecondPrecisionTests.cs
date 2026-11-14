// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using OnceHumanEventTimer.Core.Localization;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Scheduling;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class SystemSettingsAndSecondPrecisionTests
{
    private readonly LocalizationService _loc = new();

    [Fact]
    public void EventTimeCalculator_WithStartSecond29_MatchesExactWebTrackerTriggerTime()
    {
        var gravityAbyss = new EventDefinition
        {
            Id = "gravity-abyss",
            Name = "Gravity Abyss",
            CycleMinutes = 60,
            StartMinute = 22,
            StartSecond = 29,
            DurationMinutes = 30,
            TimeOffsetSeconds = 0
        };

        // 1. One second before start (14:22:28) -> StartingSoon, 1 second remaining
        var oneSecBeforeStart = new DateTimeOffset(2026, 11, 13, 14, 22, 28, TimeSpan.Zero);
        var snapBefore = EventTimeCalculator.CalculateSnapshot(oneSecBeforeStart, gravityAbyss);
        Assert.Equal(EventPhase.StartingSoon, snapBefore.Phase);
        Assert.False(snapBefore.IsActive);
        Assert.Equal(TimeSpan.FromSeconds(1), snapBefore.TimeRemaining);
        Assert.Equal("00:01", snapBefore.FormattedRemaining);
        Assert.Equal(new DateTimeOffset(2026, 11, 13, 14, 22, 29, TimeSpan.Zero), snapBefore.NextEventStart);

        // 2. Exact start (14:22:29) -> Active, 30m remaining
        var exactStart = new DateTimeOffset(2026, 11, 13, 14, 22, 29, TimeSpan.Zero);
        var snapStart = EventTimeCalculator.CalculateSnapshot(exactStart, gravityAbyss);
        Assert.Equal(EventPhase.Active, snapStart.Phase);
        Assert.True(snapStart.IsActive);
        Assert.Equal(TimeSpan.FromMinutes(30), snapStart.TimeRemaining);
        Assert.Equal("30:00", snapStart.FormattedRemaining);

        // 3. One second before end (14:52:28) -> EndingSoon, 1 second remaining
        var oneSecBeforeEnd = new DateTimeOffset(2026, 11, 13, 14, 52, 28, TimeSpan.Zero);
        var snapNearEnd = EventTimeCalculator.CalculateSnapshot(oneSecBeforeEnd, gravityAbyss);
        Assert.Equal(EventPhase.EndingSoon, snapNearEnd.Phase);
        Assert.True(snapNearEnd.IsActive);
        Assert.Equal(TimeSpan.FromSeconds(1), snapNearEnd.TimeRemaining);

        // 4. Exact end (14:52:29) -> Inactive, 30m remaining until 15:22:29
        var exactEnd = new DateTimeOffset(2026, 11, 13, 14, 52, 29, TimeSpan.Zero);
        var snapEnd = EventTimeCalculator.CalculateSnapshot(exactEnd, gravityAbyss);
        Assert.Equal(EventPhase.Inactive, snapEnd.Phase);
        Assert.False(snapEnd.IsActive);
        Assert.Equal(TimeSpan.FromMinutes(30), snapEnd.TimeRemaining);
        Assert.Equal(new DateTimeOffset(2026, 11, 13, 15, 22, 29, TimeSpan.Zero), snapEnd.NextEventStart);
    }

    [Fact]
    public void EventDefinition_Clone_PreservesStartSecond()
    {
        var original = new EventDefinition
        {
            Id = "test-sec",
            Name = "Second Test",
            StartMinute = 15,
            StartSecond = 42,
            DurationMinutes = 20
        };

        var clone = original.Clone();
        Assert.Equal(42, clone.StartSecond);
    }

    [Fact]
    public void UserSettings_SystemProperties_SerializeCorrectly()
    {
        var settings = new UserSettings
        {
            StartWithWindows = true,
            AutoHideWhenGameOutOfFocus = false,
            StartMinute = 22,
            StartSecond = 29
        };

        string json = JsonSerializer.Serialize(settings);
        var deserialized = JsonSerializer.Deserialize<UserSettings>(json);

        Assert.NotNull(deserialized);
        Assert.True(deserialized.StartWithWindows);
        Assert.False(deserialized.AutoHideWhenGameOutOfFocus);
        Assert.Equal(22, deserialized.StartMinute);
        Assert.Equal(29, deserialized.StartSecond);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("ru-RU")]
    public void SystemLocalizationKeys_AreProperlyTranslated(string langCode)
    {
        _loc.SetLanguage(langCode);

        string[] keys =
        [
            "Settings_TabSystem",
            "Settings_StartWithWindows",
            "Settings_StartWithWindows_Desc",
            "Settings_AutoHideGameFocus",
            "Settings_AutoHideGameFocus_Desc",
            "Settings_StartSecond"
        ];

        foreach (var key in keys)
        {
            string translated = _loc.GetString(key);
            Assert.False(string.IsNullOrWhiteSpace(translated), $"Missing translation for {key} in {langCode}");
            Assert.DoesNotContain("[", translated);
        }
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("ru-RU")]
    public void ConfirmationDialogLocalizationKeys_AreProperlyTranslated(string langCode)
    {
        _loc.SetLanguage(langCode);

        string deleteConfirm = _loc.Format("Settings_DeleteEvent_Confirm", "Test Event");
        Assert.Contains("Test Event", deleteConfirm);

        string deleteTitle = _loc.GetString("Settings_DeleteEvent_Title");
        Assert.False(string.IsNullOrWhiteSpace(deleteTitle));

        string restoreConfirm = _loc.GetString("Settings_RestorePresets_Confirm");
        Assert.False(string.IsNullOrWhiteSpace(restoreConfirm));

        string restoreTitle = _loc.GetString("Settings_RestorePresets_Title");
        Assert.False(string.IsNullOrWhiteSpace(restoreTitle));
    }

    [Fact]
    public void EnsureDefaultPresets_MergesMissingPresets_IntoLegacyUserSettings()
    {
        // Simulate an old config that only had 2 events:
        var legacySettings = new UserSettings
        {
            Events = new List<EventDefinition>
            {
                new() { Id = "gravity-abyss", Name = "Gravity Abyss", IsEnabled = true },
                new() { Id = "cargo-scramble", Name = "Cargo Scramble", IsEnabled = false }
            }
        };

        Assert.Equal(2, legacySettings.Events.Count);

        legacySettings.EnsureDefaultPresets();

        // All 15 presets should now exist
        Assert.Equal(15, legacySettings.Events.Count);

        // Existing events retain their state
        var gravityAbyss = legacySettings.Events.First(e => e.Id == "gravity-abyss");
        Assert.True(gravityAbyss.IsEnabled);

        // Newly added presets are disabled by default
        var skyripper = legacySettings.Events.First(e => e.Id == "skyripper");
        Assert.False(skyripper.IsEnabled);
        Assert.Equal("Skyripper Anomaly", skyripper.Name);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("ru-RU")]
    public void PauseAlertsLocalizationKeys_AreProperlyTranslated(string langCode)
    {
        _loc.SetLanguage(langCode);

        string[] requiredKeys =
        [
            "HUD_PauseAlerts",
            "HUD_ResumeAlerts",
            "HUD_AlertsPausedToast",
            "HUD_AlertsResumedToast",
            "Settings_PauseAlerts",
            "Settings_HotkeyPauseAlerts",
            "Tray_PauseAlerts",
            "Tray_ResumeAlerts"
        ];

        foreach (var key in requiredKeys)
        {
            string value = _loc.GetString(key);
            Assert.False(string.IsNullOrWhiteSpace(value), $"Missing key {key} in {langCode}");
            Assert.DoesNotContain("[", value);
        }
    }

    [Fact]
    public void UserSettings_PauseAlertsAndHotkey_DefaultAndSerialization()
    {
        var settings = new UserSettings();
        Assert.False(settings.AreAlertsPaused);
        Assert.Equal("Ctrl+Alt+P", settings.TogglePauseAlertsHotkey);

        settings.AreAlertsPaused = true;
        settings.TogglePauseAlertsHotkey = "Ctrl+Shift+P";

        string json = JsonSerializer.Serialize(settings);
        var deserialized = JsonSerializer.Deserialize<UserSettings>(json);

        Assert.NotNull(deserialized);
        Assert.True(deserialized.AreAlertsPaused);
        Assert.Equal("Ctrl+Shift+P", deserialized.TogglePauseAlertsHotkey);
    }
}
