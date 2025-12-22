using System.Text.Json;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Translaas.Client;
using Translaas.Extensions.DependencyInjection;
using Translaas.Models.Responses;

using L = Translaas.Models.LanguageCodes;

namespace Translaas.Samples.Maui.ViewModels;

/// <summary>
/// Main view model demonstrating Translaas SDK usage with MVVM pattern.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ITranslaasService _translaasService;
    private readonly ITranslaasClient _translaasClient;
    private const string ProjectId = "translaas-sdk-samples";

    /// <summary>
    /// List of available languages for the picker.
    /// </summary>
    public List<LanguageOption> AvailableLanguages { get; } =
    [
        new LanguageOption("English", L.English),
        new LanguageOption("French", L.French),
        new LanguageOption("Spanish", L.Spanish),
        new LanguageOption("German", L.German),
        new LanguageOption("Italian", L.Italian)
    ];

    [ObservableProperty]
    private LanguageOption _selectedLanguage;

    [ObservableProperty]
    private string _welcomeText = "Loading...";

    [ObservableProperty]
    private string _welcomeMessage = "Loading...";

    [ObservableProperty]
    private string _itemSingular = "Loading...";

    [ObservableProperty]
    private string _itemPlural = "Loading...";

    [ObservableProperty]
    private string _greetingWithParams = "Loading...";

    [ObservableProperty]
    private string _itemsWithParams = "Loading...";

    [ObservableProperty]
    private string _appName = "Loading...";

    [ObservableProperty]
    private string _pluralCountText = "1";

    [ObservableProperty]
    private string _pluralResult = "Enter a number above";

    [ObservableProperty]
    private string _groupEntries = "Loading...";

    [ObservableProperty]
    private string _availableLocales = "Loading...";

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _cacheStatus = "Checking...";

    /// <summary>
    /// Initializes a new instance of the MainViewModel.
    /// </summary>
    /// <param name="translaasService">The translation service.</param>
    /// <param name="translaasClient">The translation client for advanced operations.</param>
    public MainViewModel(ITranslaasService translaasService, ITranslaasClient translaasClient)
    {
        _translaasService = translaasService;
        _translaasClient = translaasClient;
        _selectedLanguage = AvailableLanguages[0]; // Default to English
    }

    /// <summary>
    /// Loads translations when the view appears.
    /// </summary>
    [RelayCommand]
    public async Task LoadTranslationsAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var lang = SelectedLanguage.Code;

            // Basic translations using ITranslaasService
            WelcomeText = await _translaasService.T("common", "welcome", lang);
            WelcomeMessage = await _translaasService.T("common", "welcome.message", lang);
            AppName = await _translaasService.T("common", "app.name", lang);

            // Pluralization examples
            ItemSingular = await _translaasService.T("messages", "item", lang, 1);
            ItemPlural = await _translaasService.T("messages", "item", lang, 5);

            // Named parameters example
            var parameters = new Dictionary<string, string>
            {
                { "userName", "John" },
                { "itemCount", "5" }
            };
            GreetingWithParams = await _translaasService.T("messages", "greeting", lang, parameters: parameters);

            // Combined number and parameters
            var userParams = new Dictionary<string, string>
            {
                { "userName", "John" }
            };
            ItemsWithParams = await _translaasService.T("messages", "items", lang, number: 5, parameters: userParams);

            // Load translation group (bulk operation)
            await LoadGroupEntriesAsync(lang);

            // Load available locales
            await LoadLocalesAsync();

            // Cache demonstration
            await DemonstrateCachingAsync(lang);
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error loading translations: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Called when the selected language changes.
    /// </summary>
    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        // Reload translations when language changes
        if (!IsLoading)
        {
            _ = LoadTranslationsAsync();
        }
    }

    /// <summary>
    /// Tests pluralization with a custom number.
    /// </summary>
    [RelayCommand]
    public async Task TestPluralAsync()
    {
        try
        {
            if (decimal.TryParse(PluralCountText, out var count))
            {
                PluralResult = await _translaasService.T(
                    "messages", "item", SelectedLanguage.Code, count);
            }
            else
            {
                PluralResult = "Please enter a valid number";
            }
        }
        catch (Exception ex)
        {
            PluralResult = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Refreshes all translations.
    /// </summary>
    [RelayCommand]
    public async Task RefreshAsync()
    {
        await LoadTranslationsAsync();
    }

    private async Task LoadGroupEntriesAsync(string lang)
    {
        try
        {
            var group = await _translaasClient.GetGroupAsync(ProjectId, "common", lang);

            // Filter and format entries for display
            var entries = group.Entries
                .Where(e => e.Value.ValueKind == JsonValueKind.String)
                .Select(e => $"• {e.Key}: {e.Value.GetString()}")
                .ToList();

            GroupEntries = entries.Count > 0
                ? string.Join("\n", entries)
                : "No entries found";
        }
        catch (Exception ex)
        {
            GroupEntries = $"Error loading group: {ex.Message}";
        }
    }

    private async Task LoadLocalesAsync()
    {
        try
        {
            var locales = await _translaasClient.GetProjectLocalesAsync(ProjectId);
            AvailableLocales = string.Join(", ", locales.Locales);
        }
        catch (Exception ex)
        {
            AvailableLocales = $"Error: {ex.Message}";
        }
    }

    private async Task DemonstrateCachingAsync(string lang)
    {
        try
        {
            // First call (potentially cache miss)
            var start1 = DateTime.UtcNow;
            await _translaasService.T("common", "welcome", lang);
            var duration1 = (DateTime.UtcNow - start1).TotalMilliseconds;

            // Second call (should be cache hit)
            var start2 = DateTime.UtcNow;
            await _translaasService.T("common", "welcome", lang);
            var duration2 = (DateTime.UtcNow - start2).TotalMilliseconds;

            var speedup = duration2 > 0 ? duration1 / duration2 : 0;
            CacheStatus = $"1st call: {duration1:F2}ms | 2nd call: {duration2:F2}ms | Speedup: {speedup:F1}x";
        }
        catch (Exception ex)
        {
            CacheStatus = $"Cache test error: {ex.Message}";
        }
    }
}

/// <summary>
/// Represents a language option for the picker.
/// </summary>
public record LanguageOption(string DisplayName, string Code)
{
    /// <summary>
    /// Returns the display name for the picker.
    /// </summary>
    public override string ToString() => DisplayName;
}
