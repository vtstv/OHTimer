// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using OnceHumanEventTimer.Core.Localization.Catalogs;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Utils;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class HotkeyAndPriorityTests
{
    [Fact]
    public void HotkeyParser_ParsesNumpadKeysCorrectly()
    {
        Assert.True(HotkeyParser.TryParse("NumPad +", out uint mod1, out uint vk1));
        Assert.Equal(0u, mod1);
        Assert.Equal(HotkeyParser.VK_ADD, vk1);

        Assert.True(HotkeyParser.TryParse("NumPad -", out uint mod2, out uint vk2));
        Assert.Equal(0u, mod2);
        Assert.Equal(HotkeyParser.VK_SUBTRACT, vk2);

        Assert.True(HotkeyParser.TryParse("NumPad 1", out uint mod3, out uint vk3));
        Assert.Equal(0u, mod3);
        Assert.Equal(HotkeyParser.VK_NUMPAD1, vk3);

        // Without spaces
        Assert.True(HotkeyParser.TryParse("NumPad+", out uint mod4, out uint vk4));
        Assert.Equal(0u, mod4);
        Assert.Equal(HotkeyParser.VK_ADD, vk4);

        Assert.True(HotkeyParser.TryParse("NumPad-", out uint mod5, out uint vk5));
        Assert.Equal(0u, mod5);
        Assert.Equal(HotkeyParser.VK_SUBTRACT, vk5);
    }

    [Fact]
    public void HotkeyParser_ParsesBarePlusAndMinusCorrectly()
    {
        Assert.True(HotkeyParser.TryParse("+", out uint mod1, out uint vk1));
        Assert.Equal(0u, mod1);
        Assert.Equal(HotkeyParser.VK_OEM_PLUS, vk1);

        Assert.True(HotkeyParser.TryParse("-", out uint mod2, out uint vk2));
        Assert.Equal(0u, mod2);
        Assert.Equal(HotkeyParser.VK_OEM_MINUS, vk2);
    }

    [Fact]
    public void HotkeyParser_ParsesCombinationsWithModifiers()
    {
        // Toggle Settings Hotkey: Ctrl+Alt+O
        Assert.True(HotkeyParser.TryParse("Ctrl+Alt+O", out uint mod1, out uint vk1));
        Assert.Equal(HotkeyParser.MOD_CONTROL | HotkeyParser.MOD_ALT, mod1);
        Assert.Equal((uint)'O', vk1);

        // Pause alerts: Ctrl+Alt+P
        Assert.True(HotkeyParser.TryParse("Ctrl+Alt+P", out uint mod2, out uint vk2));
        Assert.Equal(HotkeyParser.MOD_CONTROL | HotkeyParser.MOD_ALT, mod2);
        Assert.Equal((uint)'P', vk2);

        // Kill counter increment with modifiers: Ctrl++ or Ctrl+NumPad +
        Assert.True(HotkeyParser.TryParse("Ctrl++", out uint mod3, out uint vk3));
        Assert.Equal(HotkeyParser.MOD_CONTROL, mod3);
        Assert.Equal(HotkeyParser.VK_OEM_PLUS, vk3);

        Assert.True(HotkeyParser.TryParse("Ctrl+NumPad +", out uint mod4, out uint vk4));
        Assert.Equal(HotkeyParser.MOD_CONTROL, mod4);
        Assert.Equal(HotkeyParser.VK_ADD, vk4);

        Assert.True(HotkeyParser.TryParse("Ctrl+-", out uint mod5, out uint vk5));
        Assert.Equal(HotkeyParser.MOD_CONTROL, mod5);
        Assert.Equal(HotkeyParser.VK_OEM_MINUS, vk5);
    }

    [Fact]
    public void HotkeyParser_ParsesAllOemKeysCorrectly()
    {
        Assert.True(HotkeyParser.TryParse(";", out _, out uint vk1));
        Assert.Equal(HotkeyParser.VK_OEM_1, vk1);

        Assert.True(HotkeyParser.TryParse("/", out _, out uint vk2));
        Assert.Equal(HotkeyParser.VK_OEM_2, vk2);

        Assert.True(HotkeyParser.TryParse("`", out _, out uint vk3));
        Assert.Equal(HotkeyParser.VK_OEM_3, vk3);

        Assert.True(HotkeyParser.TryParse("[", out _, out uint vk4));
        Assert.Equal(HotkeyParser.VK_OEM_4, vk4);

        Assert.True(HotkeyParser.TryParse(@"\", out _, out uint vk5));
        Assert.Equal(HotkeyParser.VK_OEM_5, vk5);

        Assert.True(HotkeyParser.TryParse("]", out _, out uint vk6));
        Assert.Equal(HotkeyParser.VK_OEM_6, vk6);

        Assert.True(HotkeyParser.TryParse("'", out _, out uint vk7));
        Assert.Equal(HotkeyParser.VK_OEM_7, vk7);

        // Named equivalents
        Assert.True(HotkeyParser.TryParse("Oem1", out _, out uint vk8));
        Assert.Equal(HotkeyParser.VK_OEM_1, vk8);

        Assert.True(HotkeyParser.TryParse("Ctrl+Alt+OemComma", out uint mod9, out uint vk9));
        Assert.Equal(HotkeyParser.MOD_CONTROL | HotkeyParser.MOD_ALT, mod9);
        Assert.Equal(HotkeyParser.VK_OEM_COMMA, vk9);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("UnknownKey")]
    [InlineData("Ctrl+")]
    public void HotkeyParser_ReturnsFalseForInvalidHotkeys(string invalidKey)
    {
        Assert.False(HotkeyParser.TryParse(invalidKey, out _, out _));
    }

    [Fact]
    public void ToggleSettingsHotkey_DefaultsToCtrlAltO_AndPreservesInJson()
    {
        var settings = new UserSettings();
        Assert.Equal("Ctrl+Alt+O", settings.ToggleSettingsHotkey);

        settings.ToggleSettingsHotkey = "Ctrl+Shift+O";
        string json = JsonSerializer.Serialize(settings);
        var restored = JsonSerializer.Deserialize<UserSettings>(json);

        Assert.NotNull(restored);
        Assert.Equal("Ctrl+Shift+O", restored.ToggleSettingsHotkey);
    }

    [Fact]
    public void KillCounterSettings_IsEnabled_DefaultsToFalse()
    {
        var kc = new KillCounterSettings();
        Assert.False(kc.IsEnabled);

        var us = new UserSettings();
        Assert.False(us.KillCounter.IsEnabled);
    }

    [Fact]
    public void DefaultEventPresets_PriorityIsEchoAtTop_SproutletSecond()
    {
        var presets = DefaultEventPresets.CreateDefaultEvents();

        // S5: The Echo must be index 0
        Assert.Equal("dissonant-resonance", presets[0].Id);
        Assert.Equal("S5: The Echo", presets[0].Name);
        Assert.True(presets[0].IsEnabled);

        // Cargo Scramble must be index 1
        Assert.Equal("cargo-scramble", presets[1].Id);
        Assert.Equal("Cargo Scramble (Sproutlet Truck)", presets[1].Name);
    }

    [Fact]
    public void PresetRenaming_ManibusAndGiant_ConfiguredCorrectly()
    {
        var presets = DefaultEventPresets.CreateDefaultEvents();
        var primeWar = presets.Single(e => e.Id == "prime-war");
        Assert.Equal("Manibus (Prime War)", primeWar.Name);

        var en = EnglishCatalog.GetEntries();
        var de = GermanCatalog.GetEntries();
        var ru = RussianCatalog.GetEntries();

        Assert.Equal("Manibus (Prime War)", en["Event_PrimeWar_Name"]);
        Assert.Equal("Manibus (Erster Krieg)", de["Event_PrimeWar_Name"]);
        Assert.Equal("Манибус (Оборона Кошмара)", ru["Event_PrimeWar_Name"]);

        Assert.Equal("Giant", en["KillCounter_PresetGiant"]);
        Assert.Equal("Giant", de["KillCounter_PresetGiant"]);
        Assert.Equal("Giant", ru["KillCounter_PresetGiant"]);

        Assert.Equal("Manibus", en["KillCounter_PresetManibus"]);
        Assert.Equal("Manibus", de["KillCounter_PresetManibus"]);
        Assert.Equal("Манибус", ru["KillCounter_PresetManibus"]);
    }

    [Fact]
    public void EnsureDefaultPresets_MigratesOldPrimeWarNameToManibus()
    {
        var settings = new UserSettings
        {
            Events = new List<EventDefinition>
            {
                new() { Id = "prime-war", Name = "Prime War (Nightmare Defense)", IsEnabled = false }
            }
        };

        bool migrated = settings.EnsureDefaultPresets();
        Assert.True(migrated);

        var pw = settings.Events.Single(e => e.Id == "prime-war");
        Assert.Equal("Manibus (Prime War)", pw.Name);
    }

    [Fact]
    public void DefaultHotkeys_AllParseSuccessfully()
    {
        var settings = new UserSettings();
        Assert.True(HotkeyParser.TryParse(settings.BossTimer.Hotkey, out _, out uint bossVk));
        Assert.Equal(HotkeyParser.VK_NUMPAD1, bossVk);

        Assert.True(HotkeyParser.TryParse(settings.KillCounter.IncrementHotkey, out _, out uint incVk));
        Assert.Equal(HotkeyParser.VK_ADD, incVk);

        Assert.True(HotkeyParser.TryParse(settings.KillCounter.DecrementHotkey, out _, out uint decVk));
        Assert.Equal(HotkeyParser.VK_SUBTRACT, decVk);

        Assert.True(HotkeyParser.TryParse(settings.ToggleSettingsHotkey, out uint setMod, out uint setVk));
        Assert.Equal(HotkeyParser.MOD_CONTROL | HotkeyParser.MOD_ALT, setMod);
        Assert.Equal((uint)'O', setVk);

        Assert.True(HotkeyParser.TryParse(settings.ToggleVisibilityHotkey, out uint visMod, out uint visVk));
        Assert.Equal(HotkeyParser.MOD_CONTROL | HotkeyParser.MOD_ALT, visMod);
        Assert.Equal((uint)'S', visVk);

        Assert.True(HotkeyParser.TryParse(settings.TogglePauseAlertsHotkey, out uint pauseMod, out uint pauseVk));
        Assert.Equal(HotkeyParser.MOD_CONTROL | HotkeyParser.MOD_ALT, pauseMod);
        Assert.Equal((uint)'P', pauseVk);
    }
}
