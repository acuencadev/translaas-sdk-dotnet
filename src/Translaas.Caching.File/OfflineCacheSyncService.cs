using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Translaas.Caching.File.Models;
using Translaas.Client;

namespace Translaas.Caching.File;

/// <summary>
/// Service for synchronizing offline cache with the Translaas API.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="OfflineCacheSyncService"/> class.
/// </remarks>
/// <param name="client">The Translaas client for API calls.</param>
/// <param name="cacheProvider">The offline cache provider.</param>
/// <param name="options">The offline cache options.</param>
/// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
public class OfflineCacheSyncService(
    ITranslaasClient client,
    IOfflineCacheProvider cacheProvider,
    OfflineCacheOptions options) : IOfflineCacheSyncService, IDisposable
{
    private readonly ITranslaasClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly IOfflineCacheProvider _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
    private readonly OfflineCacheOptions _options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    private CancellationTokenSource? _backgroundSyncCts;
    private Task? _backgroundSyncTask;
    private bool _disposed;

    /// <inheritdoc />
    public bool IsBackgroundSyncRunning => _backgroundSyncTask != null && !_backgroundSyncTask.IsCompleted;

    /// <inheritdoc />
    public event EventHandler<CacheSyncEventArgs>? SyncCompleted;

    /// <inheritdoc />
    public event EventHandler<CacheSyncErrorEventArgs>? SyncFailed;

    /// <inheritdoc />
    public event EventHandler<CacheSyncAllEventArgs>? SyncAllCompleted;

    /// <inheritdoc />
    public async Task SyncProjectAsync(
        string project,
        string lang,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(project))
        {
            throw new ArgumentException("Project cannot be null or whitespace.", nameof(project));
        }

        if (string.IsNullOrWhiteSpace(lang))
        {
            throw new ArgumentException("Language cannot be null or whitespace.", nameof(lang));
        }

        await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var projectData = await _client.GetProjectAsync(project, lang, cancellationToken: cancellationToken).ConfigureAwait(false);
            await _cacheProvider.SaveProjectAsync(project, lang, projectData, cancellationToken).ConfigureAwait(false);

            OnSyncCompleted(new CacheSyncEventArgs(project, lang, DateTimeOffset.UtcNow));
        }
        catch (Exception ex)
        {
            OnSyncFailed(new CacheSyncErrorEventArgs(project, lang, ex));
            throw;
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task SyncProjectAllLanguagesAsync(
        string project,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(project))
        {
            throw new ArgumentException("Project cannot be null or whitespace.", nameof(project));
        }

        await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Get available locales for the project
            var locales = await _client.GetProjectLocalesAsync(project, cancellationToken).ConfigureAwait(false);
            await _cacheProvider.SaveProjectLocalesAsync(project, locales, cancellationToken).ConfigureAwait(false);

            var languagesToSync = _options.Languages.Count > 0
                ? FilterLanguages(locales.Locales, _options.Languages)
                : locales.Locales;

            foreach (var lang in languagesToSync)
            {
                try
                {
                    var projectData = await _client.GetProjectAsync(project, lang, cancellationToken: cancellationToken).ConfigureAwait(false);
                    await _cacheProvider.SaveProjectAsync(project, lang, projectData, cancellationToken).ConfigureAwait(false);

                    OnSyncCompleted(new CacheSyncEventArgs(project, lang, DateTimeOffset.UtcNow));
                }
                catch (Exception ex)
                {
                    OnSyncFailed(new CacheSyncErrorEventArgs(project, lang, ex));
                    // Continue with other languages
                }
            }
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task SyncAllAsync(CancellationToken cancellationToken = default)
    {
        var syncedProjects = new List<string>();
        var failedProjects = new List<string>();

        foreach (var project in _options.Projects)
        {
            try
            {
                await SyncProjectAllLanguagesAsync(project, cancellationToken).ConfigureAwait(false);
                syncedProjects.Add(project);
            }
            catch
            {
                failedProjects.Add(project);
                // Continue with other projects
            }
        }

        OnSyncAllCompleted(new CacheSyncAllEventArgs(syncedProjects, failedProjects, DateTimeOffset.UtcNow));
    }

    /// <inheritdoc />
    public Task StartBackgroundSyncAsync(CancellationToken cancellationToken = default)
    {
        if (_backgroundSyncTask != null && !_backgroundSyncTask.IsCompleted)
        {
            // Already running
            return Task.CompletedTask;
        }

        if (!_options.AutoSync || !_options.AutoSyncInterval.HasValue)
        {
            // Auto sync is disabled
            return Task.CompletedTask;
        }

        _backgroundSyncCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _backgroundSyncTask = RunBackgroundSyncLoopAsync(_backgroundSyncCts.Token);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopBackgroundSyncAsync()
    {
        if (_backgroundSyncCts == null || _backgroundSyncTask == null)
        {
            return;
        }

#if NETSTANDARD2_0
        _backgroundSyncCts.Cancel();
#else
        await _backgroundSyncCts.CancelAsync().ConfigureAwait(false);
#endif

        try
        {
            await _backgroundSyncTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelling
        }

        _backgroundSyncCts.Dispose();
        _backgroundSyncCts = null;
        _backgroundSyncTask = null;
    }

    /// <summary>
    /// Releases the resources used by the <see cref="OfflineCacheSyncService"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _backgroundSyncCts?.Cancel();
            _backgroundSyncCts?.Dispose();
            _syncLock.Dispose();
        }

        _disposed = true;
    }

    private async Task RunBackgroundSyncLoopAsync(CancellationToken cancellationToken)
    {
        // Perform initial sync
        try
        {
            await SyncAllAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch
        {
            // Ignore errors during initial sync, will retry on next interval
        }

        // Continue periodic sync
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_options.AutoSyncInterval!.Value, cancellationToken).ConfigureAwait(false);
                await SyncAllAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Ignore errors, will retry on next interval
            }
        }
    }

    private static List<string> FilterLanguages(List<string> availableLanguages, List<string> requestedLanguages)
    {
        return [.. requestedLanguages.Where(availableLanguages.Contains)];
    }

    /// <summary>
    /// Raises the <see cref="SyncCompleted"/> event.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    protected virtual void OnSyncCompleted(CacheSyncEventArgs e)
    {
        SyncCompleted?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="SyncFailed"/> event.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    protected virtual void OnSyncFailed(CacheSyncErrorEventArgs e)
    {
        SyncFailed?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="SyncAllCompleted"/> event.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    protected virtual void OnSyncAllCompleted(CacheSyncAllEventArgs e)
    {
        SyncAllCompleted?.Invoke(this, e);
    }
}
