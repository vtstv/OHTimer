// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using StarfallTimer.Core.Models;
using StarfallTimer.Core.Services;

namespace StarfallTimer.Core.Scheduling;

/// <summary>
/// Evaluates event phase transitions and thresholds to trigger notifications without duplicates.
/// </summary>
public sealed class NotificationEvaluator
{
    private readonly ILocalizationService _loc;
    private readonly Dictionary<string, long> _lastStartSoon = new();
    private readonly Dictionary<string, long> _lastActive = new();
    private readonly Dictionary<string, long> _lastEndSoon = new();

    public NotificationEvaluator(ILocalizationService loc) => _loc = loc;

    /// <summary>
    /// Evaluates snapshot and returns notification arguments if an alert threshold is met.
    /// </summary>
    public NotificationTriggerEventArgs? Evaluate(EventSnapshot snapshot)
    {
        string eventId = snapshot.Event?.Id ?? snapshot.EventName;
        long cycleKey = snapshot.NextEventStart.ToUnixTimeSeconds();
        string eventTitle = snapshot.Event?.Name ?? snapshot.EventName;

        if (snapshot.Phase == EventPhase.StartingSoon)
        {
            if (!_lastStartSoon.TryGetValue(eventId, out var last) || last != cycleKey)
            {
                _lastStartSoon[eventId] = cycleKey;
                int remainingMins = (int)Math.Ceiling(snapshot.TimeRemaining.TotalMinutes);
                string title = string.IsNullOrWhiteSpace(eventTitle) ? _loc.GetString("Notification_StartingSoon_Title") : $"{eventTitle} — {_loc.GetString("Notification_StartingSoon_Title")}";

                return new NotificationTriggerEventArgs
                {
                    Title = title,
                    Message = _loc.Format("Notification_StartingSoon_Body", remainingMins),
                    Phase = EventPhase.StartingSoon,
                    TimeRemaining = snapshot.TimeRemaining,
                    PlaySound = true,
                    ShowToast = true,
                    Event = snapshot.Event
                };
            }
        }
        else if (snapshot.Phase == EventPhase.Active)
        {
            if (!_lastActive.TryGetValue(eventId, out var last) || last != cycleKey)
            {
                _lastActive[eventId] = cycleKey;
                string title = string.IsNullOrWhiteSpace(eventTitle) ? _loc.GetString("Notification_Started_Title") : $"{eventTitle} — {_loc.GetString("Notification_Started_Title")}";

                return new NotificationTriggerEventArgs
                {
                    Title = title,
                    Message = _loc.GetString("Notification_Started_Body"),
                    Phase = EventPhase.Active,
                    TimeRemaining = snapshot.TimeRemaining,
                    PlaySound = true,
                    ShowToast = true,
                    Event = snapshot.Event
                };
            }
        }
        else if (snapshot.Phase == EventPhase.EndingSoon)
        {
            if (!_lastEndSoon.TryGetValue(eventId, out var last) || last != cycleKey)
            {
                _lastEndSoon[eventId] = cycleKey;
                int remainingMins = (int)Math.Ceiling(snapshot.TimeRemaining.TotalMinutes);
                string title = string.IsNullOrWhiteSpace(eventTitle) ? _loc.GetString("Notification_EndingSoon_Title") : $"{eventTitle} — {_loc.GetString("Notification_EndingSoon_Title")}";

                return new NotificationTriggerEventArgs
                {
                    Title = title,
                    Message = _loc.Format("Notification_EndingSoon_Body", remainingMins),
                    Phase = EventPhase.EndingSoon,
                    TimeRemaining = snapshot.TimeRemaining,
                    PlaySound = true,
                    ShowToast = true,
                    Event = snapshot.Event
                };
            }
        }

        return null;
    }
}
