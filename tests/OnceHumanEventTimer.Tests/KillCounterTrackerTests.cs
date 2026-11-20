// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Scheduling;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class KillCounterTrackerTests
{
    [Fact]
    public void Increment_IncreasesBothSessionAndAllTime_FiresEvents()
    {
        var tracker = new KillCounterTracker(new KillCounterSettings { IsEnabled = true, BossName = "Hound" });
        bool incrementFired = false;
        bool snapshotFired = false;
        tracker.CountIncremented += (s, e) => incrementFired = true;
        tracker.SnapshotUpdated += (s, snap) => snapshotFired = true;

        tracker.Increment();

        Assert.True(incrementFired);
        Assert.True(snapshotFired);
        Assert.Equal(1, tracker.CurrentSnapshot.CurrentCount);
        Assert.Equal(1, tracker.CurrentSnapshot.TotalAllTimeCount);
        Assert.Equal("1", tracker.CurrentSnapshot.FormattedCount);
    }

    [Fact]
    public void Decrement_DecreasesCounts_WhenGreaterThanZero()
    {
        var tracker = new KillCounterTracker();
        tracker.Increment();
        tracker.Increment();

        Assert.Equal(2, tracker.CurrentSnapshot.CurrentCount);
        Assert.Equal(2, tracker.CurrentSnapshot.TotalAllTimeCount);

        bool decrementFired = false;
        tracker.CountDecremented += (s, e) => decrementFired = true;

        tracker.Decrement();

        Assert.True(decrementFired);
        Assert.Equal(1, tracker.CurrentSnapshot.CurrentCount);
        Assert.Equal(1, tracker.CurrentSnapshot.TotalAllTimeCount);
    }

    [Fact]
    public void Decrement_DoesNotGoBelowZero()
    {
        var tracker = new KillCounterTracker();
        tracker.Decrement();

        Assert.Equal(0, tracker.CurrentSnapshot.CurrentCount);
        Assert.Equal(0, tracker.CurrentSnapshot.TotalAllTimeCount);
    }

    [Fact]
    public void ResetSession_ResetsCurrentCountToZero_PreservesAllTime()
    {
        var tracker = new KillCounterTracker();
        for (int i = 0; i < 5; i++) tracker.Increment();

        Assert.Equal(5, tracker.CurrentSnapshot.CurrentCount);
        Assert.Equal(5, tracker.CurrentSnapshot.TotalAllTimeCount);

        bool resetFired = false;
        tracker.SessionReset += (s, e) => resetFired = true;

        tracker.ResetSession();

        Assert.True(resetFired);
        Assert.Equal(0, tracker.CurrentSnapshot.CurrentCount);
        Assert.Equal(5, tracker.CurrentSnapshot.TotalAllTimeCount);
    }

    [Fact]
    public void ResetAllTime_ResetsTotalToZero_PreservesSession()
    {
        var tracker = new KillCounterTracker();
        for (int i = 0; i < 4; i++) tracker.Increment();

        tracker.ResetAllTime();

        Assert.Equal(4, tracker.CurrentSnapshot.CurrentCount);
        Assert.Equal(0, tracker.CurrentSnapshot.TotalAllTimeCount);
    }

    [Fact]
    public void GoalReached_FiresWhenTransitioningToGoalCount()
    {
        var settings = new KillCounterSettings
        {
            IsEnabled = true,
            EnableGoal = true,
            GoalCount = 3,
            FlashOnGoalReached = true
        };
        var tracker = new KillCounterTracker(settings);

        int goalEventsCount = 0;
        tracker.GoalReached += (s, e) => goalEventsCount++;

        tracker.Increment(); // 1
        Assert.Equal(0, goalEventsCount);
        Assert.False(tracker.CurrentSnapshot.IsGoalReached);

        tracker.Increment(); // 2
        Assert.Equal(0, goalEventsCount);
        Assert.False(tracker.CurrentSnapshot.IsGoalReached);

        tracker.Increment(); // 3 (Goal reached!)
        Assert.Equal(1, goalEventsCount);
        Assert.True(tracker.CurrentSnapshot.IsGoalReached);
        Assert.True(tracker.CurrentSnapshot.FlashOnGoalReached);
        Assert.Equal("3 / 3", tracker.CurrentSnapshot.FormattedCount);
        Assert.Equal(1.0, tracker.CurrentSnapshot.ProgressRatio);

        // Subsequent increment should NOT refire goal event
        tracker.Increment(); // 4
        Assert.Equal(1, goalEventsCount);
        Assert.True(tracker.CurrentSnapshot.IsGoalReached);
        Assert.Equal("4 / 3", tracker.CurrentSnapshot.FormattedCount);

        // Decrement below goal and increment back should re-fire
        tracker.Decrement(); // 3
        tracker.Decrement(); // 2
        Assert.False(tracker.CurrentSnapshot.IsGoalReached);

        tracker.Increment(); // 3
        Assert.Equal(2, goalEventsCount);
        Assert.True(tracker.CurrentSnapshot.IsGoalReached);
    }

    [Fact]
    public void SetCurrentCount_UpdatesCountAndSnapshot()
    {
        var tracker = new KillCounterTracker();
        tracker.SetCurrentCount(12);

        Assert.Equal(12, tracker.CurrentSnapshot.CurrentCount);

        tracker.SetCurrentCount(-10);
        Assert.Equal(0, tracker.CurrentSnapshot.CurrentCount);
    }

    [Fact]
    public void SetGoal_UpdatesGoalConfiguration()
    {
        var tracker = new KillCounterTracker();
        tracker.SetGoal(true, 25);

        Assert.True(tracker.CurrentSnapshot.EnableGoal);
        Assert.Equal(25, tracker.CurrentSnapshot.GoalCount);
        Assert.Equal("0 / 25", tracker.CurrentSnapshot.FormattedCount);

        tracker.SetGoal(true, 0); // clamps to 1
        Assert.Equal(1, tracker.CurrentSnapshot.GoalCount);
    }

    [Fact]
    public void SetBossName_UpdatesTargetNameInSnapshot()
    {
        var tracker = new KillCounterTracker();
        tracker.SetBossName("Forsaken Giant");

        Assert.Equal("Forsaken Giant", tracker.CurrentSnapshot.BossName);
    }

    [Fact]
    public void Configure_AppliesSettingsInstantly()
    {
        var tracker = new KillCounterTracker();
        var newSettings = new KillCounterSettings
        {
            IsEnabled = true,
            BossName = "Rabizex",
            CurrentCount = 8,
            TotalAllTimeCount = 42,
            ColorHex = "#10B981"
        };

        tracker.Configure(newSettings);

        Assert.True(tracker.IsEnabled);
        Assert.Equal("Rabizex", tracker.CurrentSnapshot.BossName);
        Assert.Equal(8, tracker.CurrentSnapshot.CurrentCount);
        Assert.Equal(42, tracker.CurrentSnapshot.TotalAllTimeCount);
        Assert.Equal("#10B981", tracker.CurrentSnapshot.ColorHex);
    }
}
