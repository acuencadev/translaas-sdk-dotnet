namespace Translaas.Extensions.DependencyInjection;

/// <summary>
/// Configuration options for the culture-based language provider.
/// </summary>
public class CultureLanguageOptions
{
    /// <summary>
    /// When true, returns full culture name (e.g., "en-US"). 
    /// When false, returns two-letter ISO code (e.g., "en").
    /// Default: false
    /// </summary>
    public bool UseFullCultureName { get; set; } = false;
}
