using Microsoft.Extensions.DependencyInjection;

using System;

using Translaas.Client;

namespace Translaas.Extensions.Http;

/// <summary>
/// Extension methods for configuring Translaas HTTP client with HttpClientFactory.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds and configures an HttpClient for use with the Translaas client.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">A delegate to configure the <see cref="TranslaasClientOptions"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to further configure the HttpClient.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configure is null.</exception>
    /// <example>
    /// <code>
    /// services.AddTranslaasHttpClient(options =>
    /// {
    ///     options.ApiKey = "your-api-key";
    ///     options.BaseUrl = "https://api.translaas.com";
    ///     options.Timeout = TimeSpan.FromSeconds(30);
    /// });
    /// </code>
    /// </example>
    public static IHttpClientBuilder AddTranslaasHttpClient(
        this IServiceCollection services,
        Action<TranslaasClientOptions> configure)
    {
        return AddTranslaasHttpClient(services, configure, skipApiValidation: false);
    }

    /// <summary>
    /// Adds and configures an HttpClient for use with the Translaas client.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">A delegate to configure the <see cref="TranslaasClientOptions"/>.</param>
    /// <param name="skipApiValidation">If true, skips validation of ApiKey and BaseUrl. Used when offline cache is enabled with CacheOnly mode.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to further configure the HttpClient.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configure is null.</exception>
    public static IHttpClientBuilder AddTranslaasHttpClient(
        this IServiceCollection services,
        Action<TranslaasClientOptions> configure,
        bool skipApiValidation)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        // Create options instance
        var options = new TranslaasClientOptions();
        configure(options);
        options.Validate(skipApiValidation);

        // Register HttpClient with the name of ITranslaasClient
        var builder = services.AddHttpClient(nameof(ITranslaasClient), client =>
        {
            // Only configure base address and API key if not skipping validation
            if (!skipApiValidation)
            {
                // Configure base address
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);

                // Set API key header
                client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
            }

            // Configure timeout if specified
            if (options.Timeout.HasValue)
            {
                client.Timeout = options.Timeout.Value;
            }
        });

        return builder;
    }
}
