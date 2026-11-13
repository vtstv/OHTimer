// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Services;

/// <summary>
/// Service managing system startup registration for the application.
/// </summary>
public interface IAutostartService
{
    /// <summary>
    /// Checks whether the application is configured to launch on Windows system startup.
    /// </summary>
    bool IsAutostartEnabled();

    /// <summary>
    /// Configures or removes Windows startup registration for the application.
    /// </summary>
    void SetAutostart(bool enabled);
}
