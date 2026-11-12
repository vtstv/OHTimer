// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using StarfallTimer.Core.Models;

namespace StarfallTimer.Core.Services;

/// <summary>
/// Service contract for checking and seamlessly applying application updates.
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Checks the release repository for newer versions of the application.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Update metadata and status.</returns>
    Task<UpdateInfo> CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the updated application binary and stages an in-place self-replacement on application exit.
    /// </summary>
    /// <param name="downloadUrl">Download URL for the latest executable or release package.</param>
    /// <param name="progress">Progress reporter for tracking download percentage (0.0 to 1.0).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the update was downloaded and replacement scheduled successfully.</returns>
    Task<bool> DownloadAndApplyUpdateAsync(string downloadUrl, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
}
