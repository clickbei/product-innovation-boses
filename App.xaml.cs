using BosesApp.Core.Interfaces;
using BosesApp.Presentation.Views;

namespace BosesApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var services = Handler?.MauiContext?.Services;
            if (services == null)
            {
                throw new InvalidOperationException("Services not available");
            }

            // Check if user has completed onboarding
            var userRepository = services.GetService<IUserRepository>();
            var hasCompletedOnboarding = CheckOnboardingStatus(userRepository);

            ContentPage startPage;
            if (hasCompletedOnboarding)
            {
                // User has completed onboarding - go to main page
                var mainPage = services.GetService<MainPage>();
                if (mainPage == null)
                {
                    throw new InvalidOperationException("MainPage service not found");
                }
                startPage = mainPage;
            }
            else
            {
                // New user - start with language selection
                var languageSelectionPage = services.GetService<LanguageSelectionPage>();
                if (languageSelectionPage == null)
                {
                    throw new InvalidOperationException("LanguageSelectionPage service not found");
                }
                startPage = languageSelectionPage;
            }

            return new Window(new NavigationPage(startPage))
            {
                Title = "Boses - Voice Assistant"
            };
        }

        private bool CheckOnboardingStatus(IUserRepository? userRepository)
        {
            if (userRepository == null) return false;

            try
            {
                // Check if any user exists with completed onboarding
                var users = userRepository.GetAllUsersAsync().GetAwaiter().GetResult();
                return users.Any(u => u.HasCompletedOnboarding || u.IsVoiceAuthEnabled);
            }
            catch
            {
                return false;
            }
        }
    }
}
