using Microsoft.Extensions.Options;

namespace Translaas.Extensions.DependencyInjection;

/// <summary>
/// Language provider that resolves language from <see cref="TranslaasOptions.DefaultLanguage"/>.
/// </summary>
/// <remarks>
/// This provider returns the configured default language from TranslaasOptions.
/// Returns null if DefaultLanguage is not configured.
/// Typically used as the last provider in the chain as a fallback.
/// </remarks>
public class DefaultLanguageProvider : ILanguageProvider
{
    private readonly TranslaasOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultLanguageProvider"/> class.
    /// </summary>
    /// <param name="options">The Translaas options.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when options is null.</exception>
    public DefaultLanguageProvider(IOptions<TranslaasOptions> options)
    {
        if (options == null)
        {
            throw new System.ArgumentNullException(nameof(options));
        }

        _options = options.Value;
    }

    /// <inheritdoc />
    public string? GetLanguage()
    {
        return string.IsNullOrWhiteSpace(_options.DefaultLanguage) 
            ? null 
            : _options.DefaultLanguage;
    }
}
