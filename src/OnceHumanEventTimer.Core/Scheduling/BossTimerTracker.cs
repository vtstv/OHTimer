// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Timers;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.Core.Scheduling;

/// <summary>
/// Engine tracking boss respawn countdown, state transitions, repeat chimes, and auto-hide.
/// </summary>
public sealed class BossTimerTracker : IBossTimerTracker
{
    private readonly System.Timers.Timer _timer;
    private readonly object _lock = new();
    private BossTimerSettings _settings;

    private BossTimerState _state = BossTimerState.Inactive;
    private TimeSpan _totalDuration;
    private DateTimeOffset _startTime;
    private DateTimeOffset _targetSpawnTime;
    private DateTimeOffset _spawnTime;
    private DateTimeOffset _nextRepeatTime;
    private int _repeatAlertsLeft;

    public BossTimerSnapshot CurrentSnapshot { get; private set; }
    public BossTimerState State { get { lock (_lock) { return _state; } } }
    public bool IsActive => State != BossTimerState.Inactive;

    public event EventHandler<BossTimerSnapshot>? SnapshotUpdated;
    public event EventHandler? HotkeyTriggered;
    public event EventHandler? WarningTriggered;
    public event EventHandler? SpawnTriggered;
    public event EventHandler? RepeatAlertTriggered;

    public BossTimerTracker(ISettingsService? settingsService = null)
    {
        _settings = settingsService?.Settings.BossTimer.Clone() ?? new BossTimerSettings();
        if (settingsService != null)
        {
            settingsService.SettingsChanged += (s, u) => Configure(u.BossTimer);
        }

        _totalDuration = _settings.GetTotalDuration();
        CurrentSnapshot = CreateSnapshotLocked(DateTimeOffset.Now);

        _timer = new System.Timers.Timer(1000) { AutoReset = true };
        _timer.Elapsed += (s, e) => Tick(DateTimeOffset.Now);
    }

    public BossTimerTracker(BossTimerSettings settings) : this((ISettingsService?)null)
    {
        Configure(settings);
    }

    public void Configure(BossTimerSettings settings)
    {
        lock (_lock)
        {
            _settings = settings?.Clone() ?? new BossTimerSettings();
            _totalDuration = _settings.GetTotalDuration();
            if (!_settings.IsEnabled)
            {
                _state = BossTimerState.Inactive;
                _repeatAlertsLeft = 0;
                _timer.Stop();
            }
            else if (_state is BossTimerState.Running or BossTimerState.Warning)
            {
                _targetSpawnTime = _startTime + _totalDuration;
            }
            CurrentSnapshot = CreateSnapshotLocked(DateTimeOffset.Now);
        }
        SnapshotUpdated?.Invoke(this, CurrentSnapshot);
    }

    public void Start(DateTimeOffset? startTime = null)
    {
        bool fireWarning = false;
        lock (_lock)
        {
            if (!_settings.IsEnabled) return;
            var now = startTime ?? DateTimeOffset.Now;
            StartInternal(now, out fireWarning);
        }
        SnapshotUpdated?.Invoke(this, CurrentSnapshot);
        if (fireWarning) WarningTriggered?.Invoke(this, EventArgs.Empty);
    }

    public void Stop()
    {
        lock (_lock)
        {
            _state = BossTimerState.Inactive;
            _repeatAlertsLeft = 0;
            _timer.Stop();
            CurrentSnapshot = CreateSnapshotLocked(DateTimeOffset.Now);
        }
        SnapshotUpdated?.Invoke(this, CurrentSnapshot);
    }

    public void Toggle()
    {
        if (IsActive) Stop();
        else Start();
    }

    public void HandleHotkeyPress()
    {
        if (!_settings.IsEnabled) return;

        bool fireHotkeySound = false;
        bool fireWarning = false;
        lock (_lock)
        {
            if (_state == BossTimerState.Inactive || _settings.RepressBehavior == BossTimerRepressBehavior.Restart)
            {
                StartInternal(DateTimeOffset.Now, out fireWarning);
                fireHotkeySound = _settings.PlayHotkeySound;
            }
            else
            {
                _state = BossTimerState.Inactive;
                _repeatAlertsLeft = 0;
                _timer.Stop();
                CurrentSnapshot = CreateSnapshotLocked(DateTimeOffset.Now);
            }
        }

        SnapshotUpdated?.Invoke(this, CurrentSnapshot);
        if (fireHotkeySound) HotkeyTriggered?.Invoke(this, EventArgs.Empty);
        if (fireWarning) WarningTriggered?.Invoke(this, EventArgs.Empty);
    }

