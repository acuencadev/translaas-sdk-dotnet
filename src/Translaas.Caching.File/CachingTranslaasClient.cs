using System;
using System.Threading;
using System.Threading.Tasks;

using Translaas.Client;
using Translaas.Models.Errors;
using Translaas.Models.Responses;

namespace Translaas.Caching.File;

/// <summary>
/// A decorator for <see cref="ITranslaasClient"/> that adds offline caching support.
/// Wraps an existing client and adds file-based caching based on the configured fallback mode.
/// </summary>
public class CachingTranslaasClient : ITranslaasClient
{
    private readonly ITranslaasClient _innerClient;
    private readonly IOfflineCacheProvider _cacheProvider;
    private readonly OfflineCacheOptions _options;
    private readonly string _projectId;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingTranslaasClient"/> class.
    /// </summary>
    /// <param name="innerClient">The underlying Translaas client.</param>
    /// <param name="cacheProvider">The offline cache provider.</param>
    /// <param name="options">The offline cache options.</param>
    /// <param name="projectId">The default project ID for entry lookups (required for caching single entries).</param>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
    public CachingTranslaasClient(
        ITranslaasClient innerClient,
        IOfflineCacheProvider cacheProvider,
        OfflineCacheOptions options,
        string projectId)
    {
        _innerClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));
        _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _projectId = projectId ?? throw new ArgumentNullException(nameof(projectId));
    }

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
            OfflineFallbackMode.CacheOnly => await GetEntryFromCacheOnlyAsync(group, entry, lang, cancellationToken).ConfigureAwait(false),
            OfflineFallbackMode.ApiOnlyWithBackup => await GetEntryWithApiOnlyBackupAsync(group, entry, lang, number, parameters, cancellationToken).ConfigureAwait(false),
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
            OfflineFallbackMode.ApiOnlyWithBackup => await GetGroupWithApiOnlyBackupAsync(project, group, lang, format, cancellationToken).ConfigureAwait(false),
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
            OfflineFallbackMode.ApiOnlyWithBackup => await GetProjectWithApiOnlyBackupAsync(project, lang, format, cancellationToken).ConfigureAwait(false),
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
            OfflineFallbackMode.ApiOnlyWithBackup => await GetProjectLocalesWithApiOnlyBackupAsync(project, cancellationToken).ConfigureAwait(false),
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
        var cachedValue = cachedGroup?.GetValue(entry);

        if (cachedValue != null)
        {
            return cachedValue;
        }

        // Cache miss, try API
        try
        {
            var result = await _innerClient.GetEntryAsync(group, entry, lang, number, parameters, cancellationToken).ConfigureAwait(false);

            // Update cache in background (fire and forget)
            _ = UpdateProjectCacheAsync(_projectId, lang, cancellationToken);

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

            // Update cache in background
            _ = UpdateProjectCacheAsync(_projectId, lang, cancellationToken);

            return result;
        }
        catch (Exception ex) when (IsNetworkOrApiError(ex))
        {
            // API failed, try cache
            var cachedGroup = await _cacheProvider.GetGroupAsync(_projectId, group, lang, cancellationToken).ConfigureAwait(false);
            var cachedValue = cachedGroup?.GetValue(entry);

            if (cachedValue != null)
            {
                return cachedValue;
            }

            throw new TranslaasOfflineCacheMissException(_projectId, lang, group, entry);
        }
    }

    private async Task<string> GetEntryFromCacheOnlyAsync(
        string group,
        string entry,
        string lang,
        CancellationToken cancellationToken)
    {
        var cachedGroup = await _cacheProvider.GetGroupAsync(_projectId, group, lang, cancellationToken).ConfigureAwait(false);
        var cachedValue = cachedGroup?.GetValue(entry);

        if (cachedValue != null)
        {
            return cachedValue;
        }

        throw new TranslaasOfflineCacheMissException(_projectId, lang, group, entry);
    }

    private async Task<string> GetEntryWithApiOnlyBackupAsync(
        string group,
        string entry,
        string lang,
        decimal? number,
        System.Collections.Generic.Dictionary<string, string>? parameters,
        CancellationToken cancellationToken)
    {
        var result = await _innerClient.GetEntryAsync(group, entry, lang, number, parameters, cancellationToken).ConfigureAwait(false);

        // Update cache in background
        _ = UpdateProjectCacheAsync(_projectId, lang, cancellationToken);

        return result;
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

            // Update cache in background
            _ = UpdateProjectCacheAsync(project, lang, cancellationToken);

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

            // Update cache in background
            _ = UpdateProjectCacheAsync(project, lang, cancellationToken);

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

    private async Task<TranslationGroup> GetGroupWithApiOnlyBackupAsync(
        string project,
        string group,
        string lang,
        string? format,
        CancellationToken cancellationToken)
    {
        var result = await _innerClient.GetGroupAsync(project, group, lang, format, cancellationToken).ConfigureAwait(false);

        // Update cache in background
        _ = UpdateProjectCacheAsync(project, lang, cancellationToken);

        return result;
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

    private async Task<TranslationProject> GetProjectWithApiOnlyBackupAsync(
        string project,
        string lang,
        string? format,
        CancellationToken cancellationToken)
    {
        var result = await _innerClient.GetProjectAsync(project, lang, format, cancellationToken).ConfigureAwait(false);

        // Update cache
        await _cacheProvider.SaveProjectAsync(project, lang, result, cancellationToken).ConfigureAwait(false);

        return result;
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

    private async Task<ProjectLocales> GetProjectLocalesWithApiOnlyBackupAsync(
        string project,
        CancellationToken cancellationToken)
    {
        var result = await _innerClient.GetProjectLocalesAsync(project, cancellationToken).ConfigureAwait(false);

        // Update cache
        await _cacheProvider.SaveProjectLocalesAsync(project, result, cancellationToken).ConfigureAwait(false);

        return result;
    }

    #endregion

    #region Helper Methods

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
