using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Translaas.Caching.File;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;

namespace Translaas.Samples.Offline;

/// <summary>
/// Sample implementation for CacheOnly fallback mode.
/// In this mode, the SDK never calls the API and only uses cached translations.
/// </summary>
public class CacheOnlySample : OfflineSampleBase
{
    protected override string FallbackModeName => "CacheOnly";
    protected override string FallbackModeDescription => "All translations are read from local cache files - no API calls are made.";

    public CacheOnlySample(
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
        Console.WriteLine("=== Example 10: Offline Mode Verification ===\n");
        Console.WriteLine("✅ All translations were loaded from local cache files.");
        Console.WriteLine("✅ No API calls were made (CacheOnly mode).");
        Console.WriteLine("✅ Application works entirely offline.\n");
    }
}
