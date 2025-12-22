using Translaas.Samples.Maui.ViewModels;

namespace Translaas.Samples.Maui.Views;

/// <summary>
/// Settings page for testing and configuring Translaas SDK.
/// </summary>
public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    /// <summary>
    /// Initializes the settings page with the view model.
    /// </summary>
    /// <param name="viewModel">The settings view model.</param>
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    /// <summary>
    /// Called when the page appears - tests connection.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Test connection when the page appears
        await _viewModel.LoadSettingsAsync();
    }
}
