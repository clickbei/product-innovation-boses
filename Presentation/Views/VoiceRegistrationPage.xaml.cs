using BosesApp.Presentation.ViewModels;

namespace BosesApp.Presentation.Views
{
    /// <summary>
    /// Voice biometric registration page.
    /// Call <see cref="SetUserId"/> before pushing this page onto the navigation stack
    /// so the correct user record is updated.
    /// </summary>
    public partial class VoiceRegistrationPage : ContentPage
    {
        private readonly VoiceRegistrationViewModel _viewModel;
        private int _userId = 1; // default — overridden via SetUserId
        private bool _initialized;

        public VoiceRegistrationPage(VoiceRegistrationViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        /// <summary>
        /// Sets the user ID before the page appears.
        /// Called by <see cref="OnboardingViewModel"/> and <see cref="MainViewModel"/>
        /// prior to navigation.
        /// </summary>
        public void SetUserId(int userId) => _userId = userId;

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Guard against double-init when navigating back/forward
            if (_initialized) return;
            _initialized = true;

            await _viewModel.InitializeAsync(_userId);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _initialized = false; // reset so re-entry re-initialises for a new user
        }
    }
}
