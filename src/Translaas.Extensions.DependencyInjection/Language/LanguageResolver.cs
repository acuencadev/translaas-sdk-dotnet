using System.Collections.Generic;

using Microsoft.Extensions.Logging;

namespace Translaas.Extensions.DependencyInjection;

/// <summary>
/// Default implementation of <see cref="ILanguageResolver"/> that queries providers in order.
/// </summary>
/// <remarks>
/// The resolver iterates through providers and returns the first non-null,
/// non-empty language code. If no provider returns a value, returns <c>null</c>.
/// Provider exceptions are caught and logged, then the next provider is tried.
/// </remarks>
public class LanguageResolver : ILanguageResolver
{
    private readonly IEnumerable<ILanguageProvider> _providers;
    private readonly ILogger<LanguageResolver>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageResolver"/> class.
    /// </summary>
    /// <param name="providers">The ordered list of language providers.</param>
    /// <param name="logger">Optional logger for provider exceptions.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when providers is null.</exception>
    public LanguageResolver(
        IEnumerable<ILanguageProvider> providers,
        ILogger<LanguageResolver>? logger = null)
    {
        _providers = providers ?? throw new System.ArgumentNullException(nameof(providers));
        _logger = logger;
    }

    /// <inheritdoc />
    public string? Resolve()
    {
        // Use simple foreach to avoid LINQ allocations in hot path
        foreach (var provider in _providers)
        {
            try
            {
                var lang = provider.GetLanguage();
                if (!string.IsNullOrWhiteSpace(lang))
                {
                    return lang;
                }
            }
            catch (System.Exception ex)
            {
                // Log warning, continue to next provider
                _logger?.LogWarning(
                    ex,
                    "Language provider {ProviderType} threw exception",
                    provider.GetType().Name);
            }
        }

        return null;
    }
}
