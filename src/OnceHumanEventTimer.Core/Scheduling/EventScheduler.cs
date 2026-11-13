// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Timers;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.Core.Scheduling;

/// <summary>
/// High-resolution engine evaluating cyclic Once Human events, applying time offsets, and triggering alerts.
/// </summary>
public class EventScheduler : IEventScheduler
{
    private readonly System.Timers.Timer _timer;
    private readonly NotificationEvaluator _evaluator;
    private readonly object _lock = new();

    private readonly List<EventDefinition> _trackedEvents = new();
    private EventDefinition _activeEvent;
    private int _globalTimeOffsetSeconds;
    private int _alertBeforeStart = 2;
    private int _alertBeforeEnd = 3;

    public EventSnapshot CurrentSnapshot { get; private set; }
    public IReadOnlyList<EventSnapshot> ActiveSnapshots { get; private set; }

    public EventDefinition ActiveEvent { get { lock (_lock) { return _activeEvent; } } }
    public IReadOnlyList<EventDefinition> TrackedEvents { get { lock (_lock) { return _trackedEvents.ToList(); } } }

    public int TimeOffsetSeconds
    {
        get { lock (_lock) { return _activeEvent.TimeOffsetSeconds + _globalTimeOffsetSeconds; } }
        set => SetTimeOffset(value);
    }

    public event EventHandler<EventSnapshot>? SnapshotUpdated;
    public event EventHandler<IReadOnlyList<EventSnapshot>>? SnapshotsUpdated;
    public event EventHandler<NotificationTriggerEventArgs>? NotificationTriggered;
    public event EventHandler<int>? TimeOffsetChanged;

    public EventScheduler(ILocalizationService loc)
    {
        _evaluator = new NotificationEvaluator(loc);
        _trackedEvents = DefaultEventPresets.CreateDefaultEvents();
        _activeEvent = _trackedEvents[0];

        var now = DateTimeOffset.UtcNow;
        CurrentSnapshot = CalculateSnapshot(now, _activeEvent, _activeEvent.TimeOffsetSeconds);
        ActiveSnapshots = new List<EventSnapshot> { CurrentSnapshot };
        UpdateSnapshotsLocked(now);

        _timer = new System.Timers.Timer(1000) { AutoReset = true };
        _timer.Elapsed += OnTimerElapsed;
    }

    public void SetActiveEvent(EventDefinition eventDefinition, int globalTimeOffsetSeconds = 0)
    {
        lock (_lock)
        {
            _activeEvent = eventDefinition ?? throw new ArgumentNullException(nameof(eventDefinition));
            _globalTimeOffsetSeconds = globalTimeOffsetSeconds;

            int index = _trackedEvents.FindIndex(e => string.Equals(e.Id, _activeEvent.Id, StringComparison.OrdinalIgnoreCase));
            if (index >= 0) _trackedEvents[index] = _activeEvent;
            else _trackedEvents.Insert(0, _activeEvent);

            UpdateSnapshotsLocked(DateTimeOffset.UtcNow);
        }

        SnapshotUpdated?.Invoke(this, CurrentSnapshot);
        SnapshotsUpdated?.Invoke(this, ActiveSnapshots);
    }

    public void SetTrackedEvents(IEnumerable<EventDefinition> events, string? activeEventId = null, int globalTimeOffsetSeconds = 0)
    {
        lock (_lock)
        {
            _trackedEvents.Clear();
            if (events != null) _trackedEvents.AddRange(events);
            if (_trackedEvents.Count == 0) _trackedEvents.AddRange(DefaultEventPresets.CreateDefaultEvents());

            _globalTimeOffsetSeconds = globalTimeOffsetSeconds;

            if (!string.IsNullOrEmpty(activeEventId))
            {
                var match = _trackedEvents.FirstOrDefault(e => string.Equals(e.Id, activeEventId, StringComparison.OrdinalIgnoreCase));
                if (match != null) _activeEvent = match;
            }
            else if (!_trackedEvents.Contains(_activeEvent))
            {
                var match = _trackedEvents.FirstOrDefault(e => string.Equals(e.Id, _activeEvent.Id, StringComparison.OrdinalIgnoreCase));
                _activeEvent = match ?? _trackedEvents[0];
            }

            UpdateSnapshotsLocked(DateTimeOffset.UtcNow);
        }

        SnapshotUpdated?.Invoke(this, CurrentSnapshot);
        SnapshotsUpdated?.Invoke(this, ActiveSnapshots);
    }

