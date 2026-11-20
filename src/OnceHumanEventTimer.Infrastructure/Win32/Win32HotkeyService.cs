// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Interop;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.Infrastructure.Win32;

/// <summary>
/// Registers and handles system-wide hotkeys using Win32 RegisterHotKey API and WPF HwndSource.
/// </summary>
public sealed class Win32HotkeyService : IHotkeyService
{
    private readonly Dictionary<int, Action> _hotkeyActions = new();
    private nint _windowHandle = 0;
    private HwndSource? _hwndSource;
    private int _currentId = 1000;

    /// <summary>
    /// Initializes the hotkey hook on an existing window handle.
    /// </summary>
    public void Initialize(nint hwnd)
    {
        _windowHandle = hwnd;
        _hwndSource = HwndSource.FromHwnd(hwnd);
        _hwndSource?.AddHook(HwndHook);
    }

    public bool RegisterHotkey(string hotkeyString, Action callback)
    {
        if (_windowHandle == 0 || string.IsNullOrWhiteSpace(hotkeyString))
        {
            return false;
        }

        if (!TryParseHotkey(hotkeyString, out uint modifiers, out uint vk))
        {
            return false;
        }

        int id = Interlocked.Increment(ref _currentId);
        bool success = NativeMethods.RegisterHotKey(_windowHandle, id, modifiers | NativeMethods.MOD_NOREPEAT, vk);
        if (!success)
        {
            success = NativeMethods.RegisterHotKey(_windowHandle, id, modifiers, vk);
        }

        if (success)
        {
            _hotkeyActions[id] = callback;
        }

        return success;
    }

    public void UnregisterAll()
    {
        if (_windowHandle == 0) return;

        foreach (var id in _hotkeyActions.Keys)
        {
            NativeMethods.UnregisterHotKey(_windowHandle, id);
        }
        _hotkeyActions.Clear();
    }

    private nint HwndHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            if (_hotkeyActions.TryGetValue(id, out var action))
            {
                action.Invoke();
                handled = true;
            }
        }
        return 0;
    }

    internal static bool TryParseHotkey(string hotkey, out uint modifiers, out uint vk)
    {
        modifiers = 0;
        vk = 0;
        if (string.IsNullOrWhiteSpace(hotkey)) return false;

        string preprocessed = hotkey
            .Replace("NumPad +", "NumPadPlus", StringComparison.OrdinalIgnoreCase)
            .Replace("NumPad -", "NumPadMinus", StringComparison.OrdinalIgnoreCase)
            .Replace("NumPad *", "NumPadMultiply", StringComparison.OrdinalIgnoreCase)
            .Replace("NumPad /", "NumPadDivide", StringComparison.OrdinalIgnoreCase)
            .Replace("NumPad .", "NumPadDecimal", StringComparison.OrdinalIgnoreCase);

        if (preprocessed.EndsWith("++", StringComparison.Ordinal))
            preprocessed = preprocessed[..^1] + "OemPlus";
        else if (preprocessed.EndsWith("+ +", StringComparison.Ordinal))
            preprocessed = preprocessed[..^3] + "+OemPlus";

        var parts = preprocessed.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return false;

        string keyPart = parts[^1];
        string normalizedKey = keyPart.Replace(" ", "");
        if (normalizedKey.Length == 1 && char.IsDigit(normalizedKey[0]))
        {
            normalizedKey = "D" + normalizedKey;
        }

        for (int i = 0; i < parts.Length - 1; i++)
        {
            string mod = parts[i].ToLowerInvariant();
            if (mod is "ctrl" or "control") modifiers |= NativeMethods.MOD_CONTROL;
            else if (mod is "alt") modifiers |= NativeMethods.MOD_ALT;
            else if (mod is "shift") modifiers |= NativeMethods.MOD_SHIFT;
            else if (mod is "win" or "windows") modifiers |= NativeMethods.MOD_WIN;
        }

        string mapped = normalizedKey.ToLowerInvariant() switch
        {
            "numpadplus" or "numpadadd" => "Add",
            "numpadminus" or "numpadsubtract" => "Subtract",
            "numpadmultiply" or "numpadstar" => "Multiply",
            "numpaddivide" or "numpadslash" => "Divide",
            "numpaddecimal" or "numpaddot" => "Decimal",
            _ => normalizedKey
        };

        if (Enum.TryParse<System.Windows.Input.Key>(mapped, true, out var key))
        {
            vk = (uint)System.Windows.Input.KeyInterop.VirtualKeyFromKey(key);
            return vk != 0;
        }

        return false;
    }

    public void Dispose()
    {
        UnregisterAll();
        if (_hwndSource != null)
        {
            _hwndSource.RemoveHook(HwndHook);
            _hwndSource = null;
        }
    }
}
