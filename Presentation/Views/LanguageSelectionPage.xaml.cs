using BosesApp.Presentation.ViewModels;

namespace BosesApp.Presentation.Views;

public partial class LanguageSelectionPage : ContentPage
{
    private readonly LanguageSelectionViewModel _viewModel;

    public LanguageSelectionPage(LanguageSelectionViewModel viewModel)
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
