using BosesApp.Presentation.ViewModels;

namespace BosesApp.Presentation.Views;

/// <summary>
/// Main page — zero code-behind pattern.
/// All business logic lives in <see cref="MainViewModel"/>.
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
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
