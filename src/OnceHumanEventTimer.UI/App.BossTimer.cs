// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.UI;

public partial class App
{
    private void SetupBossTimerAlerts(
        IBossTimerTracker bossTimer,
        ISettingsService settingsService,
        IAudioService audio,
        ILocalizationService loc,
        INotificationService notifications)
    {
        bossTimer.HotkeyTriggered += (s, e) =>
        {
            if (settingsService.Settings.BossTimer.PlayHotkeySound && !settingsService.Settings.AreAlertsPaused)
            {
                audio.PlayPreset(SoundPreset.StardustPing, settingsService.Settings.BossTimer.Volume);
            }
        };

        bossTimer.WarningTriggered += (s, e) =>
        {
            if (settingsService.Settings.BossTimer.EnableWarningAlert && !settingsService.Settings.AreAlertsPaused)
            {
                audio.PlayPreset(settingsService.Settings.BossTimer.WarningSoundPreset, settingsService.Settings.BossTimer.Volume);
            }
        };

        bossTimer.SpawnTriggered += (s, e) =>
        {
            if (!settingsService.Settings.AreAlertsPaused)
            {
                if (settingsService.Settings.BossTimer.EnableSpawnAlert)
                {
                    audio.PlayPreset(settingsService.Settings.BossTimer.SpawnSoundPreset, settingsService.Settings.BossTimer.Volume);
                }
                if (settingsService.Settings.BossTimer.ShowToastOnSpawn)
                {
                    string title = loc.GetString("BossTimer_SpawnToastTitle");
                    string msg = $"{settingsService.Settings.BossTimer.BossName} {loc.GetString("BossTimer_SpawnToastMsg")}";
                    notifications.ShowNotification(title, msg);
                }
            }
        };

        bossTimer.RepeatAlertTriggered += (s, e) =>
        {
            if (!settingsService.Settings.AreAlertsPaused && settingsService.Settings.BossTimer.EnableRepeatAlert)
            {
                audio.PlayPreset(settingsService.Settings.BossTimer.SpawnSoundPreset, settingsService.Settings.BossTimer.Volume);
            }
        };
    }
}
