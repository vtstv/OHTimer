// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;

namespace OnceHumanEventTimer.Infrastructure.Win32;

/// <summary>
/// Utility for locating and loading the application icon for system tray presentation.
/// </summary>
internal static class TrayIconLoader
{
    public static nint LoadApplicationIcon()
    {
        // 1. Look for app.ico next to executable or in Assets directory
        string baseDir = AppContext.BaseDirectory;
        string[] candidatePaths =
        [
            Path.Combine(baseDir, "app.ico"),
            Path.Combine(baseDir, "Assets", "app.ico")
        ];

        foreach (var path in candidatePaths)
        {
            if (File.Exists(path))
            {
                nint hLoaded = NativeMethods.LoadImage(0, path, NativeMethods.IMAGE_ICON, 16, 16, NativeMethods.LR_LOADFROMFILE);
                if (hLoaded != 0) return hLoaded;

                hLoaded = NativeMethods.LoadImage(0, path, NativeMethods.IMAGE_ICON, 0, 0, NativeMethods.LR_LOADFROMFILE | NativeMethods.LR_DEFAULTSIZE);
                if (hLoaded != 0) return hLoaded;
            }
        }

        // 2. Extract embedded icon from process executable if available
        try
        {
            string? procPath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(procPath) && File.Exists(procPath))
            {
                int count = NativeMethods.ExtractIconEx(procPath, 0, out nint hLarge, out nint hSmall, 1);
                if (count > 0)
                {
                    if (hLarge != 0) NativeMethods.DestroyIcon(hLarge);
                    if (hSmall != 0) return hSmall;
                }
            }
        }
        catch
        {
            // Fallback to standard icon
        }

        // 3. Fallback to standard system application icon
        return NativeMethods.LoadIcon(0, (nint)32512);
    }
}
