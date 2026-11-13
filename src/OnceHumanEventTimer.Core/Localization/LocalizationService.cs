// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using OnceHumanEventTimer.Core.Localization.Catalogs;
using OnceHumanEventTimer.Core.Models;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.Core.Localization;

/// <summary>
/// Thread-safe multilingual localization provider with built-in English, Russian, and German dictionaries.
/// </summary>
public sealed class LocalizationService : ILocalizationService
{
    private static readonly IReadOnlyList<LanguageInfo> _supportedLanguages = new List<LanguageInfo>
    {
        new("en-US", "English", "English"),
        new("ru-RU", "Russian", "Русский"),
        new("de-DE", "German", "Deutsch")
    }.AsReadOnly();

    private readonly Dictionary<string, Dictionary<string, string>> _catalogs = new(StringComparer.OrdinalIgnoreCase);
    private string _currentLanguage = "en-US";
    private readonly object _lock = new();

    public string CurrentLanguage => _currentLanguage;
    public IReadOnlyList<LanguageInfo> SupportedLanguages => _supportedLanguages;

    public event EventHandler? LanguageChanged;

    public LocalizationService()
    {
        InitializeDefaultCatalogs();
    }

    public void SetLanguage(string cultureCode)
    {
        lock (_lock)
        {
            if (_supportedLanguages.Any(l => string.Equals(l.Code, cultureCode, StringComparison.OrdinalIgnoreCase)))
            {
                _currentLanguage = cultureCode;
            }
            else
            {
                _currentLanguage = "en-US";
            }
        }

        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public string GetString(string key)
    {
        lock (_lock)
        {
            if (_catalogs.TryGetValue(_currentLanguage, out var currentMap) &&
                currentMap.TryGetValue(key, out var localized) &&
                !string.IsNullOrWhiteSpace(localized))
            {
                return localized;
            }

            // Fallback to English
            if (_catalogs.TryGetValue("en-US", out var fallbackMap) &&
                fallbackMap.TryGetValue(key, out var fallback))
            {
                return fallback;
            }

            return $"[{key}]";
        }
    }

    public string Format(string key, params object[] args)
    {
        string pattern = GetString(key);
        try
        {
            return string.Format(pattern, args);
        }
        catch (FormatException)
        {
            return pattern;
        }
    }

    public string this[string key] => GetString(key);

    /// <summary>
    /// Loads custom translations from a JSON file.
    /// </summary>
    public void LoadCustomCatalog(string cultureCode, string jsonContent)
    {
        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
            if (data == null) return;

            lock (_lock)
            {
                if (!_catalogs.TryGetValue(cultureCode, out var dict))
                {
                    dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    _catalogs[cultureCode] = dict;
                }

                foreach (var (k, v) in data)
                {
                    dict[k] = v;
                }
            }

            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            // Ignore malformed custom json
        }
    }

    private void InitializeDefaultCatalogs()
    {
        _catalogs["en-US"] = EnglishCatalog.GetEntries();
        _catalogs["de-DE"] = GermanCatalog.GetEntries();
        _catalogs["ru-RU"] = RussianCatalog.GetEntries();
    }
}
