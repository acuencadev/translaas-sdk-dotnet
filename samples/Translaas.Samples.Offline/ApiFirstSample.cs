using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Translaas.Caching.File;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;
using Translaas.Models.Errors;

namespace Translaas.Samples.Offline;

/// <summary>
/// Sample implementation for ApiFirst fallback mode.
/// In this mode, the SDK calls the API first, then falls back to cache if API fails.
/// Cache is updated in the background when successful API calls are made.
/// </summary>
public class ApiFirstSample : OfflineSampleBase
{
    protected override string FallbackModeName => "ApiFirst";
    protected override string FallbackModeDescription => "Calls API first, falls back to cache if API fails. Cache is updated in background when API calls succeed.";

    private readonly IServiceProvider _serviceProvider;

    public ApiFirstSample(
        ITranslaasService translaasService,
        ITranslaasClient translaasClient,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        IOfflineCacheProvider? cacheProvider = null,
        ILanguageResolver? languageResolver = null)
        : base(translaasService, translaasClient, configuration, cacheProvider, languageResolver)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    protected override async Task Example1_BasicTranslation()
    {
        Console.WriteLine("=== Example 1: Basic Translation ===\n");
        try
        {
            var translation1 = await TranslaasService.T("common", "welcome");
            Console.WriteLine($"Translation (group: 'common', entry: 'welcome'): {translation1}");
            Console.WriteLine("ℹ️  Note: In ApiFirst mode, this value comes from the API if the call succeeded.\n");
        }
        catch (TranslaasOfflineCacheMissException ex)
        {
            Console.WriteLine($"❌ Cache miss: {ex.Message}");
            Console.WriteLine("⚠️  API call failed and no cache available for this entry.\n");
        }
        catch (TranslaasApiException ex)
        {
            Console.WriteLine($"❌ API Error: {ex.Message}");
            Console.WriteLine("⚠️  API call failed. Check your API key, BaseUrl, and network connectivity.\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"⚠️  This might indicate an API connectivity issue.\n");
        }
    }

    protected override async Task Example5_MultipleEntries()
    {
        Console.WriteLine("=== Example 5: Multiple Entries ===\n");
        try
        {
            var appName = await TranslaasService.T("common", "app.name");
            var welcome = await TranslaasService.T("common", "welcome");
            var welcomeMessage = await TranslaasService.T("common", "welcome.message");
            Console.WriteLine($"App Name: {appName}");
            Console.WriteLine($"Welcome: {welcome}");
            Console.WriteLine($"Welcome Message: {welcomeMessage}");
            Console.WriteLine("ℹ️  Note: In ApiFirst mode, these values come from the API if calls succeeded.\n");
        }
        catch (TranslaasOfflineCacheMissException ex)
        {
            Console.WriteLine($"❌ Cache miss: {ex.Message}\n");
        }
        catch (TranslaasApiException ex)
        {
            Console.WriteLine($"❌ API Error: {ex.Message}");
            Console.WriteLine("⚠️  API call failed. Check your API key, BaseUrl, and network connectivity.\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}\n");
        }
    }

    protected override async Task VerifyCacheFilesAsync(string projectId)
    {
        await base.VerifyCacheFilesAsync(projectId);
        
        // Additional diagnostic: Test API connectivity
        Console.WriteLine("=== Testing API Connectivity ===\n");
        
        // Show cache timestamp
        if (CacheProvider != null)
        {
            try
            {
                var manifest = await CacheProvider.GetManifestAsync();
                if (manifest.Projects.TryGetValue(projectId, out var projectInfo))
                {
                    Console.WriteLine($"Cache Last Sync: {projectInfo.LastSyncAt:yyyy-MM-dd HH:mm:ss UTC}");
                    Console.WriteLine($"Manifest Last Sync: {manifest.LastSyncAt:yyyy-MM-dd HH:mm:ss UTC}");
                }
                
                // Also check the cached project file timestamp
                var cachedProject = await CacheProvider.GetProjectAsync(projectId, DefaultLanguage);
                if (cachedProject != null)
                {
                    // Note: CachedProject doesn't expose CachedAt directly, but we can check manifest
                    Console.WriteLine($"Cache file exists for project '{projectId}' and language '{DefaultLanguage}'");
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Could not read cache manifest: {ex.Message}\n");
            }
        }
        
        try
        {
            // Create a direct API client (bypassing cache wrapper) to test actual API response
            Console.WriteLine("Calling API directly (bypassing cache wrapper) for 'common.app.name'...");
            Console.WriteLine($"   Endpoint: /api/translations/text");
            Console.WriteLine($"   Request: {{ group: 'common', entry: 'app.name', lang: '{DefaultLanguage}' }}");
            var directApiClient = CreateDirectApiClient();
            var apiResult = await directApiClient.GetEntryAsync("common", "app.name", DefaultLanguage);
            Console.WriteLine($"✅ Direct API call succeeded!");
            Console.WriteLine($"   API returned: '{apiResult}'");
            Console.WriteLine($"   Length: {apiResult.Length} characters\n");
            
            // Now test through the caching wrapper
            Console.WriteLine("Calling API through caching wrapper for 'common.app.name'...");
            var cachedWrapperResult = await TranslaasClient.GetEntryAsync("common", "app.name", DefaultLanguage);
            Console.WriteLine($"   Caching wrapper returned: '{cachedWrapperResult}'");
            Console.WriteLine($"   Length: {cachedWrapperResult.Length} characters");
            
            if (apiResult != cachedWrapperResult)
            {
                Console.WriteLine($"   ⚠️  MISMATCH: Direct API returned '{apiResult}' but caching wrapper returned '{cachedWrapperResult}'");
                Console.WriteLine($"   This indicates the caching wrapper is returning cached values instead of API values!\n");
            }
            else
            {
                Console.WriteLine($"   ✅ Both return the same value.\n");
            }
            
            // Compare with cache
            var cachedGroup = await CacheProvider?.GetGroupAsync(projectId, "common", DefaultLanguage);
            var cachedValue = cachedGroup?.GetValue("app.name");
            
            if (cachedValue != null)
            {
                Console.WriteLine($"Cache value: '{cachedValue}'");
                Console.WriteLine($"Cache length: {cachedValue.Length} characters\n");
                
                if (cachedValue == apiResult)
                {
                    Console.WriteLine($"⚠️  Cache value matches API - both show: '{cachedValue}'");
                    Console.WriteLine($"   If you changed the value in the API/DB, this suggests:");
                    Console.WriteLine($"   - The API endpoint may not have the updated value yet");
                    Console.WriteLine($"   - HTTP client may be caching the response");
                    Console.WriteLine($"   - The cache was already updated with the new value\n");
                }
                else
                {
                    Console.WriteLine($"✅ Cache value differs from API!");
                    Console.WriteLine($"   Cache: '{cachedValue}' ({cachedValue.Length} chars)");
                    Console.WriteLine($"   API:   '{apiResult}' ({apiResult.Length} chars)");
                    Console.WriteLine($"   The API has a different value - cache will be updated in background.\n");
                }
            }
            else
            {
                Console.WriteLine($"   Cache doesn't have this entry yet.\n");
            }
        }
        catch (TranslaasApiException ex)
        {
            Console.WriteLine($"❌ API call failed with TranslaasApiException:");
            Console.WriteLine($"   Status Code: {ex.StatusCode}");
            Console.WriteLine($"   Message: {ex.Message}");
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"   ⚠️  NotFound (404) suggests:");
                Console.WriteLine($"      - The API endpoint might require a 'project' parameter");
                Console.WriteLine($"      - The endpoint path might be incorrect");
                Console.WriteLine($"      - The entry might not exist in the API");
                Console.WriteLine($"   Check your Postman request - does it include a 'project' parameter?\n");
            }
            else
            {
                Console.WriteLine($"   ⚠️  All requests will fall back to cache.\n");
            }
            
            // Test if GetGroupAsync works (which includes project parameter)
            try
            {
                Console.WriteLine("Testing GetGroupAsync (which includes project parameter)...");
                var groupResult = await CreateDirectApiClient().GetGroupAsync(DefaultProjectId, "common", DefaultLanguage);
                var groupAppName = groupResult.GetValue("app.name");
                Console.WriteLine($"✅ GetGroupAsync succeeded!");
                Console.WriteLine($"   Group 'common' contains 'app.name': '{groupAppName}'");
                Console.WriteLine($"   This suggests GetEntryAsync might need a project parameter.\n");
            }
            catch (Exception groupEx)
            {
                Console.WriteLine($"❌ GetGroupAsync also failed: {groupEx.Message}\n");
            }
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            Console.WriteLine($"❌ API call failed with HttpRequestException:");
            Console.WriteLine($"   Message: {ex.Message}");
            Console.WriteLine($"   ⚠️  Network error - check your BaseUrl and network connectivity.");
            Console.WriteLine($"   ⚠️  All requests will fall back to cache.\n");
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"❌ API call timed out:");
            Console.WriteLine($"   Message: {ex.Message}");
            Console.WriteLine($"   ⚠️  Check your network connection and API timeout settings.");
            Console.WriteLine($"   ⚠️  All requests will fall back to cache.\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ API call failed with unexpected error:");
            Console.WriteLine($"   Type: {ex.GetType().Name}");
            Console.WriteLine($"   Message: {ex.Message}");
            Console.WriteLine($"   ⚠️  All requests will fall back to cache.\n");
        }
    }

    protected override async Task Example10_OfflineModeVerification()
    {
        Console.WriteLine("=== Example 10: ApiFirst Mode Verification ===\n");
        Console.WriteLine("✅ ApiFirst mode: Calls API first, then cache if API fails.");
        Console.WriteLine("✅ Cache is updated in background when API calls succeed.");
        Console.WriteLine("✅ Provides fresh translations while maintaining offline capability.");
        Console.WriteLine("\n⚠️  IMPORTANT: If you see cached values instead of API values:");
        Console.WriteLine("   - Check the 'Testing API Connectivity' section above for API errors");
        Console.WriteLine("   - Verify your API key and BaseUrl are correct");
            Console.WriteLine("   - Ensure the API has the updated translations\n");
    }

    /// <summary>
    /// Creates a direct API client that bypasses the caching wrapper for diagnostic purposes.
    /// </summary>
    private ITranslaasClient CreateDirectApiClient()
    {
        // Get HttpClientFactory
        var httpClientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(ITranslaasClient));

        // Get options
        var optionsMonitor = _serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        var translaasOptions = optionsMonitor.Value;

        // Convert to TranslaasClientOptions
        var clientOptions = new TranslaasClientOptions
        {
            ApiKey = translaasOptions.ApiKey,
            BaseUrl = translaasOptions.BaseUrl,
            Timeout = translaasOptions.Timeout
        };

        // Create direct client (no caching wrapper)
        return new TranslaasClient(httpClient, clientOptions);
    }
}
