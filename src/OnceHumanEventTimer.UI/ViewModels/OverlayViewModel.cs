// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Scheduling;
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

    [ObservableProperty]
    private bool _isTimerVisible = true;

    public Action? RequestOpenSettings { get; set; }
    public Action? RequestHideOverlay { get; set; }
    public Action? RequestExitApp { get; set; }
    public Action<bool>? RequestClickThroughChanged { get; set; }

    public OverlayViewModel(
        IEventScheduler scheduler,
        ISettingsService settingsService,
        ILocalizationService loc,
        IBossTimerTracker? bossTimer = null,
        IKillCounterTracker? killCounter = null)
    {
        _scheduler = scheduler;
        _settingsService = settingsService;
        _loc = loc;

        ApplySettings(_settingsService.Settings);
        UpdatePhaseLabels(CurrentPhase);

        _settingsService.SettingsChanged += OnSettingsChanged;
        _loc.LanguageChanged += OnLanguageChanged;

        InitializeMultiTimer();
        InitializeBossTimer(bossTimer ?? new BossTimerTracker(settingsService));
        InitializeKillCounter(killCounter ?? new KillCounterTracker(settingsService));
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
            UpdateActiveTimers(_scheduler.ActiveSnapshots);
            if (_bossTimer != null)
            {
                UpdateBossTimerFromSnapshot(_bossTimer.CurrentSnapshot);
            }
        });
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            UpdatePhaseLabels(CurrentPhase, _scheduler.CurrentSnapshot?.Event);
            UpdateActiveTimers(_scheduler.ActiveSnapshots);
            if (_bossTimer != null)
            {
                UpdateBossTimerFromSnapshot(_bossTimer.CurrentSnapshot);
            }
        });
    }

    private void UpdateFromSnapshot(EventSnapshot snapshot)
    {
        FormattedRemaining = snapshot.FormattedRemaining;
        ProgressRatio = snapshot.ProgressRatio;
        CurrentPhase = snapshot.Phase;
        IsActive = snapshot.IsActive;

        if (!string.IsNullOrWhiteSpace(snapshot.EventName))
            ActiveEventName = snapshot.EventName.ToUpperInvariant();
        else if (snapshot.Event != null && !string.IsNullOrWhiteSpace(snapshot.Event.Name))
            ActiveEventName = snapshot.Event.Name.ToUpperInvariant();

        DriftSyncText = snapshot.EffectiveOffsetSeconds != 0
            ? (snapshot.EffectiveOffsetSeconds > 0 ? $"Sync: +{snapshot.EffectiveOffsetSeconds}s" : $"Sync: {snapshot.EffectiveOffsetSeconds}s")
            : string.Empty;

        if (snapshot.IsBossCountdown)
        {
            if (snapshot.IsBossOpen)
            {
                (PhaseBadgeText, PhaseDescriptionText, StatusColorHex) =
                    (_loc.GetString("Status_BossOpen"), _loc.GetString("HUD_UntilPortalCloses"), "#EC4899");
                NextEventTimeText = $"{_loc.GetString("HUD_UntilPortalCloses")}: {snapshot.NextEventEnd.ToLocalTime():HH:mm}";
            }
            else
            {
                string waitingColor = !string.IsNullOrWhiteSpace(snapshot.Event?.ColorHex) ? snapshot.Event.ColorHex : "#00F0FF";
                string badge = snapshot.Phase == EventPhase.StartingSoon ? _loc.GetString("Status_StartingSoon") : _loc.GetString("Status_Waiting");
                (PhaseBadgeText, PhaseDescriptionText, StatusColorHex) = (badge, _loc.GetString("HUD_UntilBoss"), waitingColor);
                NextEventTimeText = $"{_loc.GetString("HUD_UntilBoss")}: {snapshot.NextEventStart.ToLocalTime():HH:mm}";
            }
        }
        else
        {
            UpdatePhaseLabels(snapshot.Phase, snapshot.Event);
            NextEventTimeText = snapshot.IsActive
                ? $"{_loc.GetString("HUD_UntilEnd")}: {snapshot.NextEventEnd.ToLocalTime():HH:mm}"
                : $"{_loc.GetString("HUD_UntilStart")}: {snapshot.NextEventStart.ToLocalTime():HH:mm}";
        }

        if (!IsMultiTimer)
        {
            if (snapshot.Event != null && (!snapshot.Event.IsEnabled || (snapshot.Event.ShowOnlyWhenUpcoming && !snapshot.IsActive)))
            {
                int threshold = snapshot.Event.UpcomingThresholdMinutes > 0 ? snapshot.Event.UpcomingThresholdMinutes : 10;
                IsTimerVisible = snapshot.Event.IsEnabled && snapshot.TimeRemaining <= TimeSpan.FromMinutes(threshold);
            }
            else
            {
                IsTimerVisible = snapshot.Event?.IsEnabled ?? true;
            }
            UpdateOverlayCardVisibility();
        }
    }

    private void UpdatePhaseLabels(EventPhase phase, EventDefinition? ev = null)
    {
        string eventColor = !string.IsNullOrWhiteSpace(ev?.ColorHex) ? ev.ColorHex : "#00F0FF";
        (PhaseBadgeText, PhaseDescriptionText, StatusColorHex) = phase switch
        {
            EventPhase.StartingSoon => (_loc.GetString("Status_StartingSoon"), _loc.GetString("HUD_UntilStart"), "#F59E0B"),
            EventPhase.Active       => (_loc.GetString("Status_Active"), _loc.GetString("HUD_UntilEnd"), eventColor),
            EventPhase.EndingSoon   => (_loc.GetString("Status_EndingSoon"), _loc.GetString("HUD_UntilEnd"), "#EF4444"),
            _                       => (_loc.GetString("Status_Waiting"), _loc.GetString("HUD_UntilStart"), eventColor)
        };
    }
}
