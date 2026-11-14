// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Timers;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.Infrastructure.Win32;

/// <summary>
/// Monitors active foreground window focus to determine if Once Human or the application is in focus.
/// Features lightweight process querying, LRU process caching, and hysteresis debouncing to prevent DWM stalls.
/// </summary>
public class GameFocusMonitor : IGameFocusMonitor
{
    private static readonly HashSet<string> KnownNonGameProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "chrome", "msedge", "firefox", "opera", "brave", "vivaldi", "discord", "telegram", "slack", "skype",
        "explorer", "devenv", "code", "cmd", "powershell", "windowsterminal", "notepad", "notepad++", "sublime_text",
        "steam", "steamwebhelper", "epicgameslauncher"
    };

    private readonly System.Timers.Timer _pollTimer;
    private readonly System.Timers.Timer _debounceHideTimer;
    private readonly int _currentProcessId;
    private readonly ConcurrentDictionary<uint, string> _processNameCache = new();
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

        // Poll every 300ms (< 0.01% CPU with PID cache)
        _pollTimer = new System.Timers.Timer(300);
        _pollTimer.Elapsed += (_, _) => CheckForegroundWindow();
        _pollTimer.AutoReset = true;

        // 400ms hide debounce prevents rapid DWM surface recreate/destroy stalls during Alt-Tab
        _debounceHideTimer = new System.Timers.Timer(400);
        _debounceHideTimer.Elapsed += OnDebounceHideElapsed;
        _debounceHideTimer.AutoReset = false;
    }

    public void Start()
    {
        CheckForegroundWindow();
        _pollTimer.Start();
    }

    public void Stop()
    {
        _pollTimer.Stop();
        _debounceHideTimer.Stop();
    }

    private void OnDebounceHideElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_disposed) return;
        bool fire = false;
        lock (_lock)
        {
            if (_isGameFocused)
            {
                _isGameFocused = false;
                fire = true;
            }
        }
        if (fire) GameFocusChanged?.Invoke(this, false);
    }

    private void CheckForegroundWindow()
    {
        if (_disposed) return;

        nint fgHwnd = NativeMethods.GetForegroundWindow();
        if (fgHwnd == 0) return;

        if (fgHwnd == _lastForegroundHwnd) return;
        _lastForegroundHwnd = fgHwnd;

        NativeMethods.GetWindowThreadProcessId(fgHwnd, out uint pid);
        if (pid == 0) return;

        bool currentlyFocused = IsOnceHuman(fgHwnd, pid);

        if (currentlyFocused)
        {
            _debounceHideTimer.Stop();
            bool fire = false;
            lock (_lock)
            {
                if (!_isGameFocused)
                {
                    _isGameFocused = true;
                    fire = true;
                }
            }
            if (fire) GameFocusChanged?.Invoke(this, true);
        }
        else
        {
            lock (_lock)
            {
                if (_isGameFocused && !_debounceHideTimer.Enabled)
                {
                    _debounceHideTimer.Start();
                }
            }
        }
    }

    private bool IsOnceHuman(nint hwnd, uint pid)
    {
        if (pid == _currentProcessId) return true;

        string? procName = GetProcessName(pid);
        if (string.IsNullOrEmpty(procName)) return IsExactOnceHumanTitle(hwnd);

        // Common browsers/tools with "Once Human" in title/tab are never the game
        if (KnownNonGameProcesses.Contains(procName)) return false;

        return procName.Equals("ONCE_HUMAN", StringComparison.OrdinalIgnoreCase) ||
               procName.Equals("OnceHuman", StringComparison.OrdinalIgnoreCase);
    }

    private string? GetProcessName(uint pid)
    {
        if (_processNameCache.TryGetValue(pid, out string? cached)) return cached;

        string? name = QueryProcessName(pid);
        if (!string.IsNullOrEmpty(name))
        {
            if (_processNameCache.Count > 64) _processNameCache.Clear();
            _processNameCache[pid] = name;
        }
        return name;
    }

    private static string? QueryProcessName(uint pid)
    {
        nint h = NativeMethods.OpenProcess(NativeMethods.PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
        if (h != 0)
        {
            try
            {
                var sb = new StringBuilder(1024);
                uint size = (uint)sb.Capacity;
                if (NativeMethods.QueryFullProcessImageName(h, 0, sb, ref size))
                    return Path.GetFileNameWithoutExtension(sb.ToString());
            }
            catch { }
            finally { NativeMethods.CloseHandle(h); }
        }

        try
        {
            using var proc = Process.GetProcessById((int)pid);
            return proc.ProcessName;
        }
        catch { return null; }
    }

    private static bool IsExactOnceHumanTitle(nint hwnd)
    {
        try
        {
            var sb = new StringBuilder(256);
            int len = NativeMethods.GetWindowText(hwnd, sb, sb.Capacity);
            if (len > 0)
            {
                string title = sb.ToString().Trim();
                return title.Equals("Once Human", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch { }
        return false;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _pollTimer.Stop();
        _pollTimer.Dispose();
        _debounceHideTimer.Stop();
        _debounceHideTimer.Dispose();
        _processNameCache.Clear();
    }
}
