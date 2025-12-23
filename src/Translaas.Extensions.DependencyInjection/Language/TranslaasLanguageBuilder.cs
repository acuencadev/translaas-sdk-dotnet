using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

namespace Translaas.Extensions.DependencyInjection;

/// <summary>
/// Default implementation of <see cref="ITranslaasLanguageBuilder"/> for configuring language providers.
/// </summary>
internal class TranslaasLanguageBuilder : ITranslaasLanguageBuilder
{
    private readonly IServiceCollection _services;
    private readonly List<ServiceDescriptor> _providerDescriptors = new();

    /// <summary>
    /// Gets the service collection for registering providers.
    /// </summary>
    /// <remarks>
    /// Internal access for extension methods in other packages.
    /// </remarks>
    internal IServiceCollection Services => _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasLanguageBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection to register providers with.</param>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public TranslaasLanguageBuilder(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <inheritdoc />
    public ITranslaasLanguageBuilder UseCulture(Action<CultureLanguageOptions>? configure = null)
    {
        var options = new CultureLanguageOptions();
        configure?.Invoke(options);

        // Register options as singleton
        _services.AddSingleton(options);

        // Register provider as transient (thread culture can change)
        var descriptor = ServiceDescriptor.Transient<ILanguageProvider, CultureLanguageProvider>(
            serviceProvider => new CultureLanguageProvider(serviceProvider.GetRequiredService<CultureLanguageOptions>()));
        
        _providerDescriptors.Add(descriptor);
        _services.Add(descriptor);

        return this;
    }

    /// <inheritdoc />
    public ITranslaasLanguageBuilder UseDefault()
    {
        // Register provider as singleton (reads immutable config)
        var descriptor = ServiceDescriptor.Singleton<ILanguageProvider, DefaultLanguageProvider>();
        
        _providerDescriptors.Add(descriptor);
        _services.Add(descriptor);

        return this;
    }

    /// <inheritdoc />
    public ITranslaasLanguageBuilder UseProvider<TProvider>() 
        where TProvider : class, ILanguageProvider
    {
        // Default to transient lifetime, but can be overridden by user registration
        var descriptor = ServiceDescriptor.Transient<ILanguageProvider, TProvider>();
        
        _providerDescriptors.Add(descriptor);
        _services.Add(descriptor);

        return this;
    }

    /// <inheritdoc />
    public ITranslaasLanguageBuilder UseProvider(ILanguageProvider provider)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        // Register instance as singleton
        var descriptor = ServiceDescriptor.Singleton<ILanguageProvider>(provider);
        
        _providerDescriptors.Add(descriptor);
        _services.Add(descriptor);

        return this;
    }

    /// <inheritdoc />
    public ITranslaasLanguageBuilder UseProvider(Func<IServiceProvider, ILanguageProvider> factory)
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        // Register factory as transient (creates new instance each time)
        var descriptor = ServiceDescriptor.Transient<ILanguageProvider>(factory);
        
        _providerDescriptors.Add(descriptor);
        _services.Add(descriptor);

        return this;
    }
}
