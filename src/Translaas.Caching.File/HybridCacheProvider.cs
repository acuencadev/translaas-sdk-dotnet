using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Translaas.Caching.File.Models;
using Translaas.Models.Responses;

namespace Translaas.Caching.File;

/// <summary>
/// A hybrid cache provider that combines memory cache (L1) with file cache (L2).
/// </summary>
/// <remarks>
/// <para>
/// The hybrid cache provides fast access through in-memory caching while maintaining
/// persistence through file-based caching. This is ideal for applications that need
/// both performance and offline support.
/// </para>
/// <para>
/// Cache lookup order:
/// </para>
/// <list type="number">
/// <item><description>Check memory cache (L1) - fastest</description></item>
/// <item><description>On L1 miss, check file cache (L2)</description></item>
/// <item><description>On L2 hit, populate L1 for future requests</description></item>
/// </list>
/// <para>
/// Cache write order:
/// </para>
/// <list type="number">
/// <item><description>Write to memory cache (L1) - immediate</description></item>
/// <item><description>Write to file cache (L2) - persistent</description></item>
/// </list>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="HybridCacheProvider"/> class.
/// </remarks>
/// <param name="fileCache">The file-based cache provider (L2).</param>
/// <param name="options">The hybrid cache options.</param>
/// <exception cref="ArgumentNullException">Thrown when fileCache or options is null.</exception>
public class HybridCacheProvider(IOfflineCacheProvider fileCache, HybridCacheOptions options) : IOfflineCacheProvider
{
    private readonly IOfflineCacheProvider _fileCache = fileCache ?? throw new ArgumentNullException(nameof(fileCache));
    private readonly HybridCacheOptions _options = options ?? throw new ArgumentNullException(nameof(options));

    // In-memory cache for fast L1 lookups
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, CacheEntry<TranslationProject>> _projectCache = new();
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, CacheEntry<TranslationGroup>> _groupCache = new();
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, CacheEntry<ProjectLocales>> _localesCache = new();

