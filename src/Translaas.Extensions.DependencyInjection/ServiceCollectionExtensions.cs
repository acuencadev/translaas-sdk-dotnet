using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

#if !NETSTANDARD2_0
using Microsoft.Extensions.Http;
#endif

using System;
using System.Linq;
using System.Net.Http;

using Translaas.Caching;
using Translaas.Caching.File;
using Translaas.Client;
using Translaas.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Translaas.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring Translaas services with dependency injection.
/// </summary>
/// <remarks>
/// <para>
/// Service lifetimes:
/// </para>
/// <list type="bullet">
/// <item>
/// <description><see cref="ITranslaasClient"/> - Registered as <see cref="ServiceLifetime.Scoped"/>. A new instance is created for each service scope (e.g., per HTTP request in ASP.NET Core).</description>
/// </item>
/// <item>
/// <description><see cref="ITranslaasService"/> - Registered as <see cref="ServiceLifetime.Scoped"/>. A convenience wrapper around <see cref="ITranslaasClient"/> with a simplified API.</description>
/// </item>
/// <item>
/// <description><see cref="IOptions{TranslaasOptions}"/> - Registered as <see cref="ServiceLifetime.Singleton"/> via the Options pattern. Configuration is loaded once at application startup.</description>
/// </item>
/// <item>
/// <description><see cref="IMemoryCache"/> - Registered as <see cref="ServiceLifetime.Singleton"/> when caching is enabled. A single cache instance is shared across the application.</description>
/// </item>
/// <item>
/// <description><see cref="ITranslaasCacheProvider"/> - Registered as <see cref="ServiceLifetime.Singleton"/> when caching is enabled. A single cache provider instance wraps the memory cache.</description>
/// </item>
/// </list>
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Translaas services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">A delegate to configure the <see cref="TranslaasOptions"/>.</param>
    /// <param name="configureLanguage">
    /// Optional configuration for language resolution.
    /// When null or empty, explicit lang parameter is required on all calls.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configure is null.</exception>
    /// <example>
    /// <code>
    /// services.AddHttpClient();
    /// services.AddTranslaas(options =>
    /// {
    ///     options.ApiKey = "your-api-key";
    ///     options.BaseUrl = "https://api.translaas.com";
    ///     options.CacheMode = CacheMode.Group;
    ///     options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);
    ///     options.DefaultLanguage = LanguageCodes.English;
    /// }, language => language
    ///     .UseCulture()
    ///     .UseDefault());
    /// </code>
    /// </example>
    public static IServiceCollection AddTranslaas(
        this IServiceCollection services,
        Action<TranslaasOptions> configure,
        Action<ITranslaasLanguageBuilder>? configureLanguage = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        // Register TranslaasOptions using Options pattern
        services.Configure(configure);

        // Register HttpClient via HttpClientFactory
        services.AddTranslaasHttpClient(options =>
        {
            // Get configured options to convert to TranslaasClientOptions
            var translaasOptions = new TranslaasOptions();
            configure(translaasOptions);

            options.ApiKey = translaasOptions.ApiKey;
            options.BaseUrl = translaasOptions.BaseUrl;
            options.Timeout = translaasOptions.Timeout;
        });

        // Register caching services if caching is enabled
        var tempOptions = new TranslaasOptions();
        configure(tempOptions);

        if (tempOptions.CacheMode != CacheMode.None)
        {
            // Register IMemoryCache if not already registered
            if (!services.Any(s => s.ServiceType == typeof(IMemoryCache)))
            {
                services.AddMemoryCache();
            }

            // Register ITranslaasCacheProvider as singleton
            services.AddSingleton<ITranslaasCacheProvider>(serviceProvider =>
            {
                var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
                return new MemoryCacheProvider(memoryCache);
            });
        }

        // Register offline caching services if enabled
        if (tempOptions.OfflineCache.Enabled)
        {
            // Validate offline cache configuration
            var defaultProjectId = tempOptions.OfflineCache.DefaultProjectId
                ?? (tempOptions.OfflineCache.Projects.Count > 0 ? tempOptions.OfflineCache.Projects[0] : null);

            if (string.IsNullOrWhiteSpace(defaultProjectId))
            {
                throw new InvalidOperationException(
                    "Offline cache is enabled but no DefaultProjectId or Projects are configured. " +
                    "Set OfflineCache.DefaultProjectId or add at least one project to OfflineCache.Projects.");
            }

            // Update the options with the resolved default project ID
            tempOptions.OfflineCache.DefaultProjectId = defaultProjectId;

            // Register OfflineCacheOptions as singleton
            services.AddSingleton(tempOptions.OfflineCache);

            // Register HybridCacheOptions as singleton
            services.AddSingleton(tempOptions.OfflineCache.HybridCache);

            // Register IOfflineCacheProvider as singleton
            // Use HybridCacheProvider if hybrid caching is enabled, otherwise use FileCacheProvider
            if (tempOptions.OfflineCache.HybridCache.Enabled)
            {
                // Register file cache provider for internal use
                services.AddSingleton<FileCacheProvider>(serviceProvider =>
                {
                    var offlineOptions = serviceProvider.GetRequiredService<OfflineCacheOptions>();
                    return new FileCacheProvider(offlineOptions);
                });

                // Register hybrid cache provider as IOfflineCacheProvider
                services.AddSingleton<IOfflineCacheProvider>(serviceProvider =>
                {
                    var fileCache = serviceProvider.GetRequiredService<FileCacheProvider>();
                    var hybridOptions = serviceProvider.GetRequiredService<HybridCacheOptions>();
                    return new HybridCacheProvider(fileCache, hybridOptions);
                });
            }
            else
            {
                // Use file cache only
                services.AddSingleton<IOfflineCacheProvider>(serviceProvider =>
                {
                    var offlineOptions = serviceProvider.GetRequiredService<OfflineCacheOptions>();
                    return new FileCacheProvider(offlineOptions);
                });
            }
        }

        // Register ITranslaasClient as scoped
        services.AddScoped<ITranslaasClient>(serviceProvider =>
        {
            // Create the inner HTTP client
            var innerClient = CreateInnerClient(serviceProvider);

            // If offline caching is enabled, wrap with CachingTranslaasClient
            var optionsMonitor = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
            var translaasOptions = optionsMonitor.Value;

            if (translaasOptions.OfflineCache.Enabled)
            {
                var cacheProvider = serviceProvider.GetRequiredService<IOfflineCacheProvider>();
                var offlineOptions = serviceProvider.GetRequiredService<OfflineCacheOptions>();
                return new CachingTranslaasClient(innerClient, cacheProvider, offlineOptions, offlineOptions.DefaultProjectId!);
            }

            return innerClient;
        });

        // Configure language resolution if provided
        if (configureLanguage != null)
        {
            var languageBuilder = new TranslaasLanguageBuilder(services);
            configureLanguage(languageBuilder);

            // Replace this block inside AddTranslaas:
            services.AddScoped<ILanguageResolver>(serviceProvider =>
            {
                var providers = serviceProvider.GetServices<ILanguageProvider>();
                var loggerFactory = serviceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
                var logger = loggerFactory?.CreateLogger<LanguageResolver>();
                return new LanguageResolver(providers, logger);
            });
        }

        // Register ITranslaasService as scoped (convenience wrapper)
        services.AddScoped<ITranslaasService>(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<ITranslaasClient>();
            var resolver = serviceProvider.GetService<ILanguageResolver>();
            return new TranslaasService(client, resolver);
        });

        // Register IOfflineCacheSyncService if offline caching is enabled
        if (tempOptions.OfflineCache.Enabled)
        {
            services.AddSingleton<IOfflineCacheSyncService>(serviceProvider =>
            {
                // For sync service, we need the inner client without caching wrapper
                var innerClient = CreateInnerClient(serviceProvider);
                var cacheProvider = serviceProvider.GetRequiredService<IOfflineCacheProvider>();
                var offlineOptions = serviceProvider.GetRequiredService<OfflineCacheOptions>();
                return new OfflineCacheSyncService(innerClient, cacheProvider, offlineOptions);
            });
        }

        return services;
    }

    private static TranslaasClient CreateInnerClient(IServiceProvider serviceProvider)
    {
#if NETSTANDARD2_0
        // For netstandard2.0, IHttpClientFactory might not be available
        // Use dynamic resolution
        var httpClientFactoryType = Type.GetType("Microsoft.Extensions.Http.IHttpClientFactory, Microsoft.Extensions.Http");
        if (httpClientFactoryType == null)
        {
            throw new InvalidOperationException("IHttpClientFactory is not available. Ensure Microsoft.Extensions.Http package is referenced.");
        }
        var httpClientFactory = serviceProvider.GetService(httpClientFactoryType);
        if (httpClientFactory == null)
        {
            throw new InvalidOperationException("IHttpClientFactory is not registered. Call services.AddHttpClient() first.");
        }
        var createClientMethod = httpClientFactoryType.GetMethod("CreateClient", new[] { typeof(string) });
        var httpClient = (HttpClient)createClientMethod!.Invoke(httpClientFactory, new object[] { nameof(ITranslaasClient) })!;
#else
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(ITranslaasClient));
#endif
        var optionsMonitor = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        var translaasOptions = optionsMonitor.Value;

        // Convert TranslaasOptions to TranslaasClientOptions
        var clientOptions = new TranslaasClientOptions
        {
            ApiKey = translaasOptions.ApiKey,
            BaseUrl = translaasOptions.BaseUrl,
            Timeout = translaasOptions.Timeout
        };

        return new TranslaasClient(httpClient, clientOptions);
    }

    /// <summary>
    /// Adds Translaas services to the specified <see cref="IServiceCollection"/> using configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance.</param>
    /// <param name="sectionName">Optional configuration section name. Defaults to "Translaas".</param>
    /// <param name="configureLanguage">
    /// Optional configuration for language resolution.
    /// When null or empty, explicit lang parameter is required on all calls.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null.</exception>
    /// <example>
    /// <code>
    /// // appsettings.json:
    /// // {
    /// //   "Translaas": {
    /// //     "ApiKey": "your-api-key",
    /// //     "BaseUrl": "https://api.translaas.com",
    /// //     "CacheMode": "Group",
    /// //     "Timeout": "00:00:30",
    /// //     "DefaultLanguage": "en"
    /// //   }
    /// // }
    /// 
    /// services.AddHttpClient();
    /// services.AddTranslaas(configuration, configureLanguage: language => language
    ///     .UseCulture()
    ///     .UseDefault());
    /// </code>
    /// </example>
    public static IServiceCollection AddTranslaas(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "Translaas",
        Action<ITranslaasLanguageBuilder>? configureLanguage = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (string.IsNullOrWhiteSpace(sectionName))
        {
            throw new ArgumentException("Section name cannot be null or whitespace.", nameof(sectionName));
        }

        // Bind configuration section to TranslaasOptions
        var configurationSection = configuration.GetSection(sectionName);
        
        return services.AddTranslaas(options =>
        {
            configurationSection.Bind(options);
            
            // Validate required properties
            // Check if ApiKey was explicitly set in configuration
            var apiKeyValue = configurationSection[nameof(TranslaasOptions.ApiKey)];
            if (string.IsNullOrWhiteSpace(apiKeyValue) || string.IsNullOrWhiteSpace(options.ApiKey))
            {
                throw new InvalidOperationException(
                    $"Translaas configuration is invalid: ApiKey is required. Ensure '{sectionName}:ApiKey' is set in configuration.");
            }

            // Check if BaseUrl was explicitly set in configuration
            // BaseUrl has a default value, so we need to check if it was actually set in config
            var baseUrlValue = configurationSection[nameof(TranslaasOptions.BaseUrl)];
            if (string.IsNullOrWhiteSpace(baseUrlValue))
            {
                throw new InvalidOperationException(
                    $"Translaas configuration is invalid: BaseUrl is required. Ensure '{sectionName}:BaseUrl' is set in configuration.");
            }
        }, configureLanguage);
    }
}
