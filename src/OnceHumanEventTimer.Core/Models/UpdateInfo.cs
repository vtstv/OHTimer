// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Models;

/// <summary>
/// Encapsulates application update metadata retrieved from a release endpoint.
/// </summary>
public record UpdateInfo(
    bool HasUpdate,
    string CurrentVersion,
    string LatestVersion,
    string? ReleaseNotes,
    string? DownloadUrl,
    string? PublishedAt = null,
    bool IsNotFoundOnGitHub = false);
