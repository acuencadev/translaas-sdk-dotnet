using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Translaas.Caching.File;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;
using Translaas.Models.Errors;
using Translaas.Models.Responses;
using L = Translaas.Models.LanguageCodes;

namespace Translaas.Samples.Offline;

/// <summary>
/// Base class containing shared example logic for all offline fallback modes.
/// </summary>
public abstract class OfflineSampleBase
{
    protected readonly ITranslaasService TranslaasService;
    protected readonly ITranslaasClient TranslaasClient;
    protected readonly IConfiguration Configuration;
    protected readonly string DefaultLanguage;
    protected readonly string CacheDirectory;
    protected readonly string DefaultProjectId;
    protected readonly IOfflineCacheProvider? CacheProvider;
    protected readonly ILanguageResolver? LanguageResolver;

    protected OfflineSampleBase(
        ITranslaasService translaasService,
        ITranslaasClient translaasClient,
        IConfiguration configuration,
        IOfflineCacheProvider? cacheProvider = null,
        ILanguageResolver? languageResolver = null)
    {
        TranslaasService = translaasService ?? throw new ArgumentNullException(nameof(translaasService));
        TranslaasClient = translaasClient ?? throw new ArgumentNullException(nameof(translaasClient));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        CacheProvider = cacheProvider;
        LanguageResolver = languageResolver;

        DefaultLanguage = configuration["Translaas:DefaultLanguage"] ?? L.English;
        CacheDirectory = configuration["Translaas:OfflineCache:CacheDirectory"] ?? "./cache";
        DefaultProjectId = configuration["Translaas:OfflineCache:DefaultProjectId"] ?? "translaas-sdk-samples";
    }

    /// <summary>
    /// Gets the fallback mode name for display purposes.
    /// </summary>
    protected abstract string FallbackModeName { get; }

    /// <summary>
    /// Gets a description of the fallback mode behavior.
    /// </summary>
    protected abstract string FallbackModeDescription { get; }

