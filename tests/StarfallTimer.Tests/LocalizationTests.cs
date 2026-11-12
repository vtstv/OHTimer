// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using StarfallTimer.Core.Localization;
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
    }

    [Fact]
    public void SwitchLanguage_ToRussian_ReturnsRussianStrings()
    {
        _loc.SetLanguage("ru-RU");
        Assert.Equal("ОЖИДАНИЕ", _loc.GetString("Status_Waiting"));
        Assert.Equal("ИНВЕРСИЯ АКТИВНА", _loc.GetString("Status_Active"));
    }

    [Fact]
    public void SwitchLanguage_ToChinese_ReturnsChineseStrings()
    {
        _loc.SetLanguage("zh-CN");
        Assert.Equal("等待中", _loc.GetString("Status_Waiting"));
        Assert.Equal("反转进行中", _loc.GetString("Status_Active"));
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
