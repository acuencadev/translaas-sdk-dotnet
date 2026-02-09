using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Translaas.Caching.File.Models;
using Translaas.Models.Errors;
using Translaas.Models.Responses;

namespace Translaas.Caching.File;

/// <summary>
/// Provides file-based offline caching implementation using JSON files.
/// </summary>
public class FileCacheProvider : IOfflineCacheProvider
{
    private const string ManifestFileName = "manifest.json";
    private const string LocalesFileName = "locales.json";
    private const string ProjectFileName = "project.json";

    private readonly string _cacheDirectory;
    private readonly SemaphoreSlim _manifestLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileCacheProvider"/> class.
    /// </summary>
    /// <param name="options">The offline cache options.</param>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    public FileCacheProvider(OfflineCacheOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _cacheDirectory = ResolveCacheDirectory(options.CacheDirectory);
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileCacheProvider"/> class with a specific directory.
    /// </summary>
    /// <param name="cacheDirectory">The cache directory path.</param>
    /// <exception cref="ArgumentException">Thrown when cacheDirectory is null or whitespace.</exception>
    public FileCacheProvider(string cacheDirectory)
    {
        if (string.IsNullOrWhiteSpace(cacheDirectory))
        {
            throw new ArgumentException("Cache directory cannot be null or whitespace.", nameof(cacheDirectory));
        }

        _cacheDirectory = ResolveCacheDirectory(cacheDirectory);
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <inheritdoc />
    public async Task<TranslationProject?> GetProjectAsync(
        string project,
        string lang,
        CancellationToken cancellationToken = default)
    {
        ValidateProjectAndLang(project, lang);

        var filePath = GetProjectFilePath(project, lang);
        if (!System.IO.File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var cachedProject = await ReadJsonFileAsync<CachedProject>(filePath, cancellationToken).ConfigureAwait(false);

            // Check if cache has expired
            if (cachedProject?.ExpiresAt.HasValue == true && cachedProject.ExpiresAt.Value < DateTimeOffset.UtcNow)
            {
                return null;
            }

            return cachedProject?.Data;
        }
        catch (JsonException ex)
        {
            throw new TranslaasOfflineCacheException(
                $"Failed to deserialize cached project '{project}' for language '{lang}'.",
                _cacheDirectory, project, lang, ex);
        }
        catch (IOException ex)
        {
            throw new TranslaasOfflineCacheException(
                $"Failed to read cached project '{project}' for language '{lang}'.",
                _cacheDirectory, project, lang, ex);
        }
    }

    /// <inheritdoc />
    public async Task<TranslationGroup?> GetGroupAsync(
        string project,
        string group,
        string lang,
        CancellationToken cancellationToken = default)
    {
        ValidateProjectAndLang(project, lang);
        if (string.IsNullOrWhiteSpace(group))
        {
            throw new ArgumentException("Group cannot be null or whitespace.", nameof(group));
        }

        var translationProject = await GetProjectAsync(project, lang, cancellationToken).ConfigureAwait(false);
        return translationProject?.GetGroup(group);
    }

    /// <inheritdoc />
    public async Task<ProjectLocales?> GetProjectLocalesAsync(
        string project,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(project))
        {
            throw new ArgumentException("Project cannot be null or whitespace.", nameof(project));
        }

        var filePath = GetLocalesFilePath(project);
        if (!System.IO.File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var cachedLocales = await ReadJsonFileAsync<CachedLocales>(filePath, cancellationToken).ConfigureAwait(false);

            // Check if cache has expired
            if (cachedLocales?.ExpiresAt.HasValue == true && cachedLocales.ExpiresAt.Value < DateTimeOffset.UtcNow)
            {
                return null;
            }

            return cachedLocales?.Data;
        }
        catch (JsonException ex)
        {
            throw new TranslaasOfflineCacheException(
                $"Failed to deserialize cached locales for project '{project}'.",
                _cacheDirectory, project, null, ex);
        }
        catch (IOException ex)
        {
            throw new TranslaasOfflineCacheException(
                $"Failed to read cached locales for project '{project}'.",
                _cacheDirectory, project, null, ex);
        }
    }