    /// <summary>
    /// Runs all examples for this fallback mode.
    /// </summary>
    public virtual async Task RunAsync()
    {
        Console.WriteLine($"=== Translaas SDK Offline Mode Sample ===\n");
        Console.WriteLine($"This sample demonstrates offline mode using {FallbackModeName} fallback mode.");
        Console.WriteLine($"{FallbackModeDescription}\n");

        Console.WriteLine($"Configuration:");
        Console.WriteLine($"  Cache Directory: {CacheDirectory}");
        Console.WriteLine($"  Default Project ID: {DefaultProjectId}");
        Console.WriteLine($"  Fallback Mode: {FallbackModeName}");
        Console.WriteLine($"  Default Language: {DefaultLanguage}");
        
        // Show API configuration for modes that require it
        var apiKey = Configuration["Translaas:ApiKey"];
        var baseUrl = Configuration["Translaas:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(apiKey) || !string.IsNullOrWhiteSpace(baseUrl))
        {
            Console.WriteLine($"  API Base URL: {baseUrl ?? "(not set)"}");
            Console.WriteLine($"  API Key: {(string.IsNullOrWhiteSpace(apiKey) ? "(not set)" : new string('*', Math.Min(apiKey.Length, 8)) + "...")}");
        }
        
        Console.WriteLine();

        try
        {
            var projectId = DefaultProjectId;

            // Verify cache files exist
            await VerifyCacheFilesAsync(projectId);

            // Run all examples
            await Example1_BasicTranslation();
            await Example2_Pluralization();
            await Example3_NamedParameters();
            await Example4_NumberAndNamedParameters();
            await Example5_MultipleEntries();
            await Example6_GetTranslationGroup(projectId);
            await Example7_GetAvailableLocales(projectId);
            await Example8_LanguageResolution();
            await Example9_ExplicitLanguageOverride();
            await Example10_OfflineModeVerification();
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
            Console.WriteLine($"   In {FallbackModeName} mode, missing translations may or may not be fetched");
            Console.WriteLine($"   from the API depending on the fallback mode configuration.\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    protected virtual async Task VerifyCacheFilesAsync(string projectId)
    {
        Console.WriteLine("=== Verifying Cache Files ===\n");
        
        if (CacheProvider != null)
        {
            var isCached = await CacheProvider.IsCachedAsync(projectId, DefaultLanguage);
            
            if (!isCached)
            {
                Console.WriteLine($"⚠️  WARNING: Cache file not found for project '{projectId}' and language '{DefaultLanguage}'");
                Console.WriteLine($"   Expected location: {CacheDirectory}/{projectId}/{DefaultLanguage}/project.json");
                Console.WriteLine($"   Please ensure cache files are present before running this sample.\n");
            }
            else
            {
                Console.WriteLine($"✅ Cache file found for project '{projectId}' and language '{DefaultLanguage}'\n");
            }
        }
        else
        {
            Console.WriteLine($"ℹ️  Cache provider not available - skipping cache verification\n");
        }
    }

    protected virtual async Task Example1_BasicTranslation()
    {
        Console.WriteLine("=== Example 1: Basic Translation ===\n");
        try
        {
            var translation1 = await TranslaasService.T("common", "welcome");
            Console.WriteLine($"Translation (group: 'common', entry: 'welcome'): {translation1}\n");
        }
        catch (TranslaasOfflineCacheMissException ex)
        {
            Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
        }
    }

    protected virtual async Task Example2_Pluralization()
    {
        Console.WriteLine("=== Example 2: Pluralization ===\n");
        try
        {
            var translation2a = await TranslaasService.T("messages", "item", 1);
            var translation2b = await TranslaasService.T("messages", "item", 5);
            Console.WriteLine($"1 item: {translation2a}");
            Console.WriteLine($"5 items: {translation2b}\n");
        }
        catch (TranslaasOfflineCacheMissException ex)
        {
            Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
        }
    }

    protected virtual async Task Example3_NamedParameters()
    {
        Console.WriteLine("=== Example 3: Named Parameters ===\n");
        try
        {
            var parameters = new Dictionary<string, string>
            {
                { "userName", "John" },
                { "itemCount", "5" }
            };
            var translation3 = await TranslaasService.T("messages", "greeting", parameters);
            Console.WriteLine($"Translation with parameters: {translation3}\n");
        }
        catch (TranslaasOfflineCacheMissException ex)
        {
            Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
        }
    }

    protected virtual async Task Example4_NumberAndNamedParameters()
    {
        Console.WriteLine("=== Example 4: Number + Named Parameters ===\n");
        try
        {
            var parameters = new Dictionary<string, string>
            {
                { "userName", "John" }
            };
            var translation4 = await TranslaasService.T("messages", "items", 5, parameters);
            Console.WriteLine($"Translation with number and parameters: {translation4}\n");
        }
        catch (TranslaasOfflineCacheMissException ex)
        {
            Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
        }
    }

    protected virtual async Task Example5_MultipleEntries()
    {
        Console.WriteLine("=== Example 5: Multiple Entries ===\n");
        try
        {
            var appName = await TranslaasService.T("common", "app.name");
            var welcome = await TranslaasService.T("common", "welcome");
            var welcomeMessage = await TranslaasService.T("common", "welcome.message");
            Console.WriteLine($"App Name: {appName}");
            Console.WriteLine($"Welcome: {welcome}");
            Console.WriteLine($"Welcome Message: {welcomeMessage}\n");
        }
        catch (TranslaasOfflineCacheMissException ex)
        {
            Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
        }
    }

    protected virtual async Task Example6_GetTranslationGroup(string projectId)
    {
        Console.WriteLine("=== Example 6: Get Translation Group ===\n");
        try
        {
            const string groupName = "common";
            var group = await TranslaasClient.GetGroupAsync(projectId, groupName, DefaultLanguage);
            
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
    }

    protected virtual async Task Example7_GetAvailableLocales(string projectId)
    {
        Console.WriteLine("=== Example 7: Get Available Locales ===\n");
        try
        {
            var locales = await TranslaasClient.GetProjectLocalesAsync(projectId);
            Console.WriteLine($"Available locales: {string.Join(", ", locales.Locales)}\n");
        }
        catch (TranslaasOfflineCacheMissException ex)
        {
            Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
        }
    }

    protected virtual async Task Example8_LanguageResolution()
    {
        Console.WriteLine("=== Example 8: Language Resolution ===\n");
        Console.WriteLine($"Current thread culture: {System.Globalization.CultureInfo.CurrentUICulture.Name}");
        Console.WriteLine($"Default language (from appsettings.json): {DefaultLanguage}");
        
        if (LanguageResolver != null)
        {
            var resolvedLangCode = LanguageResolver.Resolve();
            Console.WriteLine($"Resolved language code (from providers): {resolvedLangCode ?? "(null)"}");
        }
        
        try
        {
            var autoLang = await TranslaasService.T("common", "welcome");
            Console.WriteLine($"Translation (auto-resolved): {autoLang}\n");
        }
        catch (TranslaasOfflineCacheMissException ex)
        {
            Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
        }
    }

    protected virtual async Task Example9_ExplicitLanguageOverride()
    {
        Console.WriteLine("=== Example 9: Explicit Language Override ===\n");
        try
        {
            // Test with the configured default language
            var explicitLang = await TranslaasService.T("common", "welcome", DefaultLanguage);
            Console.WriteLine($"Translation (explicit override to '{DefaultLanguage}'): {explicitLang}\n");
        }
        catch (TranslaasOfflineCacheMissException ex)
        {
            Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
        }
    }

    protected virtual async Task Example10_OfflineModeVerification()
    {
        Console.WriteLine("=== Example 10: Offline Mode Verification ===\n");
        Console.WriteLine($"✅ All translations were processed using {FallbackModeName} mode.");
        Console.WriteLine($"✅ Mode behavior: {FallbackModeDescription}\n");
    }
}
