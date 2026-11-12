// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using System.Windows.Interop;
using StarfallTimer.Core.Services;

namespace StarfallTimer.Infrastructure.Win32;

/// <summary>
/// High-performance Win32 native system tray implementation using shell32.dll and user32.dll.
/// Free of Windows Forms dependencies and fully compatible with modern Windows 10/11 taskbar.
/// </summary>
public sealed class Win32TrayService : IDisposable
{
    private const int TRAY_ID = 2001;
    private const int CMD_TOGGLE = 101;
    private const int CMD_LOCK = 102;
    private const int CMD_SETTINGS = 103;
    private const int CMD_EXIT = 104;

    private readonly ILocalizationService _loc;
    private readonly Action _toggleVisibilityAction;
    private readonly Action _toggleLockAction;
    private readonly Action _openSettingsAction;
    private readonly Action _exitAction;

    private nint _hwnd;
    private HwndSource? _hwndSource;
    private NativeMethods.NOTIFYICONDATA _nid;
    private bool _isInitialized;

    public Win32TrayService(
        ILocalizationService loc,
        Action toggleVisibilityAction,
        Action toggleLockAction,
        Action openSettingsAction,
        Action exitAction)
    {
        _loc = loc;
        _toggleVisibilityAction = toggleVisibilityAction;
        _toggleLockAction = toggleLockAction;
        _openSettingsAction = openSettingsAction;
        _exitAction = exitAction;
    }

    public void Initialize(nint hwnd)
    {
        if (_isInitialized) return;
        _hwnd = hwnd;

        _hwndSource = HwndSource.FromHwnd(hwnd);
        _hwndSource?.AddHook(HwndHook);

        // Load standard system application icon (IDI_APPLICATION = 32512)
        nint hIcon = NativeMethods.LoadIcon(0, (nint)32512);

        _nid = new NativeMethods.NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<NativeMethods.NOTIFYICONDATA>(),
            hWnd = _hwnd,
            uID = TRAY_ID,
            uFlags = NativeMethods.NIF_MESSAGE | NativeMethods.NIF_ICON | NativeMethods.NIF_TIP,
            uCallbackMessage = NativeMethods.WM_TRAYICON,
            hIcon = hIcon,
            szTip = "Once Human — Starfall Inversion Timer",
            szInfo = string.Empty,
            szInfoTitle = string.Empty
        };

        NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_ADD, ref _nid);
        _isInitialized = true;
    }

    public void ShowNotification(string title, string message)
    {
        if (!_isInitialized) return;

        _nid.uFlags = NativeMethods.NIF_MESSAGE | NativeMethods.NIF_ICON | NativeMethods.NIF_TIP | NativeMethods.NIF_INFO;
        _nid.szInfoTitle = title.Length > 63 ? title[..63] : title;
        _nid.szInfo = message.Length > 255 ? message[..255] : message;
        _nid.dwInfoFlags = (int)NativeMethods.NIIF_INFO;

        NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_MODIFY, ref _nid);
    }

    private nint HwndHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_TRAYICON)
        {
            int eventType = (int)(lParam.ToInt64() & 0xFFFF);

            if (eventType == NativeMethods.WM_LBUTTONDBLCLK)
            {
                _toggleVisibilityAction.Invoke();
                handled = true;
            }
            else if (eventType == NativeMethods.WM_RBUTTONUP)
            {
                ShowContextMenu();
                handled = true;
            }
        }

        return 0;
    }

    private void ShowContextMenu()
    {
        nint hMenu = NativeMethods.CreatePopupMenu();
        if (hMenu == 0) return;

        NativeMethods.AppendMenu(hMenu, NativeMethods.MF_STRING, (nuint)CMD_TOGGLE, _loc.GetString("Tray_OpenHUD"));
        NativeMethods.AppendMenu(hMenu, NativeMethods.MF_STRING, (nuint)CMD_LOCK, _loc.GetString("Tray_ToggleLock"));
        NativeMethods.AppendMenu(hMenu, NativeMethods.MF_STRING, (nuint)CMD_SETTINGS, _loc.GetString("Tray_Settings"));
        NativeMethods.AppendMenu(hMenu, NativeMethods.MF_SEPARATOR, 0, string.Empty);
        NativeMethods.AppendMenu(hMenu, NativeMethods.MF_STRING, (nuint)CMD_EXIT, _loc.GetString("Tray_Exit"));

        NativeMethods.GetCursorPos(out var pt);
        NativeMethods.SetForegroundWindow(_hwnd);

        int selectedCmd = NativeMethods.TrackPopupMenuEx(
            hMenu,
            (uint)(NativeMethods.TPM_RETURNCMD | NativeMethods.TPM_BOTTOMALIGN),
            pt.X,
            pt.Y,
            _hwnd,
            0);

        NativeMethods.DestroyMenu(hMenu);
        NativeMethods.PostMessage(_hwnd, NativeMethods.WM_NULL, 0, 0);

        switch (selectedCmd)
        {
            case CMD_TOGGLE:
                _toggleVisibilityAction.Invoke();
                break;
            case CMD_LOCK:
                _toggleLockAction.Invoke();
                break;
            case CMD_SETTINGS:
                _openSettingsAction.Invoke();
                break;
            case CMD_EXIT:
                _exitAction.Invoke();
                break;
        }
    }

    public void Dispose()
    {
        if (_isInitialized)
        {
            NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_DELETE, ref _nid);
            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(HwndHook);
                _hwndSource = null;
            }
            _isInitialized = false;
        }
    }
}
