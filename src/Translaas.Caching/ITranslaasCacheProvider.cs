namespace Translaas.Caching;

/// <summary>
/// Provides caching functionality for Translaas translation data.
/// </summary>
public interface ITranslaasCacheProvider
{
    /// <summary>
    /// Attempts to retrieve a value from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">When this method returns, contains the cached value if found; otherwise, the default value for the type.</param>
    /// <returns>True if the value was found in the cache; otherwise, false.</returns>
    bool TryGetValue<T>(string key, out T? value);

    /// <summary>
    /// Sets a value in the cache with optional expiration settings.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="absoluteExpiration">Optional absolute expiration time.</param>
    /// <param name="slidingExpiration">Optional sliding expiration time.</param>
    void Set<T>(string key, T value, System.TimeSpan? absoluteExpiration = null, System.TimeSpan? slidingExpiration = null);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    void Remove(string key);
}
