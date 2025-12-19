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
using static Translaas.Models.Translaas;

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
                });
            })
            .Build();

        // Get the service from DI container
        var translaasService = host.Services.GetRequiredService<ITranslaasService>();
        // Note: ITranslaasClient is only needed for bulk operations (GetGroupAsync, GetProjectAsync, GetProjectLocalesAsync)
        // For single-entry lookups, always use ITranslaasService.T()
        var translaasClient = host.Services.GetRequiredService<ITranslaasClient>();

        System.Console.WriteLine("=== Translaas SDK Console Sample ===\n");

        try
        {
            const string projectId = "translaas-sdk-samples";

            // Example 1: Using ITranslaasService (recommended approach)
            System.Console.WriteLine("Example 1: Using ITranslaasService.T()");
            var translation1 = await translaasService.T("common", "welcome", L.English);
            System.Console.WriteLine($"Translation: {translation1}\n");

            // Example 2: Pluralization
            System.Console.WriteLine("Example 2: Pluralization");
            var translation2a = await translaasService.T("messages", "item", L.English, 1);
            var translation2b = await translaasService.T("messages", "item", L.English, 5);
            System.Console.WriteLine($"1 item: {translation2a}");
            System.Console.WriteLine($"5 items: {translation2b}\n");

            // Example 3: Named Parameters
            System.Console.WriteLine("Example 3: Named Parameters");
            var parameters = new Dictionary<string, string>
            {
                { "userName", "John" },
                { "itemCount", "5" }
            };
            var translation3 = await translaasService.T("messages", "greeting", L.English, parameters: parameters);
            System.Console.WriteLine($"Translation with parameters: {translation3}\n");

            // Example 4: Combining Number and Named Parameters
            System.Console.WriteLine("Example 4: Combining Number and Named Parameters");
            var translation4 = await translaasService.T("messages", "items", L.English, number: 5, parameters: parameters);
            System.Console.WriteLine($"Translation with number and parameters: {translation4}\n");

            // Example 4: Get multiple entries using .T() helper
            System.Console.WriteLine("Example 4: Get multiple entries using .T() helper");
            var appName = await translaasService.T("common", "app.name", L.English);
            var welcome = await translaasService.T("common", "welcome", L.English);
            var welcomeMessage = await translaasService.T("common", "welcome.message", L.English);
            System.Console.WriteLine($"App Name: {appName}");
            System.Console.WriteLine($"Welcome: {welcome}");
            System.Console.WriteLine($"Message: {welcomeMessage}\n");

            // Example 5: Get entire translation group (bulk operation)
            System.Console.WriteLine("Example 5: Get entire translation group (bulk operation)");
            System.Console.WriteLine("Note: GetGroupAsync() retrieves all entries in a group at once.");
            System.Console.WriteLine("Use this when you need multiple entries, or use .T() for individual entries.\n");
            const string groupName = "common";
            var group = await translaasClient.GetGroupAsync(projectId, groupName, L.English);
            
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

            // Example 7: Caching demonstration
            System.Console.WriteLine("Example 7: Caching demonstration");
            System.Console.WriteLine("First call (cache miss):");
            var start1 = DateTime.UtcNow;
            await translaasService.T("common", "welcome", L.English);
            var duration1 = DateTime.UtcNow - start1;
            System.Console.WriteLine($"Duration: {duration1.TotalMilliseconds:F2}ms");

            System.Console.WriteLine("Second call (cache hit):");
            var start2 = DateTime.UtcNow;
            await translaasService.T("common", "welcome", L.English);
            var duration2 = DateTime.UtcNow - start2;
            System.Console.WriteLine($"Duration: {duration2.TotalMilliseconds:F2}ms");
            var speedup = duration1.TotalMilliseconds / duration2.TotalMilliseconds;
            System.Console.WriteLine($"Cache speedup: {speedup:F2}x faster\n");
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
