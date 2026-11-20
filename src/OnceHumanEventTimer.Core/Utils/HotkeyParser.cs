// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Utils;

/// <summary>
/// Universal parser for human-readable hotkey strings into standard Win32 Virtual Key codes and modifier flags.
/// Pure .NET 8 implementation with zero UI/WPF dependencies.
/// </summary>
public static class HotkeyParser
{
    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_WIN = 0x0008;

    public const uint VK_RETURN = 0x0D;
    public const uint VK_ESCAPE = 0x1B;
    public const uint VK_SPACE = 0x20;
    public const uint VK_TAB = 0x09;

    public const uint VK_NUMPAD0 = 0x60;
    public const uint VK_NUMPAD1 = 0x61;
    public const uint VK_NUMPAD2 = 0x62;
    public const uint VK_NUMPAD3 = 0x63;
    public const uint VK_NUMPAD4 = 0x64;
    public const uint VK_NUMPAD5 = 0x65;
    public const uint VK_NUMPAD6 = 0x66;
    public const uint VK_NUMPAD7 = 0x67;
    public const uint VK_NUMPAD8 = 0x68;
    public const uint VK_NUMPAD9 = 0x69;
    public const uint VK_MULTIPLY = 0x6A;
    public const uint VK_ADD = 0x6B;
    public const uint VK_SEPARATOR = 0x6C;
    public const uint VK_SUBTRACT = 0x6D;
    public const uint VK_DECIMAL = 0x6E;
    public const uint VK_DIVIDE = 0x6F;

    public const uint VK_OEM_PLUS = 0xBB;
    public const uint VK_OEM_COMMA = 0xBC;
    public const uint VK_OEM_MINUS = 0xBD;
    public const uint VK_OEM_PERIOD = 0xBE;
    public const uint VK_OEM_1 = 0xBA;
    public const uint VK_OEM_2 = 0xBF;
    public const uint VK_OEM_3 = 0xC0;
    public const uint VK_OEM_4 = 0xDB;
    public const uint VK_OEM_5 = 0xDC;
    public const uint VK_OEM_6 = 0xDD;
    public const uint VK_OEM_7 = 0xDE;

    public static bool TryParse(string? hotkey, out uint modifiers, out uint vk)
    {
        modifiers = 0;
        vk = 0;
        if (string.IsNullOrWhiteSpace(hotkey)) return false;

        string trimmed = hotkey.Trim();

        // Edge case: bare single-character symbols without modifiers
        if (trimmed == "+") { vk = VK_OEM_PLUS; return true; }
        if (trimmed == "-") { vk = VK_OEM_MINUS; return true; }
        if (trimmed == ",") { vk = VK_OEM_COMMA; return true; }
        if (trimmed == ".") { vk = VK_OEM_PERIOD; return true; }

        string preprocessed = trimmed
            .Replace("NumPad +", "NumPadPlus", StringComparison.OrdinalIgnoreCase)
            .Replace("NumPad+", "NumPadPlus", StringComparison.OrdinalIgnoreCase)
            .Replace("NumPad -", "NumPadMinus", StringComparison.OrdinalIgnoreCase)
            .Replace("NumPad-", "NumPadMinus", StringComparison.OrdinalIgnoreCase)
            .Replace("NumPad *", "NumPadMultiply", StringComparison.OrdinalIgnoreCase)
            .Replace("NumPad*", "NumPadMultiply", StringComparison.OrdinalIgnoreCase)
            .Replace("NumPad /", "NumPadDivide", StringComparison.OrdinalIgnoreCase)
            .Replace("NumPad/", "NumPadDivide", StringComparison.OrdinalIgnoreCase)
            .Replace("NumPad .", "NumPadDecimal", StringComparison.OrdinalIgnoreCase)
            .Replace("NumPad.", "NumPadDecimal", StringComparison.OrdinalIgnoreCase);

        if (preprocessed.EndsWith("++", StringComparison.Ordinal))
            preprocessed = preprocessed[..^1] + "OemPlus";
        else if (preprocessed.EndsWith("+ +", StringComparison.Ordinal))
            preprocessed = preprocessed[..^3] + "+OemPlus";
        else if (preprocessed.EndsWith("+-", StringComparison.Ordinal))
            preprocessed = preprocessed[..^1] + "OemMinus";
        else if (preprocessed.EndsWith("+ -", StringComparison.Ordinal))
            preprocessed = preprocessed[..^3] + "+OemMinus";

        var parts = preprocessed.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return false;

        string keyPart = parts[^1].Replace(" ", "");

        for (int i = 0; i < parts.Length - 1; i++)
        {
            string mod = parts[i].ToLowerInvariant();
            if (mod is "ctrl" or "control") modifiers |= MOD_CONTROL;
            else if (mod is "alt") modifiers |= MOD_ALT;
            else if (mod is "shift") modifiers |= MOD_SHIFT;
            else if (mod is "win" or "windows") modifiers |= MOD_WIN;
        }

        vk = ResolveVirtualKey(keyPart);
        return vk != 0;
    }

