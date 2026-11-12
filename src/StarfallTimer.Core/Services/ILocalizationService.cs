// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using StarfallTimer.Core.Models;

namespace StarfallTimer.Core.Services;

/// <summary>
/// Service managing dynamic multilingual UI string resolution.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Currently active culture code (e.g. "en-US", "ru-RU", "zh-CN").
    /// </summary>
    string CurrentLanguage { get; }

    /// <summary>
    /// Available supported languages.
    /// </summary>
    IReadOnlyList<LanguageInfo> SupportedLanguages { get; }

    /// <summary>
    /// Raised when the active language changes.
    /// </summary>
    event EventHandler? LanguageChanged;

    /// <summary>
    /// Switches the active language.
    /// </summary>
    void SetLanguage(string cultureCode);

    /// <summary>
    /// Gets localized string by key with fallback to English.
    /// </summary>
    string GetString(string key);

    /// <summary>
    /// Formats a localized string with arguments.
    /// </summary>
    string Format(string key, params object[] args);

    /// <summary>
    /// Indexer for quick lookup.
    /// </summary>
    string this[string key] { get; }
}
