// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Services;
using OnceHumanEventTimer.Infrastructure.Win32;

namespace OnceHumanEventTimer.UI.ViewModels;

public record BossRepressOption(BossTimerRepressBehavior Behavior, string DisplayName);

public partial class SettingsViewModel
{
    private IBossTimerTracker? _bossTimerTracker;

    [ObservableProperty]
    private bool _bossTimerEnabled = true;

    partial void OnBossTimerEnabledChanged(bool value)
    {
        if (!value)
        {
            _bossTimerTracker?.Stop();
            if (_settingsService?.Settings.BossTimer != null)
            {
                _settingsService.Settings.BossTimer.IsEnabled = false;
                _bossTimerTracker?.Configure(_settingsService.Settings.BossTimer);
                _settingsService.NotifyChanged();
            }
        }
        else
        {
            if (_settingsService?.Settings.BossTimer != null)
            {
                _settingsService.Settings.BossTimer.IsEnabled = true;
                _bossTimerTracker?.Configure(_settingsService.Settings.BossTimer);
                _settingsService.NotifyChanged();
            }
        }
    }

    [ObservableProperty]
    private string _bossTimerHotkey = "Alt+NumPad 1";

    [ObservableProperty]
    private BossTimerRepressBehavior _bossTimerRepressBehavior = BossTimerRepressBehavior.Restart;

    [ObservableProperty]
    private bool _bossTimerPlayHotkeySound = true;

    [ObservableProperty]
    private string _bossTimerName = "Boss Respawn";

    [ObservableProperty]
    private int _bossTimerMinutes = 5;

    [ObservableProperty]
    private int _bossTimerSeconds = 0;

    [ObservableProperty]
    private bool _bossTimerEnableWarningAlert = true;

    [ObservableProperty]
    private int _bossTimerWarningSeconds = 30;

    [ObservableProperty]
    private SoundPreset _bossTimerWarningSoundPreset = SoundPreset.CyberWarning;

    [ObservableProperty]
    private bool _bossTimerEnableSpawnAlert = true;

    [ObservableProperty]
    private SoundPreset _bossTimerSpawnSoundPreset = SoundPreset.EmergencySiren;

    [ObservableProperty]
    private double _bossTimerVolume = 0.8;

    [ObservableProperty]
    private bool _bossTimerEnableRepeatAlert = true;

    [ObservableProperty]
    private int _bossTimerRepeatCount = 3;

    [ObservableProperty]
    private int _bossTimerRepeatInterval = 10;

    [ObservableProperty]
    private bool _bossTimerShowToast = true;

    [ObservableProperty]
    private bool _bossTimerFlashCard = true;

    [ObservableProperty]
    private int _bossTimerAutoHideSeconds = 15;

    private string _bossTimerColorHex = "#EF4444";
    public string BossTimerColorHex
    {
        get => _bossTimerColorHex;
        set
        {
            var val = value?.Trim();
            if (!string.IsNullOrEmpty(val) && !val.StartsWith('#') && (val.Length == 6 || val.Length == 8))
            {
                val = "#" + val;
            }
            SetProperty(ref _bossTimerColorHex, val ?? "#EF4444");
        }
    }

    [ObservableProperty]
    private ObservableCollection<BossRepressOption> _availableRepressBehaviors = new();

    private void InitializeBossTimer(UserSettings current, IBossTimerTracker? bossTimerTracker = null)
    {
        _bossTimerTracker = bossTimerTracker;
        var bt = current.BossTimer ?? new BossTimerSettings();
        BossTimerEnabled = bt.IsEnabled;
        BossTimerHotkey = !string.IsNullOrWhiteSpace(bt.Hotkey) ? bt.Hotkey : "Alt+NumPad 1";
        BossTimerRepressBehavior = bt.RepressBehavior;
        BossTimerPlayHotkeySound = bt.PlayHotkeySound;
        BossTimerName = !string.IsNullOrWhiteSpace(bt.BossName) ? bt.BossName : "Boss Respawn";
        BossTimerMinutes = bt.DurationMinutes;
        BossTimerSeconds = bt.DurationSeconds;
        BossTimerEnableWarningAlert = bt.EnableWarningAlert;
        BossTimerWarningSeconds = bt.WarningSecondsAhead;
        BossTimerWarningSoundPreset = bt.WarningSoundPreset;
        BossTimerEnableSpawnAlert = bt.EnableSpawnAlert;
        BossTimerSpawnSoundPreset = bt.SpawnSoundPreset;
        BossTimerVolume = bt.Volume;
        BossTimerEnableRepeatAlert = bt.EnableRepeatAlert;
        BossTimerRepeatCount = bt.RepeatAlertCount;
        BossTimerRepeatInterval = bt.RepeatAlertIntervalSeconds;
        BossTimerShowToast = bt.ShowToastOnSpawn;
        BossTimerFlashCard = bt.FlashCardOnSpawn;
        BossTimerAutoHideSeconds = bt.AutoHideSeconds;
        BossTimerColorHex = !string.IsNullOrWhiteSpace(bt.ColorHex) ? bt.ColorHex : "#EF4444";

        PopulateRepressBehaviors();
        _loc.LanguageChanged += (s, e) => PopulateRepressBehaviors();
    }

