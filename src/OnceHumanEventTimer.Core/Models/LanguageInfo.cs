// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace OnceHumanEventTimer.Core.Models;

/// <summary>
/// Metadata for supported interface languages.
/// </summary>
/// <param name="Code">IETF language code (e.g., en-US, ru-RU, zh-CN).</param>
/// <param name="DisplayName">English name of the language.</param>
/// <param name="NativeName">Native name in its own language.</param>
public record LanguageInfo(string Code, string DisplayName, string NativeName);
