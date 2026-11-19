// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace OnceHumanEventTimer.UI.ViewModels;

public partial class SettingsViewModel
{
    public bool SelectedEventAlertOnStart
    {
        get => SelectedEvent?.AlertOnEventStart ?? true;
        set
        {
            if (SelectedEvent != null && SelectedEvent.AlertOnEventStart != value)
            {
                SelectedEvent.AlertOnEventStart = value;
                OnPropertyChanged();
            }
        }
    }

    public bool SelectedEventEnableExtraAlert
    {
        get => SelectedEvent?.EnableExtraAlert ?? false;
        set
        {
            if (SelectedEvent != null && SelectedEvent.EnableExtraAlert != value)
            {
                SelectedEvent.EnableExtraAlert = value;
                OnPropertyChanged();
            }
        }
    }

    public int SelectedEventExtraAlertMinutes
    {
        get => SelectedEvent?.ExtraAlertMinutes ?? 15;
        set
        {
            if (SelectedEvent != null && SelectedEvent.ExtraAlertMinutes != value)
            {
                SelectedEvent.ExtraAlertMinutes = Math.Clamp(value, 1, Math.Max(1, SelectedEvent.DurationMinutes));
                OnPropertyChanged();
            }
        }
    }

    public bool SelectedEventCountdownToBoss
    {
        get => SelectedEvent?.CountdownToBoss ?? false;
        set
        {
            if (SelectedEvent != null && SelectedEvent.CountdownToBoss != value)
            {
                SelectedEvent.CountdownToBoss = value;
                OnPropertyChanged();
            }
        }
    }

    private void NotifyEventAlertProperties()
    {
        OnPropertyChanged(nameof(SelectedEventAlertOnStart));
        OnPropertyChanged(nameof(SelectedEventEnableExtraAlert));
        OnPropertyChanged(nameof(SelectedEventExtraAlertMinutes));
        OnPropertyChanged(nameof(SelectedEventCountdownToBoss));
    }
}