    private void PopulateRepressBehaviors()
    {
        var current = BossTimerRepressBehavior;
        AvailableRepressBehaviors.Clear();
        AvailableRepressBehaviors.Add(new BossRepressOption(BossTimerRepressBehavior.Restart, _loc.GetString("BossTimer_Repress_Restart")));
        AvailableRepressBehaviors.Add(new BossRepressOption(BossTimerRepressBehavior.Toggle, _loc.GetString("BossTimer_Repress_Toggle")));
        AvailableRepressBehaviors.Add(new BossRepressOption(BossTimerRepressBehavior.Stop, _loc.GetString("BossTimer_Repress_Stop")));
        BossTimerRepressBehavior = current;
    }

    [RelayCommand]
    private void StartBossTimer()
    {
        if (_bossTimerTracker != null)
        {
            var s = _settingsService.Settings.BossTimer;
            s.IsEnabled = true;
            BossTimerEnabled = true;
            s.DurationMinutes = BossTimerMinutes;
            s.DurationSeconds = BossTimerSeconds;
            s.BossName = BossTimerName;
            s.Hotkey = BossTimerHotkey;
            s.RepressBehavior = BossTimerRepressBehavior;
            s.PlayHotkeySound = BossTimerPlayHotkeySound;
            s.EnableWarningAlert = BossTimerEnableWarningAlert;
            s.WarningSecondsAhead = BossTimerWarningSeconds;
            s.WarningSoundPreset = BossTimerWarningSoundPreset;
            s.EnableSpawnAlert = BossTimerEnableSpawnAlert;
            s.SpawnSoundPreset = BossTimerSpawnSoundPreset;
            s.Volume = BossTimerVolume;
            s.EnableRepeatAlert = BossTimerEnableRepeatAlert;
            s.RepeatAlertCount = BossTimerRepeatCount;
            s.RepeatAlertIntervalSeconds = BossTimerRepeatInterval;
            s.ShowToastOnSpawn = BossTimerShowToast;
            s.FlashCardOnSpawn = BossTimerFlashCard;
            s.AutoHideSeconds = BossTimerAutoHideSeconds;
            s.ColorHex = BossTimerColorHex;
            _bossTimerTracker.Configure(s);
            _bossTimerTracker.Start();
        }
    }

    [RelayCommand]
    private void StopBossTimer()
    {
        _bossTimerTracker?.Stop();
    }

    [RelayCommand]
    private void ResetBossTimerHotkey()
    {
        BossTimerHotkey = "Alt+NumPad 1";
    }

    [RelayCommand]
    private void SetBossPreset(string minutesStr)
    {
        if (int.TryParse(minutesStr, out int m) && m > 0)
        {
            BossTimerMinutes = m;
            BossTimerSeconds = 0;
        }
    }

    [RelayCommand]
    private void TestBossWarningSound()
    {
        _audio.PlayPreset(BossTimerWarningSoundPreset, BossTimerVolume);
    }

    [RelayCommand]
    private void TestBossSpawnSound()
    {
        _audio.PlayPreset(BossTimerSpawnSoundPreset, BossTimerVolume);
    }

    [RelayCommand]
    private void PickBossColor()
    {
        var activeWindow = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                           ?? Application.Current?.MainWindow;
        nint hwnd = activeWindow != null ? new WindowInteropHelper(activeWindow).Handle : 0;

        string? newColor = NativeColorDialog.ShowDialog(hwnd, BossTimerColorHex);
        if (!string.IsNullOrWhiteSpace(newColor))
        {
            BossTimerColorHex = newColor;
        }
    }
}