    public void ConfigureSchedule(int startMinute, int durationMinutes, int alertBeforeStart, int alertBeforeEnd, int startSecond = 0)
    {
        lock (_lock)
        {
            _activeEvent.StartMinute = Math.Clamp(startMinute, 0, Math.Max(1, _activeEvent.CycleMinutes - 1));
            _activeEvent.StartSecond = Math.Clamp(startSecond, 0, 59);
            _activeEvent.DurationMinutes = Math.Clamp(durationMinutes, 1, Math.Max(1, _activeEvent.CycleMinutes));
            _alertBeforeStart = Math.Max(0, alertBeforeStart);
            _alertBeforeEnd = Math.Max(0, alertBeforeEnd);
            UpdateSnapshotsLocked(DateTimeOffset.UtcNow);
        }

        SnapshotUpdated?.Invoke(this, CurrentSnapshot);
        SnapshotsUpdated?.Invoke(this, ActiveSnapshots);
    }

    public void SetTimeOffset(int offsetSeconds)
    {
        lock (_lock)
        {
            _activeEvent.TimeOffsetSeconds = offsetSeconds - _globalTimeOffsetSeconds;
            UpdateSnapshotsLocked(DateTimeOffset.UtcNow);
        }

        TimeOffsetChanged?.Invoke(this, _activeEvent.TimeOffsetSeconds);
        SnapshotUpdated?.Invoke(this, CurrentSnapshot);
        SnapshotsUpdated?.Invoke(this, ActiveSnapshots);
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
            UpdateSnapshotsLocked(now);
        }

        TimeOffsetChanged?.Invoke(this, newOffset);
        SnapshotUpdated?.Invoke(this, CurrentSnapshot);
        SnapshotsUpdated?.Invoke(this, ActiveSnapshots);
        return newOffset;
    }

    public void Start()
    {
        lock (_lock) { UpdateSnapshotsLocked(DateTimeOffset.UtcNow); }
        _timer.Start();
    }

    public void Stop() => _timer.Stop();

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        EventSnapshot currentSnap;
        IReadOnlyList<EventSnapshot> activeSnaps;
        var pendingAlerts = new List<NotificationTriggerEventArgs>();

        lock (_lock)
        {
            UpdateSnapshotsLocked(DateTimeOffset.UtcNow);
            currentSnap = CurrentSnapshot;
            activeSnaps = ActiveSnapshots;

            foreach (var snapshot in activeSnaps)
            {
                var alert = _evaluator.Evaluate(snapshot);
                if (alert != null) pendingAlerts.Add(alert);
            }
        }

        SnapshotUpdated?.Invoke(this, currentSnap);
        SnapshotsUpdated?.Invoke(this, activeSnaps);

        foreach (var alert in pendingAlerts)
        {
            NotificationTriggered?.Invoke(this, alert);
        }
    }

    private void UpdateSnapshotsLocked(DateTimeOffset now)
    {
        int activeOffset = _activeEvent.TimeOffsetSeconds + _globalTimeOffsetSeconds;
        CurrentSnapshot = CalculateSnapshot(now, _activeEvent, activeOffset);

        var enabledEvents = _trackedEvents.Where(e => e.IsEnabled).ToList();
        if (enabledEvents.Count == 0)
        {
            ActiveSnapshots = new List<EventSnapshot> { CurrentSnapshot };
            return;
        }

        var list = new List<EventSnapshot>(enabledEvents.Count);
        foreach (var ev in enabledEvents)
        {
            int effectiveOffset = ev.TimeOffsetSeconds + _globalTimeOffsetSeconds;
            list.Add(CalculateSnapshot(now, ev, effectiveOffset));
        }
        ActiveSnapshots = list;
    }

    public EventSnapshot CalculateSnapshot(DateTimeOffset referenceTime)
    {
        lock (_lock)
        {
            return CalculateSnapshot(referenceTime, _activeEvent, _activeEvent.TimeOffsetSeconds + _globalTimeOffsetSeconds);
        }
    }

    public EventSnapshot CalculateSnapshot(DateTimeOffset referenceTime, EventDefinition eventDefinition, int offsetSeconds = 0)
        => EventTimeCalculator.CalculateSnapshot(referenceTime, eventDefinition, offsetSeconds, _alertBeforeStart, _alertBeforeEnd);

    public IReadOnlyList<EventSnapshot> CalculateActiveSnapshots(DateTimeOffset referenceTime)
    {
        lock (_lock)
        {
            var enabled = _trackedEvents.Where(e => e.IsEnabled).ToList();
            if (enabled.Count == 0)
                return new[] { CalculateSnapshot(referenceTime, _activeEvent, _activeEvent.TimeOffsetSeconds + _globalTimeOffsetSeconds) };

            return enabled.Select(ev => CalculateSnapshot(referenceTime, ev, ev.TimeOffsetSeconds + _globalTimeOffsetSeconds)).ToList();
        }
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}