    /// <inheritdoc />
    public async Task SaveProjectAsync(
        string project,
        string lang,
        TranslationProject data,
        CancellationToken cancellationToken = default)
    {
        ValidateProjectAndLang(project, lang);
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var filePath = GetProjectFilePath(project, lang);
        var directory = Path.GetDirectoryName(filePath);

        try
        {
            EnsureDirectoryExists(directory!);

            var cachedProject = new CachedProject
            {
                CachedAt = DateTimeOffset.UtcNow,
                ExpiresAt = null,
                Data = data
            };

            await WriteJsonFileAtomicAsync(filePath, cachedProject, cancellationToken).ConfigureAwait(false);
            
            await UpdateManifestForProjectAsync(project, lang, CacheSyncStatus.Synced, cancellationToken).ConfigureAwait(false);
        }
        catch (IOException ex)
        {
            throw new TranslaasOfflineCacheException(
                $"Failed to save project '{project}' for language '{lang}' to cache.",
                _cacheDirectory, project, lang, ex);
        }
        catch (Exception ex)
        {
            throw new TranslaasOfflineCacheException(
                $"Unexpected error saving project '{project}' for language '{lang}' to cache: {ex.Message}",
                _cacheDirectory, project, lang, ex);
        }
    }

    /// <inheritdoc />
    public async Task SaveProjectLocalesAsync(
        string project,
        ProjectLocales locales,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(project))
        {
            throw new ArgumentException("Project cannot be null or whitespace.", nameof(project));
        }

        if (locales == null)
        {
            throw new ArgumentNullException(nameof(locales));
        }

        var filePath = GetLocalesFilePath(project);
        var directory = Path.GetDirectoryName(filePath);

