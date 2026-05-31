using BosesApp.Presentation.ViewModels;

namespace BosesApp.Presentation.Views
{
    public partial class OnboardingPage : ContentPage
    {
        private readonly OnboardingViewModel _viewModel;

        public OnboardingPage(OnboardingViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync();
        }
    }
}
