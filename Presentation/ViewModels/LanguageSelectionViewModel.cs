using BosesApp.Core.Data.Models;
using BosesApp.Core.Interfaces;
using BosesApp.Presentation.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BosesApp.Presentation.ViewModels;

/// <summary>
/// ViewModel for language selection screen
/// First screen shown to new users
/// </summary>
public partial class LanguageSelectionViewModel : ObservableObject
{
    private readonly ILocalizationService _localizationService;
    private readonly IVoiceService _voiceService;

    [ObservableProperty]
    private AppLanguage _selectedLanguage = AppLanguage.English;

    [ObservableProperty]
    private bool _isEnglishSelected = true;

    [ObservableProperty]
    private bool _isTagalogSelected = false;

    public LanguageSelectionViewModel(
        ILocalizationService localizationService,
        IVoiceService voiceService)
    {
        _localizationService = localizationService;
        _voiceService = voiceService;
    }

    [RelayCommand]
    private async Task SelectEnglishAsync()
    {
        SelectedLanguage = AppLanguage.English;
        IsEnglishSelected = true;
        IsTagalogSelected = false;

        // Speak confirmation in English
        await _voiceService.SpeakAsync("English selected");
    }

    [RelayCommand]
    private async Task SelectTagalogAsync()
    {
        SelectedLanguage = AppLanguage.Tagalog;
        IsEnglishSelected = false;
        IsTagalogSelected = true;

        // Speak confirmation in Tagalog
        await _voiceService.SpeakAsync("Tagalog ang napili");
    }

    [RelayCommand]
    private async Task ContinueAsync()
    {
        try
        {
            // Set the selected language
            _localizationService.SetLanguage(SelectedLanguage);

            // Speak confirmation
            var message = SelectedLanguage == AppLanguage.Tagalog
                ? "Magpatuloy sa pagpaparehistro"
                : "Continuing to registration";
            await _voiceService.SpeakAsync(message);

            // Navigate to onboarding using NavigationPage
            if (Application.Current?.Windows.FirstOrDefault()?.Page is NavigationPage navPage)
            {
                var onboardingPage = Application.Current.Handler?.MauiContext?.Services
                    .GetService<OnboardingPage>();

                if (onboardingPage != null)
                {
                    // Update localized strings in the onboarding page
                    if (onboardingPage.BindingContext is OnboardingViewModel onboardingViewModel)
                    {
                        onboardingViewModel.UpdateLocalizedStrings();
                    }

                    await navPage.PushAsync(onboardingPage);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Speak welcome message in both languages
            await _voiceService.SpeakAsync("Maligayang pagdating sa Boses. Please choose your language. Pumili ng iyong wika.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Initialization error: {ex.Message}");
        }
    }
}
