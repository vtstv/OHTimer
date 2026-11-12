// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using StarfallTimer.Core.Models;
using StarfallTimer.Core.Services;
using Xunit;

namespace StarfallTimer.Tests;

public class UpdateServiceTests
{
    private sealed class MockUpdateService(UpdateInfo result) : IUpdateService
    {
        public Task<UpdateInfo> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(result);
        }

        public Task<bool> DownloadAndApplyUpdateAsync(string downloadUrl, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            progress?.Report(1.0);
            return Task.FromResult(true);
        }
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WhenNewerReleaseExists_ReturnsHasUpdateTrue()
    {
        var expectedInfo = new UpdateInfo(
            HasUpdate: true,
            CurrentVersion: "1.1.0",
            LatestVersion: "1.2.0",
            ReleaseNotes: "Added multi-timer and auto-update support.",
            DownloadUrl: "https://github.com/vtstv/OHStarfarllTimer/releases/download/v1.2.0/OnceHumanStarfallTimer.exe");

        var service = new MockUpdateService(expectedInfo);
        var info = await service.CheckForUpdatesAsync();

        Assert.True(info.HasUpdate);
        Assert.Equal("1.2.0", info.LatestVersion);
        Assert.Equal("1.1.0", info.CurrentVersion);
        Assert.Equal("Added multi-timer and auto-update support.", info.ReleaseNotes);
        Assert.NotNull(info.DownloadUrl);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_WhenUpToDate_ReturnsHasUpdateFalse()
    {
        var expectedInfo = new UpdateInfo(
            HasUpdate: false,
            CurrentVersion: "1.2.0",
            LatestVersion: "1.2.0",
            ReleaseNotes: null,
            DownloadUrl: null);

        var service = new MockUpdateService(expectedInfo);
        var info = await service.CheckForUpdatesAsync();

        Assert.False(info.HasUpdate);
        Assert.Equal("1.2.0", info.LatestVersion);
    }

    [Fact]
    public async Task DownloadAndApplyUpdateAsync_ReportsProgressAndSucceeds()
    {
        var expectedInfo = new UpdateInfo(
            HasUpdate: true,
            CurrentVersion: "1.1.0",
            LatestVersion: "1.2.0",
            ReleaseNotes: "Notes",
            DownloadUrl: "https://example.com/app.exe");

        var service = new MockUpdateService(expectedInfo);
        double reportedProgress = 0;
        var progress = new Progress<double>(p => reportedProgress = p);

        bool success = await service.DownloadAndApplyUpdateAsync(expectedInfo.DownloadUrl!, progress);

        Assert.True(success);
    }
}