        try
        {
            EnsureDirectoryExists(directory!);

            var cachedLocales = new CachedLocales
            {
                CachedAt = DateTimeOffset.UtcNow,
                ExpiresAt = null,
                Data = locales
            };

            await WriteJsonFileAtomicAsync(filePath, cachedLocales, cancellationToken).ConfigureAwait(false);
        }
        catch (IOException ex)
        {
            throw new TranslaasOfflineCacheException(
                $"Failed to save locales for project '{project}' to cache.",
                _cacheDirectory, project, null, ex);
        }
    }

    /// <inheritdoc />
    public Task<bool> IsCachedAsync(
        string project,
        string lang,
        CancellationToken cancellationToken = default)
    {
        ValidateProjectAndLang(project, lang);

        var filePath = GetProjectFilePath(project, lang);
        return Task.FromResult(System.IO.File.Exists(filePath));
    }

    /// <inheritdoc />
    public Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (Directory.Exists(_cacheDirectory))
            {
                Directory.Delete(_cacheDirectory, recursive: true);
            }

            return Task.CompletedTask;
        }
        catch (IOException ex)
        {
            throw new TranslaasOfflineCacheException(
                "Failed to clear all cached data.",
                _cacheDirectory, null, null, ex);
        }
    }

    /// <inheritdoc />
    public async Task ClearProjectAsync(
        string project,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(project))
        {
            throw new ArgumentException("Project cannot be null or whitespace.", nameof(project));
        }

        var projectDirectory = GetProjectDirectory(project);

        try
        {
            if (Directory.Exists(projectDirectory))
            {
                Directory.Delete(projectDirectory, recursive: true);
            }

            await RemoveProjectFromManifestAsync(project, cancellationToken).ConfigureAwait(false);
        }
        catch (IOException ex)
        {
            throw new TranslaasOfflineCacheException(
                $"Failed to clear cached data for project '{project}'.",
                _cacheDirectory, project, null, ex);
        }
    }

    /// <inheritdoc />
    public async Task<CacheManifest> GetManifestAsync(CancellationToken cancellationToken = default)
    {
        var manifestPath = GetManifestFilePath();

        if (!System.IO.File.Exists(manifestPath))
        {
            return CreateNewManifest();
        }

        try
        {
            var manifest = await ReadJsonFileAsync<CacheManifest>(manifestPath, cancellationToken).ConfigureAwait(false);
            return manifest ?? CreateNewManifest();
        }
        catch (JsonException)
        {
            // Manifest is corrupted, create a new one
            return CreateNewManifest();
        }
        catch (IOException)
        {
            // Can't read manifest, create a new one
            return CreateNewManifest();
        }
    }

    private static string ResolveCacheDirectory(string cacheDirectory)
    {
        if (Path.IsPathRooted(cacheDirectory))
        {
            return cacheDirectory;
        }

        // Resolve relative to application base directory
        var baseDirectory = AppContext.BaseDirectory;
        return Path.Combine(baseDirectory, cacheDirectory);
    }

    private static void ValidateProjectAndLang(string project, string lang)
    {
        if (string.IsNullOrWhiteSpace(project))
        {
            throw new ArgumentException("Project cannot be null or whitespace.", nameof(project));
        }

        if (string.IsNullOrWhiteSpace(lang))
        {
            throw new ArgumentException("Language cannot be null or whitespace.", nameof(lang));
        }
    }

    private static void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static CacheManifest CreateNewManifest()
    {
        var sdkVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

        return new CacheManifest
        {
            Version = CacheManifest.CurrentVersion,
            SdkVersion = sdkVersion,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private string GetManifestFilePath() => Path.Combine(_cacheDirectory, ManifestFileName);

    private string GetProjectDirectory(string project) => Path.Combine(_cacheDirectory, SanitizeFileName(project));

    private string GetLanguageDirectory(string project, string lang) =>
        Path.Combine(GetProjectDirectory(project), SanitizeFileName(lang));

    private string GetProjectFilePath(string project, string lang) =>
        Path.Combine(GetLanguageDirectory(project, lang), ProjectFileName);

    private string GetLocalesFilePath(string project) =>
        Path.Combine(GetProjectDirectory(project), LocalesFileName);

    private static string SanitizeFileName(string name)
    {
        // Replace invalid file name characters with underscores
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            name = name.Replace(c, '_');
        }

        return name;
    }

    private async Task<T?> ReadJsonFileAsync<T>(string filePath, CancellationToken cancellationToken) where T : class
    {
#if NETSTANDARD2_0
        var json = System.IO.File.ReadAllText(filePath);
        await Task.CompletedTask;
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
#else
        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false);
#endif
    }

    private async Task WriteJsonFileAtomicAsync<T>(string filePath, T data, CancellationToken cancellationToken)
    {
        // Write to a temp file first, then rename for atomicity
        var tempFilePath = filePath + ".tmp";

        try
        {
#if NETSTANDARD2_0
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            System.IO.File.WriteAllText(tempFilePath, json);
            await Task.CompletedTask;
#else
            await using (var stream = new FileStream(
                tempFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true))
            {
                await JsonSerializer.SerializeAsync(stream, data, _jsonOptions, cancellationToken).ConfigureAwait(false);
            }
#endif

            // Atomic rename (delete target first if it exists)
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            System.IO.File.Move(tempFilePath, filePath);
        }
        finally
        {
            // Clean up temp file if it still exists
            if (System.IO.File.Exists(tempFilePath))
            {
                try
                {
                    System.IO.File.Delete(tempFilePath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }

    private async Task UpdateManifestForProjectAsync(
        string project,
        string lang,
        CacheSyncStatus status,
        CancellationToken cancellationToken)
    {
        await _manifestLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var manifest = await GetManifestAsync(cancellationToken).ConfigureAwait(false);

            if (!manifest.Projects.TryGetValue(project, out var projectInfo))
            {
                projectInfo = new ProjectCacheInfo();
                manifest.Projects[project] = projectInfo;
            }

            if (!projectInfo.Languages.Contains(lang))
            {
                projectInfo.Languages.Add(lang);
            }

            projectInfo.LastSyncAt = DateTimeOffset.UtcNow;
            projectInfo.Status = status;
            manifest.LastSyncAt = DateTimeOffset.UtcNow;

            var manifestPath = GetManifestFilePath();
            EnsureDirectoryExists(_cacheDirectory);
            await WriteJsonFileAtomicAsync(manifestPath, manifest, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _manifestLock.Release();
        }
    }

    private async Task RemoveProjectFromManifestAsync(string project, CancellationToken cancellationToken)
    {
        await _manifestLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var manifest = await GetManifestAsync(cancellationToken).ConfigureAwait(false);

            if (manifest.Projects.Remove(project))
            {
                var manifestPath = GetManifestFilePath();
                await WriteJsonFileAtomicAsync(manifestPath, manifest, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            _manifestLock.Release();
        }
    }
}
