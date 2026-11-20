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
        try
        {
            NativeMethods.ChangeWindowMessageFilter(NativeMethods.WM_HOTKEY, NativeMethods.MSGFLT_ADD);
        }
        catch { }

        if (_windowHandle != 0)
        {
            try
            {
                NativeMethods.ChangeWindowMessageFilterEx(_windowHandle, NativeMethods.WM_HOTKEY, NativeMethods.MSGFLT_ALLOW, 0);
            }
            catch { }
        }
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
        return OnceHumanEventTimer.Core.Utils.HotkeyParser.TryParse(hotkey, out modifiers, out vk);
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