    /// <inheritdoc />
    public async Task<TranslationProject?> GetProjectAsync(
        string project,
        string lang,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildProjectKey(project, lang);

        // L1: Check memory cache first
        if (_projectCache.TryGetValue(cacheKey, out var entry) && !entry.IsExpired)
        {
            return entry.Value;
        }

        // L2: Check file cache
        var result = await _fileCache.GetProjectAsync(project, lang, cancellationToken).ConfigureAwait(false);

        if (result != null)
        {
            // Populate L1 cache
            SetMemoryCache(_projectCache, cacheKey, result);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<TranslationGroup?> GetGroupAsync(
        string project,
        string group,
        string lang,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildGroupKey(project, group, lang);

        // L1: Check memory cache first
        if (_groupCache.TryGetValue(cacheKey, out var entry) && !entry.IsExpired)
        {
            return entry.Value;
        }

        // L2: Check file cache (via project)
        var result = await _fileCache.GetGroupAsync(project, group, lang, cancellationToken).ConfigureAwait(false);

        if (result != null)
        {
            // Populate L1 cache
            SetMemoryCache(_groupCache, cacheKey, result);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<ProjectLocales?> GetProjectLocalesAsync(
        string project,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildLocalesKey(project);

        // L1: Check memory cache first
        if (_localesCache.TryGetValue(cacheKey, out var entry) && !entry.IsExpired)
        {
            return entry.Value;
        }

        // L2: Check file cache
        var result = await _fileCache.GetProjectLocalesAsync(project, cancellationToken).ConfigureAwait(false);

        if (result != null)
        {
            // Populate L1 cache
            SetMemoryCache(_localesCache, cacheKey, result);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task SaveProjectAsync(
        string project,
        string lang,
        TranslationProject data,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildProjectKey(project, lang);

        // L1: Update memory cache immediately
        SetMemoryCache(_projectCache, cacheKey, data);

        // Also cache individual groups in memory for faster group lookups
        foreach (var groupEntry in data.Groups)
        {
            var group = data.GetGroup(groupEntry.Key);
            if (group != null)
            {
                var groupKey = BuildGroupKey(project, groupEntry.Key, lang);
                SetMemoryCache(_groupCache, groupKey, group);
            }
        }

        // L2: Persist to file cache
        await _fileCache.SaveProjectAsync(project, lang, data, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SaveProjectLocalesAsync(
        string project,
        ProjectLocales locales,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildLocalesKey(project);

        // L1: Update memory cache immediately
        SetMemoryCache(_localesCache, cacheKey, locales);

        // L2: Persist to file cache
        await _fileCache.SaveProjectLocalesAsync(project, locales, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<bool> IsCachedAsync(
        string project,
        string lang,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildProjectKey(project, lang);

        // Check L1 first
        if (_projectCache.TryGetValue(cacheKey, out var entry) && !entry.IsExpired)
        {
            return Task.FromResult(true);
        }

        // Check L2
        return _fileCache.IsCachedAsync(project, lang, cancellationToken);
    }

    /// <inheritdoc />
    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        // Clear L1
        _projectCache.Clear();
        _groupCache.Clear();
        _localesCache.Clear();

        // Clear L2
        await _fileCache.ClearAllAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ClearProjectAsync(
        string project,
        CancellationToken cancellationToken = default)
    {
        // Clear L1 entries for this project
        ClearMemoryCacheForProject(project);

        // Clear L2
        await _fileCache.ClearProjectAsync(project, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<CacheManifest> GetManifestAsync(CancellationToken cancellationToken = default)
    {
        // Manifest is stored in L2 only
        return _fileCache.GetManifestAsync(cancellationToken);
    }

    /// <summary>
    /// Clears only the L1 (memory) cache, leaving L2 (file) cache intact.
    /// </summary>
    /// <remarks>
    /// This is useful when you need to free memory but want to keep the persistent cache.
    /// </remarks>
    public void ClearMemoryCache()
    {
        _projectCache.Clear();
        _groupCache.Clear();
        _localesCache.Clear();
    }

    /// <summary>
    /// Gets the current number of items in the L1 (memory) cache.
    /// </summary>
    /// <returns>A tuple containing counts for projects, groups, and locales.</returns>
    public (int Projects, int Groups, int Locales) GetMemoryCacheStats()
    {
        return (_projectCache.Count, _groupCache.Count, _localesCache.Count);
    }

    /// <summary>
    /// Warms up the L1 cache by loading data from L2 for the specified project and language.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="lang">The language code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the cache was warmed up; false if the project was not in L2.</returns>
    public async Task<bool> WarmupAsync(string project, string lang, CancellationToken cancellationToken = default)
    {
        var projectData = await _fileCache.GetProjectAsync(project, lang, cancellationToken).ConfigureAwait(false);

        if (projectData == null)
        {
            return false;
        }

        var projectKey = BuildProjectKey(project, lang);
        SetMemoryCache(_projectCache, projectKey, projectData);

        // Cache individual groups
        foreach (var groupEntry in projectData.Groups)
        {
            var group = projectData.GetGroup(groupEntry.Key);
            if (group != null)
            {
                var groupKey = BuildGroupKey(project, groupEntry.Key, lang);
                SetMemoryCache(_groupCache, groupKey, group);
            }
        }

        return true;
    }

    private void SetMemoryCache<T>(
        System.Collections.Concurrent.ConcurrentDictionary<string, CacheEntry<T>> cache,
        string key,
        T value) where T : class
    {
        var expiration = _options.MemoryCacheExpiration.HasValue
            ? DateTimeOffset.UtcNow.Add(_options.MemoryCacheExpiration.Value)
            : (DateTimeOffset?)null;

        cache[key] = new CacheEntry<T>(value, expiration);

        // Evict old entries if we exceed the max size
        if (_options.MaxMemoryCacheEntries.HasValue && cache.Count > _options.MaxMemoryCacheEntries.Value)
        {
            EvictOldestEntries(cache, _options.MaxMemoryCacheEntries.Value / 2);
        }
    }

    private static void EvictOldestEntries<T>(
        System.Collections.Concurrent.ConcurrentDictionary<string, CacheEntry<T>> cache,
        int targetCount) where T : class
    {
        // Simple eviction: remove expired entries first, then oldest entries
        var entriesToRemove = cache
            .Where(e => e.Value.IsExpired)
            .Select(e => e.Key)
            .ToList();

        foreach (var key in entriesToRemove)
        {
            cache.TryRemove(key, out _);
        }

        // If still over target, remove oldest
        if (cache.Count > targetCount)
        {
            var oldestKeys = cache
                .OrderBy(e => e.Value.CreatedAt)
                .Take(cache.Count - targetCount)
                .Select(e => e.Key)
                .ToList();

            foreach (var key in oldestKeys)
            {
                cache.TryRemove(key, out _);
            }
        }
    }

    private void ClearMemoryCacheForProject(string project)
    {
        var projectPrefix = $"project:{project}:";
        var groupPrefix = $"group:{project}:";
        var localesKey = BuildLocalesKey(project);

        // Remove project entries
        foreach (var key in _projectCache.Keys.Where(k => k.StartsWith(projectPrefix, StringComparison.Ordinal)))
        {
            _projectCache.TryRemove(key, out _);
        }

        // Remove group entries
        foreach (var key in _groupCache.Keys.Where(k => k.StartsWith(groupPrefix, StringComparison.Ordinal)))
        {
            _groupCache.TryRemove(key, out _);
        }

        // Remove locales entry
        _localesCache.TryRemove(localesKey, out _);
    }

    private static string BuildProjectKey(string project, string lang) => $"project:{project}:{lang}";
    private static string BuildGroupKey(string project, string group, string lang) => $"group:{project}:{group}:{lang}";
    private static string BuildLocalesKey(string project) => $"locales:{project}";

    /// <summary>
    /// Represents a cache entry with optional expiration.
    /// </summary>
    private sealed class CacheEntry<T>(T value, DateTimeOffset? expiresAt) where T : class
    {
        public T Value { get; } = value;
        public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ExpiresAt { get; } = expiresAt;

        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;
    }
}
