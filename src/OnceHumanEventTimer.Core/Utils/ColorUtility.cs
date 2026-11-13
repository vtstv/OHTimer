// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;

namespace OnceHumanEventTimer.Core.Utils;

/// <summary>
/// Utility for parsing, validating, and converting color HEX representations.
/// </summary>
public static class ColorUtility
{
    public const string DefaultCyan = "#00F0FF";

    /// <summary>
    /// Normalizes a HEX color string into canonical #RRGGBB uppercase format.
    /// Supports #RGB, #RRGGBB, and #AARRGGBB (alpha stripped).
    /// </summary>
    public static string NormalizeHex(string? hex, string fallback = DefaultCyan)
    {
        if (string.IsNullOrWhiteSpace(hex)) return fallback;

        hex = hex.Trim().TrimStart('#');
        if (hex.Length == 8)
        {
            hex = hex[2..]; // Strip alpha prefix
        }
        else if (hex.Length == 3)
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }

        if (hex.Length == 6 &&
            int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
        {
            return $"#{hex.ToUpperInvariant()}";
        }

        return fallback;
    }

    /// <summary>
    /// Parses a HEX string into RGB byte components.
    /// </summary>
    public static bool TryParseRgb(string? hex, out byte r, out byte g, out byte b)
    {
        r = g = b = 0;
        string normalized = NormalizeHex(hex, string.Empty);
        if (string.IsNullOrEmpty(normalized)) return false;

        string clean = normalized.TrimStart('#');
        if (byte.TryParse(clean[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r) &&
            byte.TryParse(clean[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out g) &&
            byte.TryParse(clean[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Converts RGB byte components into canonical #RRGGBB hex format.
    /// </summary>
    public static string ToHex(byte r, byte g, byte b) => $"#{r:X2}{g:X2}{b:X2}";
}
