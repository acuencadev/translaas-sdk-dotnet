using System;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Translaas.Extensions.DependencyInjection;

namespace Translaas.Extensions.Mvc;

/// <summary>
/// Extension methods for web-specific language providers.
/// </summary>
public static class TranslaasLanguageBuilderExtensions
{
    /// <summary>
    /// Adds the HTTP request-based language provider.
    /// </summary>
    /// <param name="builder">The language builder.</param>
    /// <param name="configure">Optional configuration for request options.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// Automatically registers <see cref="IHttpContextAccessor"/> if not present.
    /// </remarks>
    public static ITranslaasLanguageBuilder UseRequest(
        this ITranslaasLanguageBuilder builder,
        Action<RequestLanguageOptions>? configure = null)
    {
        if (builder == null)
        {
            throw new System.ArgumentNullException(nameof(builder));
        }

        // Get the internal builder to access services
        if (builder is not TranslaasLanguageBuilder internalBuilder)
        {
            throw new System.ArgumentException(
                "Builder must be an instance of TranslaasLanguageBuilder.",
                nameof(builder));
        }

        var services = internalBuilder.Services;
        
        // Register IHttpContextAccessor if not present
        if (!services.Any(s => s.ServiceType == typeof(IHttpContextAccessor)))
        {
#if NETSTANDARD2_0
            // For netstandard2.0, manually register HttpContextAccessor
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
#else
            services.AddHttpContextAccessor();
#endif
        }

        // Configure options
        var options = new RequestLanguageOptions();
        configure?.Invoke(options);

        // Register options as singleton
        services.AddSingleton(options);

        // Register provider as scoped (tied to HTTP request)
        var descriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Scoped<ILanguageProvider, RequestLanguageProvider>(
            serviceProvider => new RequestLanguageProvider(
                serviceProvider.GetRequiredService<IHttpContextAccessor>(),
                serviceProvider.GetRequiredService<RequestLanguageOptions>()));

        services.Add(descriptor);

        return builder;
    }
}
