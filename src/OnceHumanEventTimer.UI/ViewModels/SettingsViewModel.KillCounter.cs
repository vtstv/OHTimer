// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Services;
using OnceHumanEventTimer.Infrastructure.Win32;

namespace OnceHumanEventTimer.UI.ViewModels;

public partial class SettingsViewModel
{
    private IKillCounterTracker? _killCounterTracker;
    private EventHandler<KillCounterSnapshot>? _killCounterSnapshotHandler;
    private bool _isUpdatingFromSnapshot;

    [ObservableProperty]
    private bool _killCounterEnabled;

    partial void OnKillCounterEnabledChanged(bool value)
    {
        if (_settingsService?.Settings.KillCounter != null)
        {
            _settingsService.Settings.KillCounter.IsEnabled = value;
            _killCounterTracker?.Configure(_settingsService.Settings.KillCounter);
        }
    }

    [ObservableProperty]
    private string _killCounterBossName = "Shadow Hound";

    partial void OnKillCounterBossNameChanged(string value)
    {
        _killCounterTracker?.SetBossName(value);
    }

    [ObservableProperty]
    private int _killCounterCurrentCount;

    partial void OnKillCounterCurrentCountChanged(int value)
    {
        if (value < 0)
        {
            KillCounterCurrentCount = 0;
            return;
        }
        if (!_isUpdatingFromSnapshot)
            _killCounterTracker?.SetCurrentCount(value);
    }

    [ObservableProperty]
    private bool _killCounterEnableGoal;

    partial void OnKillCounterEnableGoalChanged(bool value)
    {
        _killCounterTracker?.SetGoal(value, KillCounterGoalCount);
    }

    [ObservableProperty]
    private int _killCounterGoalCount = 20;

    partial void OnKillCounterGoalCountChanged(int value)
    {
        if (value < 1)
        {
            KillCounterGoalCount = 1;
            return;
        }
        _killCounterTracker?.SetGoal(KillCounterEnableGoal, value);
    }

    [ObservableProperty]
    private int _killCounterTotalAllTimeCount;

    [ObservableProperty]
    private string _killCounterIncrementHotkey = "NumPad +";

    [ObservableProperty]
    private string _killCounterDecrementHotkey = "NumPad -";

    [ObservableProperty]
    private bool _killCounterPlaySound = true;

    [ObservableProperty]
    private bool _killCounterAutoStartBossTimer;

    [ObservableProperty]
    private bool _killCounterShowInMiniHud = true;

    [ObservableProperty]
    private bool _killCounterFlashOnGoalReached = true;

    private string _killCounterColorHex = "#EF4444";
    public string KillCounterColorHex
    {
        get => _killCounterColorHex;
        set
        {
            var val = value?.Trim();
            if (!string.IsNullOrEmpty(val) && !val.StartsWith('#') && (val.Length == 6 || val.Length == 8))
            {
                val = "#" + val;
            }
            SetProperty(ref _killCounterColorHex, val ?? "#EF4444");
        }
    }

    private void InitializeKillCounter(UserSettings current, IKillCounterTracker? killCounterTracker = null)
    {
        _killCounterTracker = killCounterTracker;
        var ks = current.KillCounter ?? new KillCounterSettings();
        KillCounterEnabled = ks.IsEnabled;
        KillCounterBossName = ks.BossName;
        KillCounterCurrentCount = ks.CurrentCount;
        KillCounterEnableGoal = ks.EnableGoal;
        KillCounterGoalCount = ks.GoalCount;
        KillCounterTotalAllTimeCount = ks.TotalAllTimeCount;
        KillCounterIncrementHotkey = ks.IncrementHotkey;
        KillCounterDecrementHotkey = ks.DecrementHotkey;
        KillCounterPlaySound = ks.PlaySound;
        KillCounterAutoStartBossTimer = ks.AutoStartBossTimer;
        KillCounterShowInMiniHud = ks.ShowInMiniHud;
        KillCounterFlashOnGoalReached = ks.FlashOnGoalReached;
        KillCounterColorHex = ks.ColorHex;

        if (_killCounterTracker != null)
        {
            _killCounterSnapshotHandler = (s, snapshot) =>
            {
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    _isUpdatingFromSnapshot = true;
                    try
                    {
                        KillCounterCurrentCount = snapshot.CurrentCount;
                        KillCounterTotalAllTimeCount = snapshot.TotalAllTimeCount;
                    }
                    finally
                    {
                        _isUpdatingFromSnapshot = false;
                    }
                });
            };
            _killCounterTracker.SnapshotUpdated += _killCounterSnapshotHandler;
        }
    }

    public void CleanupKillCounter()
    {
        if (_killCounterTracker != null && _killCounterSnapshotHandler != null)
        {
            _killCounterTracker.SnapshotUpdated -= _killCounterSnapshotHandler;
            _killCounterSnapshotHandler = null;
        }
    }

    [RelayCommand]
    private void IncrementKillCount() => _killCounterTracker?.Increment();

    [RelayCommand]
    private void DecrementKillCount() => _killCounterTracker?.Decrement();

    [RelayCommand]
    private void ResetKillCounterSession()
    {
        if (KillCounterCurrentCount > 0)
        {
            string prompt = _loc.GetString("KillCounter_ResetPrompt");
            string title = _loc.GetString("KillCounter_ResetSession");
            if (!Confirm(prompt, title)) return;
        }
        _killCounterTracker?.ResetSession();
    }

    [RelayCommand]
    private void SetKillCounterPreset(string presetName)
    {
        if (!string.IsNullOrWhiteSpace(presetName))
        {
            KillCounterBossName = presetName;
        }
    }

    [RelayCommand]
    private void ResetKillCounterIncrementHotkey() => KillCounterIncrementHotkey = "NumPad +";

    [RelayCommand]
    private void ResetKillCounterDecrementHotkey() => KillCounterDecrementHotkey = "NumPad -";

    [RelayCommand]
    private void PickKillCounterColor()
    {
        var activeWindow = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                           ?? Application.Current?.MainWindow;
        nint hwnd = activeWindow != null ? new WindowInteropHelper(activeWindow).Handle : 0;

        string? newColor = NativeColorDialog.ShowDialog(hwnd, KillCounterColorHex);
        if (!string.IsNullOrWhiteSpace(newColor))
        {
            KillCounterColorHex = newColor;
        }
    }
}
