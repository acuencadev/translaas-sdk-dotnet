using System.Globalization;

namespace Translaas.Extensions.DependencyInjection;

/// <summary>
/// Language provider that resolves language from <see cref="CultureInfo.CurrentUICulture"/>.
/// </summary>
/// <remarks>
/// This provider reads the current thread's UI culture and returns either
/// the two-letter ISO language code or the full culture name based on configuration.
/// </remarks>
public class CultureLanguageProvider : ILanguageProvider
{
    private readonly CultureLanguageOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CultureLanguageProvider"/> class.
    /// </summary>
    /// <param name="options">The culture language options.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when options is null.</exception>
    public CultureLanguageProvider(CultureLanguageOptions options)
    {
        _options = options ?? throw new System.ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public string? GetLanguage()
    {
        var culture = CultureInfo.CurrentUICulture;
        
        // Handle invariant culture
        if (culture.Equals(CultureInfo.InvariantCulture))
        {
            return null;
        }

        return _options.UseFullCultureName ? culture.Name : culture.TwoLetterISOLanguageName;
    }
}
