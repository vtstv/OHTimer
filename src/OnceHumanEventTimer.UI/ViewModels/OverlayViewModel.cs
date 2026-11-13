// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.UI.ViewModels;

/// <summary>
/// ViewModel driving the floating HUD overlay widget.
/// </summary>
public partial class OverlayViewModel : ObservableObject
{
    private readonly IEventScheduler _scheduler;
    private readonly ISettingsService _settingsService;
    private readonly ILocalizationService _loc;

    [ObservableProperty]
    private string _formattedRemaining = "00:00";

    [ObservableProperty]
    private string _phaseBadgeText = string.Empty;

    [ObservableProperty]
    private string _phaseDescriptionText = string.Empty;

    [ObservableProperty]
    private double _progressRatio = 0.0;

    [ObservableProperty]
    private EventPhase _currentPhase = EventPhase.Inactive;

    [ObservableProperty]
    private bool _isActive = false;

    [ObservableProperty]
    private bool _isClickThrough = false;

    [ObservableProperty]
    private bool _isLocked = false;

    [ObservableProperty]
    private bool _alwaysOnTop = true;

    [ObservableProperty]
    private bool _compactMode = false;

    [ObservableProperty]
    private bool _showBorder = true;

    [ObservableProperty]
    private bool _showBackground = true;

    [ObservableProperty]
    private bool _showProgressRing = true;

    [ObservableProperty]
    private bool _showNextEventTime = true;

    [ObservableProperty]
    private bool _showMiniControls = true;

    [ObservableProperty]
    private double _opacity = 0.92;

    [ObservableProperty]
    private double _scale = 1.0;

    [ObservableProperty]
    private string _statusColorHex = "#00E5FF";

    [ObservableProperty]
    private string _nextEventTimeText = string.Empty;

    [ObservableProperty]
    private string _activeEventName = "GRAVITY ABYSS";

    [ObservableProperty]
    private string _driftSyncText = string.Empty;

    [ObservableProperty]
    private bool _areAlertsPaused = false;

    public Action? RequestOpenSettings { get; set; }
    public Action? RequestHideOverlay { get; set; }
    public Action? RequestExitApp { get; set; }
    public Action<bool>? RequestClickThroughChanged { get; set; }

    public OverlayViewModel(
        IEventScheduler scheduler,
        ISettingsService settingsService,
        ILocalizationService loc)
    {
        _scheduler = scheduler;
        _settingsService = settingsService;
        _loc = loc;

        ApplySettings(_settingsService.Settings);

        _scheduler.SnapshotUpdated += OnSnapshotUpdated;
        _settingsService.SettingsChanged += OnSettingsChanged;
        _loc.LanguageChanged += OnLanguageChanged;

        UpdateFromSnapshot(_scheduler.CurrentSnapshot);
        InitializeMultiTimer();
    }

    private void ApplySettings(UserSettings settings)
    {
        Opacity = settings.Opacity;
        Scale = settings.Scale;
        IsClickThrough = settings.IsClickThrough;
        IsLocked = settings.IsLocked;
        AlwaysOnTop = settings.AlwaysOnTop;
        CompactMode = settings.CompactMode;
        ShowBorder = settings.ShowBorder;
        ShowBackground = settings.ShowBackground;
        ShowProgressRing = settings.ShowProgressRing;
        ShowNextEventTime = settings.ShowNextEventTime;
        ShowMiniControls = settings.ShowMiniControls;
        AreAlertsPaused = settings.AreAlertsPaused;
    }

    private void OnSettingsChanged(object? sender, UserSettings settings)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            ApplySettings(settings);
            UpdatePhaseLabels(CurrentPhase);
        });
    }

    private void OnSnapshotUpdated(object? sender, EventSnapshot snapshot)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            UpdateFromSnapshot(snapshot);
        });
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            UpdatePhaseLabels(CurrentPhase);
        });
    }

    private void UpdateFromSnapshot(EventSnapshot snapshot)
    {
        FormattedRemaining = snapshot.FormattedRemaining;
        ProgressRatio = snapshot.ProgressRatio;
        CurrentPhase = snapshot.Phase;
        IsActive = snapshot.IsActive;

        if (!string.IsNullOrWhiteSpace(snapshot.EventName))
        {
            ActiveEventName = snapshot.EventName.ToUpperInvariant();
        }
        else if (snapshot.Event != null && !string.IsNullOrWhiteSpace(snapshot.Event.Name))
        {
            ActiveEventName = snapshot.Event.Name.ToUpperInvariant();
        }

        if (snapshot.EffectiveOffsetSeconds != 0)
        {
            DriftSyncText = snapshot.EffectiveOffsetSeconds > 0
                ? $"Sync: +{snapshot.EffectiveOffsetSeconds}s"
                : $"Sync: {snapshot.EffectiveOffsetSeconds}s";
        }
        else
        {
            DriftSyncText = string.Empty;
        }

        UpdatePhaseLabels(snapshot.Phase);

        NextEventTimeText = snapshot.IsActive
            ? $"{_loc.GetString("HUD_UntilEnd")}: {snapshot.NextEventEnd.ToLocalTime():HH:mm}"
            : $"{_loc.GetString("HUD_UntilStart")}: {snapshot.NextEventStart.ToLocalTime():HH:mm}";
    }

    private void UpdatePhaseLabels(EventPhase phase)
    {
        (PhaseBadgeText, PhaseDescriptionText, StatusColorHex) = phase switch
        {
            EventPhase.StartingSoon => (_loc.GetString("Status_StartingSoon"), _loc.GetString("HUD_UntilStart"), "#F59E0B"),
            EventPhase.Active       => (_loc.GetString("Status_Active"), _loc.GetString("HUD_UntilEnd"), "#A855F7"),
            EventPhase.EndingSoon   => (_loc.GetString("Status_EndingSoon"), _loc.GetString("HUD_UntilEnd"), "#EF4444"),
            _                       => (_loc.GetString("Status_Waiting"), _loc.GetString("HUD_UntilStart"), "#00E5FF")
        };
    }

    [RelayCommand]
    private async Task ToggleLockAsync()
    {
        IsLocked = !IsLocked;
        _settingsService.Settings.IsLocked = IsLocked;
        await _settingsService.SaveAsync();
    }

    [RelayCommand]
    private async Task ToggleClickThroughAsync()
    {
        IsClickThrough = !IsClickThrough;
        _settingsService.Settings.IsClickThrough = IsClickThrough;
        RequestClickThroughChanged?.Invoke(IsClickThrough);
        await _settingsService.SaveAsync();
    }

    [RelayCommand]
    private async Task ToggleCompactModeAsync()
    {
        CompactMode = !CompactMode;
        _settingsService.Settings.CompactMode = CompactMode;
        await _settingsService.SaveAsync();
    }

    [RelayCommand]
    private async Task TogglePauseAlertsAsync()
    {
        AreAlertsPaused = !AreAlertsPaused;
        _settingsService.Settings.AreAlertsPaused = AreAlertsPaused;
        await _settingsService.SaveAsync();
    }

    [RelayCommand]
    private void OpenSettings()
    {
        RequestOpenSettings?.Invoke();
    }

    [RelayCommand]
    private void HideOverlay()
    {
        RequestHideOverlay?.Invoke();
    }

    [RelayCommand]
    private void ExitApp()
    {
        RequestExitApp?.Invoke();
    }
}
