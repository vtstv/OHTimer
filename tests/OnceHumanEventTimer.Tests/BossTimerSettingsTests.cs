// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using OnceHumanEventTimer.Core.Localization.Catalogs;
using OnceHumanEventTimer.Core.Models;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class BossTimerSettingsTests
{
    [Fact]
    public void BossTimerSettings_DefaultValues_AreCorrect()
    {
        var settings = new BossTimerSettings();

        Assert.True(settings.IsEnabled);
        Assert.Equal("NumPad 1", settings.Hotkey);
        Assert.Equal(BossTimerRepressBehavior.Restart, settings.RepressBehavior);
        Assert.True(settings.PlayHotkeySound);
        Assert.Equal("Boss Respawn", settings.BossName);
        Assert.Equal(5, settings.DurationMinutes);
        Assert.Equal(0, settings.DurationSeconds);
        Assert.True(settings.EnableWarningAlert);
        Assert.Equal(30, settings.WarningSecondsAhead);
        Assert.True(settings.EnableSpawnAlert);
        Assert.Equal(SoundPreset.EmergencySiren, settings.SpawnSoundPreset);
        Assert.True(settings.EnableRepeatAlert);
        Assert.Equal(3, settings.RepeatAlertCount);
        Assert.Equal(10, settings.RepeatAlertIntervalSeconds);
        Assert.True(settings.ShowToastOnSpawn);
        Assert.True(settings.FlashCardOnSpawn);
        Assert.Equal(15, settings.AutoHideSeconds);
        Assert.Equal("#EF4444", settings.ColorHex);
        Assert.Equal(TimeSpan.FromMinutes(5), settings.GetTotalDuration());
    }

    [Fact]
    public void BossTimerSettings_JsonSerialization_PreservesValues()
    {
        var original = new BossTimerSettings
        {
            BossName = "Shadow Prime",
            DurationMinutes = 12,
            DurationSeconds = 30,
            Hotkey = "F8",
            RepressBehavior = BossTimerRepressBehavior.Toggle,
            ColorHex = "#00F0FF",
            RepeatAlertCount = 5
        };

        var userSettings = new UserSettings { BossTimer = original };
        string json = JsonSerializer.Serialize(userSettings);
        var restored = JsonSerializer.Deserialize<UserSettings>(json);

        Assert.NotNull(restored?.BossTimer);
        Assert.Equal("Shadow Prime", restored.BossTimer.BossName);
        Assert.Equal(12, restored.BossTimer.DurationMinutes);
        Assert.Equal(30, restored.BossTimer.DurationSeconds);
        Assert.Equal("F8", restored.BossTimer.Hotkey);
        Assert.Equal(BossTimerRepressBehavior.Toggle, restored.BossTimer.RepressBehavior);
        Assert.Equal("#00F0FF", restored.BossTimer.ColorHex);
        Assert.Equal(5, restored.BossTimer.RepeatAlertCount);
    }

    [Fact]
    public void BossTimerSettings_NullOrWhitespace_FallbacksSafely()
    {
        var settings = new BossTimerSettings
        {
            BossName = "   ",
            ColorHex = "",
            Hotkey = null!
        };

        Assert.Equal("Boss Respawn", settings.BossName);
        Assert.Equal("#EF4444", settings.ColorHex);
        Assert.Equal("NumPad 1", settings.Hotkey);
    }

    [Fact]
    public void BossTimerSettings_ZeroOrNegativeDuration_FallbacksSafely()
    {
        var settings = new BossTimerSettings
        {
            DurationMinutes = -2,
            DurationSeconds = -10
        };

        Assert.Equal(TimeSpan.FromMinutes(5), settings.GetTotalDuration());

        settings.DurationMinutes = 0;
        settings.DurationSeconds = 0;
        Assert.Equal(TimeSpan.FromMinutes(5), settings.GetTotalDuration());
    }

    [Fact]
    public void UserSettings_BossTimer_NullSafety()
    {
        var userSettings = new UserSettings { BossTimer = null! };
        Assert.NotNull(userSettings.BossTimer);
        Assert.Equal("NumPad 1", userSettings.BossTimer.Hotkey);
    }

    [Fact]
    public void BossTimer_Localization_StrictBidirectionalParity()
    {
        var en = EnglishCatalog.GetEntries();
        var de = GermanCatalog.GetEntries();
        var ru = RussianCatalog.GetEntries();

        var bossKeys = en.Keys.Where(k => k.StartsWith("BossTimer_") || k == "Settings_TabBossTimer").ToList();
        Assert.NotEmpty(bossKeys);

        foreach (var key in bossKeys)
        {
            Assert.True(de.ContainsKey(key), $"German missing {key}");
            Assert.True(ru.ContainsKey(key), $"Russian missing {key}");
            Assert.False(string.IsNullOrWhiteSpace(en[key]));
            Assert.False(string.IsNullOrWhiteSpace(de[key]));
            Assert.False(string.IsNullOrWhiteSpace(ru[key]));
        }

        var extraDe = de.Keys.Where(k => (k.StartsWith("BossTimer_") || k == "Settings_TabBossTimer") && !en.ContainsKey(k));
        var extraRu = ru.Keys.Where(k => (k.StartsWith("BossTimer_") || k == "Settings_TabBossTimer") && !en.ContainsKey(k));

        Assert.Empty(extraDe);
        Assert.Empty(extraRu);
    }
}