    private static uint ResolveVirtualKey(string key)
    {
        string lower = key.ToLowerInvariant();

        // 1. Numpad Keys
        switch (lower)
        {
            case "numpadplus" or "numpadadd" or "add": return VK_ADD;
            case "numpadminus" or "numpadsubtract" or "subtract": return VK_SUBTRACT;
            case "numpadmultiply" or "numpadstar" or "multiply": return VK_MULTIPLY;
            case "numpaddivide" or "numpadslash" or "divide": return VK_DIVIDE;
            case "numpaddecimal" or "numpaddot" or "decimal": return VK_DECIMAL;
            case "numpad0": return VK_NUMPAD0;
            case "numpad1": return VK_NUMPAD1;
            case "numpad2": return VK_NUMPAD2;
            case "numpad3": return VK_NUMPAD3;
            case "numpad4": return VK_NUMPAD4;
            case "numpad5": return VK_NUMPAD5;
            case "numpad6": return VK_NUMPAD6;
            case "numpad7": return VK_NUMPAD7;
            case "numpad8": return VK_NUMPAD8;
            case "numpad9": return VK_NUMPAD9;
        }

        // 2. Letters A-Z
        if (lower.Length == 1 && lower[0] >= 'a' && lower[0] <= 'z')
        {
            return (uint)(0x41 + (lower[0] - 'a'));
        }

        // 3. Top-row digits 0-9
        if (lower.Length == 1 && lower[0] >= '0' && lower[0] <= '9')
        {
            return (uint)(0x30 + (lower[0] - '0'));
        }
        if (lower.StartsWith('d') && lower.Length == 2 && lower[1] >= '0' && lower[1] <= '9')
        {
            return (uint)(0x30 + (lower[1] - '0'));
        }

        // 4. Function keys F1-F24
        if (lower.StartsWith('f') && int.TryParse(lower[1..], out int fIndex) && fIndex >= 1 && fIndex <= 24)
        {
            return (uint)(0x70 + (fIndex - 1));
        }

        // 5. OEM Punctuation & Symbols
        return lower switch
        {
            "+" or "plus" or "oemplus" => VK_OEM_PLUS,
            "-" or "minus" or "oemminus" => VK_OEM_MINUS,
            "," or "comma" or "oemcomma" => VK_OEM_COMMA,
            "." or "period" or "oemperiod" => VK_OEM_PERIOD,
            "oem1" or "semicolon" or ";" => VK_OEM_1,
            "oem2" or "slash" or "/" or "?" => VK_OEM_2,
            "oem3" or "tilde" or "backquote" or "`" => VK_OEM_3,
            "oem4" or "openbracket" or "[" => VK_OEM_4,
            "oem5" or "backslash" or "\\" => VK_OEM_5,
            "oem6" or "closebracket" or "]" => VK_OEM_6,
            "oem7" or "quote" or "'" => VK_OEM_7,
            "space" => VK_SPACE,
            "escape" or "esc" => VK_ESCAPE,
            "tab" => VK_TAB,
            "enter" or "return" => VK_RETURN,
            _ => 0
        };
    }
}
