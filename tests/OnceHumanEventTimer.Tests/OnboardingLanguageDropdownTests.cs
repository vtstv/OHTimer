// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Localization;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class OnboardingLanguageDropdownTests
{
    private readonly LocalizationService _loc = new();

    [Fact]
    public void SupportedLanguages_HasConciseNativeNames_EnglishDeutschRussian()
    {
        var langs = _loc.SupportedLanguages;
        Assert.Equal(3, langs.Count);

        var en = langs.Single(l => l.Code == "en-US");
        Assert.Equal("English", en.NativeName);

        var de = langs.Single(l => l.Code == "de-DE");
        Assert.Equal("Deutsch", de.NativeName);

        var ru = langs.Single(l => l.Code == "ru-RU");
        Assert.Equal("Русский", ru.NativeName);
    }

    [Theory]
    [InlineData("en-US", "English")]
    [InlineData("en-us", "English")]
    [InlineData("EN-US", "English")]
    [InlineData("de-DE", "Deutsch")]
    [InlineData("de-de", "Deutsch")]
    [InlineData("DE-DE", "Deutsch")]
    [InlineData("ru-RU", "Русский")]
    [InlineData("ru-ru", "Русский")]
    [InlineData("RU-RU", "Русский")]
    public void LocalizationService_SetLanguage_CaseInsensitiveSupport(string langCode, string expectedNativeName)
    {
        _loc.SetLanguage(langCode);
        var activeLang = _loc.SupportedLanguages.FirstOrDefault(l =>
            string.Equals(l.Code, _loc.CurrentLanguage, StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(activeLang);
        Assert.Equal(expectedNativeName, activeLang.NativeName);
    }

    [Theory]
    [InlineData("de-DE", "Onboarding_Header")]
    [InlineData("ru-RU", "Onboarding_Header")]
    [InlineData("en-US", "Onboarding_Header")]
    public void Onboarding_HeaderLocalizationKey_ExistsInAllCatalogs(string langCode, string key)
    {
        _loc.SetLanguage(langCode);
        string text = _loc.GetString(key);
        Assert.False(string.IsNullOrWhiteSpace(text));
        Assert.DoesNotContain("[", text);
    }
}
