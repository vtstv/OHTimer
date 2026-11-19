// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Infrastructure.Win32;

/// <summary>
/// Helper for managing extended window styles (Click-through, ToolWindow).
/// </summary>
public static class WindowStyleManager
{
    /// <summary>
    /// Enables or disables click-through behavior (WS_EX_TRANSPARENT).
    /// When enabled, mouse events pass straight through the window to applications underneath.
    /// </summary>
    public static void SetClickThrough(nint hwnd, bool clickThrough)
    {
        if (hwnd == 0) return;

        nint currentStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE);
        nint newStyle = clickThrough
            ? (currentStyle | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_LAYERED)
            : (currentStyle & ~NativeMethods.WS_EX_TRANSPARENT);

        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE, newStyle);
    }

    /// <summary>
    /// Hides window from Alt+Tab switcher and prevents activation steal (WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE).
    /// </summary>
    public static void SetToolWindow(nint hwnd, bool isToolWindow)
    {
        if (hwnd == 0) return;

        nint currentStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE);
        nint flags = NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_NOACTIVATE;
        nint newStyle = isToolWindow
            ? (currentStyle | flags)
            : (currentStyle & ~flags);

        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE, newStyle);
    }

    /// <summary>
    /// Sets WS_EX_TOOLWINDOW to hide the window from the Alt+Tab switcher while preserving activation capability.
    /// </summary>
    public static void SetToolWindowOnly(nint hwnd, bool isToolWindow)
    {
        if (hwnd == 0) return;

        nint currentStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE);
        nint flags = NativeMethods.WS_EX_TOOLWINDOW;
        nint newStyle = isToolWindow
            ? (currentStyle | flags)
            : (currentStyle & ~flags);

        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE, newStyle);
    }

    /// <summary>
    /// Brings the specified window to the foreground.
    /// </summary>
    public static bool BringToForeground(nint hwnd)
    {
        return hwnd != 0 && NativeMethods.SetForegroundWindow(hwnd);
    }

    /// <summary>
    /// Posts a WM_NULL message to the specified window to flush task switch queues.
    /// </summary>
    public static void PostNullMessage(nint hwnd)
    {
        if (hwnd != 0)
        {
            NativeMethods.PostMessage(hwnd, NativeMethods.WM_NULL, 0, 0);
        }
    }

    /// <summary>
    /// Gets the current cursor position in physical screen coordinates.
    /// </summary>
    public static (int X, int Y) GetCursorPosition()
    {
        NativeMethods.GetCursorPos(out var pt);
        return (pt.X, pt.Y);
    }

    /// <summary>
    /// Gets the work area rectangle in physical screen coordinates for the monitor containing the specified point.
    /// </summary>
    public static (int Left, int Top, int Right, int Bottom) GetMonitorWorkArea(int x, int y)
    {
        var pt = new NativeMethods.POINT { X = x, Y = y };
        nint hMonitor = NativeMethods.MonitorFromPoint(pt, NativeMethods.MONITOR_DEFAULTTONEAREST);
        var mi = new NativeMethods.MONITORINFO { cbSize = System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.MONITORINFO>() };
        if (hMonitor != 0 && NativeMethods.GetMonitorInfo(hMonitor, ref mi))
        {
            return (mi.rcWork.Left, mi.rcWork.Top, mi.rcWork.Right, mi.rcWork.Bottom);
        }

        return (0, 0, 1920, 1080);
    }
}
