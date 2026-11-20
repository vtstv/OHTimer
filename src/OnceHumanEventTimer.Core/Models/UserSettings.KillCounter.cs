// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Models;

public partial class UserSettings
{
    private KillCounterSettings _killCounter = new();

    /// <summary>
    /// Configuration options for the dedicated Farm &amp; Kill Counter.
    /// </summary>
    public KillCounterSettings KillCounter
    {
        get => _killCounter ??= new();
        set => _killCounter = value ?? new();
    }
}
