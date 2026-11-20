// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using OnceHumanEventTimer.Core.Localization.Catalogs;
using OnceHumanEventTimer.Core.Models;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class KillCounterSettingsTests
{
    [Fact]
    public void KillCounterSettings_DefaultValues_AreCorrect()
    {
        var settings = new KillCounterSettings();

        Assert.False(settings.IsEnabled);
        Assert.Equal("Shadow Hound", settings.BossName);
        Assert.Equal(0, settings.CurrentCount);
        Assert.False(settings.EnableGoal);
        Assert.Equal(20, settings.GoalCount);
        Assert.Equal(0, settings.TotalAllTimeCount);
        Assert.Equal("NumPad +", settings.IncrementHotkey);
        Assert.Equal("NumPad -", settings.DecrementHotkey);
        Assert.True(settings.PlaySound);
        Assert.False(settings.AutoStartBossTimer);
        Assert.Equal("#EF4444", settings.ColorHex);
        Assert.True(settings.ShowInMiniHud);
        Assert.True(settings.FlashOnGoalReached);
    }

    [Fact]
    public void KillCounterSettings_JsonSerialization_PreservesValues()
    {
        var original = new KillCounterSettings
        {
            IsEnabled = true,
            BossName = "Treant",
            CurrentCount = 14,
            EnableGoal = true,
            GoalCount = 50,
            TotalAllTimeCount = 120,
            IncrementHotkey = "Ctrl+NumPad +",
            DecrementHotkey = "Ctrl+NumPad -",
            PlaySound = false,
            AutoStartBossTimer = true,
            ColorHex = "#A855F7",
            ShowInMiniHud = false,
            FlashOnGoalReached = false
        };

        var userSettings = new UserSettings { KillCounter = original };
        string json = JsonSerializer.Serialize(userSettings);
        var restored = JsonSerializer.Deserialize<UserSettings>(json);

        Assert.NotNull(restored?.KillCounter);
        Assert.True(restored.KillCounter.IsEnabled);
        Assert.Equal("Treant", restored.KillCounter.BossName);
        Assert.Equal(14, restored.KillCounter.CurrentCount);
        Assert.True(restored.KillCounter.EnableGoal);
        Assert.Equal(50, restored.KillCounter.GoalCount);
        Assert.Equal(120, restored.KillCounter.TotalAllTimeCount);
        Assert.Equal("Ctrl+NumPad +", restored.KillCounter.IncrementHotkey);
        Assert.Equal("Ctrl+NumPad -", restored.KillCounter.DecrementHotkey);
        Assert.False(restored.KillCounter.PlaySound);
        Assert.True(restored.KillCounter.AutoStartBossTimer);
        Assert.Equal("#A855F7", restored.KillCounter.ColorHex);
        Assert.False(restored.KillCounter.ShowInMiniHud);
        Assert.False(restored.KillCounter.FlashOnGoalReached);
    }

    [Fact]
    public void KillCounterSettings_Clone_CreatesDistinctInstance()
    {
        var original = new KillCounterSettings
        {
            IsEnabled = true,
            BossName = "Giant",
            CurrentCount = 7,
            GoalCount = 15
        };

        var clone = original.Clone();
        clone.BossName = "Modified";
        clone.CurrentCount = 99;

        Assert.Equal("Giant", original.BossName);
        Assert.Equal(7, original.CurrentCount);
        Assert.Equal("Modified", clone.BossName);
        Assert.Equal(99, clone.CurrentCount);
    }

    [Fact]
    public void KillCounterSettings_IsMarkedSerializable()
    {
        var type = typeof(KillCounterSettings);
        Assert.True(type.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0, "KillCounterSettings must be marked with [Serializable].");
    }

    [Fact]
    public void KillCounterSettings_ClampsNegativeValuesAndInvalidGoalCount()
    {
        var settings = new KillCounterSettings
        {
            CurrentCount = -5,
            TotalAllTimeCount = -10,
            GoalCount = -1
        };

        Assert.Equal(0, settings.CurrentCount);
        Assert.Equal(0, settings.TotalAllTimeCount);
        Assert.Equal(1, settings.GoalCount);

        settings.GoalCount = 0;
        Assert.Equal(1, settings.GoalCount);
    }

    [Fact]
    public void KillCounter_LocalizationKeys_ExistInAllCatalogs()
    {
        var en = EnglishCatalog.GetEntries();
        var de = GermanCatalog.GetEntries();
        var ru = RussianCatalog.GetEntries();

        string[] requiredKeys =
        [
            "Settings_TabKillCounter",
            "KillCounter_CardTarget",
            "KillCounter_TargetName",
            "KillCounter_QuickPresets",
            "KillCounter_PresetGiant",
            "KillCounter_PresetManibus",
            "KillCounter_CurrentCount",
            "KillCounter_EnableGoal",
            "KillCounter_GoalCount",
            "KillCounter_TotalAllTime",
            "KillCounter_ResetSession",
            "KillCounter_CardActivation",
            "KillCounter_Enable",
            "KillCounter_IncrementHotkey",
            "KillCounter_DecrementHotkey",
            "KillCounter_HotkeyResetTooltip",
            "KillCounter_PlaySound",
            "KillCounter_AutoStartBossTimer",
            "KillCounter_ShowInMiniHud",
            "KillCounter_FlashOnGoalReached",
            "KillCounter_Color",
            "KillCounter_ColorTooltip",
            "KillCounter_Status_GoalReached",
            "KillCounter_GoalReachedTitle",
            "KillCounter_GoalReachedMsg",
            "Settings_HotkeyKillIncrement",
            "Settings_HotkeyKillIncrementDesc",
            "Settings_HotkeyKillDecrement",
            "Settings_HotkeyKillDecrementDesc",
            "KillCounter_BadgeKills",
            "KillCounter_BadgeGoal",
            "KillCounter_ResetPrompt"
        ];

        foreach (var key in requiredKeys)
        {
            Assert.True(en.ContainsKey(key), $"English catalog missing '{key}'");
            Assert.False(string.IsNullOrWhiteSpace(en[key]), $"English catalog empty value for '{key}'");

            Assert.True(de.ContainsKey(key), $"German catalog missing '{key}'");
            Assert.False(string.IsNullOrWhiteSpace(de[key]), $"German catalog empty value for '{key}'");

            Assert.True(ru.ContainsKey(key), $"Russian catalog missing '{key}'");
            Assert.False(string.IsNullOrWhiteSpace(ru[key]), $"Russian catalog empty value for '{key}'");
        }
    }
}
