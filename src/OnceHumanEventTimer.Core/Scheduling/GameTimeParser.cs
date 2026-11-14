// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.RegularExpressions;

namespace OnceHumanEventTimer.Core.Scheduling;

/// <summary>
/// Robust parser for in-game countdown strings across multiple formats and languages
/// (e.g. "08:23", "01:03:29", "1h 3m 29s", "Opens at 2 h 29 m 4 s", "1 Std 3 Min 29 Sek", "1ч 3м 29с").
/// </summary>
public static class GameTimeParser
{
    private static readonly Regex PrefixRegex = new(
        @"^(?:opens?\s+(?:at|in)|öffnet\s+(?:um|in)|откроется\s+(?:через|в))\s*:?\s*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex NaturalTimeRegex = new(
        @"^(?:(?<h>\d+)\s*(?:h(?:ours?|rs?)?|ч(?:ас(?:а|ов)?)?|std\.?|stunden?)\s*)?" +
        @"(?:(?<m>\d+)\s*(?:m(?:in(?:utes?)?|ins?)?|м(?:ин(?:ут[ыа]?)?)?|minuten?|min\.?)\s*)?" +
        @"(?:(?<s>\d+)\s*(?:s(?:ec(?:onds?)?|ecs?)?|с(?:ек(?:унд[ыа]?)?)?|sekunden?|sek\.?)\s*)?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Attempts to parse an in-game countdown or remaining time string into a TimeSpan.
    /// </summary>
    public static bool TryParse(string? input, out TimeSpan result)
    {
        result = TimeSpan.Zero;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        string raw = input.Trim();
        raw = PrefixRegex.Replace(raw, string.Empty).Trim();

        // 1. Check colon-delimited formats (mm:ss or hh:mm:ss)
        var parts = raw.Split(':');
        if (parts.Length == 2 && int.TryParse(parts[0], out int mins) && int.TryParse(parts[1], out int secs))
        {
            if (mins >= 0 && secs >= 0 && secs < 60)
            {
                result = new TimeSpan(0, mins, secs);
                return result > TimeSpan.Zero;
            }
        }
        else if (parts.Length == 3 && int.TryParse(parts[0], out int hrs) && int.TryParse(parts[1], out int m) && int.TryParse(parts[2], out int s))
        {
            if (hrs >= 0 && m >= 0 && m < 60 && s >= 0 && s < 60)
            {
                result = new TimeSpan(hrs, m, s);
                return result > TimeSpan.Zero;
            }
        }

        // 2. Check natural language multi-lingual format (e.g. "2 h 29 m 4 s", "1 Std 3 Min 29 Sek", "1ч 3м 29с")
        var match = NaturalTimeRegex.Match(raw);
        if (match.Success && (match.Groups["h"].Success || match.Groups["m"].Success || match.Groups["s"].Success))
        {
            int hours = match.Groups["h"].Success ? int.Parse(match.Groups["h"].Value) : 0;
            int minutes = match.Groups["m"].Success ? int.Parse(match.Groups["m"].Value) : 0;
            int seconds = match.Groups["s"].Success ? int.Parse(match.Groups["s"].Value) : 0;

            result = new TimeSpan(hours, minutes, seconds);
            return result > TimeSpan.Zero;
        }

        // 3. Fallback to standard TimeSpan.TryParse
        if (TimeSpan.TryParse(raw, out var parsed) && parsed > TimeSpan.Zero)
        {
            result = parsed;
            return true;
        }

        return false;
    }
}
