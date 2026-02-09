using Microsoft.Extensions.Configuration;
using Translaas.Caching.File;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;

namespace Translaas.Samples.Offline;

/// <summary>
/// Sample implementation for CacheFirst fallback mode.
/// In this mode, the SDK checks cache first, then falls back to API if cache miss.
/// Cache is updated in the background when API calls are made.
/// </summary>
public class CacheFirstSample : OfflineSampleBase
{
    protected override string FallbackModeName => "CacheFirst";
    protected override string FallbackModeDescription => "Checks cache first, falls back to API on cache miss. Cache is updated in background when API calls are made.";

    public CacheFirstSample(
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
        Console.WriteLine("=== Example 10: CacheFirst Mode Verification ===\n");
        Console.WriteLine("✅ CacheFirst mode: Checks cache first, then API if needed.");
        Console.WriteLine("✅ Cache is updated in background when API calls are made.");
        Console.WriteLine("✅ Subsequent requests for the same translations will use cache.\n");
    }
}
