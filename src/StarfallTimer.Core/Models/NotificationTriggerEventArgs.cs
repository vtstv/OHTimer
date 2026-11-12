// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StarfallTimer.Core.Models;

/// <summary>
/// Event arguments fired when a notification threshold is triggered.
/// </summary>
public class NotificationTriggerEventArgs : EventArgs
{
    public required string Title { get; init; }
    public required string Message { get; init; }
    public required EventPhase Phase { get; init; }
    public required TimeSpan TimeRemaining { get; init; }
    public required bool PlaySound { get; init; }
    public required bool ShowToast { get; init; }
}
