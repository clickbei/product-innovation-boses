using BosesApp.Core.Data.Models;
using BosesApp.Core.Interfaces;
using BosesApp.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BosesApp.Presentation.ViewModels;

/// <summary>
/// Onboarding ViewModel for user type identification and accessibility setup
/// Supports both visual and voice-only modes
/// </summary>
public partial class OnboardingViewModel : ObservableObject
{
    private readonly IVoiceService _voiceService;
    private readonly IUserRepository _userRepository;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private int _currentStep = 0;

    [ObservableProperty]
    private string _statusMessage = "Welcome to Boses!";

    // Localized UI Strings
    [ObservableProperty]
    private string _canSeeTitle = "Can you see this screen?";

    [ObservableProperty]
    private string _canSeeYes = "Yes, I can see";

    [ObservableProperty]
    private string _canSeeNo = "No, I cannot see";

    [ObservableProperty]
    private string _userTypeTitle = "What describes you best?";

    [ObservableProperty]
    private string _userTypeSenior = "Senior Citizen (60+)";

    [ObservableProperty]
    private string _userTypePwd = "Person with Disability (PWD)";

    [ObservableProperty]
    private string _userTypeBoth = "Both (Senior + PWD)";

    [ObservableProperty]
    private string _pwdCategoryTitle = "What type of disability?";

    [ObservableProperty]
    private string _pwdVisual = "Visual (Blind/Low Vision)";

    [ObservableProperty]
    private string _pwdHearing = "Hearing (Deaf/Hard of Hearing)";

    [ObservableProperty]
    private string _pwdMobility = "Mobility (Wheelchair/Crutches)";

    [ObservableProperty]
    private string _pwdCognitive = "Cognitive (Intellectual)";

    [ObservableProperty]
    private string _pwdPsychosocial = "Psychosocial (Mental Health)";

    [ObservableProperty]
    private string _pwdMultiple = "Multiple Disabilities";

    [ObservableProperty]
    private string _buttonNext = "Next";

    [ObservableProperty]
    private string _buttonBack = "Back";

    [ObservableProperty]
    private string _buttonFinish = "Finish";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _voiceOnlyMode;

    // User Information
    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private DateTime _dateOfBirth = DateTime.Now.AddYears(-65);

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    // User Type
    [ObservableProperty]
    private bool _isSenior;

    [ObservableProperty]
    private bool _isPwd;

    [ObservableProperty]
    private PwdCategory _pwdCategory = PwdCategory.None;

    [ObservableProperty]
    private string _pwdIdNumber = string.Empty;

    [ObservableProperty]
    private string _seniorCitizenIdNumber = string.Empty;

    // Accessibility
    [ObservableProperty]
    private bool _hasVisualImpairment;

    [ObservableProperty]
    private bool _hasHearingImpairment;

    [ObservableProperty]
    private bool _hasMotorImpairment;

    [ObservableProperty]
    private bool _hasCognitiveImpairment;

    // Guardian
    [ObservableProperty]
    private string _guardianName = string.Empty;

    [ObservableProperty]
    private string _guardianPhone = string.Empty;

    public OnboardingViewModel(
        IVoiceService voiceService,
        IUserRepository userRepository,
        ILocalizationService localizationService)
    {
        _voiceService = voiceService;
        _userRepository = userRepository;
        _localizationService = localizationService;

        // Initialize localized strings
        UpdateLocalizedStrings();
    }

    /// <summary>
    /// Update all localized strings based on current language
    /// </summary>
    public void UpdateLocalizedStrings()
    {
        CanSeeTitle = _localizationService.GetString("onboarding_can_see_title");
        CanSeeYes = _localizationService.GetString("onboarding_can_see_yes");
        CanSeeNo = _localizationService.GetString("onboarding_can_see_no");

        UserTypeTitle = _localizationService.GetString("onboarding_user_type_title");
        UserTypeSenior = _localizationService.GetString("onboarding_user_type_senior");
        UserTypePwd = _localizationService.GetString("onboarding_user_type_pwd");
        UserTypeBoth = _localizationService.GetString("onboarding_user_type_both");

        PwdCategoryTitle = _localizationService.GetString("onboarding_pwd_category_title");
        PwdVisual = _localizationService.GetString("onboarding_pwd_visual");
        PwdHearing = _localizationService.GetString("onboarding_pwd_hearing");
        PwdMobility = _localizationService.GetString("onboarding_pwd_mobility");
        PwdCognitive = _localizationService.GetString("onboarding_pwd_cognitive");
        PwdPsychosocial = _localizationService.GetString("onboarding_pwd_psychosocial");
        PwdMultiple = _localizationService.GetString("onboarding_pwd_multiple");

        ButtonNext = _localizationService.GetString("button_next");
        ButtonBack = _localizationService.GetString("button_back");
        ButtonFinish = _localizationService.GetString("button_finish");
    }

