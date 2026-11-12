// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Timers;
using StarfallTimer.Core.Models;
using StarfallTimer.Core.Services;

namespace StarfallTimer.Core.Scheduling;

/// <summary>
/// Engine evaluating the Starfall Inversion hourly cycle and raising timer/alert events.
/// </summary>
public sealed class StarfallEventScheduler : IEventScheduler
{
    private readonly System.Timers.Timer _timer;
    private readonly ILocalizationService _loc;
    private readonly object _lock = new();

    private int _startMinute = 22;
    private int _durationMinutes = 30;
    private int _alertBeforeStart = 2;
    private int _alertBeforeEnd = 3;

    // Cycle tracking to avoid duplicate notifications within the same event
    private long _lastNotifiedStartSoonCycle = -1;
    private long _lastNotifiedActiveCycle = -1;
    private long _lastNotifiedEndSoonCycle = -1;

    public EventSnapshot CurrentSnapshot { get; private set; }

    public event EventHandler<EventSnapshot>? SnapshotUpdated;
    public event EventHandler<NotificationTriggerEventArgs>? NotificationTriggered;

    public StarfallEventScheduler(ILocalizationService loc)
    {
        _loc = loc;
        CurrentSnapshot = CalculateSnapshot(DateTimeOffset.UtcNow);

        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
    }

    public void ConfigureSchedule(int startMinute, int durationMinutes, int alertBeforeStart, int alertBeforeEnd)
    {
        lock (_lock)
        {
            _startMinute = Math.Clamp(startMinute, 0, 59);
            _durationMinutes = Math.Clamp(durationMinutes, 1, 59);
            _alertBeforeStart = Math.Max(0, alertBeforeStart);
            _alertBeforeEnd = Math.Max(0, alertBeforeEnd);

            CurrentSnapshot = CalculateSnapshot(DateTimeOffset.UtcNow);
        }
    }

    public void Start()
    {
        CurrentSnapshot = CalculateSnapshot(DateTimeOffset.UtcNow);
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

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

        // Trigger: Starting Soon (e.g. 2 minutes before)
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

        // Trigger: Event Started (Active)
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

        // Trigger: Ending Soon (e.g. 3 minutes before ending)
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
        // Align to base hour
        var baseHour = new DateTimeOffset(
            referenceTime.Year,
            referenceTime.Month,
            referenceTime.Day,
            referenceTime.Hour,
            0,
            0,
            referenceTime.Offset);

        var currentHourStart = baseHour.AddMinutes(_startMinute);
        var currentHourEnd = currentHourStart.AddMinutes(_durationMinutes);

        DateTimeOffset nextStart;
        DateTimeOffset nextEnd;
        EventPhase phase;
        TimeSpan timeRemaining;
        TimeSpan totalPhaseDuration;

        if (referenceTime < currentHourStart)
        {
            // Case 1: Before current hour's event
            var prevHourEnd = baseHour.AddHours(-1).AddMinutes(_startMinute).AddMinutes(_durationMinutes);
            totalPhaseDuration = currentHourStart - prevHourEnd;
            timeRemaining = currentHourStart - referenceTime;
            nextStart = currentHourStart;
            nextEnd = currentHourEnd;

            bool isStartingSoon = timeRemaining <= TimeSpan.FromMinutes(_alertBeforeStart);
            phase = isStartingSoon ? EventPhase.StartingSoon : EventPhase.Inactive;
        }
        else if (referenceTime < currentHourEnd)
        {
            // Case 2: During current hour's event
            totalPhaseDuration = TimeSpan.FromMinutes(_durationMinutes);
            timeRemaining = currentHourEnd - referenceTime;
            nextStart = currentHourStart;
            nextEnd = currentHourEnd;

            bool isEndingSoon = timeRemaining <= TimeSpan.FromMinutes(_alertBeforeEnd);
            phase = isEndingSoon ? EventPhase.EndingSoon : EventPhase.Active;
        }
        else
        {
            // Case 3: After current hour's event, waiting for next hour
            var nextHourBase = baseHour.AddHours(1);
            nextStart = nextHourBase.AddMinutes(_startMinute);
            nextEnd = nextStart.AddMinutes(_durationMinutes);

            totalPhaseDuration = nextStart - currentHourEnd;
            timeRemaining = nextStart - referenceTime;

            bool isStartingSoon = timeRemaining <= TimeSpan.FromMinutes(_alertBeforeStart);
            phase = isStartingSoon ? EventPhase.StartingSoon : EventPhase.Inactive;
        }

        // Clamp remaining time from dipping below zero
        if (timeRemaining < TimeSpan.Zero)
        {
            timeRemaining = TimeSpan.Zero;
        }

        double elapsedSeconds = Math.Max(0, totalPhaseDuration.TotalSeconds - timeRemaining.TotalSeconds);
        double progress = totalPhaseDuration.TotalSeconds > 0
            ? Math.Clamp(elapsedSeconds / totalPhaseDuration.TotalSeconds, 0.0, 1.0)
            : 0.0;

        return new EventSnapshot
        {
            Phase = phase,
            TimeRemaining = timeRemaining,
            TotalPhaseDuration = totalPhaseDuration,
            ProgressRatio = progress,
            NextEventStart = nextStart,
            NextEventEnd = nextEnd
        };
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}
