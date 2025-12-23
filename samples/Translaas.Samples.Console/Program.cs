using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Text.Json;

using Translaas.Caching;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;
using Translaas.Models;
using L = Translaas.Models.LanguageCodes;

namespace Translaas.Samples.Console;

/// <summary>
/// Console application demonstrating Translaas SDK usage with dependency injection.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Build the host with dependency injection
        // Host.CreateDefaultBuilder automatically loads:
        // - appsettings.json
        // - appsettings.{Environment}.json (e.g., appsettings.Development.json)
        // - User secrets (when environment is Development or explicitly added)
        // - Environment variables
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                // Explicitly add user secrets for console apps
                // This ensures user secrets are loaded regardless of environment
                // User secrets are stored in: %APPDATA%\Microsoft\UserSecrets\{UserSecretsId}\secrets.json (Windows)
                // or ~/.microsoft/usersecrets/{UserSecretsId}/secrets.json (Linux/Mac)
                config.AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly());
            })
            .ConfigureLogging(logging =>
            {
                // Suppress HTTP client logging for cleaner console output
                // Only filter out HTTP client logs, keep other logs at default level
                logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
            })
            .ConfigureServices((context, services) =>
            {
                // Add HttpClient support (required for Translaas)
                services.AddHttpClient();

                // Configure Translaas with options from appsettings.json
                // API key should be stored in user secrets (secrets.json)
                var configuration = context.Configuration;
                
                // Debug: Check if configuration is loading correctly
                var apiKey = configuration["Translaas:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    // Provide helpful error message with troubleshooting steps
                    var errorMessage = "Translaas:ApiKey must be configured in appsettings.json or user secrets.\n" +
                        "To set user secrets, run:\n" +
                        "  dotnet user-secrets set \"Translaas:ApiKey\" \"your-api-key\" --project samples/Translaas.Samples.Console\n" +
                        "\nCurrent configuration sources: " + string.Join(", ", configuration.GetChildren().Select(c => c.Key));
                    throw new InvalidOperationException(errorMessage);
                }
                
                services.AddTranslaas(options =>
                {
                    // Required: Set your API key (from user secrets or appsettings.json)
                    options.ApiKey = apiKey;

                    // Required: Set the base URL
                    // Note: Do NOT include /api in the BaseUrl - the client adds /api/ to all endpoints
                    options.BaseUrl = configuration["Translaas:BaseUrl"] 
                        ?? "https://sdk-api.translaas.local";

                    // Optional: Configure caching
                    options.CacheMode = configuration.GetValue<CacheMode?>("Translaas:CacheMode") ?? CacheMode.Group;
                    options.CacheAbsoluteExpiration = TimeSpan.TryParse(configuration["Translaas:CacheAbsoluteExpiration"], out var absoluteExpiration)
                        ? absoluteExpiration
                        : TimeSpan.FromHours(1);
                    
                    options.CacheSlidingExpiration = TimeSpan.TryParse(configuration["Translaas:CacheSlidingExpiration"], out var slidingExpiration)
                        ? slidingExpiration
                        : TimeSpan.FromMinutes(30);

                    // Optional: Configure timeout
                    options.Timeout = TimeSpan.TryParse(configuration["Translaas:Timeout"], out var timeout)
                        ? timeout
                        : TimeSpan.FromSeconds(30);

                    // Optional: Set default language fallback (for console apps, this is the final fallback)
                    options.DefaultLanguage = configuration["Translaas:DefaultLanguage"] ?? L.English;
                }, language =>
                {
                    // Configure language resolution providers for console apps
                    // Note: Console apps don't have HTTP context, so RequestLanguageProvider is not available
                    // 
                    // Language providers are checked in the order they are registered.
                    // The first provider that returns a non-null language wins.
                    // 
                    // Available providers for console apps:
                    // - UseCulture() - Resolves from CultureInfo.CurrentUICulture
                    // - UseDefault() - Resolves from TranslaasOptions.DefaultLanguage
                    // 
                    // You can configure the order and which providers to use based on your needs.
                    language
                        .UseCulture()  // Resolves from thread culture (CultureInfo.CurrentUICulture)
                        .UseDefault(); // Resolves from DefaultLanguage option (appsettings.json)
                });
            })
            .Build();

        // Get the service from DI container
        var translaasService = host.Services.GetRequiredService<ITranslaasService>();
        // Note: ITranslaasClient is only needed for bulk operations (GetGroupAsync, GetProjectAsync, GetProjectLocalesAsync)
        // For single-entry lookups, always use ITranslaasService.T()
        var translaasClient = host.Services.GetRequiredService<ITranslaasClient>();

        // Get the language resolver for debugging (optional - only if registered)
        var languageResolver = host.Services.GetService<ILanguageResolver>();

        // Get the default language from configuration
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var defaultLanguage = configuration["Translaas:DefaultLanguage"] ?? L.English;

        System.Console.WriteLine("=== Translaas SDK Console Sample ===\n");
        System.Console.WriteLine($"Default Language (from appsettings.json): {defaultLanguage}\n");

        try
        {
            const string projectId = "translaas-sdk-samples";

            // Example 1: Using ITranslaasService with default language (from appsettings.json)
            System.Console.WriteLine("Example 1: Using ITranslaasService.T() with default language");
            var translation1 = await translaasService.T("common", "welcome"); // Uses default language from config
            System.Console.WriteLine($"Translation (default language '{defaultLanguage}'): {translation1}\n");

            // Example 1b: Override with explicit language
            System.Console.WriteLine("Example 1b: Override with explicit language");
            var translation1b = await translaasService.T("common", "welcome", L.English); // Explicit override
            System.Console.WriteLine($"Translation (explicit override to '{L.English}'): {translation1b}\n");

            // Example 2: Pluralization with default language
            System.Console.WriteLine("Example 2: Pluralization with default language");
            var translation2a = await translaasService.T("messages", "item", 1); // Uses default language
            var translation2b = await translaasService.T("messages", "item", 5); // Uses default language
            System.Console.WriteLine($"1 item (default language '{defaultLanguage}'): {translation2a}");
            System.Console.WriteLine($"5 items (default language '{defaultLanguage}'): {translation2b}\n");

            // Example 2b: Pluralization with override
            System.Console.WriteLine("Example 2b: Pluralization with language override");
            var translation2c = await translaasService.T("messages", "item", L.English, 1); // Explicit override
            var translation2d = await translaasService.T("messages", "item", L.English, 5); // Explicit override
            System.Console.WriteLine($"1 item (override to '{L.English}'): {translation2c}");
            System.Console.WriteLine($"5 items (override to '{L.English}'): {translation2d}\n");

            // Example 3: Named Parameters with default language
            System.Console.WriteLine("Example 3: Named Parameters with default language");
            var parameters = new Dictionary<string, string>
            {
                { "userName", "John" },
                { "itemCount", "5" }
            };
            var translation3 = await translaasService.T("messages", "greeting", parameters); // Uses default language
            System.Console.WriteLine($"Translation with parameters (default language '{defaultLanguage}'): {translation3}\n");

            // Example 3b: Named Parameters with override
            System.Console.WriteLine("Example 3b: Named Parameters with language override");
            var translation3b = await translaasService.T("messages", "greeting", L.English, parameters); // Explicit override
            System.Console.WriteLine($"Translation with parameters (override to '{L.English}'): {translation3b}\n");

            // Example 4: Combining Number and Named Parameters with default language
            System.Console.WriteLine("Example 4: Combining Number and Named Parameters with default language");
            var translation4 = await translaasService.T("messages", "items", 5, parameters); // Uses default language
            System.Console.WriteLine($"Translation with number and parameters (default language '{defaultLanguage}'): {translation4}\n");

            // Example 4b: Combining Number and Named Parameters with override
            System.Console.WriteLine("Example 4b: Combining Number and Named Parameters with language override");
            var translation4b = await translaasService.T("messages", "items", L.English, 5, parameters); // Explicit override
            System.Console.WriteLine($"Translation with number and parameters (override to '{L.English}'): {translation4b}\n");

            // Example 5: Get multiple entries using .T() helper with default language
            System.Console.WriteLine("Example 5: Get multiple entries using .T() helper with default language");
            var appName = await translaasService.T("common", "app.name"); // Uses default language
            var welcome = await translaasService.T("common", "welcome"); // Uses default language
            var welcomeMessage = await translaasService.T("common", "welcome.message"); // Uses default language
            System.Console.WriteLine($"App Name (default language '{defaultLanguage}'): {appName}");
            System.Console.WriteLine($"Welcome (default language '{defaultLanguage}'): {welcome}");
            System.Console.WriteLine($"Message (default language '{defaultLanguage}'): {welcomeMessage}\n");

            // Example 5b: Get multiple entries with override
            System.Console.WriteLine("Example 5b: Get multiple entries with language override");
            var appNameOverride = await translaasService.T("common", "app.name", L.English); // Explicit override
            var welcomeOverride = await translaasService.T("common", "welcome", L.English); // Explicit override
            System.Console.WriteLine($"App Name (override to '{L.English}'): {appNameOverride}");
            System.Console.WriteLine($"Welcome (override to '{L.English}'): {welcomeOverride}\n");

            // Example 6: Get entire translation group (bulk operation) with default language
            System.Console.WriteLine("Example 6: Get entire translation group (bulk operation) with default language");
            System.Console.WriteLine("Note: GetGroupAsync() retrieves all entries in a group at once.");
            System.Console.WriteLine("Use this when you need multiple entries, or use .T() for individual entries.\n");
            const string groupName = "common";
            var group = await translaasClient.GetGroupAsync(projectId, groupName, defaultLanguage); // Uses default language
            
            // Filter out metadata fields and only show actual translation entries
            var translationEntries = group.Entries
                .Where(e => e.Value.ValueKind == JsonValueKind.String)
                .ToDictionary(e => e.Key, e => e.Value.GetString() ?? string.Empty);
            
            System.Console.WriteLine($"Group '{groupName}' contains {translationEntries.Count} translation entries:");
            foreach (var entry in translationEntries)
            {
                System.Console.WriteLine($"  {entry.Key}: {entry.Value}");
            }
            System.Console.WriteLine();

            // Example 6: Get project locales
            System.Console.WriteLine("Example 6: Get available locales");
            var locales = await translaasClient.GetProjectLocalesAsync(projectId);
            System.Console.WriteLine($"Available locales: {string.Join(", ", locales.Locales)}\n");

            // Example 7: Caching demonstration with default language
            System.Console.WriteLine("Example 7: Caching demonstration with default language");
            System.Console.WriteLine("First call (cache miss):");
            var start1 = DateTime.UtcNow;
            await translaasService.T("common", "welcome"); // Uses default language
            var duration1 = DateTime.UtcNow - start1;
            System.Console.WriteLine($"Duration: {duration1.TotalMilliseconds:F2}ms");

            System.Console.WriteLine("Second call (cache hit):");
            var start2 = DateTime.UtcNow;
            await translaasService.T("common", "welcome"); // Uses default language
            var duration2 = DateTime.UtcNow - start2;
            System.Console.WriteLine($"Duration: {duration2.TotalMilliseconds:F2}ms");
            var speedup = duration1.TotalMilliseconds / duration2.TotalMilliseconds;
            System.Console.WriteLine($"Cache speedup: {speedup:F2}x faster\n");

            // Example 8: Automatic Language Resolution
            System.Console.WriteLine("Example 8: Automatic Language Resolution");
            System.Console.WriteLine("Language is resolved from configured providers in the order they were registered.\n");
            System.Console.WriteLine($"Current configuration: Culture → Default (from appsettings.json: {defaultLanguage})\n");
            
            System.Console.WriteLine("8a. Using automatic resolution (from configured providers):");
            System.Console.WriteLine($"  Current thread culture: {System.Globalization.CultureInfo.CurrentUICulture.Name}");
            System.Console.WriteLine($"  Default language (from appsettings.json): {defaultLanguage}");
            var autoLang = await translaasService.T("common", "welcome"); // Uses configured providers
            System.Console.WriteLine($"  Translation: {autoLang}\n");

            System.Console.WriteLine("8b. Override with explicit language (bypasses all providers):");
            var explicitLang = await translaasService.T("common", "welcome", L.English); // Explicit override
            System.Console.WriteLine($"  Override language: {L.English}");
            System.Console.WriteLine($"  Translation: {explicitLang}\n");

            System.Console.WriteLine("8c. Changing thread culture to French:");
            var originalCulture = System.Globalization.CultureInfo.CurrentUICulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr-FR");
            System.Console.WriteLine($"  Thread culture: {System.Globalization.CultureInfo.CurrentUICulture.Name}");
            System.Console.WriteLine($"  Two-letter ISO code (what CultureLanguageProvider returns): {System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName}");
            
            // Show what language code is actually being resolved
            if (languageResolver != null)
            {
                var resolvedLangCode = languageResolver.Resolve();
                System.Console.WriteLine($"  Resolved language code (from providers): {resolvedLangCode ?? "(null)"}");
            }
            
            var frenchLang = await translaasService.T("common", "welcome"); // Uses thread culture if Culture provider is configured first
            System.Console.WriteLine($"  Translation: {frenchLang}");
            System.Console.WriteLine($"  Note: If translation is still in English, verify:");
            System.Console.WriteLine($"    - The API has French translations for this entry");
            System.Console.WriteLine($"    - The resolved language code above is 'fr'");
            System.Threading.Thread.CurrentThread.CurrentUICulture = originalCulture; // Restore
            System.Console.WriteLine();

            System.Console.WriteLine("8d. Testing fallback behavior (when culture is invariant):");
            var originalCulture2 = System.Globalization.CultureInfo.CurrentUICulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
            var defaultLangTranslation = await translaasService.T("common", "welcome"); // Falls back to next provider
            System.Console.WriteLine($"  Thread culture: InvariantCulture (no language)");
            System.Console.WriteLine($"  Default language (from appsettings.json): {defaultLanguage}");
            System.Console.WriteLine($"  Translation: {defaultLangTranslation}");
            System.Threading.Thread.CurrentThread.CurrentUICulture = originalCulture2; // Restore
            System.Console.WriteLine();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
            System.Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        System.Console.WriteLine("Press any key to exit...");
        System.Console.ReadKey();
    }
}