    public async Task InitializeAsync()
    {
        CurrentStep = 0;
        await StartOnboardingAsync();
    }

    private async Task StartOnboardingAsync()
    {
        IsBusy = true;

        // Step 1: Ask if user can see the screen
        StatusMessage = "Welcome to Boses! Can you see this screen?";
        await _voiceService.SpeakAsync(
            "Welcome to Boses, your voice-first accessibility assistant. " +
            "Can you see this screen? If yes, tap anywhere. " +
            "If no, say 'I cannot see' or wait 5 seconds.");

        // Wait for response or timeout
        await Task.Delay(5000);

        IsBusy = false;
    }

    [RelayCommand]
    private async Task UserCanSeeAsync()
    {
        VoiceOnlyMode = false;
        await _voiceService.SpeakAsync("Great! Let's set up your profile with visual guidance.");
        CurrentStep = 1;
        await AskUserTypeAsync();
    }

    [RelayCommand]
    private async Task UserCannotSeeAsync()
    {
        VoiceOnlyMode = true;
        HasVisualImpairment = true;
        await _voiceService.SpeakAsync(
            "Understood. I will guide you through the setup using only voice. " +
            "You won't need to press anything. Just speak your answers clearly.");

        CurrentStep = 1;
        await StartVoiceOnlyOnboardingAsync();
    }

    private async Task AskUserTypeAsync()
    {
        StatusMessage = "Are you a Senior Citizen, a Person with Disability, or both?";
        await _voiceService.SpeakAsync(
            "Are you a Senior Citizen aged 60 or above, " +
            "a Person with Disability, or both?");
    }

    [RelayCommand]
    private async Task SelectSeniorAsync()
    {
        IsSenior = true;
        IsPwd = false;
        await _voiceService.SpeakAsync("Senior Citizen selected.");
        await ProceedToPersonalInfoAsync();
    }

    [RelayCommand]
    private async Task SelectPwdAsync()
    {
        IsSenior = false;
        IsPwd = true;
        await _voiceService.SpeakAsync("Person with Disability selected.");
        CurrentStep = 2;
        await AskPwdCategoryAsync();
    }

    [RelayCommand]
    private async Task SelectBothAsync()
    {
        IsSenior = true;
        IsPwd = true;
        await _voiceService.SpeakAsync("Senior Citizen and Person with Disability selected.");
        CurrentStep = 2;
        await AskPwdCategoryAsync();
    }

    private async Task AskPwdCategoryAsync()
    {
        StatusMessage = "What type of disability do you have?";
        await _voiceService.SpeakAsync(
            "What type of disability do you have? " +
            "Visual, Hearing, Mobility, Cognitive, Psychosocial, or Multiple?");
    }

    [RelayCommand]
    private async Task SelectPwdCategoryAsync(string category)
    {
        PwdCategory = category switch
        {
            "Visual" => PwdCategory.Visual,
            "Hearing" => PwdCategory.Hearing,
            "Mobility" => PwdCategory.Mobility,
            "Cognitive" => PwdCategory.Cognitive,
            "Psychosocial" => PwdCategory.Psychosocial,
            "Multiple" => PwdCategory.Multiple,
            _ => PwdCategory.None
        };

        if (PwdCategory == PwdCategory.Visual)
        {
            HasVisualImpairment = true;
        }
        else if (PwdCategory == PwdCategory.Hearing)
        {
            HasHearingImpairment = true;
        }
        else if (PwdCategory == PwdCategory.Mobility)
        {
            HasMotorImpairment = true;
        }
        else if (PwdCategory == PwdCategory.Cognitive)
        {
            HasCognitiveImpairment = true;
        }

        await _voiceService.SpeakAsync($"{category} disability selected.");
        await ProceedToPersonalInfoAsync();
    }

    private async Task ProceedToPersonalInfoAsync()
    {
        CurrentStep = 3;
        StatusMessage = "Let's collect your personal information";
        await _voiceService.SpeakAsync("Now, let's collect your personal information.");
    }

