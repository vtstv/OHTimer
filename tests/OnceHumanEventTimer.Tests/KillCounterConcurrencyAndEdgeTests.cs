// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Scheduling;
using Xunit;

namespace OnceHumanEventTimer.Tests;

public class KillCounterConcurrencyAndEdgeTests
{
    [Fact]
    public async Task Increment_ConcurrentCalls_AreThreadSafeAndAccurate()
    {
        var settings = new KillCounterSettings { IsEnabled = true, BossName = "Hound" };
        var tracker = new KillCounterTracker(settings);

        const int threadCount = 10;
        const int incrementsPerThread = 50;
        var tasks = new Task[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < incrementsPerThread; j++)
                {
                    tracker.Increment();
                }
            });
        }

        await Task.WhenAll(tasks);

        const int expected = threadCount * incrementsPerThread;
        Assert.Equal(expected, tracker.CurrentSnapshot.CurrentCount);
        Assert.Equal(expected, tracker.CurrentSnapshot.TotalAllTimeCount);
    }

    [Fact]
    public void LargeGoalCount_FormatsCorrectlyInSnapshot()
    {
        var settings = new KillCounterSettings
        {
            IsEnabled = true,
            EnableGoal = true,
            GoalCount = 9999,
            CurrentCount = 42
        };
        var tracker = new KillCounterTracker(settings);

        Assert.Equal("42 / 9999", tracker.CurrentSnapshot.FormattedCount);
        Assert.Equal(42.0 / 9999.0, tracker.CurrentSnapshot.ProgressRatio, 5);
        Assert.False(tracker.CurrentSnapshot.IsGoalReached);
    }

    [Fact]
    public void TrackerConstructor_WhenCurrentCountAlreadyMeetsGoal_DoesNotFireGoalReached()
    {
        var settings = new KillCounterSettings
        {
            IsEnabled = true,
            EnableGoal = true,
            GoalCount = 10,
            CurrentCount = 10
        };

        bool goalFired = false;
        var tracker = new KillCounterTracker(settings);
        tracker.GoalReached += (s, e) => goalFired = true;

        Assert.False(goalFired, "Existing completed goal should not re-trigger GoalReached event on load.");
        Assert.True(tracker.CurrentSnapshot.IsGoalReached);

        // Incrementing further should also NOT fire
        tracker.Increment();
        Assert.False(goalFired);
        Assert.Equal(11, tracker.CurrentSnapshot.CurrentCount);
    }

    [Fact]
    public void NegativeInputs_AreSafelyClampedInTracker()
    {
        var tracker = new KillCounterTracker();
        tracker.SetCurrentCount(-100);
        Assert.Equal(0, tracker.CurrentSnapshot.CurrentCount);

        tracker.SetGoal(true, -50);
        Assert.Equal(1, tracker.CurrentSnapshot.GoalCount);
    }
}
