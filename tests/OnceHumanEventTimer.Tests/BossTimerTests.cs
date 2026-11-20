// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Scheduling;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class BossTimerTests
{
    [Fact]
    public void BossTimerTracker_InitialState_IsInactive()
    {
        using var tracker = new BossTimerTracker();
        Assert.Equal(BossTimerState.Inactive, tracker.State);
        Assert.False(tracker.IsActive);
        Assert.Equal("00:00", tracker.CurrentSnapshot.FormattedRemaining);
    }

    [Fact]
    public void BossTimerTracker_StartAndTick_TransitionsToWarningAndSpawned()
    {
        var settings = new BossTimerSettings
        {
            DurationMinutes = 1,
            DurationSeconds = 0, // 60 seconds total
            WarningSecondsAhead = 20,
            AutoHideSeconds = 15,
            RepeatAlertCount = 2,
            RepeatAlertIntervalSeconds = 5
        };

        using var tracker = new BossTimerTracker(settings);
        var baseTime = new DateTimeOffset(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

        bool warningFired = false;
        bool spawnFired = false;
        int repeatCount = 0;

        tracker.WarningTriggered += (s, e) => warningFired = true;
        tracker.SpawnTriggered += (s, e) => spawnFired = true;
        tracker.RepeatAlertTriggered += (s, e) => repeatCount++;

        tracker.Start(baseTime);
        tracker.Tick(baseTime);

        tracker.Tick(baseTime.AddSeconds(10));
        Assert.Equal(BossTimerState.Running, tracker.State);
        Assert.False(warningFired);

        tracker.Tick(baseTime.AddSeconds(45));
        Assert.Equal(BossTimerState.Warning, tracker.State);
        Assert.True(warningFired);

        tracker.Tick(baseTime.AddSeconds(60));
        Assert.Equal(BossTimerState.Spawned, tracker.State);
        Assert.True(spawnFired);
        Assert.Equal("00:00", tracker.CurrentSnapshot.FormattedRemaining);
        Assert.Equal(1.0, tracker.CurrentSnapshot.ProgressRatio);

        tracker.Tick(baseTime.AddSeconds(65));
        Assert.Equal(1, repeatCount);

        tracker.Tick(baseTime.AddSeconds(70));
        Assert.Equal(2, repeatCount);

        tracker.Tick(baseTime.AddSeconds(75));
        Assert.Equal(BossTimerState.Inactive, tracker.State);
        Assert.False(tracker.IsActive);
    }

    [Fact]
    public void BossTimerTracker_RepressBehavior_RestartWorksProperly()
    {
        var settings = new BossTimerSettings
        {
            DurationMinutes = 5,
            RepressBehavior = BossTimerRepressBehavior.Restart
        };

        using var tracker = new BossTimerTracker(settings);
        bool hotkeyFired = false;
        tracker.HotkeyTriggered += (s, e) => hotkeyFired = true;

        tracker.HandleHotkeyPress();
        Assert.Equal(BossTimerState.Running, tracker.State);
        Assert.True(hotkeyFired);

        hotkeyFired = false;
        tracker.HandleHotkeyPress();
        Assert.Equal(BossTimerState.Running, tracker.State);
        Assert.True(hotkeyFired);
    }

    [Fact]
    public void BossTimerTracker_RepressBehavior_ToggleStopsActiveTimer()
    {
        var settings = new BossTimerSettings
        {
            DurationMinutes = 5,
            RepressBehavior = BossTimerRepressBehavior.Toggle
        };

        using var tracker = new BossTimerTracker(settings);
        tracker.Start();
        Assert.True(tracker.IsActive);

        tracker.HandleHotkeyPress();
        Assert.Equal(BossTimerState.Inactive, tracker.State);
        Assert.False(tracker.IsActive);
    }

    [Fact]
    public void BossTimerTracker_RepressBehavior_StopStopsActiveTimer()
    {
        var settings = new BossTimerSettings
        {
            DurationMinutes = 5,
            RepressBehavior = BossTimerRepressBehavior.Stop
        };

        using var tracker = new BossTimerTracker(settings);
        tracker.Start();
        Assert.True(tracker.IsActive);

        tracker.HandleHotkeyPress();
        Assert.Equal(BossTimerState.Inactive, tracker.State);
        Assert.False(tracker.IsActive);
    }

    [Fact]
    public void BossTimerTracker_Toggle_StartsWhenInactive_StopsWhenActive()
    {
        using var tracker = new BossTimerTracker();
        Assert.False(tracker.IsActive);

        tracker.Toggle();
        Assert.True(tracker.IsActive);

        tracker.Toggle();
        Assert.False(tracker.IsActive);
    }

    [Fact]
    public void BossTimerTracker_ShortDuration_StartsInWarningState_AndFiresWarningEvent()
    {
        var settings = new BossTimerSettings
        {
            DurationMinutes = 0,
            DurationSeconds = 20, // 20s total, warning threshold is 30s
            WarningSecondsAhead = 30
        };

        using var tracker = new BossTimerTracker(settings);
        bool warningFired = false;
        tracker.WarningTriggered += (s, e) => warningFired = true;

        tracker.Start();
        Assert.Equal(BossTimerState.Warning, tracker.State);
        Assert.True(warningFired);
    }

    [Fact]
    public void BossTimerTracker_ConfigureWhileRunning_UpdatesDurationAndFiresSnapshot()
    {
        using var tracker = new BossTimerTracker(new BossTimerSettings { DurationMinutes = 5 });
        bool snapshotFired = false;
        tracker.SnapshotUpdated += (s, e) => snapshotFired = true;

        tracker.Start();
        Assert.Equal(TimeSpan.FromMinutes(5), tracker.CurrentSnapshot.TotalDuration);

        snapshotFired = false;
        tracker.Configure(new BossTimerSettings { DurationMinutes = 10 });
        Assert.True(snapshotFired);
        Assert.Equal(TimeSpan.FromMinutes(10), tracker.CurrentSnapshot.TotalDuration);
    }

    [Fact]
    public void BossTimerTracker_Disabled_HandleHotkeyDoesNothing()
    {
        var settings = new BossTimerSettings { IsEnabled = false };
        using var tracker = new BossTimerTracker(settings);
        tracker.HandleHotkeyPress();
        Assert.False(tracker.IsActive);
    }

    [Fact]
    public void BossTimerTracker_PlayHotkeySoundFalse_DoesNotFireHotkeyTriggered()
    {
        var settings = new BossTimerSettings { PlayHotkeySound = false };
        using var tracker = new BossTimerTracker(settings);
        bool hotkeyFired = false;
        tracker.HotkeyTriggered += (s, e) => hotkeyFired = true;
        tracker.HandleHotkeyPress();
        Assert.False(hotkeyFired);
        Assert.True(tracker.IsActive);
    }

    [Fact]
    public void BossTimerTracker_WarningAlertDisabled_DoesNotTransitionToWarning()
    {
        var settings = new BossTimerSettings
        {
            DurationMinutes = 1,
            DurationSeconds = 0,
            EnableWarningAlert = false,
            WarningSecondsAhead = 30
        };
        using var tracker = new BossTimerTracker(settings);
        var baseTime = new DateTimeOffset(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);
        tracker.Start(baseTime);
        tracker.Tick(baseTime.AddSeconds(45));
        Assert.Equal(BossTimerState.Running, tracker.State);
    }

    [Fact]
    public void BossTimerTracker_Start_WhenDisabled_DoesNotStart()
    {
        var settings = new BossTimerSettings { IsEnabled = false };
        using var tracker = new BossTimerTracker(settings);
        tracker.Start();
        Assert.Equal(BossTimerState.Inactive, tracker.State);
        Assert.False(tracker.IsActive);
    }

    [Fact]
    public void BossTimerTracker_Configure_WhenDisabled_ImmediatelyStopsRunningTimer()
    {
        var settings = new BossTimerSettings { IsEnabled = true, DurationMinutes = 5 };
        using var tracker = new BossTimerTracker(settings);
        tracker.Start();
        Assert.True(tracker.IsActive);
        Assert.Equal(BossTimerState.Running, tracker.State);

        tracker.Configure(new BossTimerSettings { IsEnabled = false });
        Assert.False(tracker.IsActive);
        Assert.Equal(BossTimerState.Inactive, tracker.State);
    }
}
