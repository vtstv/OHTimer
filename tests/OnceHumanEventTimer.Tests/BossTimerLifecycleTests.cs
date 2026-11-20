// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Scheduling;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class BossTimerLifecycleTests
{
    [Fact]
    public void BossTimerTracker_HandleHotkeyPress_WhenDisabled_DoesNotStart()
    {
        var settings = new BossTimerSettings { IsEnabled = false };
        using var tracker = new BossTimerTracker(settings);
        bool hotkeyFired = false;
        tracker.HotkeyTriggered += (s, e) => hotkeyFired = true;

        tracker.HandleHotkeyPress();

        Assert.Equal(BossTimerState.Inactive, tracker.State);
        Assert.False(tracker.IsActive);
        Assert.False(hotkeyFired);
    }

    [Fact]
    public void BossTimerTracker_Toggle_WhenDisabled_DoesNotStart()
    {
        var settings = new BossTimerSettings { IsEnabled = false };
        using var tracker = new BossTimerTracker(settings);

        tracker.Toggle();

        Assert.Equal(BossTimerState.Inactive, tracker.State);
        Assert.False(tracker.IsActive);
    }

    [Fact]
    public void BossTimerTracker_ConfigureDisabled_CancelsRunningCountdown_AndIgnoresFutureHotkeys()
    {
        var settings = new BossTimerSettings { IsEnabled = true, DurationMinutes = 5 };
        using var tracker = new BossTimerTracker(settings);
        tracker.Start();
        Assert.True(tracker.IsActive);

        // Disable via configuration
        settings.IsEnabled = false;
        tracker.Configure(settings);

        Assert.False(tracker.IsActive);
        Assert.Equal(BossTimerState.Inactive, tracker.State);

        // Further hotkey attempts must be suppressed
        tracker.HandleHotkeyPress();
        Assert.False(tracker.IsActive);
        Assert.Equal(BossTimerState.Inactive, tracker.State);
    }

    [Fact]
    public void BossTimerTracker_Start_WithCustomConfig_AppliesAllSettingsToSnapshot()
    {
        var settings = new BossTimerSettings
        {
            IsEnabled = true,
            BossName = "Fallen Treant",
            DurationMinutes = 15,
            DurationSeconds = 30,
            ColorHex = "#A855F7"
        };
        using var tracker = new BossTimerTracker(settings);
        tracker.Start();

        var snap = tracker.CurrentSnapshot;
        Assert.True(snap.IsActive);
        Assert.Equal("Fallen Treant", snap.BossName);
        Assert.Equal("#A855F7", snap.ColorHex);
        Assert.Equal("15:30", snap.FormattedRemaining);
    }
}
