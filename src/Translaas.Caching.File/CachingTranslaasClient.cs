using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Translaas.Client;
using Translaas.Models;
using Translaas.Models.Errors;
using Translaas.Models.Responses;

namespace Translaas.Caching.File;

/// <summary>
/// A decorator for <see cref="ITranslaasClient"/> that adds offline caching support.
/// Wraps an existing client and adds file-based caching based on the configured fallback mode.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CachingTranslaasClient"/> class.
/// </remarks>
/// <param name="innerClient">The underlying Translaas client.</param>
/// <param name="cacheProvider">The offline cache provider.</param>
/// <param name="options">The offline cache options.</param>
/// <param name="projectId">The default project ID for entry lookups (required for caching single entries).</param>
/// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
public class CachingTranslaasClient(
    ITranslaasClient innerClient,
    IOfflineCacheProvider cacheProvider,
    OfflineCacheOptions options,
    string projectId) : ITranslaasClient
{
    private readonly ITranslaasClient _innerClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));
    private readonly IOfflineCacheProvider _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
    private readonly OfflineCacheOptions _options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly string _projectId = projectId ?? throw new ArgumentNullException(nameof(projectId));

    /// <inheritdoc />
    public async Task<string> GetEntryAsync(
        string group,
        string entry,
        string lang,
        decimal? number = null,
        System.Collections.Generic.Dictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        return _options.FallbackMode switch
        {
            OfflineFallbackMode.CacheFirst => await GetEntryWithCacheFirstAsync(group, entry, lang, number, parameters, cancellationToken).ConfigureAwait(false),
            OfflineFallbackMode.ApiFirst => await GetEntryWithApiFirstAsync(group, entry, lang, number, parameters, cancellationToken).ConfigureAwait(false),
            OfflineFallbackMode.CacheOnly => await GetEntryFromCacheOnlyAsync(group, entry, lang, number, parameters, cancellationToken).ConfigureAwait(false),
            _ => await _innerClient.GetEntryAsync(group, entry, lang, number, parameters, cancellationToken).ConfigureAwait(false)
        };
    }

    /// <inheritdoc />
    public async Task<TranslationGroup> GetGroupAsync(
        string project,
        string group,
        string lang,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        return _options.FallbackMode switch
        {
            OfflineFallbackMode.CacheFirst => await GetGroupWithCacheFirstAsync(project, group, lang, format, cancellationToken).ConfigureAwait(false),
            OfflineFallbackMode.ApiFirst => await GetGroupWithApiFirstAsync(project, group, lang, format, cancellationToken).ConfigureAwait(false),
            OfflineFallbackMode.CacheOnly => await GetGroupFromCacheOnlyAsync(project, group, lang, cancellationToken).ConfigureAwait(false),
            _ => await _innerClient.GetGroupAsync(project, group, lang, format, cancellationToken).ConfigureAwait(false)
        };
    }

    /// <inheritdoc />
    public async Task<TranslationProject> GetProjectAsync(
        string project,
        string lang,
        string? format = null,
        CancellationToken cancellationToken = default)
    {
        return _options.FallbackMode switch
        {
            OfflineFallbackMode.CacheFirst => await GetProjectWithCacheFirstAsync(project, lang, format, cancellationToken).ConfigureAwait(false),
            OfflineFallbackMode.ApiFirst => await GetProjectWithApiFirstAsync(project, lang, format, cancellationToken).ConfigureAwait(false),
            OfflineFallbackMode.CacheOnly => await GetProjectFromCacheOnlyAsync(project, lang, cancellationToken).ConfigureAwait(false),
            _ => await _innerClient.GetProjectAsync(project, lang, format, cancellationToken).ConfigureAwait(false)
        };
    }

    /// <inheritdoc />
    public async Task<ProjectLocales> GetProjectLocalesAsync(
        string project,
        CancellationToken cancellationToken = default)
    {
        return _options.FallbackMode switch
        {
            OfflineFallbackMode.CacheFirst => await GetProjectLocalesWithCacheFirstAsync(project, cancellationToken).ConfigureAwait(false),
            OfflineFallbackMode.ApiFirst => await GetProjectLocalesWithApiFirstAsync(project, cancellationToken).ConfigureAwait(false),
            OfflineFallbackMode.CacheOnly => await GetProjectLocalesFromCacheOnlyAsync(project, cancellationToken).ConfigureAwait(false),
            _ => await _innerClient.GetProjectLocalesAsync(project, cancellationToken).ConfigureAwait(false)
        };
    }

    #region GetEntry Implementations

    private async Task<string> GetEntryWithCacheFirstAsync(
        string group,
        string entry,
        string lang,
        decimal? number,
        System.Collections.Generic.Dictionary<string, string>? parameters,
        CancellationToken cancellationToken)
    {
        // Try cache first
        var cachedGroup = await _cacheProvider.GetGroupAsync(_projectId, group, lang, cancellationToken).ConfigureAwait(false);
        
        if (cachedGroup != null)
        {
            // Check if entry has plural forms
            if (cachedGroup.HasPluralForms(entry))
            {
                // Determine plural category based on number
                var pluralCategory = DeterminePluralCategory(number, lang);
                
                // Get the plural form
                var pluralForm = cachedGroup.GetPluralForm(entry, pluralCategory);
                
                // If the specific category is not found, try "other" as fallback
                if (pluralForm == null && pluralCategory != PluralCategory.Other)
                {
                    pluralForm = cachedGroup.GetPluralForm(entry, PluralCategory.Other);
                }
                
                if (pluralForm != null)
                {
                    // Perform parameter substitution on cached template
                    return SubstituteParameters(pluralForm, number, parameters);
                }
            }
            else
            {
                // Simple string entry
                var cachedValue = cachedGroup.GetValue(entry);
                
                if (cachedValue != null)
                {
                    // Perform parameter substitution on cached template
                    return SubstituteParameters(cachedValue, number, parameters);
                }
            }
        }

        // Cache miss, try API
        try
        {
            var result = await _innerClient.GetEntryAsync(group, entry, lang, number, parameters, cancellationToken).ConfigureAwait(false);

            // Update cache - fetch the specific group and update cache
            // Note: We await this to ensure it completes, but catch errors so API call still succeeds
            try
            {
                await UpdateGroupCacheAsync(_projectId, group, lang, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log but don't throw - cache update failure shouldn't break the API call
                // Note: In a production app, you might want to use ILogger here
                System.Diagnostics.Debug.WriteLine($"Cache update failed for project '{_projectId}', group '{group}', lang '{lang}': {ex.GetType().Name}: {ex.Message}");
            }

            return result;
        }
        catch (Exception ex) when (IsNetworkOrApiError(ex))
        {
            throw new TranslaasOfflineCacheMissException(_projectId, lang, group, entry);
        }
    }

    private async Task<string> GetEntryWithApiFirstAsync(
        string group,
        string entry,
        string lang,
        decimal? number,
        System.Collections.Generic.Dictionary<string, string>? parameters,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _innerClient.GetEntryAsync(group, entry, lang, number, parameters, cancellationToken).ConfigureAwait(false);

            // Update cache - fetch the specific group and update cache
            // Note: We await this to ensure it completes, but catch errors so API call still succeeds
            try
            {
                await UpdateGroupCacheAsync(_projectId, group, lang, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log but don't throw - cache update failure shouldn't break the API call
                // Note: In a production app, you might want to use ILogger here
                System.Diagnostics.Debug.WriteLine($"Cache update failed for project '{_projectId}', group '{group}', lang '{lang}': {ex.GetType().Name}: {ex.Message}");
            }

            return result;
        }
        catch (Exception ex) when (IsNetworkOrApiError(ex))
        {
            // API failed, try cache
            var cachedGroup = await _cacheProvider.GetGroupAsync(_projectId, group, lang, cancellationToken).ConfigureAwait(false);
            
            if (cachedGroup != null)
            {
                // Check if entry has plural forms
                if (cachedGroup.HasPluralForms(entry))
                {
                    // Determine plural category based on number
                    var pluralCategory = DeterminePluralCategory(number, lang);
                    
                    // Get the plural form
                    var pluralForm = cachedGroup.GetPluralForm(entry, pluralCategory);
                    
                    // If the specific category is not found, try "other" as fallback
                    if (pluralForm == null && pluralCategory != PluralCategory.Other)
                    {
                        pluralForm = cachedGroup.GetPluralForm(entry, PluralCategory.Other);
                    }
                    
                    if (pluralForm != null)
                    {
                        // Perform parameter substitution on cached template
                        return SubstituteParameters(pluralForm, number, parameters);
                    }
                }
                else
                {
                    // Simple string entry
                    var cachedValue = cachedGroup.GetValue(entry);
                    
                    if (cachedValue != null)
                    {
                        // Perform parameter substitution on cached template
                        return SubstituteParameters(cachedValue, number, parameters);
                    }
                }
            }

            throw new TranslaasOfflineCacheMissException(_projectId, lang, group, entry);
        }
    }

    private async Task<string> GetEntryFromCacheOnlyAsync(
        string group,
        string entry,
        string lang,
        decimal? number,
        System.Collections.Generic.Dictionary<string, string>? parameters,
        CancellationToken cancellationToken)
    {
        var cachedGroup = await _cacheProvider.GetGroupAsync(_projectId, group, lang, cancellationToken).ConfigureAwait(false) ?? throw new TranslaasOfflineCacheMissException(_projectId, lang, group, entry);

        // Check if entry has plural forms
        if (cachedGroup.HasPluralForms(entry))
        {
            // Determine plural category based on number
            var pluralCategory = DeterminePluralCategory(number, lang);
            
            // Get the plural form
            var pluralForm = cachedGroup.GetPluralForm(entry, pluralCategory);
            
            // If the specific category is not found, try "other" as fallback
            if (pluralForm == null && pluralCategory != PluralCategory.Other)
            {
                pluralForm = cachedGroup.GetPluralForm(entry, PluralCategory.Other);
            }
            
            if (pluralForm != null)
            {
                // Perform parameter substitution on cached template
                return SubstituteParameters(pluralForm, number, parameters);
            }
        }
        else
        {
            // Simple string entry
            var cachedValue = cachedGroup.GetValue(entry);
            
            if (cachedValue != null)
            {
                // Perform parameter substitution on cached template
                return SubstituteParameters(cachedValue, number, parameters);
            }
        }

        throw new TranslaasOfflineCacheMissException(_projectId, lang, group, entry);
    }


    #endregion

    #region GetGroup Implementations

    private async Task<TranslationGroup> GetGroupWithCacheFirstAsync(
        string project,
        string group,
        string lang,
        string? format,
        CancellationToken cancellationToken)
    {
        // Try cache first
        var cachedGroup = await _cacheProvider.GetGroupAsync(project, group, lang, cancellationToken).ConfigureAwait(false);

        if (cachedGroup != null)
        {
            return cachedGroup;
        }

        // Cache miss, try API
        try
        {
            var result = await _innerClient.GetGroupAsync(project, group, lang, format, cancellationToken).ConfigureAwait(false);

            // Update cache in background - only fetch the specific group, not entire project
            _ = UpdateGroupCacheAsync(project, group, lang, cancellationToken);

            return result;
        }
        catch (Exception ex) when (IsNetworkOrApiError(ex))
        {
            throw new TranslaasOfflineCacheMissException(project, lang, group);
        }
    }

    private async Task<TranslationGroup> GetGroupWithApiFirstAsync(
        string project,
        string group,
        string lang,
        string? format,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _innerClient.GetGroupAsync(project, group, lang, format, cancellationToken).ConfigureAwait(false);

            // Update cache in background - only fetch the specific group, not entire project
            _ = UpdateGroupCacheAsync(project, group, lang, cancellationToken);

            return result;
        }
        catch (Exception ex) when (IsNetworkOrApiError(ex))
        {
            // API failed, try cache
            var cachedGroup = await _cacheProvider.GetGroupAsync(project, group, lang, cancellationToken).ConfigureAwait(false);

            if (cachedGroup != null)
            {
                return cachedGroup;
            }

            throw new TranslaasOfflineCacheMissException(project, lang, group);
        }
    }

    private async Task<TranslationGroup> GetGroupFromCacheOnlyAsync(
        string project,
        string group,
        string lang,
        CancellationToken cancellationToken)
    {
        var cachedGroup = await _cacheProvider.GetGroupAsync(project, group, lang, cancellationToken).ConfigureAwait(false);

        if (cachedGroup != null)
        {
            return cachedGroup;
        }

        throw new TranslaasOfflineCacheMissException(project, lang, group);
    }


    #endregion

    #region GetProject Implementations

    private async Task<TranslationProject> GetProjectWithCacheFirstAsync(
        string project,
        string lang,
        string? format,
        CancellationToken cancellationToken)
    {
        // Try cache first
        var cachedProject = await _cacheProvider.GetProjectAsync(project, lang, cancellationToken).ConfigureAwait(false);

        if (cachedProject != null)
        {
            return cachedProject;
        }

        // Cache miss, try API
        try
        {
            var result = await _innerClient.GetProjectAsync(project, lang, format, cancellationToken).ConfigureAwait(false);

            // Update cache
            await _cacheProvider.SaveProjectAsync(project, lang, result, cancellationToken).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex) when (IsNetworkOrApiError(ex))
        {
            throw new TranslaasOfflineCacheMissException(project, lang);
        }
    }

    private async Task<TranslationProject> GetProjectWithApiFirstAsync(
        string project,
        string lang,
        string? format,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _innerClient.GetProjectAsync(project, lang, format, cancellationToken).ConfigureAwait(false);

            // Update cache
            await _cacheProvider.SaveProjectAsync(project, lang, result, cancellationToken).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex) when (IsNetworkOrApiError(ex))
        {
            // API failed, try cache
            var cachedProject = await _cacheProvider.GetProjectAsync(project, lang, cancellationToken).ConfigureAwait(false);

            if (cachedProject != null)
            {
                return cachedProject;
            }

            throw new TranslaasOfflineCacheMissException(project, lang);
        }
    }

    private async Task<TranslationProject> GetProjectFromCacheOnlyAsync(
        string project,
        string lang,
        CancellationToken cancellationToken)
    {
        var cachedProject = await _cacheProvider.GetProjectAsync(project, lang, cancellationToken).ConfigureAwait(false);

        if (cachedProject != null)
        {
            return cachedProject;
        }

        throw new TranslaasOfflineCacheMissException(project, lang);
    }


    #endregion

    #region GetProjectLocales Implementations

    private async Task<ProjectLocales> GetProjectLocalesWithCacheFirstAsync(
        string project,
        CancellationToken cancellationToken)
    {
        // Try cache first
        var cachedLocales = await _cacheProvider.GetProjectLocalesAsync(project, cancellationToken).ConfigureAwait(false);

        if (cachedLocales != null)
        {
            return cachedLocales;
        }

        // Cache miss, try API
        try
        {
            var result = await _innerClient.GetProjectLocalesAsync(project, cancellationToken).ConfigureAwait(false);

            // Update cache
            await _cacheProvider.SaveProjectLocalesAsync(project, result, cancellationToken).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex) when (IsNetworkOrApiError(ex))
        {
            throw new TranslaasOfflineCacheException(
                $"Project locales for '{project}' not found in offline cache and API is unavailable.",
                null, project, null, ex);
        }
    }

    private async Task<ProjectLocales> GetProjectLocalesWithApiFirstAsync(
        string project,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _innerClient.GetProjectLocalesAsync(project, cancellationToken).ConfigureAwait(false);

            // Update cache
            await _cacheProvider.SaveProjectLocalesAsync(project, result, cancellationToken).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex) when (IsNetworkOrApiError(ex))
        {
            // API failed, try cache
            var cachedLocales = await _cacheProvider.GetProjectLocalesAsync(project, cancellationToken).ConfigureAwait(false);

            if (cachedLocales != null)
            {
                return cachedLocales;
            }

            throw new TranslaasOfflineCacheException(
                $"Project locales for '{project}' not found in offline cache and API is unavailable.",
                null, project, null, ex);
        }
    }

    private async Task<ProjectLocales> GetProjectLocalesFromCacheOnlyAsync(
        string project,
        CancellationToken cancellationToken)
    {
        var cachedLocales = await _cacheProvider.GetProjectLocalesAsync(project, cancellationToken).ConfigureAwait(false);

        if (cachedLocales != null)
        {
            return cachedLocales;
        }

        throw new TranslaasOfflineCacheException(
            $"Project locales for '{project}' not found in offline cache.",
            null, project, null);
    }


    #endregion

    #region Helper Methods

    /// <summary>
    /// Substitutes parameters in a translation template string.
    /// Replaces placeholders like {userName} with values from the parameters dictionary.
    /// </summary>
    /// <param name="template">The template string containing placeholders (e.g., "Hello {userName}").</param>
    /// <param name="number">Optional number for pluralization (populates {N} placeholder).</param>
    /// <param name="parameters">Optional dictionary of named parameters for substitution.</param>
    /// <returns>The template with placeholders replaced by parameter values.</returns>
    private static string SubstituteParameters(
        string template,
        decimal? number,
        Dictionary<string, string>? parameters)
    {
        if (string.IsNullOrEmpty(template))
        {
            return template ?? string.Empty;
        }

        // Merge number into parameters if provided
        var mergedParameters = MergeNumberIntoParameters(number, parameters);

        // If no parameters, return template as-is
        if (mergedParameters == null || mergedParameters.Count == 0)
        {
            return template;
        }

        // Use regex to find all placeholders in the format {parameterName}
        // Match {parameterName} where parameterName can contain letters, numbers, and underscores
        var result = Regex.Replace(
            template,
            @"\{([a-zA-Z0-9_]+)\}",
            match =>
            {
                var placeholderName = match.Groups[1].Value;

                // Case-insensitive lookup
                if (mergedParameters.TryGetValue(placeholderName, out var value))
                {
                    return value;
                }

                // If parameter not found, return the placeholder as-is
                return match.Value;
            },
            RegexOptions.None);

        return result;
    }

    /// <summary>
    /// Determines the plural category based on the number and language.
    /// Uses simple rules: for most languages, 1 = One, everything else = Other.
    /// </summary>
    /// <param name="number">The number value (for pluralization).</param>
    /// <param name="lang">The language code.</param>
    /// <returns>The plural category.</returns>
    private static PluralCategory DeterminePluralCategory(decimal? number, string lang)
    {
        // If no number provided, default to Other
        if (!number.HasValue)
        {
            return PluralCategory.Other;
        }

        var num = number.Value;

        // Simple rule for most languages: 1 = One, everything else = Other
        // This covers English, Spanish, French, German, Italian, Portuguese, etc.
        if (num == 1)
        {
            return PluralCategory.One;
        }

        // For more complex languages (Russian, Arabic, etc.), we'd need CLDR rules
        // For now, default to Other as it's the most common fallback
        return PluralCategory.Other;
    }

    /// <summary>
    /// Merges the number parameter into the parameters dictionary, creating a case-insensitive dictionary.
    /// </summary>
    /// <param name="number">The number parameter (for pluralization).</param>
    /// <param name="parameters">The existing parameters dictionary (may be null).</param>
    /// <returns>A new dictionary with number merged in, or null if both number and parameters are null/empty.</returns>
    private static Dictionary<string, string>? MergeNumberIntoParameters(decimal? number, Dictionary<string, string>? parameters)
    {
        // If no number and no parameters, return null
        if (!number.HasValue && (parameters == null || parameters.Count == 0))
        {
            return null;
        }

        // Create a new dictionary to avoid modifying the input
        var merged = parameters == null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase);

        // Only add N if it doesn't already exist (case-insensitive check)
        if (number.HasValue && !merged.ContainsKey("N"))
        {
            // Format number using invariant culture to ensure consistent formatting
            merged["N"] = number.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return merged.Count > 0 ? merged : null;
    }

    /// <summary>
    /// Updates the cache with a specific translation group (more efficient than updating entire project).
    /// Fetches only the group from API and merges it into the existing cached project.
    /// </summary>
    private async Task UpdateGroupCacheAsync(string project, string group, string lang, CancellationToken cancellationToken)
    {
        try
        {
            // Fetch only the specific group from API (more efficient than entire project)
            var groupData = await _innerClient.GetGroupAsync(project, group, lang, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            if (groupData == null)
            {
                System.Diagnostics.Debug.WriteLine($"GetGroupAsync returned null for project '{project}', group '{group}', lang '{lang}'");
                return;
            }
            
            // Get existing project from cache (if exists)
            var existingProject = await _cacheProvider.GetProjectAsync(project, lang, cancellationToken).ConfigureAwait(false);
            
            TranslationProject projectToSave;
            if (existingProject != null)
            {
                // Merge the new group into existing project
                projectToSave = existingProject;
                
                // Extract only the Entries dictionary from the group (not the metadata)
                // The cache file stores groups as flat entry dictionaries, not full TranslationGroup objects
                // Use encoder that doesn't escape non-ASCII characters for readability
                var jsonOptions = new System.Text.Json.JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                var entriesJson = System.Text.Json.JsonSerializer.SerializeToElement(groupData.Entries, jsonOptions);
                projectToSave.Groups[group] = entriesJson;
            }
            else
            {
                // No existing project, create a new one with just this group
                projectToSave = new TranslationProject();
                // Extract only the Entries dictionary from the group (not the metadata)
                // The cache file stores groups as flat entry dictionaries, not full TranslationGroup objects
                // Use encoder that doesn't escape non-ASCII characters for readability
                var jsonOptions = new System.Text.Json.JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                var entriesJson = System.Text.Json.JsonSerializer.SerializeToElement(groupData.Entries, jsonOptions);
                projectToSave.Groups[group] = entriesJson;
            }
            
            // Save the updated project
            await _cacheProvider.SaveProjectAsync(project, lang, projectToSave, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Log cache update errors for debugging, but don't throw - this is a background operation
            // Note: In a production app, you might want to use ILogger here
            System.Diagnostics.Debug.WriteLine($"Failed to update cache for project '{project}', group '{group}', lang '{lang}': {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"  Inner exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
            // Re-throw to let caller handle it
            throw;
        }
    }

    /// <summary>
    /// Updates the cache with the entire translation project.
    /// Used when fetching the full project (GetProjectAsync) or when group is unknown.
    /// </summary>
    private async Task UpdateProjectCacheAsync(string project, string lang, CancellationToken cancellationToken)
    {
        try
        {
            var projectData = await _innerClient.GetProjectAsync(project, lang, cancellationToken: cancellationToken).ConfigureAwait(false);
            await _cacheProvider.SaveProjectAsync(project, lang, projectData, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            // Ignore cache update errors - this is a background operation
        }
    }

    private static bool IsNetworkOrApiError(Exception ex)
    {
        // Check for common network/API errors that should trigger cache fallback
        return ex is System.Net.Http.HttpRequestException
            || ex is TaskCanceledException
            || ex is TimeoutException
            || ex is TranslaasApiException;
    }

    #endregion
}
