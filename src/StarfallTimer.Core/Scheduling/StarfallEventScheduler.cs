// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Timers;
using StarfallTimer.Core.Models;
using StarfallTimer.Core.Services;

namespace StarfallTimer.Core.Scheduling;

/// <summary>
/// High-resolution engine evaluating cyclic Once Human events, applying time offsets, and triggering alerts.
/// </summary>
public sealed class StarfallEventScheduler : IEventScheduler
{
    private readonly System.Timers.Timer _timer;
    private readonly ILocalizationService _loc;
    private readonly object _lock = new();

    private EventDefinition _activeEvent;
    private int _globalTimeOffsetSeconds = 0;
    private int _alertBeforeStart = 2;
    private int _alertBeforeEnd = 3;

    private long _lastNotifiedStartSoonCycle = -1;
    private long _lastNotifiedActiveCycle = -1;
    private long _lastNotifiedEndSoonCycle = -1;

    public EventSnapshot CurrentSnapshot { get; private set; }

    public EventDefinition ActiveEvent
    {
        get
        {
            lock (_lock)
            {
                return _activeEvent;
            }
        }
    }

    public int TimeOffsetSeconds
    {
        get
        {
            lock (_lock)
            {
                return _activeEvent.TimeOffsetSeconds + _globalTimeOffsetSeconds;
            }
        }
        set => SetTimeOffset(value);
    }

    public event EventHandler<EventSnapshot>? SnapshotUpdated;
    public event EventHandler<NotificationTriggerEventArgs>? NotificationTriggered;
    public event EventHandler<int>? TimeOffsetChanged;

    public StarfallEventScheduler(ILocalizationService loc)
    {
        _loc = loc;
        _activeEvent = new EventDefinition
        {
            Id = "gravity-abyss",
            Name = "Gravity Abyss (Starfall Inversion)",
            CycleMinutes = 60,
            StartMinute = 22,
            DurationMinutes = 30,
            TimeOffsetSeconds = 0,
            ColorHex = "#00E5FF",
            Description = "Once Human Starfall Inversion / Gravity Abyss hourly event with low-gravity tide and Stardust Crystals."
        };

        CurrentSnapshot = CalculateSnapshot(DateTimeOffset.UtcNow);

        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
    }

    public void SetActiveEvent(EventDefinition eventDefinition, int globalTimeOffsetSeconds = 0)
    {
        lock (_lock)
        {
            _activeEvent = eventDefinition ?? throw new ArgumentNullException(nameof(eventDefinition));
            _globalTimeOffsetSeconds = globalTimeOffsetSeconds;
            CurrentSnapshot = CalculateSnapshot(DateTimeOffset.UtcNow);
        }

        SnapshotUpdated?.Invoke(this, CurrentSnapshot);
    }

    public void ConfigureSchedule(int startMinute, int durationMinutes, int alertBeforeStart, int alertBeforeEnd)
    {
        lock (_lock)
        {
            _activeEvent.StartMinute = Math.Clamp(startMinute, 0, Math.Max(1, _activeEvent.CycleMinutes - 1));
            _activeEvent.DurationMinutes = Math.Clamp(durationMinutes, 1, Math.Max(1, _activeEvent.CycleMinutes));
            _alertBeforeStart = Math.Max(0, alertBeforeStart);
            _alertBeforeEnd = Math.Max(0, alertBeforeEnd);

            CurrentSnapshot = CalculateSnapshot(DateTimeOffset.UtcNow);
        }

        SnapshotUpdated?.Invoke(this, CurrentSnapshot);
    }

    public void SetTimeOffset(int offsetSeconds)
    {
        lock (_lock)
        {
            _activeEvent.TimeOffsetSeconds = offsetSeconds - _globalTimeOffsetSeconds;
            CurrentSnapshot = CalculateSnapshot(DateTimeOffset.UtcNow);
        }

        TimeOffsetChanged?.Invoke(this, _activeEvent.TimeOffsetSeconds);
        SnapshotUpdated?.Invoke(this, CurrentSnapshot);
    }

