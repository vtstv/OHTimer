// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Scheduling;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class GameTimeParserTests
{
    [Theory]
    [InlineData("08:23", 0, 8, 23)]
    [InlineData("00:45", 0, 0, 45)]
    [InlineData("12:00", 0, 12, 0)]
    [InlineData("01:03:29", 1, 3, 29)]
    [InlineData("2:29:04", 2, 29, 4)]
    [InlineData("0:15:30", 0, 15, 30)]
    public void TryParse_ColonFormats_Succeeds(string input, int expectedHours, int expectedMinutes, int expectedSeconds)
    {
        bool success = GameTimeParser.TryParse(input, out var result);

        Assert.True(success);
        Assert.Equal(new TimeSpan(expectedHours, expectedMinutes, expectedSeconds), result);
    }

    [Theory]
    [InlineData("1h 3m 29s", 1, 3, 29)]
    [InlineData("2 h 29 m 4 s", 2, 29, 4)]
    [InlineData("2 h 54 m 4 s", 2, 54, 4)]
    [InlineData("1h3m29s", 1, 3, 29)]
    [InlineData("45m", 0, 45, 0)]
    [InlineData("30s", 0, 0, 30)]
    [InlineData("2h", 2, 0, 0)]
    [InlineData("2 hours 30 minutes 15 seconds", 2, 30, 15)]
    public void TryParse_EnglishNaturalFormats_Succeeds(string input, int expectedHours, int expectedMinutes, int expectedSeconds)
    {
        bool success = GameTimeParser.TryParse(input, out var result);

        Assert.True(success);
        Assert.Equal(new TimeSpan(expectedHours, expectedMinutes, expectedSeconds), result);
    }

    [Theory]
    [InlineData("1 Std 3 Min 29 Sek", 1, 3, 29)]
    [InlineData("1 Std. 3 Min. 29 Sek.", 1, 3, 29)]
    [InlineData("2 Stunden 15 Minuten 10 Sekunden", 2, 15, 10)]
    [InlineData("45 Min", 0, 45, 0)]
    [InlineData("30 Sek", 0, 0, 30)]
    public void TryParse_GermanNaturalFormats_Succeeds(string input, int expectedHours, int expectedMinutes, int expectedSeconds)
    {
        bool success = GameTimeParser.TryParse(input, out var result);

        Assert.True(success);
        Assert.Equal(new TimeSpan(expectedHours, expectedMinutes, expectedSeconds), result);
    }

    [Theory]
    [InlineData("1ч 3м 29с", 1, 3, 29)]
    [InlineData("1 ч 3 м 29 с", 1, 3, 29)]
    [InlineData("2 часа 15 минут 30 секунд", 2, 15, 30)]
    [InlineData("45 мин", 0, 45, 0)]
    [InlineData("30 сек", 0, 0, 30)]
    public void TryParse_RussianNaturalFormats_Succeeds(string input, int expectedHours, int expectedMinutes, int expectedSeconds)
    {
        bool success = GameTimeParser.TryParse(input, out var result);

        Assert.True(success);
        Assert.Equal(new TimeSpan(expectedHours, expectedMinutes, expectedSeconds), result);
    }

    [Theory]
    [InlineData("Opens at 2 h 29 m 4 s", 2, 29, 4)]
    [InlineData("Opens at 1 h 3 m 29 s", 1, 3, 29)]
    [InlineData("Opens in 45m", 0, 45, 0)]
    [InlineData("Öffnet in 1 Std 3 Min 29 Sek", 1, 3, 29)]
    [InlineData("Откроется через 1ч 3м 29с", 1, 3, 29)]
    [InlineData("Opens at: 01:03:29", 1, 3, 29)]
    public void TryParse_PrefixedInGameStrings_Succeeds(string input, int expectedHours, int expectedMinutes, int expectedSeconds)
    {
        bool success = GameTimeParser.TryParse(input, out var result);

        Assert.True(success);
        Assert.Equal(new TimeSpan(expectedHours, expectedMinutes, expectedSeconds), result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("00:00")]
    [InlineData("0s")]
    [InlineData("not a time")]
    [InlineData("abc:def")]
    public void TryParse_InvalidOrZeroInputs_ReturnsFalse(string? input)
    {
        bool success = GameTimeParser.TryParse(input, out var result);

        Assert.False(success);
        Assert.Equal(TimeSpan.Zero, result);
    }
}
