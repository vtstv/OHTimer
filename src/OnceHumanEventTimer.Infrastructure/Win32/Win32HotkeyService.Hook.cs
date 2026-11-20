// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;

namespace OnceHumanEventTimer.Infrastructure.Win32;

public sealed partial class Win32HotkeyService
{
    private nint HwndHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            Action? actionToInvoke = null;

            lock (_lock)
            {
                if (_registeredHotkeys.TryGetValue(id, out var entry) && TryDebounce(id))
                {
                    actionToInvoke = entry.Callback;
                }
            }

            if (actionToInvoke != null)
            {
                try
                {
                    actionToInvoke.Invoke();
                }
                catch { }
                handled = true;
            }
        }
        return 0;
    }

    private nint LowLevelKeyboardProc(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0)
        {
            int msg = (int)wParam;
            var kbd = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
            uint vk = kbd.vkCode;

            if (msg is NativeMethods.WM_KEYDOWN or NativeMethods.WM_SYSKEYDOWN)
            {
                bool isRepeat;
                lock (_lock)
                {
                    isRepeat = !_pressedKeys.Add(vk);
                }

                if (!isRepeat)
                {
                    uint activeMods = GetActiveModifiers();
                    Action? actionToInvoke = null;

                    lock (_lock)
                    {
                        foreach (var entry in _registeredHotkeys.Values)
                        {
                            if (entry.Modifiers == activeMods && MatchesVirtualKey(entry.Vk, vk, kbd.flags))
                            {
                                if (TryDebounce(entry.Id))
                                {
                                    actionToInvoke = entry.Callback;
                                }
                                break;
                            }
                        }
                    }

                    if (actionToInvoke != null)
                    {
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            try { actionToInvoke.Invoke(); } catch { }
                        });
                    }
                }
            }
            else if (msg is NativeMethods.WM_KEYUP or NativeMethods.WM_SYSKEYUP)
            {
                lock (_lock)
                {
                    _pressedKeys.Remove(vk);
                }
            }
        }

        return NativeMethods.CallNextHookEx(_llHookHandle, nCode, wParam, lParam);
    }

    private static bool MatchesVirtualKey(uint registeredVk, uint pressedVk, uint flags)
    {
        if (registeredVk == pressedVk) return true;

        bool isExtended = (flags & NativeMethods.LLKHF_EXTENDED) != 0;
        if (!isExtended)
        {
            return (registeredVk, pressedVk) switch
            {
                (0x60, 0x2D) => true, // NumPad 0 / Insert
                (0x61, 0x23) => true, // NumPad 1 / End
                (0x62, 0x28) => true, // NumPad 2 / Down
                (0x63, 0x22) => true, // NumPad 3 / PageDown
                (0x64, 0x25) => true, // NumPad 4 / Left
                (0x65, 0x0C) => true, // NumPad 5 / Clear
                (0x66, 0x27) => true, // NumPad 6 / Right
                (0x67, 0x24) => true, // NumPad 7 / Home
                (0x68, 0x26) => true, // NumPad 8 / Up
                (0x69, 0x21) => true, // NumPad 9 / PageUp
                (0x6E, 0x2E) => true, // NumPad . / Delete
                _ => false
            };
        }

        return false;
    }

    private static uint GetActiveModifiers()
    {
        uint mods = 0;
        if ((NativeMethods.GetKeyState(NativeMethods.VK_CONTROL) & 0x8000) != 0 ||
            (NativeMethods.GetAsyncKeyState(NativeMethods.VK_CONTROL) & 0x8000) != 0)
        {
            mods |= NativeMethods.MOD_CONTROL;
        }

        if ((NativeMethods.GetKeyState(NativeMethods.VK_MENU) & 0x8000) != 0 ||
            (NativeMethods.GetAsyncKeyState(NativeMethods.VK_MENU) & 0x8000) != 0)
        {
            mods |= NativeMethods.MOD_ALT;
        }

        if ((NativeMethods.GetKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0 ||
            (NativeMethods.GetAsyncKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0)
        {
            mods |= NativeMethods.MOD_SHIFT;
        }

        if ((NativeMethods.GetKeyState(NativeMethods.VK_LWIN) & 0x8000) != 0 ||
            (NativeMethods.GetKeyState(NativeMethods.VK_RWIN) & 0x8000) != 0 ||
            (NativeMethods.GetAsyncKeyState(NativeMethods.VK_LWIN) & 0x8000) != 0 ||
            (NativeMethods.GetAsyncKeyState(NativeMethods.VK_RWIN) & 0x8000) != 0)
        {
            mods |= NativeMethods.MOD_WIN;
        }

        return mods;
    }

    private bool TryDebounce(int id)
    {
        long now = Environment.TickCount64;
        if (_lastTriggerTicks.TryGetValue(id, out long last) && now - last < 250)
        {
            return false;
        }
        _lastTriggerTicks[id] = now;
        return true;
    }
}
