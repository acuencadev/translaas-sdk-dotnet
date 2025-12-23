namespace Translaas.Extensions.DependencyInjection;

/// <summary>
/// Resolves the current language by querying registered providers in order.
/// </summary>
/// <remarks>
/// The resolver iterates through providers and returns the first non-null,
/// non-empty language code. If no provider returns a value, returns <c>null</c>.
/// </remarks>
public interface ILanguageResolver
{
    /// <summary>
    /// Resolves the current language from registered providers.
    /// </summary>
    /// <returns>
    /// The resolved language code, or <c>null</c> if no provider returned a value.
    /// </returns>
    string? Resolve();
}
