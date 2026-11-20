// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.UI;

public partial class App
{
    private void SetupKillCounterAlerts(
        IKillCounterTracker killCounter,
        IBossTimerTracker bossTimer,
        ISettingsService settingsService,
        IAudioService audio,
        ILocalizationService loc,
        INotificationService notifications)
    {
        killCounter.CountIncremented += (s, e) =>
        {
            var ks = settingsService.Settings.KillCounter;
            if (!ks.IsEnabled) return;

            if (ks.PlaySound && !settingsService.Settings.AreAlertsPaused)
            {
                audio.PlayPreset(SoundPreset.StardustPing, settingsService.Settings.Volume);
            }
            if (ks.AutoStartBossTimer)
            {
                if (!settingsService.Settings.BossTimer.IsEnabled)
                {
                    settingsService.Settings.BossTimer.IsEnabled = true;
                    bossTimer.Configure(settingsService.Settings.BossTimer);
                }
                bossTimer.Start();
            }
        };

        killCounter.CountDecremented += (s, e) =>
        {
            var ks = settingsService.Settings.KillCounter;
            if (!ks.IsEnabled) return;

            if (ks.PlaySound && !settingsService.Settings.AreAlertsPaused)
            {
                audio.PlayPreset(SoundPreset.CosmicPulse, settingsService.Settings.Volume * 0.7);
            }
        };

        killCounter.GoalReached += (s, e) =>
        {
            var ks = settingsService.Settings.KillCounter;
            if (!ks.IsEnabled) return;

            if (ks.PlaySound && !settingsService.Settings.AreAlertsPaused)
            {
                audio.PlayPreset(SoundPreset.QuantumChord, settingsService.Settings.Volume);
            }
            if (settingsService.Settings.ShowToastAlert)
            {
                string title = loc.GetString("KillCounter_GoalReachedTitle");
                string msg = string.Format(loc.GetString("KillCounter_GoalReachedMsg"), ks.GoalCount, ks.BossName);
                notifications.ShowNotification(title, msg);
            }
        };
    }
}
