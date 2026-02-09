using Microsoft.Extensions.Configuration;
using Translaas.Caching.File;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;

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

    public ApiFirstSample(
        ITranslaasService translaasService,
        ITranslaasClient translaasClient,
        IConfiguration configuration,
        IOfflineCacheProvider? cacheProvider = null,
        ILanguageResolver? languageResolver = null)
        : base(translaasService, translaasClient, configuration, cacheProvider, languageResolver)
    {
    }

    protected override async Task Example10_OfflineModeVerification()
    {
        Console.WriteLine("=== Example 10: ApiFirst Mode Verification ===\n");
        Console.WriteLine("✅ ApiFirst mode: Calls API first, then cache if API fails.");
        Console.WriteLine("✅ Cache is updated in background when API calls succeed.");
        Console.WriteLine("✅ Provides fresh translations while maintaining offline capability.\n");
    }
}