    [RelayCommand]
    private async Task ContinueToVoiceRegistrationAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName))
        {
            await _voiceService.SpeakAsync("Please enter your full name.");
            return;
        }

        // Save user profile first (without voice print)
        await SaveUserProfileAsync();

        // Navigate to VoiceRegistrationPage
        await NavigateToVoiceRegistrationAsync();
    }

    private async Task SaveUserProfileAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Saving your information...";

            // Determine user type
            var userType = UserType.None;
            if (IsSenior && IsPwd) userType = UserType.Both;
            else if (IsSenior) userType = UserType.Senior;
            else if (IsPwd) userType = UserType.PWD;

            // Determine accessibility needs
            var accessibilityNeeds = AccessibilityNeeds.None;
            if (HasVisualImpairment) accessibilityNeeds |= AccessibilityNeeds.VisualImpairment;
            if (HasHearingImpairment) accessibilityNeeds |= AccessibilityNeeds.HearingImpairment;
            if (HasMotorImpairment) accessibilityNeeds |= AccessibilityNeeds.MotorImpairment;
            if (HasCognitiveImpairment) accessibilityNeeds |= AccessibilityNeeds.CognitiveImpairment;

            // Create user profile (without voice print - that will be added in VoiceRegistrationPage)
            var user = new UserProfile
            {
                FullName = FullName,
                PhoneNumber = PhoneNumber,
                DateOfBirth = DateOfBirth,
                UserType = userType,
                AccessibilityNeeds = accessibilityNeeds,
                PwdCategory = PwdCategory,
                PwdIdNumber = PwdIdNumber,
                SeniorCitizenIdNumber = SeniorCitizenIdNumber,
                VoicePrintData = null, // Will be set in VoiceRegistrationPage
                IsVoiceAuthEnabled = false, // Will be enabled after voice registration
                VoiceOnlyMode = VoiceOnlyMode,
                HasCompletedOnboarding = false, // Will be set to true after voice registration
                GuardianName = GuardianName,
                GuardianPhoneNumber = GuardianPhone
            };

            await _userRepository.CreateUserAsync(user);

            IsBusy = false;
        }
        catch (Exception ex)
        {
            IsBusy = false;
            await _voiceService.SpeakAsync($"Failed to save profile: {ex.Message}");
        }
    }

    private async Task NavigateToVoiceRegistrationAsync()
    {
        try
        {
            await _voiceService.SpeakAsync("Now let's register your voice for secure authentication.");

            // Get the user ID we just created
            var users = await _userRepository.GetAllUsersAsync();
            var currentUser = users.OrderByDescending(u => u.Id).FirstOrDefault();

            if (currentUser == null)
            {
                await _voiceService.SpeakAsync("Error: Could not find user profile.");
                return;
            }

            // Navigate to VoiceRegistrationPage
            var voiceRegistrationPage = Application.Current?.Handler?.MauiContext?.Services
                .GetService<Presentation.Views.VoiceRegistrationPage>();

            if (voiceRegistrationPage != null && Application.Current?.Windows.FirstOrDefault()?.Page is NavigationPage navPage)
            {
                // Pass the new user ID to the page before navigation
                voiceRegistrationPage.SetUserId(currentUser.Id);

                await navPage.PushAsync(voiceRegistrationPage);
            }
        }
        catch (Exception ex)
        {
            await _voiceService.SpeakAsync($"Navigation error: {ex.Message}");
        }
    }

    // Voice-Only Onboarding Flow
    private async Task StartVoiceOnlyOnboardingAsync()
    {
        try
        {
            // Step 1 — collect name via STT
            await _voiceService.SpeakAsync("What is your full name? Please speak clearly after the tone.");
            await _voiceService.StartListeningAsync();
            await Task.Delay(4000);
            var nameResult = await _voiceService.StopListeningAsync();
            FullName = !string.IsNullOrWhiteSpace(nameResult) ? nameResult : "Voice User";

            await _voiceService.SpeakAsync($"Thank you. I heard: {FullName}. Continuing with your profile setup.");

            // Step 2 — set default senior + PWD for voice-only users
            // (Full STT confirmation loop requires real-time wake-word detection,
            // which is handled in production via CommunityToolkit STT on Android/iOS.)
            IsSenior = true;
            IsPwd = false;
            PwdCategory = PwdCategory.None;

            await _voiceService.SpeakAsync(
                "For your security, a guardian contact helps protect your account from scams. " +
                "You can update guardian details in Settings after setup.");

            // Proceed to save and navigate
            await ContinueToVoiceRegistrationAsync();
        }
        catch (Exception ex)
        {
            await _voiceService.SpeakAsync($"Error during voice-only onboarding: {ex.Message}");
        }
    }
}
