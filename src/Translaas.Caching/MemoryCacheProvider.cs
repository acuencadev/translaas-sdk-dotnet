using Microsoft.Extensions.Caching.Memory;

using System;

namespace Translaas.Caching;

/// <summary>
/// Provides in-memory caching implementation using IMemoryCache.
/// </summary>
public class MemoryCacheProvider : ITranslaasCacheProvider
{
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryCacheProvider"/> class.
    /// </summary>
    /// <param name="cache">The memory cache instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when cache is null.</exception>
    public MemoryCacheProvider(IMemoryCache cache)
    {
        if (cache == null)
        {
            throw new ArgumentNullException(nameof(cache));
        }

        _cache = cache;
    }

    /// <inheritdoc />
    public bool TryGetValue<T>(string key, out T? value)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        return _cache.TryGetValue(key, out value);
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        var options = new MemoryCacheEntryOptions();

        if (absoluteExpiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = absoluteExpiration.Value;
        }

        if (slidingExpiration.HasValue)
        {
            options.SlidingExpiration = slidingExpiration.Value;
        }

        _cache.Set(key, value, options);
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        _cache.Remove(key);
    }
}
