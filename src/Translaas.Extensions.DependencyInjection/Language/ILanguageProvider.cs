namespace Translaas.Extensions.DependencyInjection;

/// <summary>
/// Provides language resolution from a specific source.
/// </summary>
/// <remarks>
/// <para>
/// Implementations should return <c>null</c> when they cannot determine
/// the language, allowing the resolver to try the next provider in the chain.
/// </para>
/// <para>
/// Providers should be stateless or scoped appropriately to their data source.
/// </para>
/// </remarks>
public interface ILanguageProvider
{
    /// <summary>
    /// Attempts to resolve the current language code.
    /// </summary>
    /// <returns>
    /// The resolved language code (e.g., "en", "fr-CA"), 
    /// or <c>null</c> if this provider cannot determine the language.
    /// </returns>
    string? GetLanguage();
}
