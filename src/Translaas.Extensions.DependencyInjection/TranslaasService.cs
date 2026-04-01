using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Translaas.Client;

namespace Translaas.Extensions.DependencyInjection;

/// <summary>
/// Convenience service implementation for translation lookups with a simplified API.
/// </summary>
/// <remarks>
/// This service wraps <see cref="ITranslaasClient"/> to provide a more convenient API
/// for common translation lookups. Supports automatic language resolution when providers are configured.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="TranslaasService"/> class.
/// </remarks>
/// <param name="client">The Translaas client to use for translation lookups.</param>
/// <param name="resolver">Optional language resolver for automatic language resolution.</param>
/// <exception cref="System.ArgumentNullException">Thrown when client is null.</exception>
public class TranslaasService(ITranslaasClient client, ILanguageResolver? resolver = null) : ITranslaasService
{
    private readonly ITranslaasClient _client = client ?? throw new System.ArgumentNullException(nameof(client));
    private readonly ILanguageResolver? _resolver = resolver;

    private string ResolveLanguage()
    {
        if (_resolver != null)
        {
            var resolvedLang = _resolver.Resolve();
            if (resolvedLang != null && !string.IsNullOrWhiteSpace(resolvedLang))
            {
                return resolvedLang;
            }
        }

        throw new System.InvalidOperationException(
            "Unable to determine language for translation request. " +
            "Either provide the 'lang' parameter explicitly, or configure language providers " +
            "using AddTranslaas(..., language => language.UseCulture().UseDefault()).");
    }

    /// <inheritdoc />
    public Task<string> T(string group, string entry, CancellationToken cancellationToken = default)
    {
        var lang = ResolveLanguage();
        return _client.GetEntryAsync(group, entry, lang, null, null, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> T(string group, string entry, string lang, CancellationToken cancellationToken = default)
    {
        // If lang is empty/whitespace, use resolver instead
        if (string.IsNullOrWhiteSpace(lang))
        {
            var resolvedLang = ResolveLanguage();
            return _client.GetEntryAsync(group, entry, resolvedLang, null, null, null, cancellationToken);
        }
        return _client.GetEntryAsync(group, entry, lang, null, null, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> T(string group, string entry, string lang, decimal number, CancellationToken cancellationToken = default)
    {
        // If lang is empty/whitespace, use resolver instead
        if (string.IsNullOrWhiteSpace(lang))
        {
            var resolvedLang = ResolveLanguage();
            return _client.GetEntryAsync(group, entry, resolvedLang, number, null, null, cancellationToken);
        }
        return _client.GetEntryAsync(group, entry, lang, number, null, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> T(string group, string entry, string lang, Dictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        // If lang is empty/whitespace, use resolver instead
        if (string.IsNullOrWhiteSpace(lang))
        {
            var resolvedLang = ResolveLanguage();
            return _client.GetEntryAsync(group, entry, resolvedLang, null, parameters, null, cancellationToken);
        }
        return _client.GetEntryAsync(group, entry, lang, null, parameters, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> T(string group, string entry, string lang, decimal number, Dictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        // If lang is empty/whitespace, use resolver instead
        if (string.IsNullOrWhiteSpace(lang))
        {
            var resolvedLang = ResolveLanguage();
            return _client.GetEntryAsync(group, entry, resolvedLang, number, parameters, null, cancellationToken);
        }
        return _client.GetEntryAsync(group, entry, lang, number, parameters, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> T(string group, string entry, decimal number, CancellationToken cancellationToken = default)
    {
        var lang = ResolveLanguage();
        return _client.GetEntryAsync(group, entry, lang, number, null, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> T(string group, string entry, Dictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        var lang = ResolveLanguage();
        return _client.GetEntryAsync(group, entry, lang, null, parameters, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> T(string group, string entry, decimal number, Dictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        var lang = ResolveLanguage();
        return _client.GetEntryAsync(group, entry, lang, number, parameters, null, cancellationToken);
    }
}
