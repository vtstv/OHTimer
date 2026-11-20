// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.UI.ViewModels;

public partial class OverlayViewModel
{
    private IKillCounterTracker? _killCounter;

    [ObservableProperty]
    private bool _isKillCounterActive;

    [ObservableProperty]
    private string _killCounterBossName = "SHADOW HOUND";

    [ObservableProperty]
    private int _killCounterCurrentCount;

    [ObservableProperty]
    private bool _killCounterEnableGoal;

    [ObservableProperty]
    private int _killCounterGoalCount = 20;

    [ObservableProperty]
    private string _killCounterFormattedCount = "0";

    [ObservableProperty]
    private double _killCounterProgressRatio;

    [ObservableProperty]
    private bool _isKillCounterGoalReached;

    [ObservableProperty]
    private string _killCounterStatusColorHex = "#EF4444";

    [ObservableProperty]
    private bool _isKillCounterFlashing;

    [ObservableProperty]
    private bool _killCounterShowInMiniHud = true;

    [ObservableProperty]
    private bool _isMiniKillCounterVisible;

    [ObservableProperty]
    private string _killCounterBadge = "KILLS";

    private void InitializeKillCounter(IKillCounterTracker killCounter)
    {
        _killCounter = killCounter;
        _killCounter.SnapshotUpdated += OnKillCounterSnapshotUpdated;
        UpdateKillCounterFromSnapshot(_killCounter.CurrentSnapshot);
    }

    private void OnKillCounterSnapshotUpdated(object? sender, KillCounterSnapshot snapshot)
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => UpdateKillCounterFromSnapshot(snapshot));
        }
        else
        {
            UpdateKillCounterFromSnapshot(snapshot);
        }
    }

    private void UpdateKillCounterFromSnapshot(KillCounterSnapshot snapshot)
    {
        bool isEnabled = snapshot.IsEnabled;
        IsKillCounterActive = isEnabled;
        KillCounterBossName = !string.IsNullOrWhiteSpace(snapshot.BossName) ? snapshot.BossName.ToUpperInvariant() : "TARGET";
        KillCounterCurrentCount = snapshot.CurrentCount;
        KillCounterEnableGoal = snapshot.EnableGoal;
        KillCounterGoalCount = snapshot.GoalCount;
        KillCounterFormattedCount = snapshot.FormattedCount;
        KillCounterProgressRatio = snapshot.ProgressRatio;
        IsKillCounterGoalReached = snapshot.IsGoalReached;
        KillCounterStatusColorHex = !string.IsNullOrWhiteSpace(snapshot.ColorHex) ? snapshot.ColorHex : "#EF4444";
        IsKillCounterFlashing = snapshot.FlashOnGoalReached;
        KillCounterShowInMiniHud = snapshot.ShowInMiniHud;
        IsMiniKillCounterVisible = isEnabled && snapshot.ShowInMiniHud;

        if (snapshot.IsGoalReached)
        {
            KillCounterBadge = $"🎉 {_loc.GetString("KillCounter_BadgeGoal")}";
        }
        else if (snapshot.EnableGoal)
        {
            int pct = (int)Math.Round(snapshot.ProgressRatio * 100);
            KillCounterBadge = $"{pct}%";
        }
        else
        {
            KillCounterBadge = $"⚔️ {_loc.GetString("KillCounter_BadgeKills")}";
        }

        UpdateOverlayCardVisibility();
    }

    [RelayCommand]
    private void IncrementKillCounter() => _killCounter?.Increment();

    [RelayCommand]
    private void DecrementKillCounter() => _killCounter?.Decrement();

    [RelayCommand]
    private async Task DismissKillCounterAsync()
    {
        if (_settingsService?.Settings.KillCounter != null)
        {
            _settingsService.Settings.KillCounter.IsEnabled = false;
            _killCounter?.Configure(_settingsService.Settings.KillCounter);
            await _settingsService.SaveAsync();
        }
        IsKillCounterActive = false;
        UpdateOverlayCardVisibility();
    }
}
