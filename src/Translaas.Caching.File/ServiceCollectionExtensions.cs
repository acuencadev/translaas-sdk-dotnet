using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Translaas.Caching.File;

/// <summary>
/// Extension methods for configuring offline cache services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the offline cache sync hosted service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when required services are not registered.</exception>
    /// <remarks>
    /// <para>
    /// This method registers <see cref="OfflineCacheSyncHostedService"/> as a hosted service.
    /// The hosted service will automatically start background synchronization when the application starts.
    /// </para>
    /// <para>
    /// Prerequisites:
    /// </para>
    /// <list type="bullet">
    /// <item><description><see cref="IOfflineCacheSyncService"/> must be registered (done automatically by AddTranslaas when offline cache is enabled).</description></item>
    /// <item><description><see cref="OfflineCacheOptions"/> must be registered (done automatically by AddTranslaas when offline cache is enabled).</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddTranslaas(options =>
    /// {
    ///     options.ApiKey = "your-api-key";
    ///     options.BaseUrl = "https://api.translaas.com";
    ///     options.OfflineCache.Enabled = true;
    ///     options.OfflineCache.Projects.Add("my-project");
    ///     options.OfflineCache.AutoSync = true;
    ///     options.OfflineCache.AutoSyncInterval = TimeSpan.FromHours(1);
    /// });
    /// 
    /// // Add the hosted service for background sync
    /// services.AddTranslaasOfflineCacheSyncHostedService();
    /// </code>
    /// </example>
    public static IServiceCollection AddTranslaasOfflineCacheSyncHostedService(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Register the hosted service
        services.AddHostedService<OfflineCacheSyncHostedService>();

        return services;
    }

    /// <summary>
    /// Adds the offline cache file provider services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">A delegate to configure the <see cref="OfflineCacheOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configure is null.</exception>
    /// <remarks>
    /// <para>
    /// This method is typically not called directly. Instead, use the main AddTranslaas method
    /// and enable offline caching through <see cref="TranslaasOptions.OfflineCache"/>.
    /// </para>
    /// <para>
    /// This method can be useful for standalone offline cache scenarios where you want to
    /// use the cache provider without the full Translaas client.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Standalone offline cache (without Translaas client)
    /// services.AddTranslaasOfflineCache(options =>
    /// {
    ///     options.Enabled = true;
    ///     options.CacheDirectory = "./my-cache";
    /// });
    /// 
    /// // Use the cache provider directly
    /// var cacheProvider = serviceProvider.GetRequiredService&lt;IOfflineCacheProvider&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddTranslaasOfflineCache(
        this IServiceCollection services,
        Action<OfflineCacheOptions> configure)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var options = new OfflineCacheOptions();
        configure(options);

        // Register OfflineCacheOptions as singleton
        services.TryAddSingleton(options);

        // Register IOfflineCacheProvider as singleton
        services.TryAddSingleton<IOfflineCacheProvider>(serviceProvider =>
        {
            var offlineOptions = serviceProvider.GetRequiredService<OfflineCacheOptions>();
            return new FileCacheProvider(offlineOptions);
        });

        return services;
    }
}
