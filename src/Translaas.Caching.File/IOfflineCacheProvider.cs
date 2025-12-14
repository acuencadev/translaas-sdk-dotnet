using System.Threading;
using System.Threading.Tasks;

using Translaas.Caching.File.Models;
using Translaas.Models.Responses;

namespace Translaas.Caching.File;

/// <summary>
/// Provides file-based offline caching for Translaas translation data.
/// </summary>
public interface IOfflineCacheProvider
{
    /// <summary>
    /// Gets a cached translation project.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="lang">The language code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached translation project, or null if not found in cache.</returns>
    Task<TranslationProject?> GetProjectAsync(
        string project,
        string lang,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cached translation group from a cached project.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="group">The group name.</param>
    /// <param name="lang">The language code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached translation group, or null if not found in cache.</returns>
    Task<TranslationGroup?> GetGroupAsync(
        string project,
        string group,
        string lang,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cached project locales.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached project locales, or null if not found in cache.</returns>
    Task<ProjectLocales?> GetProjectLocalesAsync(
        string project,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a translation project to the cache.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="lang">The language code.</param>
    /// <param name="data">The translation project data to cache.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveProjectAsync(
        string project,
        string lang,
        TranslationProject data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves project locales to the cache.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="locales">The project locales to cache.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveProjectLocalesAsync(
        string project,
        ProjectLocales locales,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a project is cached for a specific language.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="lang">The language code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the project is cached for the specified language; otherwise, false.</returns>
    Task<bool> IsCachedAsync(
        string project,
        string lang,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all cached data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ClearAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears cached data for a specific project.
    /// </summary>
    /// <param name="project">The project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ClearProjectAsync(
        string project,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the cache manifest containing metadata about cached projects.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cache manifest.</returns>
    Task<CacheManifest> GetManifestAsync(CancellationToken cancellationToken = default);
}
