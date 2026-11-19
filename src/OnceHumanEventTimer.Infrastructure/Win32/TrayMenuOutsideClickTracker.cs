// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;

namespace OnceHumanEventTimer.Infrastructure.Win32;

/// <summary>
/// Low-level mouse hook utility to detect clicks outside the dark tray menu window.
/// Installed exclusively while the tray menu is visible to eliminate background CPU overhead.
/// </summary>
public sealed class TrayMenuOutsideClickTracker : IDisposable
{
    private readonly Func<nint> _getHwndFunc;
    private readonly Action _onOutsideClick;
    private readonly NativeMethods.HookProc _hookProc;
    private nint _hookHandle;

    public TrayMenuOutsideClickTracker(Func<nint> getHwndFunc, Action onOutsideClick)
    {
        _getHwndFunc = getHwndFunc;
        _onOutsideClick = onOutsideClick;
        _hookProc = HookCallback;
    }

    public void Start()
    {
        if (_hookHandle != 0) return;
        _hookHandle = NativeMethods.SetWindowsHookEx(
            NativeMethods.WH_MOUSE_LL,
            _hookProc,
            0,
            0);
    }

    public void Stop()
    {
        if (_hookHandle != 0)
        {
            NativeMethods.UnhookWindowsHookEx(_hookHandle);
            _hookHandle = 0;
        }
    }

    private nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0)
        {
            int msg = (int)wParam;
            if (msg is NativeMethods.WM_LBUTTONDOWN or NativeMethods.WM_RBUTTONDOWN or
                NativeMethods.WM_NCLBUTTONDOWN or NativeMethods.WM_NCRBUTTONDOWN or
                NativeMethods.WM_MBUTTONDOWN)
            {
                var hookStruct = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
                nint hwnd = _getHwndFunc();
                if (hwnd != 0 && NativeMethods.GetWindowRect(hwnd, out var rect))
                {
                    bool isInside = hookStruct.pt.X >= rect.Left && hookStruct.pt.X <= rect.Right &&
                                   hookStruct.pt.Y >= rect.Top && hookStruct.pt.Y <= rect.Bottom;

                    if (!isInside)
                    {
                        _onOutsideClick.Invoke();
                    }
                }
            }
        }

        return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        Stop();
    }
}
