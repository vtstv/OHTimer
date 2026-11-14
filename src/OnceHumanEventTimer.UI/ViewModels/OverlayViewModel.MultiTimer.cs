// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using OnceHumanEventTimer.Core.Models;

namespace OnceHumanEventTimer.UI.ViewModels;

public partial class OverlayViewModel
{
    [ObservableProperty]
    private ObservableCollection<TimerItemViewModel> _activeTimers = new();

    [ObservableProperty]
    private bool _isMultiTimer;

    private void InitializeMultiTimer()
    {
        _scheduler.SnapshotsUpdated += OnSnapshotsUpdated;
        UpdateActiveTimers(_scheduler.ActiveSnapshots);
    }

    private void OnSnapshotsUpdated(object? sender, IReadOnlyList<EventSnapshot> snapshots)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            UpdateActiveTimers(snapshots);
        });
    }

    private void UpdateActiveTimers(IReadOnlyList<EventSnapshot> snapshots)
    {
        if (snapshots == null || snapshots.Count == 0)
        {
            var fallback = _scheduler.CurrentSnapshot;
            if (fallback != null) snapshots = new List<EventSnapshot> { fallback };
            else return;
        }

        // Filter snapshots: events with ShowOnlyWhenUpcoming are displayed only when active or within threshold
        var visibleSnapshots = snapshots.Where(s =>
        {
            if (s.Event == null) return true;
            if (!s.Event.ShowOnlyWhenUpcoming) return true;
            if (s.IsActive) return true;
            int threshold = s.Event.UpcomingThresholdMinutes > 0 ? s.Event.UpcomingThresholdMinutes : 10;
            return s.TimeRemaining <= TimeSpan.FromMinutes(threshold);
        }).ToList();

        // If all enabled events are hidden by upcoming filter, keep the closest one as fallback
        if (visibleSnapshots.Count == 0 && snapshots.Count > 0)
        {
            var closest = snapshots.OrderBy(s => s.TimeRemaining).FirstOrDefault();
            if (closest != null) visibleSnapshots.Add(closest);
        }

        IsMultiTimer = visibleSnapshots.Count > 1;

        // Dynamic ordering: active events first, followed by events closest to starting (shortest remaining time)
        var sortedSnapshots = visibleSnapshots
            .OrderBy(s => s.IsActive ? 0 : 1)
            .ThenBy(s => s.TimeRemaining)
            .ToList();

        var targetIds = new HashSet<string>();

        foreach (var snapshot in sortedSnapshots)
        {
            string id = snapshot.Event?.Id ?? "default";
            targetIds.Add(id);

            var item = ActiveTimers.FirstOrDefault(t => t.Id == id);
            if (item == null)
            {
                item = new TimerItemViewModel { Id = id };
                ActiveTimers.Add(item);
            }

            string eventName = !string.IsNullOrWhiteSpace(snapshot.EventName)
                ? snapshot.EventName
                : (snapshot.Event?.Name ?? "EVENT");

            item.Name = eventName.ToUpperInvariant();
            item.FormattedRemaining = snapshot.FormattedRemaining;
            item.ProgressRatio = snapshot.ProgressRatio;
            item.Phase = snapshot.Phase;
            item.IsActive = snapshot.IsActive;
            item.StatusColorHex = GetPhaseColor(snapshot.Phase, snapshot.Event?.ColorHex);
            item.PhaseBadgeText = GetPhaseBadgeText(snapshot.Phase);
            item.PhaseDescriptionText = snapshot.IsActive
                ? _loc.GetString("HUD_UntilEnd")
                : _loc.GetString("HUD_UntilStart");

            item.NextEventTimeText = snapshot.IsActive
                ? $"{_loc.GetString("HUD_UntilEnd")}: {snapshot.NextEventEnd.ToLocalTime():HH:mm}"
                : $"{_loc.GetString("HUD_UntilStart")}: {snapshot.NextEventStart.ToLocalTime():HH:mm}";

            item.DriftSyncText = snapshot.EffectiveOffsetSeconds != 0
                ? (snapshot.EffectiveOffsetSeconds > 0 ? $"+{snapshot.EffectiveOffsetSeconds}s" : $"{snapshot.EffectiveOffsetSeconds}s")
                : string.Empty;
        }

        for (int i = ActiveTimers.Count - 1; i >= 0; i--)
        {
            if (!targetIds.Contains(ActiveTimers[i].Id))
            {
                ActiveTimers.RemoveAt(i);
            }
        }

        // Align ActiveTimers order with dynamically sorted snapshots
        for (int i = 0; i < sortedSnapshots.Count; i++)
        {
            string expectedId = sortedSnapshots[i].Event?.Id ?? "default";
            int currentIndex = -1;
            for (int j = 0; j < ActiveTimers.Count; j++)
            {
                if (ActiveTimers[j].Id == expectedId)
                {
                    currentIndex = j;
                    break;
                }
            }

            if (currentIndex >= 0 && currentIndex != i && i < ActiveTimers.Count)
            {
                ActiveTimers.Move(currentIndex, i);
            }
        }
    }

    private static string GetPhaseColor(EventPhase phase, string? customColor)
    {
        return phase switch
        {
            EventPhase.StartingSoon => "#F59E0B",
            EventPhase.Active => !string.IsNullOrWhiteSpace(customColor) ? customColor : "#A855F7",
            EventPhase.EndingSoon => "#EF4444",
            _ => !string.IsNullOrWhiteSpace(customColor) ? customColor : "#00E5FF"
        };
    }

    private string GetPhaseBadgeText(EventPhase phase)
    {
        return phase switch
        {
            EventPhase.StartingSoon => _loc.GetString("Status_StartingSoon"),
            EventPhase.Active => _loc.GetString("Status_Active"),
            EventPhase.EndingSoon => _loc.GetString("Status_EndingSoon"),
            _ => _loc.GetString("Status_Waiting")
        };
    }
}
