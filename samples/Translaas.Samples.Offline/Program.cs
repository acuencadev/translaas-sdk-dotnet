using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Translaas.Caching.File;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;
using L = Translaas.Models.LanguageCodes;

namespace Translaas.Samples.Offline;

/// <summary>
/// Console application demonstrating Translaas SDK usage in offline modes.
/// Supports three fallback modes: CacheOnly, CacheFirst, and ApiFirst.
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

        // Determine which mode to run
        OfflineFallbackMode selectedMode = SelectFallbackMode(args);

        // Build the host with dependency injection
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                // Suppress HTTP client logging for cleaner console output
                logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
            })
            .ConfigureServices((context, services) =>
            {
                // Add HttpClient support (required for Translaas)
                services.AddHttpClient();

                // Configure Translaas with the selected offline cache mode
                var configuration = context.Configuration;
                
                services.AddTranslaas(options =>
                {
                    // Configure offline cache
                    options.OfflineCache.Enabled = true;
                    options.OfflineCache.CacheDirectory = configuration["Translaas:OfflineCache:CacheDirectory"] ?? "./cache";
                    options.OfflineCache.FallbackMode = selectedMode;
                    options.OfflineCache.AutoSync = false; // Disable sync to prevent background sync
                    options.OfflineCache.DefaultProjectId = configuration["Translaas:OfflineCache:DefaultProjectId"] ?? "translaas-sdk-samples";

                    // Configure API settings (required for CacheFirst and ApiFirst modes)
                    if (selectedMode != OfflineFallbackMode.CacheOnly)
                    {
                        // ApiKey and BaseUrl are required for CacheFirst and ApiFirst modes
                        var apiKey = configuration["Translaas:ApiKey"];
                        var baseUrl = configuration["Translaas:BaseUrl"];
                        
                        if (string.IsNullOrWhiteSpace(apiKey))
                        {
                            throw new InvalidOperationException(
                                $"ApiKey is required for {selectedMode} mode. " +
                                "Please set 'Translaas:ApiKey' in appsettings.json or user secrets.");
                        }
                        
                        if (string.IsNullOrWhiteSpace(baseUrl))
                        {
                            throw new InvalidOperationException(
                                $"BaseUrl is required for {selectedMode} mode. " +
                                "Please set 'Translaas:BaseUrl' in appsettings.json.");
                        }
                        
                        options.ApiKey = apiKey;
                        options.BaseUrl = baseUrl;
                    }
                    // In CacheOnly mode, ApiKey and BaseUrl are optional and won't be validated

                    // Optional: Set default language fallback
                    options.DefaultLanguage = configuration["Translaas:DefaultLanguage"] ?? L.English;
                }, language =>
                {
                    // Configure language resolution providers for console apps
                    // For offline mode, prioritize DefaultLanguage over thread culture
                    language
                        .UseDefault(); // Resolves from DefaultLanguage option (appsettings.json)
                    // Note: UseCulture() is intentionally omitted for offline mode to ensure
                    // the configured DefaultLanguage is always used, regardless of thread culture
                });
            })
            .Build();

        // Get services from DI container
        var translaasService = host.Services.GetRequiredService<ITranslaasService>();
        var translaasClient = host.Services.GetRequiredService<ITranslaasClient>();
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var cacheProvider = host.Services.GetService<IOfflineCacheProvider>();
        var languageResolver = host.Services.GetService<ILanguageResolver>();

        // Create and run the appropriate sample
        OfflineSampleBase sample = selectedMode switch
        {
            OfflineFallbackMode.CacheOnly => new CacheOnlySample(
                translaasService, translaasClient, configuration, cacheProvider, languageResolver),
            OfflineFallbackMode.CacheFirst => new CacheFirstSample(
                translaasService, translaasClient, configuration, cacheProvider, languageResolver),
            OfflineFallbackMode.ApiFirst => new ApiFirstSample(
                translaasService, translaasClient, configuration, cacheProvider, languageResolver),
            _ => throw new InvalidOperationException($"Unsupported fallback mode: {selectedMode}")
        };

        await sample.RunAsync();

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    /// <summary>
    /// Selects the fallback mode to use, either from command line arguments or interactive menu.
    /// </summary>
    private static OfflineFallbackMode SelectFallbackMode(string[] args)
    {
        // Check if mode is specified via command line argument
        if (args.Length > 0)
        {
            var modeArg = args[0].ToLowerInvariant();
            return modeArg switch
            {
                "cacheonly" or "cache-only" or "1" => OfflineFallbackMode.CacheOnly,
                "cachefirst" or "cache-first" or "2" => OfflineFallbackMode.CacheFirst,
                "apifirst" or "api-first" or "3" => OfflineFallbackMode.ApiFirst,
                _ => ShowInteractiveMenu()
            };
        }

        // Show interactive menu if no argument provided
        return ShowInteractiveMenu();
    }

    /// <summary>
    /// Shows an interactive menu for selecting the fallback mode.
    /// </summary>
    private static OfflineFallbackMode ShowInteractiveMenu()
    {
        Console.WriteLine("=== Translaas SDK Offline Mode Sample ===\n");
        Console.WriteLine("Select a fallback mode to test:\n");
        Console.WriteLine("  1. CacheOnly - Only use cache, never call API (fully offline)");
        Console.WriteLine("  2. CacheFirst - Check cache first, fall back to API on miss");
        Console.WriteLine("  3. ApiFirst - Call API first, fall back to cache on API failure\n");
        Console.Write("Enter your choice (1-3): ");

        while (true)
        {
            var input = Console.ReadLine()?.Trim();
            
            switch (input)
            {
                case "1":
                    return OfflineFallbackMode.CacheOnly;
                case "2":
                    return OfflineFallbackMode.CacheFirst;
                case "3":
                    return OfflineFallbackMode.ApiFirst;
                default:
                    Console.Write("Invalid choice. Please enter 1, 2, or 3: ");
                    break;
            }
        }
    }
}
