// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.Core.Scheduling;

/// <summary>
/// Legacy compatibility wrapper for EventScheduler.
/// </summary>
public class StarfallEventScheduler : EventScheduler
{
    public StarfallEventScheduler(ILocalizationService loc) : base(loc)
    {
    }
}
