// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using StarfallTimer.Core.Localization;
using StarfallTimer.Core.Localization.Catalogs;
using Xunit;

namespace StarfallTimer.Tests;

public class LocalizationTests
{
    private readonly LocalizationService _loc = new();

    [Fact]
    public void DefaultLanguage_IsEnglish()
    {
        Assert.Equal("en-US", _loc.CurrentLanguage);
        Assert.Equal("WAITING", _loc.GetString("Status_Waiting"));
        Assert.Equal("EVENT ACTIVE", _loc.GetString("Status_Active"));
    }

    [Fact]
    public void SwitchLanguage_ToRussian_ReturnsRussianStrings()
    {
        _loc.SetLanguage("ru-RU");
        Assert.Equal("ОЖИДАНИЕ", _loc.GetString("Status_Waiting"));
        Assert.Equal("СОБЫТИЕ АКТИВНО", _loc.GetString("Status_Active"));
    }

    [Fact]
    public void SwitchLanguage_ToGerman_ReturnsGermanStrings()
    {
        _loc.SetLanguage("de-DE");
        Assert.Equal("WARTEN", _loc.GetString("Status_Waiting"));
        Assert.Equal("EVENT AKTIV", _loc.GetString("Status_Active"));
        Assert.Equal("BEGINNT BALD", _loc.GetString("Status_StartingSoon"));
        Assert.Equal("ENDET BALD", _loc.GetString("Status_EndingSoon"));
        Assert.Equal("EVENT BEGINNT IN", _loc.GetString("HUD_UntilStart"));
        Assert.Equal("EVENT ENDET IN", _loc.GetString("HUD_UntilEnd"));
    }

    [Fact]
    public void GermanCatalog_ContainsAllKeysDefinedInEnglishCatalog()
    {
        var englishKeys = EnglishCatalog.GetEntries().Keys;
        var germanEntries = GermanCatalog.GetEntries();

        foreach (var key in englishKeys)
        {
            Assert.True(germanEntries.ContainsKey(key), $"GermanCatalog is missing key '{key}'");
            Assert.False(string.IsNullOrWhiteSpace(germanEntries[key]), $"GermanCatalog has empty value for key '{key}'");
        }
    }

    [Fact]
    public void GermanCatalog_FormatsPlaceholdersProperly()
    {
        _loc.SetLanguage("de-DE");

        string startingSoon = _loc.Format("Notification_StartingSoon_Body", 5);
        Assert.Contains("5 Minuten", startingSoon);

        string endingSoon = _loc.Format("Notification_EndingSoon_Body", 2);
        Assert.Contains("2 Minuten", endingSoon);

        string syncSuccess = _loc.Format("Settings_SyncSuccess", 31);
        Assert.Contains("31s", syncSuccess);
    }

    [Fact]
    public void RussianCatalog_ContainsAllKeysDefinedInEnglishCatalog()
    {
        var englishKeys = EnglishCatalog.GetEntries().Keys;
        var russianEntries = RussianCatalog.GetEntries();

        foreach (var key in englishKeys)
        {
            Assert.True(russianEntries.ContainsKey(key), $"RussianCatalog is missing key '{key}'");
            Assert.False(string.IsNullOrWhiteSpace(russianEntries[key]), $"RussianCatalog has empty value for key '{key}'");
        }
    }

    [Fact]
    public void Catalogs_TranslateEventNamesCorrectly()
    {
        _loc.SetLanguage("en-US");
        Assert.Equal("Gravity Abyss (Starfall Inversion)", _loc.GetString("Event_GravityAbyss_Name"));
        Assert.Equal("Cargo Scramble (Sproutlet Truck)", _loc.GetString("Event_CargoScramble_Name"));

        _loc.SetLanguage("de-DE");
        Assert.Equal("Gravitationsabgrund (Sternenfall-Inversion)", _loc.GetString("Event_GravityAbyss_Name"));
        Assert.Equal("Frachtgerangel (Spross-LKW)", _loc.GetString("Event_CargoScramble_Name"));

        _loc.SetLanguage("ru-RU");
        Assert.Equal("Гравитационный провал (Инверсия Звездопада)", _loc.GetString("Event_GravityAbyss_Name"));
        Assert.Equal("Заварушка с грузом (Грузовик со спорками)", _loc.GetString("Event_CargoScramble_Name"));
    }

    [Fact]
    public void MissingKey_FallsBackToEnglishOrKeyName()
    {
        _loc.SetLanguage("ru-RU");
        string nonExistent = _loc.GetString("Totally_Unknown_Key_123");
        Assert.Equal("[Totally_Unknown_Key_123]", nonExistent);
    }

    [Fact]
    public void Format_SubstitutesArgumentsCorrectly()
    {
        _loc.SetLanguage("en-US");
        string formatted = _loc.Format("Notification_StartingSoon_Body", 2);
        Assert.Contains("2 minutes", formatted);
    }
}
