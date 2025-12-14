using System;

namespace Translaas.Models.Errors;

/// <summary>
/// Exception thrown when offline cache operations fail.
/// </summary>
public class TranslaasOfflineCacheException : TranslaasException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasOfflineCacheException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public TranslaasOfflineCacheException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasOfflineCacheException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TranslaasOfflineCacheException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslaasOfflineCacheException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="cacheDirectory">The cache directory path.</param>
    /// <param name="project">The project identifier.</param>
    /// <param name="language">The language code.</param>
    /// <param name="innerException">The inner exception.</param>
    public TranslaasOfflineCacheException(
        string message,
        string? cacheDirectory,
        string? project,
        string? language,
        Exception? innerException = null)
        : base(message, innerException)
    {
        CacheDirectory = cacheDirectory;
        Project = project;
        Language = language;
    }

    /// <summary>
    /// Gets the cache directory path where the error occurred.
    /// </summary>
    public string? CacheDirectory { get; }

    /// <summary>
    /// Gets the project identifier related to the error.
    /// </summary>
    public string? Project { get; }

    /// <summary>
    /// Gets the language code related to the error.
    /// </summary>
    public string? Language { get; }
}
