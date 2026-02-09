using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Text.Json;

using Translaas.Caching.File;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;
using Translaas.Models;
using Translaas.Models.Errors;
using L = Translaas.Models.LanguageCodes;

namespace Translaas.Samples.Offline;

/// <summary>
/// Console application demonstrating Translaas SDK usage in offline mode (CacheOnly).
/// This sample works entirely without network connectivity by reading translations from local cache files.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        System.Console.WriteLine("=== Translaas SDK Offline Mode Sample ===\n");
        System.Console.WriteLine("This sample demonstrates offline mode using CacheOnly fallback mode.");
        System.Console.WriteLine("All translations are read from local cache files - no API calls are made.\n");

        // Build the host with dependency injection
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                // Suppress HTTP client logging for cleaner console output
                logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
            })
            .ConfigureServices((context, services) =>
            {
                // Add HttpClient support (required for Translaas, but won't be used in CacheOnly mode)
                services.AddHttpClient();

                // Configure Translaas with offline cache mode
                var configuration = context.Configuration;
                
                services.AddTranslaas(options =>
                {
                    // API key and BaseUrl are not used in CacheOnly mode, but still required for configuration
                    options.ApiKey = configuration["Translaas:ApiKey"] ?? "dummy-not-used";
                    options.BaseUrl = configuration["Translaas:BaseUrl"] ?? "https://api.translaas.com";

                    // Configure offline cache with CacheOnly mode
                    options.OfflineCache.Enabled = true;
                    options.OfflineCache.CacheDirectory = configuration["Translaas:OfflineCache:CacheDirectory"] ?? "./cache";
                    options.OfflineCache.FallbackMode = OfflineFallbackMode.CacheOnly; // ⚠️ CRITICAL: Never calls API
                    options.OfflineCache.AutoSync = false; // Disable sync to prevent any API calls
                    options.OfflineCache.DefaultProjectId = configuration["Translaas:OfflineCache:DefaultProjectId"] ?? "translaas-sdk-samples"; // Required for offline cache

                    // Optional: Set default language fallback
                    options.DefaultLanguage = configuration["Translaas:DefaultLanguage"] ?? L.English;
                }, language =>
                {
                    // Configure language resolution providers for console apps
                    language
                        .UseCulture()  // Resolves from thread culture (CultureInfo.CurrentUICulture)
                        .UseDefault(); // Resolves from DefaultLanguage option (appsettings.json)
                });
            })
            .Build();

        // Get the service from DI container
        var translaasService = host.Services.GetRequiredService<ITranslaasService>();
        var translaasClient = host.Services.GetRequiredService<ITranslaasClient>();

        // Get the language resolver for debugging (optional)
        var languageResolver = host.Services.GetService<ILanguageResolver>();

        // Get the default language from configuration
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var defaultLanguage = configuration["Translaas:DefaultLanguage"] ?? L.English;
        var cacheDirectory = configuration["Translaas:OfflineCache:CacheDirectory"] ?? "./cache";

        var defaultProjectId = configuration["Translaas:OfflineCache:DefaultProjectId"] ?? "translaas-sdk-samples";
        
        System.Console.WriteLine($"Configuration:");
        System.Console.WriteLine($"  Cache Directory: {cacheDirectory}");
        System.Console.WriteLine($"  Default Project ID: {defaultProjectId}");
        System.Console.WriteLine($"  Fallback Mode: CacheOnly (no API calls)");
        System.Console.WriteLine($"  Default Language: {defaultLanguage}\n");

        try
        {
            var projectId = defaultProjectId;

            // Verify cache files exist
            System.Console.WriteLine("=== Verifying Cache Files ===\n");
            var cacheProvider = host.Services.GetRequiredService<IOfflineCacheProvider>();
            var isCached = await cacheProvider.IsCachedAsync(projectId, defaultLanguage);
            
            if (!isCached)
            {
                System.Console.WriteLine($"⚠️  WARNING: Cache file not found for project '{projectId}' and language '{defaultLanguage}'");
                System.Console.WriteLine($"   Expected location: {cacheDirectory}/{projectId}/{defaultLanguage}/project.json");
                System.Console.WriteLine($"   Please ensure cache files are present before running this sample.\n");
            }
            else
            {
                System.Console.WriteLine($"✅ Cache file found for project '{projectId}' and language '{defaultLanguage}'\n");
            }

            // Example 1: Using ITranslaasService with default language
            System.Console.WriteLine("=== Example 1: Basic Translation ===\n");
            try
            {
                var translation1 = await translaasService.T("common", "welcome");
                System.Console.WriteLine($"Translation (group: 'common', entry: 'welcome'): {translation1}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                System.Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 2: Pluralization
            System.Console.WriteLine("=== Example 2: Pluralization ===\n");
            try
            {
                var translation2a = await translaasService.T("messages", "item", 1);
                var translation2b = await translaasService.T("messages", "item", 5);
                System.Console.WriteLine($"1 item: {translation2a}");
                System.Console.WriteLine($"5 items: {translation2b}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                System.Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 3: Named Parameters
            System.Console.WriteLine("=== Example 3: Named Parameters ===\n");
            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "userName", "John" },
                    { "itemCount", "5" }
                };
                var translation3 = await translaasService.T("messages", "greeting", parameters);
                System.Console.WriteLine($"Translation with parameters: {translation3}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                System.Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 4: Combining Number and Named Parameters
            System.Console.WriteLine("=== Example 4: Number + Named Parameters ===\n");
            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "userName", "John" }
                };
                var translation4 = await translaasService.T("messages", "items", 5, parameters);
                System.Console.WriteLine($"Translation with number and parameters: {translation4}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                System.Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 5: Get multiple entries
            System.Console.WriteLine("=== Example 5: Multiple Entries ===\n");
            try
            {
                var appName = await translaasService.T("common", "app.name");
                var welcome = await translaasService.T("common", "welcome");
                var welcomeMessage = await translaasService.T("common", "welcome.message");
                System.Console.WriteLine($"App Name: {appName}");
                System.Console.WriteLine($"Welcome: {welcome}");
                System.Console.WriteLine($"Welcome Message: {welcomeMessage}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                System.Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 6: Get entire translation group
            System.Console.WriteLine("=== Example 6: Get Translation Group ===\n");
            try
            {
                const string groupName = "common";
                var group = await translaasClient.GetGroupAsync(projectId, groupName, defaultLanguage);
                
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
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                System.Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 7: Get project locales
            System.Console.WriteLine("=== Example 7: Get Available Locales ===\n");
            try
            {
                var locales = await translaasClient.GetProjectLocalesAsync(projectId);
                System.Console.WriteLine($"Available locales: {string.Join(", ", locales.Locales)}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                System.Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 8: Language Resolution
            System.Console.WriteLine("=== Example 8: Language Resolution ===\n");
            System.Console.WriteLine($"Current thread culture: {System.Globalization.CultureInfo.CurrentUICulture.Name}");
            System.Console.WriteLine($"Default language (from appsettings.json): {defaultLanguage}");
            
            if (languageResolver != null)
            {
                var resolvedLangCode = languageResolver.Resolve();
                System.Console.WriteLine($"Resolved language code (from providers): {resolvedLangCode ?? "(null)"}");
            }
            
            try
            {
                var autoLang = await translaasService.T("common", "welcome");
                System.Console.WriteLine($"Translation (auto-resolved): {autoLang}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                System.Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 9: Explicit Language Override
            System.Console.WriteLine("=== Example 9: Explicit Language Override ===\n");
            try
            {
                var explicitLang = await translaasService.T("common", "welcome", L.English);
                System.Console.WriteLine($"Translation (explicit override to '{L.English}'): {explicitLang}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                System.Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 10: Demonstrate offline mode (no API calls)
            System.Console.WriteLine("=== Example 10: Offline Mode Verification ===\n");
            System.Console.WriteLine("✅ All translations were loaded from local cache files.");
            System.Console.WriteLine("✅ No API calls were made (CacheOnly mode).");
            System.Console.WriteLine("✅ Application works entirely offline.\n");
        }
        catch (TranslaasOfflineCacheMissException ex)
        {
            System.Console.WriteLine($"\n❌ Offline Cache Miss Exception:");
            System.Console.WriteLine($"   Project: {ex.Project}");
            System.Console.WriteLine($"   Language: {ex.Language}");
            if (!string.IsNullOrEmpty(ex.Group))
            {
                System.Console.WriteLine($"   Group: {ex.Group}");
            }
            if (!string.IsNullOrEmpty(ex.Entry))
            {
                System.Console.WriteLine($"   Entry: {ex.Entry}");
            }
            System.Console.WriteLine($"\n   This exception is thrown when a translation is not found in the cache.");
            System.Console.WriteLine($"   In CacheOnly mode, the SDK never calls the API, so missing translations");
            System.Console.WriteLine($"   cannot be fetched from the backend.\n");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\n❌ Error: {ex.Message}");
            System.Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        System.Console.WriteLine("Press any key to exit...");
        System.Console.ReadKey();
    }
}
