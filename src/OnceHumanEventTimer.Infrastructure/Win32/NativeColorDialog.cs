// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using OnceHumanEventTimer.Core.Utils;

namespace OnceHumanEventTimer.Infrastructure.Win32;

/// <summary>
/// Native Win32 Color Picker dialog wrapper using ChooseColor from comdlg32.dll.
/// </summary>
public static class NativeColorDialog
{
    private static readonly int[] CustomColors = new int[16]
    {
        0x00FFF000, 0x00F755A8, 0x000B9EF5, 0x0081B910,
        0x009948EC, 0x00F6823B, 0x0008B3EA, 0x004444EF,
        0x00FFFFFF, 0x00C0C0C0, 0x00808080, 0x00404040,
        0x00000000, 0x00222222, 0x00111111, 0x00334155
    };

    /// <summary>
    /// Displays the modal Windows ChooseColor dialog and returns the selected HEX color (#RRGGBB) or null if cancelled.
    /// </summary>
    public static string? ShowDialog(nint ownerHwnd, string initialHex)
    {
        int initialColor = HexToColorRef(initialHex);
        var custColorsHandle = GCHandle.Alloc(CustomColors, GCHandleType.Pinned);

        try
        {
            var cc = new NativeMethods.CHOOSECOLOR
            {
                lStructSize = Marshal.SizeOf<NativeMethods.CHOOSECOLOR>(),
                hwndOwner = ownerHwnd,
                rgbResult = initialColor,
                lpCustColors = custColorsHandle.AddrOfPinnedObject(),
                Flags = NativeMethods.CC_RGBINIT | NativeMethods.CC_FULLOPEN | NativeMethods.CC_ANYCOLOR
            };

            if (NativeMethods.ChooseColor(ref cc))
            {
                return ColorRefToHex(cc.rgbResult);
            }

            return null;
        }
        finally
        {
            custColorsHandle.Free();
        }
    }

    /// <summary>
    /// Converts a HEX color string (#RRGGBB) to Win32 COLORREF integer (0x00BBGGRR).
    /// </summary>
    public static int HexToColorRef(string hex)
    {
        if (ColorUtility.TryParseRgb(hex, out byte r, out byte g, out byte b))
        {
            return r | (g << 8) | (b << 16);
        }

        return 0x00FFF000; // Cyan fallback
    }

    /// <summary>
    /// Converts a Win32 COLORREF integer (0x00BBGGRR) to HEX color string (#RRGGBB).
    /// </summary>
    public static string ColorRefToHex(int colorRef)
    {
        byte r = (byte)(colorRef & 0xFF);
        byte g = (byte)((colorRef >> 8) & 0xFF);
        byte b = (byte)((colorRef >> 16) & 0xFF);
        return ColorUtility.ToHex(r, g, b);
    }
}
