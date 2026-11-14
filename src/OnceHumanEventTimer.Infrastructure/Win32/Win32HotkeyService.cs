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

    private static bool TryParseHotkey(string hotkey, out uint modifiers, out uint vk)
    {
        modifiers = 0;
        vk = 0;

        var parts = hotkey.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return false;

        string keyPart = parts[^1];

        for (int i = 0; i < parts.Length - 1; i++)
        {
            string mod = parts[i].ToLowerInvariant();
            if (mod is "ctrl" or "control") modifiers |= NativeMethods.MOD_CONTROL;
            else if (mod is "alt") modifiers |= NativeMethods.MOD_ALT;
            else if (mod is "shift") modifiers |= NativeMethods.MOD_SHIFT;
            else if (mod is "win" or "windows") modifiers |= NativeMethods.MOD_WIN;
        }

        if (Enum.TryParse<System.Windows.Input.Key>(keyPart, true, out var key))
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
