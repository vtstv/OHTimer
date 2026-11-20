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
    private IBossTimerTracker? _bossTimer;

    [ObservableProperty]
    private bool _isBossTimerActive = false;

    [ObservableProperty]
    private string _bossTimerBadge = "RESPAWN";

    [ObservableProperty]
    private string _bossTimerName = "BOSS RESPAWN";

    [ObservableProperty]
    private string _bossTimerFormattedRemaining = "00:00";

    [ObservableProperty]
    private double _bossTimerProgressRatio = 0.0;

    [ObservableProperty]
    private string _bossTimerStatusColorHex = "#EF4444";

    [ObservableProperty]
    private bool _isBossTimerFlashing = false;

    [ObservableProperty]
    private bool _isBossTimerWarning = false;

    [ObservableProperty]
    private bool _isBossTimerSpawned = false;

    [ObservableProperty]
    private string _bossTimerStateIcon = "🎯";

    [ObservableProperty]
    private bool _isOverlayCardVisible = true;

    private void InitializeBossTimer(IBossTimerTracker bossTimer)
    {
        _bossTimer = bossTimer;
        _bossTimer.SnapshotUpdated += OnBossTimerSnapshotUpdated;
        UpdateBossTimerFromSnapshot(_bossTimer.CurrentSnapshot);
    }

    private void OnBossTimerSnapshotUpdated(object? sender, BossTimerSnapshot snapshot)
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => UpdateBossTimerFromSnapshot(snapshot));
        }
        else
        {
            UpdateBossTimerFromSnapshot(snapshot);
        }
    }

    private void UpdateBossTimerFromSnapshot(BossTimerSnapshot snapshot)
    {
        bool isEnabled = _settingsService?.Settings.BossTimer.IsEnabled ?? true;
        IsBossTimerActive = snapshot.IsActive && isEnabled;
        BossTimerFormattedRemaining = snapshot.FormattedRemaining;
        BossTimerProgressRatio = snapshot.ProgressRatio;
        BossTimerName = !string.IsNullOrWhiteSpace(snapshot.BossName) ? snapshot.BossName.ToUpperInvariant() : "BOSS RESPAWN";
        IsBossTimerWarning = snapshot.IsWarning;
        IsBossTimerSpawned = snapshot.IsSpawned;
        IsBossTimerFlashing = snapshot.IsFlashing;

        if (snapshot.IsSpawned)
        {
            BossTimerStateIcon = "✨";
            BossTimerBadge = $"✨ {_loc.GetString("BossTimer_Status_Spawned")}";
            BossTimerStatusColorHex = "#10B981";
        }
        else if (snapshot.IsWarning)
        {
            BossTimerStateIcon = "⚠️";
            BossTimerBadge = $"⚠️ {_loc.GetString("BossTimer_Status_Warning")}";
            BossTimerStatusColorHex = "#F59E0B";
        }
        else
        {
            BossTimerStateIcon = "🎯";
            BossTimerBadge = $"🎯 {_loc.GetString("BossTimer_Status_Respawn")}";
            BossTimerStatusColorHex = !string.IsNullOrWhiteSpace(snapshot.ColorHex) ? snapshot.ColorHex : "#EF4444";
        }

        UpdateOverlayCardVisibility();
    }

    public void UpdateOverlayCardVisibility()
    {
        bool isBossEnabled = _settingsService?.Settings.BossTimer.IsEnabled ?? true;
        bool isKillEnabled = _settingsService?.Settings.KillCounter.IsEnabled ?? false;
        IsOverlayCardVisible = IsTimerVisible || (IsBossTimerActive && isBossEnabled) || (IsKillCounterActive && isKillEnabled);
    }

    [RelayCommand]
    private void DismissBossTimer()
    {
        _bossTimer?.Stop();
    }

    [RelayCommand]
    private void TriggerBossTimer()
    {
        _bossTimer?.HandleHotkeyPress();
    }
}
