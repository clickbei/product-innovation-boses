using BosesApp.Presentation.Views;

namespace BosesApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute(nameof(LanguageSelectionPage), typeof(LanguageSelectionPage));
        Routing.RegisterRoute(nameof(OnboardingPage), typeof(OnboardingPage));
        Routing.RegisterRoute(nameof(VoiceRegistrationPage), typeof(VoiceRegistrationPage));
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
    }
}
