using BosesApp.Presentation.ViewModels;

namespace BosesApp.Presentation.Views
{
    public partial class VoiceRegistrationPage : ContentPage
    {
        private readonly VoiceRegistrationViewModel _viewModel;

        public VoiceRegistrationPage(VoiceRegistrationViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Initialize with user ID 1 (default user)
            await _viewModel.InitializeAsync(1);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Clean up if needed
        }
    }
}
