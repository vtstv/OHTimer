// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnceHumanEventTimer.Core.Models;

namespace OnceHumanEventTimer.UI.ViewModels;

public partial class SoundPresetOption : ObservableObject
{
    public SoundPreset Preset { get; }

    [ObservableProperty]
    private string _displayName;

    public SoundPresetOption(SoundPreset preset, string displayName)
    {
        Preset = preset;
        _displayName = displayName;
    }
}

public partial class SettingsViewModel
{
    [ObservableProperty]
    private SoundPreset _alertSoundPreset = SoundPreset.CosmicPulse;

    [ObservableProperty]
    private ObservableCollection<SoundPresetOption> _availableSoundPresets = new();

    public bool IsCustomAlertSound => AlertSoundPreset == SoundPreset.Custom;

    partial void OnAlertSoundPresetChanged(SoundPreset value)
    {
        OnPropertyChanged(nameof(IsCustomAlertSound));
    }

    private void InitializeAudio(UserSettings current)
    {
        AlertSoundPreset = current.AlertSoundPreset;
        PopulateSoundPresets();
        _loc.LanguageChanged += (s, e) => PopulateSoundPresets();
    }

    private void PopulateSoundPresets()
    {
        if (AvailableSoundPresets.Count == 0)
        {
            foreach (SoundPreset preset in Enum.GetValues<SoundPreset>())
            {
                string locKey = $"SoundPreset_{preset}";
                string name = _loc.GetString(locKey);
                AvailableSoundPresets.Add(new SoundPresetOption(preset, name));
            }
        }
        else
        {
            foreach (var option in AvailableSoundPresets)
            {
                string locKey = $"SoundPreset_{option.Preset}";
                option.DisplayName = _loc.GetString(locKey);
            }
        }
    }

    [RelayCommand]
    private void PreviewAlertSound()
    {
        _audio.PlayPreset(AlertSoundPreset, Volume, CustomSoundPath);
    }

    [RelayCommand]
    private void PreviewEventSound()
    {
        _audio.PlayPreset(SelectedEventSoundPreset, Volume, SelectedEventCustomSoundPath ?? CustomSoundPath);
    }
}
