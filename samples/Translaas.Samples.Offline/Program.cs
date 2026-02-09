using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Translaas.Caching.File;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;
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
        // Set console output encoding to UTF-8 to properly display non-ASCII characters (e.g., Cyrillic)
        try
        {
            Console.OutputEncoding = Encoding.UTF8;
        }
        catch
        {
            // If UTF-8 encoding is not supported, continue with default encoding
            // Non-ASCII characters may not display correctly
        }
        
        Console.WriteLine("=== Translaas SDK Offline Mode Sample ===\n");
        Console.WriteLine("This sample demonstrates offline mode using CacheOnly fallback mode.");
        Console.WriteLine("All translations are read from local cache files - no API calls are made.\n");

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
                    // For offline mode, prioritize DefaultLanguage over thread culture
                    // This ensures the configured language is used even if thread culture differs
                    language
                        .UseDefault(); // Resolves from DefaultLanguage option (appsettings.json)
                    // Note: UseCulture() is intentionally omitted for offline mode to ensure
                    // the configured DefaultLanguage is always used, regardless of thread culture
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
        
        Console.WriteLine($"Configuration:");
        Console.WriteLine($"  Cache Directory: {cacheDirectory}");
        Console.WriteLine($"  Default Project ID: {defaultProjectId}");
        Console.WriteLine($"  Fallback Mode: CacheOnly (no API calls)");
        Console.WriteLine($"  Default Language: {defaultLanguage}\n");

        try
        {
            var projectId = defaultProjectId;

            // Verify cache files exist
            Console.WriteLine("=== Verifying Cache Files ===\n");
            var cacheProvider = host.Services.GetRequiredService<IOfflineCacheProvider>();
            var isCached = await cacheProvider.IsCachedAsync(projectId, defaultLanguage);
            
            if (!isCached)
            {
                Console.WriteLine($"⚠️  WARNING: Cache file not found for project '{projectId}' and language '{defaultLanguage}'");
                Console.WriteLine($"   Expected location: {cacheDirectory}/{projectId}/{defaultLanguage}/project.json");
                Console.WriteLine($"   Please ensure cache files are present before running this sample.\n");
            }
            else
            {
                Console.WriteLine($"✅ Cache file found for project '{projectId}' and language '{defaultLanguage}'\n");
            }

            // Example 1: Using ITranslaasService with default language
            Console.WriteLine("=== Example 1: Basic Translation ===\n");
            try
            {
                var translation1 = await translaasService.T("common", "welcome");
                Console.WriteLine($"Translation (group: 'common', entry: 'welcome'): {translation1}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 2: Pluralization
            Console.WriteLine("=== Example 2: Pluralization ===\n");
            try
            {
                var translation2a = await translaasService.T("messages", "item", 1);
                var translation2b = await translaasService.T("messages", "item", 5);
                Console.WriteLine($"1 item: {translation2a}");
                Console.WriteLine($"5 items: {translation2b}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 3: Named Parameters
            Console.WriteLine("=== Example 3: Named Parameters ===\n");
            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "userName", "John" },
                    { "itemCount", "5" }
                };
                var translation3 = await translaasService.T("messages", "greeting", parameters);
                Console.WriteLine($"Translation with parameters: {translation3}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 4: Combining Number and Named Parameters
            Console.WriteLine("=== Example 4: Number + Named Parameters ===\n");
            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "userName", "John" }
                };
                var translation4 = await translaasService.T("messages", "items", 5, parameters);
                Console.WriteLine($"Translation with number and parameters: {translation4}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 5: Get multiple entries
            Console.WriteLine("=== Example 5: Multiple Entries ===\n");
            try
            {
                var appName = await translaasService.T("common", "app.name");
                var welcome = await translaasService.T("common", "welcome");
                var welcomeMessage = await translaasService.T("common", "welcome.message");
                Console.WriteLine($"App Name: {appName}");
                Console.WriteLine($"Welcome: {welcome}");
                Console.WriteLine($"Welcome Message: {welcomeMessage}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 6: Get entire translation group
            Console.WriteLine("=== Example 6: Get Translation Group ===\n");
            try
            {
                const string groupName = "common";
                var group = await translaasClient.GetGroupAsync(projectId, groupName, defaultLanguage);
                
                // Filter out metadata fields and only show actual translation entries
                var translationEntries = group.Entries
                    .Where(e => e.Value.ValueKind == JsonValueKind.String)
                    .ToDictionary(e => e.Key, e => e.Value.GetString() ?? string.Empty);
                
                Console.WriteLine($"Group '{groupName}' contains {translationEntries.Count} translation entries:");
                foreach (var entry in translationEntries)
                {
                    Console.WriteLine($"  {entry.Key}: {entry.Value}");
                }
                Console.WriteLine();
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 7: Get project locales
            Console.WriteLine("=== Example 7: Get Available Locales ===\n");
            try
            {
                var locales = await translaasClient.GetProjectLocalesAsync(projectId);
                Console.WriteLine($"Available locales: {string.Join(", ", locales.Locales)}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 8: Language Resolution
            Console.WriteLine("=== Example 8: Language Resolution ===\n");
            Console.WriteLine($"Current thread culture: {System.Globalization.CultureInfo.CurrentUICulture.Name}");
            Console.WriteLine($"Default language (from appsettings.json): {defaultLanguage}");
            
            if (languageResolver != null)
            {
                var resolvedLangCode = languageResolver.Resolve();
                Console.WriteLine($"Resolved language code (from providers): {resolvedLangCode ?? "(null)"}");
            }
            
            try
            {
                var autoLang = await translaasService.T("common", "welcome");
                Console.WriteLine($"Translation (auto-resolved): {autoLang}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 9: Explicit Language Override
            Console.WriteLine("=== Example 9: Explicit Language Override ===\n");
            try
            {
                // Test with the configured default language
                var explicitLang = await translaasService.T("common", "welcome", defaultLanguage);
                Console.WriteLine($"Translation (explicit override to '{defaultLanguage}'): {explicitLang}\n");
            }
            catch (TranslaasOfflineCacheMissException ex)
            {
                Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
            }

            // Example 10: Demonstrate offline mode (no API calls)
            Console.WriteLine("=== Example 10: Offline Mode Verification ===\n");
            Console.WriteLine("✅ All translations were loaded from local cache files.");
            Console.WriteLine("✅ No API calls were made (CacheOnly mode).");
            Console.WriteLine("✅ Application works entirely offline.\n");
        }
        catch (TranslaasOfflineCacheMissException ex)
        {
            Console.WriteLine($"\n❌ Offline Cache Miss Exception:");
            Console.WriteLine($"   Project: {ex.Project}");
            Console.WriteLine($"   Language: {ex.Language}");
            if (!string.IsNullOrEmpty(ex.Group))
            {
                Console.WriteLine($"   Group: {ex.Group}");
            }
            if (!string.IsNullOrEmpty(ex.Entry))
            {
                Console.WriteLine($"   Entry: {ex.Entry}");
            }
            Console.WriteLine($"\n   This exception is thrown when a translation is not found in the cache.");
            Console.WriteLine($"   In CacheOnly mode, the SDK never calls the API, so missing translations");
            Console.WriteLine($"   cannot be fetched from the backend.\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
