// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using StarfallTimer.Core.Services;

namespace StarfallTimer.Infrastructure.Notifications;

/// <summary>
/// Dispatches system notifications to the active tray or toast handler.
/// </summary>
public sealed class WindowsNotificationService : INotificationService
{
    /// <summary>
    /// Action invoked when a notification should be rendered (typically wired to the Tray Icon).
    /// </summary>
    public Action<string, string>? OnShowNotification { get; set; }

    public void ShowNotification(string title, string message)
    {
        OnShowNotification?.Invoke(title, message);
    }
}
