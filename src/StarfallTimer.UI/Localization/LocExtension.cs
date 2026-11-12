// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;
using StarfallTimer.Core.Services;

namespace StarfallTimer.UI.Localization;

/// <summary>
/// Dynamic binding helper allowing WPF XAML to re-evaluate localized strings on language switch.
/// </summary>
public sealed class LocalizationProvider : INotifyPropertyChanged
{
    private static LocalizationProvider? _instance;
    public static LocalizationProvider Instance => _instance ??= new LocalizationProvider();

    private ILocalizationService? _localizationService;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Initialize(ILocalizationService service)
    {
        if (_localizationService != null)
        {
            _localizationService.LanguageChanged -= OnLanguageChanged;
        }

        _localizationService = service;
        _localizationService.LanguageChanged += OnLanguageChanged;
        RaisePropertyChanged(string.Empty);
    }

    public string this[string key] => _localizationService?.GetString(key) ?? $"[{key}]";

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        RaisePropertyChanged(string.Empty);
    }

    private void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Markup extension providing real-time localized string binding in XAML:
/// Text="{loc:Loc Status_Waiting}"
/// </summary>
[MarkupExtensionReturnType(typeof(BindingExpression))]
public class LocExtension : MarkupExtension
{
    public string Key { get; set; }

    public LocExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding($"[{Key}]")
        {
            Source = LocalizationProvider.Instance,
            Mode = BindingMode.OneWay
        };

        return binding.ProvideValue(serviceProvider);
    }
}
