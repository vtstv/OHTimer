// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Text;
using System.Timers;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.Infrastructure.Win32;

/// <summary>
/// Monitors active foreground window focus to determine if Once Human or the application is in focus.
/// </summary>
public class GameFocusMonitor : IGameFocusMonitor
{
    private readonly System.Timers.Timer _timer;
    private readonly int _currentProcessId;
    private readonly object _lock = new();
    private nint _lastForegroundHwnd;
    private bool _isGameFocused = true;
    private bool _disposed;

    public bool IsGameFocused
    {
        get { lock (_lock) { return _isGameFocused; } }
        private set { lock (_lock) { _isGameFocused = value; } }
    }

    public event EventHandler<bool>? GameFocusChanged;

    public GameFocusMonitor()
    {
        _currentProcessId = Environment.ProcessId;
        _timer = new System.Timers.Timer(500);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
    }

    public void Start()
    {
        CheckForegroundWindow();
        _timer.Start();
    }

    public void Stop() => _timer.Stop();

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        CheckForegroundWindow();
    }

    private void CheckForegroundWindow()
    {
        if (_disposed) return;

        nint fgHwnd = NativeMethods.GetForegroundWindow();
        if (fgHwnd == 0) return;

        // Fast-path: if the foreground window hasn't changed, zero CPU overhead
        if (fgHwnd == _lastForegroundHwnd) return;
        _lastForegroundHwnd = fgHwnd;

        NativeMethods.GetWindowThreadProcessId(fgHwnd, out uint pid);
        if (pid == 0) return;

        bool focused;
        if (pid == _currentProcessId)
        {
            // The user is interacting with our own overlay, settings, or dialogs
            focused = true;
        }
        else
        {
            // Fast title check first (< 1 microsecond, no heavy Process allocation)
            focused = IsOnceHumanWindow(fgHwnd) || IsOnceHumanProcess((int)pid);
        }

        bool stateChanged = false;
        lock (_lock)
        {
            if (_isGameFocused != focused)
            {
                _isGameFocused = focused;
                stateChanged = true;
            }
        }

        if (stateChanged)
        {
            GameFocusChanged?.Invoke(this, focused);
        }
    }

    private static bool IsOnceHumanProcess(int pid)
    {
        try
        {
            using var proc = Process.GetProcessById(pid);
            string name = proc.ProcessName;
            return name.Contains("ONCE_HUMAN", StringComparison.OrdinalIgnoreCase) ||
                   name.Contains("OnceHuman", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsOnceHumanWindow(nint hwnd)
    {
        try
        {
            var sb = new StringBuilder(256);
            int len = NativeMethods.GetWindowText(hwnd, sb, sb.Capacity);
            if (len > 0)
            {
                string title = sb.ToString();
                return title.Contains("Once Human", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
        }
        return false;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        _timer.Dispose();
    }
}