    public int SyncToGameTime(TimeSpan remainingTime, bool? isUntilEnd = null)
    {
        int newOffset;
        lock (_lock)
        {
            var now = DateTimeOffset.UtcNow;
            int currentEffectiveOffset = _activeEvent.TimeOffsetSeconds + _globalTimeOffsetSeconds;
            var currentSnap = CalculateSnapshot(now, _activeEvent, currentEffectiveOffset);

            bool syncingToEnd = isUntilEnd ?? (remainingTime <= TimeSpan.FromMinutes(_activeEvent.DurationMinutes) && currentSnap.IsActive);

            DateTimeOffset targetTime = syncingToEnd
                ? (currentSnap.IsActive ? currentSnap.NextEventEnd : currentSnap.NextEventStart.AddMinutes(_activeEvent.DurationMinutes))
                : currentSnap.NextEventStart;

            TimeSpan rawRemainingWithoutOffset = targetTime - now;
            TimeSpan desiredOffset = rawRemainingWithoutOffset - remainingTime;
            newOffset = (int)Math.Round(desiredOffset.TotalSeconds);

            _activeEvent.TimeOffsetSeconds = newOffset - _globalTimeOffsetSeconds;
            CurrentSnapshot = CalculateSnapshot(now);
        }

        TimeOffsetChanged?.Invoke(this, newOffset);
        SnapshotUpdated?.Invoke(this, CurrentSnapshot);
        return newOffset;
    }

    public void Start()
    {
        lock (_lock)
        {
            CurrentSnapshot = CalculateSnapshot(DateTimeOffset.UtcNow);
        }
        _timer.Start();
    }

    public void Stop() => _timer.Stop();

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        EventSnapshot snapshot;
        lock (_lock)
        {
            snapshot = CalculateSnapshot(DateTimeOffset.UtcNow);
            CurrentSnapshot = snapshot;
            CheckNotificationTriggers(snapshot);
        }

        SnapshotUpdated?.Invoke(this, snapshot);
    }

    private void CheckNotificationTriggers(EventSnapshot snapshot)
    {
        long cycleKey = snapshot.NextEventStart.ToUnixTimeSeconds();

        if (snapshot.Phase == EventPhase.StartingSoon && _lastNotifiedStartSoonCycle != cycleKey)
        {
            int remainingMins = (int)Math.Ceiling(snapshot.TimeRemaining.TotalMinutes);
            _lastNotifiedStartSoonCycle = cycleKey;

            NotificationTriggered?.Invoke(this, new NotificationTriggerEventArgs
            {
                Title = _loc.GetString("Notification_StartingSoon_Title"),
                Message = _loc.Format("Notification_StartingSoon_Body", remainingMins),
                Phase = EventPhase.StartingSoon,
                TimeRemaining = snapshot.TimeRemaining,
                PlaySound = true,
                ShowToast = true
            });
        }

        if (snapshot.Phase == EventPhase.Active && _lastNotifiedActiveCycle != cycleKey)
        {
            _lastNotifiedActiveCycle = cycleKey;

            NotificationTriggered?.Invoke(this, new NotificationTriggerEventArgs
            {
                Title = _loc.GetString("Notification_Started_Title"),
                Message = _loc.GetString("Notification_Started_Body"),
                Phase = EventPhase.Active,
                TimeRemaining = snapshot.TimeRemaining,
                PlaySound = true,
                ShowToast = true
            });
        }

        if (snapshot.Phase == EventPhase.EndingSoon && _lastNotifiedEndSoonCycle != cycleKey)
        {
            int remainingMins = (int)Math.Ceiling(snapshot.TimeRemaining.TotalMinutes);
            _lastNotifiedEndSoonCycle = cycleKey;

            NotificationTriggered?.Invoke(this, new NotificationTriggerEventArgs
            {
                Title = _loc.GetString("Notification_EndingSoon_Title"),
                Message = _loc.Format("Notification_EndingSoon_Body", remainingMins),
                Phase = EventPhase.EndingSoon,
                TimeRemaining = snapshot.TimeRemaining,
                PlaySound = true,
                ShowToast = true
            });
        }
    }

    public EventSnapshot CalculateSnapshot(DateTimeOffset referenceTime)
    {
        lock (_lock)
        {
            int effectiveOffset = _activeEvent.TimeOffsetSeconds + _globalTimeOffsetSeconds;
            return CalculateSnapshot(referenceTime, _activeEvent, effectiveOffset);
        }
    }

    public EventSnapshot CalculateSnapshot(DateTimeOffset referenceTime, EventDefinition eventDefinition, int offsetSeconds = 0)
    {
        return EventTimeCalculator.CalculateSnapshot(
            referenceTime,
            eventDefinition,
            offsetSeconds,
            _alertBeforeStart,
            _alertBeforeEnd);
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}
