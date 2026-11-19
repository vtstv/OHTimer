// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnceHumanEventTimer.Core.Localization;
using OnceHumanEventTimer.Core.Services;

namespace OnceHumanEventTimer.UI.ViewModels;

/// <summary>
/// ViewModel controlling the interactive first-run onboarding tutorial wizard.
/// </summary>
public partial class OnboardingViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly ILocalizationService _loc;
    private readonly IEventScheduler? _scheduler;

    public const int MaxSteps = 5;

    [ObservableProperty]
    private int _currentStep = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentLanguageDisplayName))]
    [NotifyPropertyChangedFor(nameof(IsEnglish))]
    [NotifyPropertyChangedFor(nameof(IsGerman))]
    [NotifyPropertyChangedFor(nameof(IsRussian))]
    private string _currentLanguageCode = "en-US";

    [ObservableProperty]
    private bool _isLanguageMenuOpen;

    [ObservableProperty]
    private string _stepTitle = string.Empty;

    [ObservableProperty]
    private string _stepSubtitle = string.Empty;

    [ObservableProperty]
    private string _stepDescription = string.Empty;

    [ObservableProperty]
    private string _stepIcon = string.Empty;

    [ObservableProperty]
    private string _stepTip = string.Empty;

    [ObservableProperty]
    private string _stepIndicatorText = string.Empty;

    [ObservableProperty]
    private bool _canGoBack;

    [ObservableProperty]
    private bool _isLastStep;

    public bool IsEnglish => string.Equals(CurrentLanguageCode, "en-US", StringComparison.OrdinalIgnoreCase);
    public bool IsGerman => string.Equals(CurrentLanguageCode, "de-DE", StringComparison.OrdinalIgnoreCase);
    public bool IsRussian => string.Equals(CurrentLanguageCode, "ru-RU", StringComparison.OrdinalIgnoreCase);

    public string CurrentLanguageDisplayName =>
        IsGerman ? "Deutsch" :
        IsRussian ? "Русский" :
        "English";

    public Action? RequestClose { get; set; }

    public OnboardingViewModel(ISettingsService settingsService, ILocalizationService loc, IEventScheduler? scheduler = null)
    {
        _settingsService = settingsService;
        _loc = loc;
        _scheduler = scheduler;
        _currentLanguageCode = _loc.CurrentLanguage;

        InitializeEvents();
        UpdateStepContent();
    }

    [RelayCommand]
    public async Task SetLanguageAsync(string langCode)
    {
        if (string.IsNullOrWhiteSpace(langCode)) return;
        _loc.SetLanguage(langCode);
        _settingsService.Settings.Language = langCode;
        await _settingsService.SaveAsync();
        CurrentLanguageCode = _loc.CurrentLanguage;
        IsLanguageMenuOpen = false;
        UpdateStepContent();
        OnPropertyChanged(nameof(CurrentLanguageDisplayName));
        OnPropertyChanged(nameof(IsEnglish));
        OnPropertyChanged(nameof(IsGerman));
        OnPropertyChanged(nameof(IsRussian));
    }

    [RelayCommand]
    private void ToggleLanguageMenu()
    {
        IsLanguageMenuOpen = !IsLanguageMenuOpen;
    }

    private void UpdateStepContent()
    {
        CanGoBack = CurrentStep > 1;
        IsLastStep = CurrentStep == MaxSteps;
        StepIndicatorText = string.Format(_loc.GetString("Onboarding_StepIndicator"), CurrentStep, MaxSteps);

        switch (CurrentStep)
        {
            case 1:
                StepIcon = "⏱️";
                StepTitle = _loc.GetString("Onboarding_Step1_Title");
                StepSubtitle = _loc.GetString("Onboarding_Step1_Subtitle");
                StepDescription = _loc.GetString("Onboarding_Step1_Desc");
                StepTip = _loc.GetString("Onboarding_Step1_Tip");
                break;
            case 2:
                StepIcon = "📋";
                StepTitle = _loc.GetString("Onboarding_Step2_Title");
                StepSubtitle = _loc.GetString("Onboarding_Step2_Subtitle");
                StepDescription = _loc.GetString("Onboarding_Step2_Desc");
                StepTip = _loc.GetString("Onboarding_Step2_Tip");
                break;
            case 3:
                StepIcon = "🌌";
                StepTitle = _loc.GetString("Onboarding_Step3_Title");
                StepSubtitle = _loc.GetString("Onboarding_Step3_Subtitle");
                StepDescription = _loc.GetString("Onboarding_Step3_Desc");
                StepTip = _loc.GetString("Onboarding_Step3_Tip");
                break;
            case 4:
                StepIcon = "🎯";
                StepTitle = _loc.GetString("Onboarding_Step4_Title");
                StepSubtitle = _loc.GetString("Onboarding_Step4_Subtitle");
                StepDescription = _loc.GetString("Onboarding_Step4_Desc");
                StepTip = _loc.GetString("Onboarding_Step4_Tip");
                break;
            case 5:
                StepIcon = "⌨️";
                StepTitle = _loc.GetString("Onboarding_Step5_Title");
                StepSubtitle = _loc.GetString("Onboarding_Step5_Subtitle");
                StepDescription = _loc.GetString("Onboarding_Step5_Desc");
                StepTip = _loc.GetString("Onboarding_Step5_Tip");
                break;
        }
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        await SaveEventSelectionsAsync(ensureAtLeastOne: true);
        if (CurrentStep < MaxSteps)
        {
            CurrentStep++;
            UpdateStepContent();
        }
        else
        {
            await FinishAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousAsync()
    {
        await SaveEventSelectionsAsync(ensureAtLeastOne: true);
        if (CurrentStep > 1)
        {
            CurrentStep--;
            UpdateStepContent();
        }
    }

    [RelayCommand]
    private async Task SkipAsync()
    {
        await SaveEventSelectionsAsync(ensureAtLeastOne: true);
        _settingsService.Settings.HasCompletedOnboarding = true;
        await _settingsService.SaveAsync();
        RequestClose?.Invoke();
    }

    [RelayCommand]
    private async Task FinishAsync()
    {
        await SaveEventSelectionsAsync(ensureAtLeastOne: true);
        _settingsService.Settings.HasCompletedOnboarding = true;
        await _settingsService.SaveAsync();
        RequestClose?.Invoke();
    }
}