    public void Tick(DateTimeOffset referenceTime)
    {
        bool fireWarning = false;
        bool fireSpawn = false;
        bool fireRepeat = false;
        BossTimerSnapshot snapshot;

        lock (_lock)
        {
            if (_state == BossTimerState.Inactive) return;

            if (_state is BossTimerState.Running or BossTimerState.Warning)
            {
                var remaining = _targetSpawnTime - referenceTime;
                if (remaining <= TimeSpan.Zero)
                {
                    _state = BossTimerState.Spawned;
                    _spawnTime = referenceTime;
                    _repeatAlertsLeft = _settings.RepeatAlertCount;
                    _nextRepeatTime = referenceTime.AddSeconds(_settings.RepeatAlertIntervalSeconds);
                    fireSpawn = true;
                }
                else if (_state == BossTimerState.Running && _settings.EnableWarningAlert &&
                         remaining.TotalSeconds <= _settings.WarningSecondsAhead)
                {
                    _state = BossTimerState.Warning;
                    fireWarning = true;
                }
            }
            else if (_state == BossTimerState.Spawned)
            {
                var elapsed = referenceTime - _spawnTime;
                if (_settings.AutoHideSeconds > 0 && elapsed.TotalSeconds >= _settings.AutoHideSeconds)
                {
                    _state = BossTimerState.Inactive;
                    _timer.Stop();
                }
                else if (_settings.EnableRepeatAlert && _repeatAlertsLeft > 0 && referenceTime >= _nextRepeatTime)
                {
                    _repeatAlertsLeft--;
                    _nextRepeatTime = referenceTime.AddSeconds(_settings.RepeatAlertIntervalSeconds);
                    fireRepeat = true;
                }
            }

            snapshot = CreateSnapshotLocked(referenceTime);
            CurrentSnapshot = snapshot;
        }

        SnapshotUpdated?.Invoke(this, snapshot);
        if (fireWarning) WarningTriggered?.Invoke(this, EventArgs.Empty);
        if (fireSpawn) SpawnTriggered?.Invoke(this, EventArgs.Empty);
        if (fireRepeat) RepeatAlertTriggered?.Invoke(this, EventArgs.Empty);
    }

    private void StartInternal(DateTimeOffset now, out bool fireWarning)
    {
        _totalDuration = _settings.GetTotalDuration();
        _startTime = now;
        _targetSpawnTime = now + _totalDuration;
        _repeatAlertsLeft = _settings.RepeatAlertCount;

        bool startsInWarning = _settings.EnableWarningAlert && _totalDuration.TotalSeconds <= _settings.WarningSecondsAhead;
        _state = startsInWarning ? BossTimerState.Warning : BossTimerState.Running;
        fireWarning = startsInWarning;
        _timer.Start();

        CurrentSnapshot = CreateSnapshotLocked(now);
    }

    private BossTimerSnapshot CreateSnapshotLocked(DateTimeOffset referenceTime)
    {
        if (_state == BossTimerState.Inactive)
        {
            return new BossTimerSnapshot(BossTimerState.Inactive, TimeSpan.Zero, _totalDuration, 0.0,
                "00:00", _settings.BossName, _settings.ColorHex, false, false, false, false);
        }

        if (_state is BossTimerState.Running or BossTimerState.Warning)
        {
            var rem = _targetSpawnTime - referenceTime;
            if (rem < TimeSpan.Zero) rem = TimeSpan.Zero;

            double totalSec = _totalDuration.TotalSeconds;
            double elapsedSec = Math.Clamp((referenceTime - _startTime).TotalSeconds, 0, totalSec);
            double progress = totalSec > 0 ? Math.Clamp(elapsedSec / totalSec, 0.0, 1.0) : 1.0;
            string formatted = $"{(int)rem.TotalMinutes:D2}:{rem.Seconds:D2}";

            return new BossTimerSnapshot(_state, rem, _totalDuration, progress, formatted,
                _settings.BossName, _settings.ColorHex, true, _state == BossTimerState.Warning, false, false);
        }

        // Spawned state
        return new BossTimerSnapshot(BossTimerState.Spawned, TimeSpan.Zero, _totalDuration, 1.0,
            "00:00", _settings.BossName, _settings.ColorHex, true, false, true, _settings.FlashCardOnSpawn);
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}
