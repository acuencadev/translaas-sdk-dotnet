using System;

namespace Translaas.Extensions.DependencyInjection;

/// <summary>
/// Builder for configuring language resolution providers.
/// </summary>
/// <remarks>
/// Providers are queried in the order they are registered.
/// Register providers from most-specific to least-specific (fallback).
/// </remarks>
public interface ITranslaasLanguageBuilder
{
    /// <summary>
    /// Adds the culture-based language provider.
    /// </summary>
    /// <param name="configure">Optional configuration for culture options.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddTranslaas(options => { /* ... */ }, language => language
    ///     .UseCulture()); // Uses CultureInfo.CurrentUICulture
    /// </code>
    /// </example>
    ITranslaasLanguageBuilder UseCulture(Action<CultureLanguageOptions>? configure = null);
    
    /// <summary>
    /// Adds the configuration default language provider.
    /// </summary>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// Reads from <see cref="TranslaasOptions.DefaultLanguage"/>.
    /// Typically registered last as a fallback.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddTranslaas(options => 
    /// {
    ///     options.DefaultLanguage = "en";
    /// }, language => language
    ///     .UseDefault()); // Falls back to DefaultLanguage when other providers fail
    /// </code>
    /// </example>
    ITranslaasLanguageBuilder UseDefault();
    
    /// <summary>
    /// Adds a custom language provider type.
    /// </summary>
    /// <typeparam name="TProvider">The provider type to register.</typeparam>
    /// <returns>The builder for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddTranslaas(options => { /* ... */ }, language => language
    ///     .UseProvider&lt;MyCustomLanguageProvider&gt;());
    /// </code>
    /// </example>
    ITranslaasLanguageBuilder UseProvider<TProvider>() 
        where TProvider : class, ILanguageProvider;
    
    /// <summary>
    /// Adds a custom language provider instance.
    /// </summary>
    /// <param name="provider">The provider instance.</param>
    /// <returns>The builder for chaining.</returns>
    ITranslaasLanguageBuilder UseProvider(ILanguageProvider provider);
    
    /// <summary>
    /// Adds a custom language provider using a factory.
    /// </summary>
    /// <param name="factory">Factory function to create the provider.</param>
    /// <returns>The builder for chaining.</returns>
    ITranslaasLanguageBuilder UseProvider(Func<IServiceProvider, ILanguageProvider> factory);
}
