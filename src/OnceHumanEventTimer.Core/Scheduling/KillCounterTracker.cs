// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.Core.Scheduling;

/// <summary>
/// Thread-safe engine tracking session kills, all-time totals, run goals, and snapshots.
/// </summary>
public sealed class KillCounterTracker : IKillCounterTracker
{
    private readonly object _lock = new();
    private readonly ISettingsService? _settingsService;
    private KillCounterSettings _settings;
    private bool _goalCelebrated;

    public KillCounterSnapshot CurrentSnapshot { get; private set; }
    public bool IsEnabled { get { lock (_lock) { return _settings.IsEnabled; } } }

    public event EventHandler<KillCounterSnapshot>? SnapshotUpdated;
    public event EventHandler? CountIncremented;
    public event EventHandler? CountDecremented;
    public event EventHandler? SessionReset;
    public event EventHandler? GoalReached;

    public KillCounterTracker(ISettingsService? settingsService = null)
    {
        _settingsService = settingsService;
        _settings = settingsService?.Settings.KillCounter.Clone() ?? new KillCounterSettings();
        if (_settingsService != null)
        {
            _settingsService.SettingsChanged += (s, u) => Configure(u.KillCounter);
        }

        _goalCelebrated = _settings.EnableGoal && _settings.CurrentCount >= _settings.GoalCount;
        CurrentSnapshot = CreateSnapshotLocked();
    }

    public KillCounterTracker(KillCounterSettings settings) : this((ISettingsService?)null)
    {
        Configure(settings);
    }

    public void Configure(KillCounterSettings settings)
    {
        lock (_lock)
        {
            _settings = settings?.Clone() ?? new KillCounterSettings();
            _goalCelebrated = _settings.EnableGoal && _settings.CurrentCount >= _settings.GoalCount;
            CurrentSnapshot = CreateSnapshotLocked();
        }
        SnapshotUpdated?.Invoke(this, CurrentSnapshot);
    }

    public void Increment()
    {
        bool fireGoal = false;
        KillCounterSnapshot snapshot;
        lock (_lock)
        {
            _settings.CurrentCount++;
            _settings.TotalAllTimeCount++;
            if (_settings.EnableGoal && _settings.CurrentCount >= _settings.GoalCount && !_goalCelebrated)
            {
                fireGoal = true;
                _goalCelebrated = true;
            }
            SyncSettingsLocked();
            snapshot = CreateSnapshotLocked();
            CurrentSnapshot = snapshot;
        }
        SnapshotUpdated?.Invoke(this, snapshot);
        CountIncremented?.Invoke(this, EventArgs.Empty);
        if (fireGoal) GoalReached?.Invoke(this, EventArgs.Empty);
    }

    public void Decrement()
    {
        KillCounterSnapshot snapshot;
        lock (_lock)
        {
            if (_settings.CurrentCount > 0)
            {
                _settings.CurrentCount--;
                if (_settings.TotalAllTimeCount > 0) _settings.TotalAllTimeCount--;
            }
            if (_settings.EnableGoal && _settings.CurrentCount < _settings.GoalCount)
                _goalCelebrated = false;

            SyncSettingsLocked();
            snapshot = CreateSnapshotLocked();
            CurrentSnapshot = snapshot;
        }
        SnapshotUpdated?.Invoke(this, snapshot);
        CountDecremented?.Invoke(this, EventArgs.Empty);
    }

    public void ResetSession()
    {
        KillCounterSnapshot snapshot;
        lock (_lock)
        {
            _settings.CurrentCount = 0;
            _goalCelebrated = false;
            SyncSettingsLocked();
            snapshot = CreateSnapshotLocked();
            CurrentSnapshot = snapshot;
        }
        SnapshotUpdated?.Invoke(this, snapshot);
        SessionReset?.Invoke(this, EventArgs.Empty);
    }

    public void ResetAllTime()
    {
        KillCounterSnapshot snapshot;
        lock (_lock)
        {
            _settings.TotalAllTimeCount = 0;
            SyncSettingsLocked();
            snapshot = CreateSnapshotLocked();
            CurrentSnapshot = snapshot;
        }
        SnapshotUpdated?.Invoke(this, snapshot);
    }

    public void SetCurrentCount(int count)
    {
        KillCounterSnapshot snapshot;
        lock (_lock)
        {
            _settings.CurrentCount = Math.Max(0, count);
            _goalCelebrated = _settings.EnableGoal && _settings.CurrentCount >= _settings.GoalCount;
            SyncSettingsLocked();
            snapshot = CreateSnapshotLocked();
            CurrentSnapshot = snapshot;
        }
        SnapshotUpdated?.Invoke(this, snapshot);
    }

    public void SetGoal(bool enableGoal, int goalCount)
    {
        KillCounterSnapshot snapshot;
        lock (_lock)
        {
            _settings.EnableGoal = enableGoal;
            _settings.GoalCount = Math.Max(1, goalCount);
            _goalCelebrated = _settings.EnableGoal && _settings.CurrentCount >= _settings.GoalCount;
            SyncSettingsLocked();
            snapshot = CreateSnapshotLocked();
            CurrentSnapshot = snapshot;
        }
        SnapshotUpdated?.Invoke(this, snapshot);
    }

    public void SetBossName(string name)
    {
        KillCounterSnapshot snapshot;
        lock (_lock)
        {
            _settings.BossName = name;
            SyncSettingsLocked();
            snapshot = CreateSnapshotLocked();
            CurrentSnapshot = snapshot;
        }
        SnapshotUpdated?.Invoke(this, snapshot);
    }

    private void SyncSettingsLocked()
    {
        if (_settingsService == null) return;
        var s = _settingsService.Settings.KillCounter;
        s.CurrentCount = _settings.CurrentCount;
        s.TotalAllTimeCount = _settings.TotalAllTimeCount;
        s.EnableGoal = _settings.EnableGoal;
        s.GoalCount = _settings.GoalCount;
        s.BossName = _settings.BossName;
    }

    private KillCounterSnapshot CreateSnapshotLocked()
    {
        bool hasGoal = _settings.EnableGoal && _settings.GoalCount > 0;
        double progress = hasGoal ? Math.Clamp((double)_settings.CurrentCount / _settings.GoalCount, 0.0, 1.0) : 0.0;
        bool isGoalReached = hasGoal && _settings.CurrentCount >= _settings.GoalCount;
        string formatted = hasGoal ? $"{_settings.CurrentCount} / {_settings.GoalCount}" : _settings.CurrentCount.ToString();

        return new KillCounterSnapshot(
            _settings.IsEnabled,
            _settings.BossName,
            _settings.CurrentCount,
            _settings.EnableGoal,
            _settings.GoalCount,
            _settings.TotalAllTimeCount,
            progress,
            isGoalReached,
            formatted,
            _settings.ColorHex,
            _settings.FlashOnGoalReached && isGoalReached,
            _settings.ShowInMiniHud
        );
    }

    public void Dispose() { }
}
