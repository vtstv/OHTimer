// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using StarfallTimer.Core.Models;

namespace StarfallTimer.UI.ViewModels;

/// <summary>
/// Represents a single event timer card or row in the multi-timer HUD overlay.
/// </summary>
public partial class TimerItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _formattedRemaining = "00:00";

    [ObservableProperty]
    private string _phaseBadgeText = string.Empty;

    [ObservableProperty]
    private string _phaseDescriptionText = string.Empty;

    [ObservableProperty]
    private double _progressRatio;

    [ObservableProperty]
    private EventPhase _phase = EventPhase.Inactive;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private string _statusColorHex = "#00F0FF";

    [ObservableProperty]
    private string _nextEventTimeText = string.Empty;

    [ObservableProperty]
    private string _driftSyncText = string.Empty;
}
