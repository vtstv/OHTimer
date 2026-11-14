// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using CommunityToolkit.Mvvm.Input;
using OnceHumanEventTimer.Core.Models;

namespace OnceHumanEventTimer.UI.ViewModels;

public partial class SettingsViewModel
{
    public Func<string, string, bool>? ConfirmAction { get; set; }

    private bool Confirm(string message, string title)
    {
        if (ConfirmAction != null) return ConfirmAction(message, title);
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }

    [RelayCommand]
    private void DeleteEvent()
    {
        if (SelectedEvent == null || Events.Count <= 1) return;

        string msg = _loc.Format("Settings_DeleteEvent_Confirm", SelectedEvent.Name);
        string title = _loc.GetString("Settings_DeleteEvent_Title");
        if (!Confirm(msg, title)) return;

        var toRemove = SelectedEvent;
        int index = Events.IndexOf(toRemove);
        Events.Remove(toRemove);
        if (ActiveEventId == toRemove.Id)
        {
            ActiveEventId = Events[0].Id;
        }
        SelectedEvent = Events[Math.Min(index, Events.Count - 1)];
    }

    [RelayCommand]
    private void RestoreDefaultPresets()
    {
        string msg = _loc.GetString("Settings_RestorePresets_Confirm");
        string title = _loc.GetString("Settings_RestorePresets_Title");
        if (!Confirm(msg, title)) return;

        var defaults = DefaultEventPresets.CreateDefaultEvents();
        var existingIds = new HashSet<string>(Events.Select(e => e.Id), StringComparer.OrdinalIgnoreCase);

        foreach (var def in defaults)
        {
            if (!existingIds.Contains(def.Id))
            {
                def.IsEnabled = false;
                Events.Add(def);
                existingIds.Add(def.Id);
            }
        }

        SelectedEvent = Events.FirstOrDefault(e => e.Id == ActiveEventId) ?? Events.FirstOrDefault();
    }
}
