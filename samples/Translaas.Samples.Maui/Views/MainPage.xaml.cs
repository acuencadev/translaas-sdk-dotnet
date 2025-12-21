using Translaas.Samples.Maui.ViewModels;

namespace Translaas.Samples.Maui.Views;

/// <summary>
/// Main page demonstrating Translaas SDK features.
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    /// <summary>
    /// Initializes the main page with the view model.
    /// </summary>
    /// <param name="viewModel">The main view model.</param>
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    /// <summary>
    /// Called when the page appears - loads translations.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Load translations when the page appears
        await _viewModel.LoadTranslationsAsync();
    }
}
