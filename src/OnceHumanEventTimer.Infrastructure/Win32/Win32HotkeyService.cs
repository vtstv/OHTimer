// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Interop;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.Infrastructure.Win32;

/// <summary>
/// Registers and handles system-wide hotkeys using a hybrid engine:
/// 1. Win32 RegisterHotKey API with persistent host window and UIPI message filter bypass.
/// 2. Low-Level Keyboard Hook (WH_KEYBOARD_LL) capturing bare unmodified keys (e.g. NumPad 1, +, -)
///    inside full-screen DirectX games where standard Win32 WM_HOTKEY messages are suppressed by RawInput.
/// </summary>
public sealed partial class Win32HotkeyService : IHotkeyService
{
    private sealed record RegisteredHotkey(int Id, uint Modifiers, uint Vk, Action Callback);

    private readonly Dictionary<int, RegisteredHotkey> _registeredHotkeys = new();
    private readonly HashSet<uint> _pressedKeys = new();
    private readonly Dictionary<int, long> _lastTriggerTicks = new();
    private readonly object _lock = new();

    private nint _windowHandle = 0;
    private HwndSource? _hwndSource;
    private HwndSource? _dedicatedHostSource;
    private nint _llHookHandle = 0;
    private readonly NativeMethods.HookProc _llKeyboardProc;
    private int _currentId = 1000;
    private bool _isInitialized;

    public Win32HotkeyService()
    {
        _llKeyboardProc = LowLevelKeyboardProc;
    }

    /// <summary>
    /// Initializes the hotkey hook. If hwnd is 0 or omitted, a dedicated persistent message-only host window is created.
    /// </summary>
    public void Initialize(nint hwnd = 0)
    {
        if (_isInitialized) return;

        if (hwnd != 0)
        {
            _windowHandle = hwnd;
            _hwndSource = HwndSource.FromHwnd(hwnd);
            _hwndSource?.AddHook(HwndHook);
        }
        else
        {
            var parameters = new HwndSourceParameters("OnceHumanHotkeyHost")
            {
                ParentWindow = new nint(-3), // HWND_MESSAGE
                Width = 0,
                Height = 0,
                PositionX = 0,
                PositionY = 0,
                WindowStyle = 0
            };
            _dedicatedHostSource = new HwndSource(parameters);
            _windowHandle = _dedicatedHostSource.Handle;
            _dedicatedHostSource.AddHook(HwndHook);
        }

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

        try
        {
            _llHookHandle = NativeMethods.SetWindowsHookEx(
                NativeMethods.WH_KEYBOARD_LL,
                _llKeyboardProc,
                NativeMethods.GetModuleHandle(null),
                0);
        }
        catch { }

        _isInitialized = true;
    }

    public bool RegisterHotkey(string hotkeyString, Action callback)
    {
        if (!_isInitialized)
        {
            if (System.Windows.Application.Current?.Dispatcher != null && !System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => Initialize());
            }
            else
            {
                Initialize();
            }
        }

        if (string.IsNullOrWhiteSpace(hotkeyString))
        {
            return false;
        }

        if (!TryParseHotkey(hotkeyString, out uint modifiers, out uint vk))
        {
            return false;
        }

        int id = Interlocked.Increment(ref _currentId);
        bool success = false;

        if (_windowHandle != 0)
        {
            success = NativeMethods.RegisterHotKey(_windowHandle, id, modifiers | NativeMethods.MOD_NOREPEAT, vk);
            if (!success)
            {
                success = NativeMethods.RegisterHotKey(_windowHandle, id, modifiers, vk);
            }
        }

        // Low-level hook ensures bare keys or conflicted keys still capture inside active games
        if (_llHookHandle != 0)
        {
            success = true;
        }

        if (success)
        {
            lock (_lock)
            {
                _registeredHotkeys[id] = new RegisteredHotkey(id, modifiers, vk, callback);
            }
        }

        return success;
    }

    public void UnregisterAll()
    {
        lock (_lock)
        {
            if (_windowHandle != 0)
            {
                foreach (var id in _registeredHotkeys.Keys)
                {
                    NativeMethods.UnregisterHotKey(_windowHandle, id);
                }
            }
            _registeredHotkeys.Clear();
            _pressedKeys.Clear();
            _lastTriggerTicks.Clear();
        }
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

        if (_dedicatedHostSource != null)
        {
            _dedicatedHostSource.RemoveHook(HwndHook);
            _dedicatedHostSource.Dispose();
            _dedicatedHostSource = null;
        }

        if (_llHookHandle != 0)
        {
            NativeMethods.UnhookWindowsHookEx(_llHookHandle);
            _llHookHandle = 0;
        }

        _windowHandle = 0;
        _isInitialized = false;
    }
}
