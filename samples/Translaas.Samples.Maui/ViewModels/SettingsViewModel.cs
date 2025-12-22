using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Translaas.Client;
using Translaas.Extensions.DependencyInjection;

using L = Translaas.Models.LanguageCodes;

namespace Translaas.Samples.Maui.ViewModels;

/// <summary>
/// Settings view model demonstrating configuration and testing of Translaas SDK.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ITranslaasService _translaasService;
    private readonly ITranslaasClient _translaasClient;
    private const string ProjectId = "translaas-sdk-samples";

    [ObservableProperty]
    private string _testGroup = "common";

    [ObservableProperty]
    private string _testEntry = "welcome";

    [ObservableProperty]
    private string _testLanguage = L.English;

    [ObservableProperty]
    private string _testResult = "Press 'Test Translation' to see the result";

    [ObservableProperty]
    private string _connectionStatus = "Not tested";

    [ObservableProperty]
    private bool _isTesting;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorDetails = string.Empty;

    /// <summary>
    /// List of supported language codes for reference.
    /// </summary>
    public List<string> SupportedLanguages { get; } =
    [
        L.English,
        L.French,
        L.Spanish,
        L.German,
        L.Italian,
        L.Portuguese,
        L.Dutch,
        L.Russian,
        L.Chinese,
        L.Japanese
    ];

    /// <summary>
    /// Initializes a new instance of the SettingsViewModel.
    /// </summary>
    /// <param name="translaasService">The translation service.</param>
    /// <param name="translaasClient">The translation client.</param>
    public SettingsViewModel(ITranslaasService translaasService, ITranslaasClient translaasClient)
    {
        _translaasService = translaasService;
        _translaasClient = translaasClient;
    }

    /// <summary>
    /// Tests the translation service with the specified parameters.
    /// </summary>
    [RelayCommand]
    public async Task TestTranslationAsync()
    {
        try
        {
            IsTesting = true;
            HasError = false;
            ErrorDetails = string.Empty;
            TestResult = "Testing...";

            var result = await _translaasService.T(TestGroup, TestEntry, TestLanguage);
            TestResult = $"✓ Translation: {result}";
        }
        catch (Exception ex)
        {
            HasError = true;
            TestResult = "✗ Translation failed";
            ErrorDetails = ex.Message;
        }
        finally
        {
            IsTesting = false;
        }
    }

    /// <summary>
    /// Tests the connection to the Translaas API.
    /// </summary>
    [RelayCommand]
    public async Task TestConnectionAsync()
    {
        try
        {
            IsTesting = true;
            HasError = false;
            ErrorDetails = string.Empty;
            ConnectionStatus = "Testing connection...";

            // Try to get project locales as a connection test
            var locales = await _translaasClient.GetProjectLocalesAsync(ProjectId);

            if (locales.Locales.Count > 0)
            {
                ConnectionStatus = $"✓ Connected - {locales.Locales.Count} locale(s) available";
            }
            else
            {
                ConnectionStatus = "✓ Connected - No locales found";
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ConnectionStatus = "✗ Connection failed";
            ErrorDetails = ex.Message;
        }
        finally
        {
            IsTesting = false;
        }
    }

    /// <summary>
    /// Loads settings when the view appears.
    /// </summary>
    [RelayCommand]
    public async Task LoadSettingsAsync()
    {
        await TestConnectionAsync();
    }
}
