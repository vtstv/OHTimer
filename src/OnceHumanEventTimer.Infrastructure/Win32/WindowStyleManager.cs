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
}
