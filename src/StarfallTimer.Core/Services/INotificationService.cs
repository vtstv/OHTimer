// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StarfallTimer.Core.Services;

/// <summary>
/// Service responsible for displaying system/toast notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Displays an OS notification banner with the given title and message.
    /// </summary>
    void ShowNotification(string title, string message);
}
