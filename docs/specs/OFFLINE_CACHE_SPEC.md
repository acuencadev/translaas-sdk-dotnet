# Translaas SDK - Offline File Cache Specification

## 1. Overview

This specification describes the implementation of a file-based offline caching system for the Translaas SDK. This feature allows users to cache entire translation projects locally in JSON files, enabling offline operation when the Translaas API is unavailable.

## 2. Goals

- **Offline Support**: Allow applications to function without network connectivity
- **Persistence**: Cache translations to survive application restarts
- **Seamless Integration**: Configure via standard .NET DI patterns
- **Backward Compatibility**: Existing in-memory caching continues to work unchanged
- **Hybrid Mode**: Support combinations of file cache + memory cache for optimal performance

## 3. Proposed Configuration API

### 3.1 New Options Properties

Add to `TranslaasOptions`:

```csharp
/// <summary>
/// Configuration options for file-based offline caching.
/// </summary>
public class OfflineCacheOptions
{
    /// <summary>
    /// Gets or sets whether offline file caching is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the directory path for storing cache files.
    /// Can be absolute or relative to the application base directory.
    /// </summary>
    /// <remarks>
    /// Defaults to ".translaas-cache" in the application's base directory.
    /// </remarks>
    public string CacheDirectory { get; set; } = ".translaas-cache";

    /// <summary>
    /// Gets or sets the fallback behavior when offline cache is enabled.
    /// </summary>
    public OfflineFallbackMode FallbackMode { get; set; } = OfflineFallbackMode.CacheFirst;

    /// <summary>
    /// Gets or sets whether to automatically sync cache when online.
    /// </summary>
    public bool AutoSync { get; set; } = true;

    /// <summary>
    /// Gets or sets the interval for automatic cache synchronization.
    /// Only applies when AutoSync is true.
    /// </summary>
    public TimeSpan? AutoSyncInterval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets or sets the list of project IDs to pre-cache.
    /// When specified, these projects will be automatically downloaded and cached.
    /// </summary>
    public List<string> Projects { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of language codes to pre-cache.
    /// When specified, only these languages will be cached for each project.
    /// If empty, all available languages are cached.
    /// </summary>
    public List<string> Languages { get; set; } = new();
}

/// <summary>
/// Specifies the behavior when both online API and offline cache are available.
/// </summary>
public enum OfflineFallbackMode
{
    /// <summary>
    /// Always try cache first, fall back to API on cache miss.
    /// Best for performance when cache is likely to be warm.
    /// </summary>
    CacheFirst = 0,

    /// <summary>
    /// Always try API first, fall back to cache on API failure.
    /// Ensures freshest data when online.
    /// </summary>
    ApiFirst = 1,

    /// <summary>
    /// Use cache only, never call API (true offline mode).
    /// Useful when network is known to be unavailable.
    /// </summary>
    CacheOnly = 2,

    /// <summary>
    /// Use API only, but update cache in background.
    /// Cache serves as backup for future offline use.
    /// </summary>
    ApiOnlyWithBackup = 3
}
```

### 3.2 Updated TranslaasOptions

```csharp
public class TranslaasOptions
{
    // ... existing properties ...

    /// <summary>
    /// Gets or sets the offline caching options.
    /// </summary>
    public OfflineCacheOptions OfflineCache { get; set; } = new();
}
```

### 3.3 DI Configuration Examples

**Fluent API:**

```csharp
services.AddTranslaas(options =>
{
    options.ApiKey = "your-api-key";
    options.BaseUrl = "https://api.translaas.com";
    
    // Enable offline caching
    options.OfflineCache.Enabled = true;
    options.OfflineCache.CacheDirectory = "./translations-cache";
    options.OfflineCache.FallbackMode = OfflineFallbackMode.CacheFirst;
    options.OfflineCache.AutoSync = true;
    options.OfflineCache.AutoSyncInterval = TimeSpan.FromHours(2);
    
    // Pre-cache specific projects and languages
    options.OfflineCache.Projects.Add("my-project");
    options.OfflineCache.Languages.AddRange(new[] { "en", "es", "fr" });
});
```

**appsettings.json:**

```json
{
  "Translaas": {
    "ApiKey": "your-api-key",
    "BaseUrl": "https://api.translaas.com",
    "OfflineCache": {
      "Enabled": true,
      "CacheDirectory": "./translations-cache",
      "FallbackMode": "CacheFirst",
      "AutoSync": true,
      "AutoSyncInterval": "02:00:00",
      "Projects": ["my-project", "another-project"],
      "Languages": ["en", "es", "fr", "de"]
    }
  }
}
```

## 4. Cache File Structure

### 4.1 Directory Layout

```
{CacheDirectory}/
├── manifest.json                    # Cache metadata and version info
├── {project-id}/
│   ├── locales.json                 # Available locales for this project
│   ├── {lang}/
│   │   ├── project.json             # Full project data for this language
│   │   └── groups/
│   │       ├── {group-name}.json    # Individual group cache (optional)
│   │       └── ...
│   └── ...
└── ...
```

### 4.2 Manifest File (`manifest.json`)

```json
{
  "version": "1.0",
  "sdkVersion": "1.0.0",
  "createdAt": "2025-12-14T10:30:00Z",
  "lastSyncAt": "2025-12-14T14:30:00Z",
  "projects": {
    "my-project": {
      "languages": ["en", "es", "fr"],
      "lastSyncAt": "2025-12-14T14:30:00Z",
      "status": "synced"
    }
  }
}
```

### 4.3 Project Cache File (`project.json`)

```json
{
  "cachedAt": "2025-12-14T14:30:00Z",
  "expiresAt": null,
  "data": {
    "common": {
      "welcome": "Welcome",
      "goodbye": "Goodbye"
    },
    "errors": {
      "notFound": "Not found",
      "serverError": "Server error"
    }
  }
}
```

## 5. New Interfaces and Classes

### 5.1 `IOfflineCacheProvider` Interface

```csharp
/// <summary>
/// Provides file-based offline caching for Translaas translation data.
/// </summary>
public interface IOfflineCacheProvider
{
    /// <summary>
    /// Gets a cached translation project.
    /// </summary>
    Task<TranslationProject?> GetProjectAsync(
        string project, 
        string lang, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cached translation group.
    /// </summary>
    Task<TranslationGroup?> GetGroupAsync(
        string project, 
        string group, 
        string lang, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cached project locales.
    /// </summary>
    Task<ProjectLocales?> GetProjectLocalesAsync(
        string project, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a translation project to the cache.
    /// </summary>
    Task SaveProjectAsync(
        string project, 
        string lang, 
        TranslationProject data, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves project locales to the cache.
    /// </summary>
    Task SaveProjectLocalesAsync(
        string project, 
        ProjectLocales locales, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a project is cached for a specific language.
    /// </summary>
    Task<bool> IsCachedAsync(
        string project, 
        string lang, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all cached data.
    /// </summary>
    Task ClearAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears cached data for a specific project.
    /// </summary>
    Task ClearProjectAsync(
        string project, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the cache manifest containing metadata about cached projects.
    /// </summary>
    Task<CacheManifest> GetManifestAsync(CancellationToken cancellationToken = default);
}
```

### 5.2 `IOfflineCacheSyncService` Interface

```csharp
/// <summary>
/// Service for synchronizing offline cache with the Translaas API.
/// </summary>
public interface IOfflineCacheSyncService
{
    /// <summary>
    /// Synchronizes a specific project and language to the cache.
    /// </summary>
    Task SyncProjectAsync(
        string project, 
        string lang, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes all configured projects and languages.
    /// </summary>
    Task SyncAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts background synchronization based on configured interval.
    /// </summary>
    Task StartBackgroundSyncAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops background synchronization.
    /// </summary>
    Task StopBackgroundSyncAsync();

    /// <summary>
    /// Event raised when synchronization completes.
    /// </summary>
    event EventHandler<CacheSyncEventArgs>? SyncCompleted;

    /// <summary>
    /// Event raised when synchronization fails.
    /// </summary>
    event EventHandler<CacheSyncErrorEventArgs>? SyncFailed;
}
```

### 5.3 `CacheManifest` Model

```csharp
/// <summary>
/// Represents metadata about the offline cache.
/// </summary>
public class CacheManifest
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("sdkVersion")]
    public string SdkVersion { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("lastSyncAt")]
    public DateTimeOffset? LastSyncAt { get; set; }

    [JsonPropertyName("projects")]
    public Dictionary<string, ProjectCacheInfo> Projects { get; set; } = new();
}

/// <summary>
/// Information about a cached project.
/// </summary>
public class ProjectCacheInfo
{
    [JsonPropertyName("languages")]
    public List<string> Languages { get; set; } = new();

    [JsonPropertyName("lastSyncAt")]
    public DateTimeOffset? LastSyncAt { get; set; }

    [JsonPropertyName("status")]
    public CacheSyncStatus Status { get; set; }
}

/// <summary>
/// Status of cache synchronization.
/// </summary>
public enum CacheSyncStatus
{
    Pending,
    Syncing,
    Synced,
    Failed,
    Stale
}
```

## 6. Project Structure Changes

### 6.1 New Project: `Translaas.Caching.File`

```
src/
└── Translaas.Caching.File/
    ├── Translaas.Caching.File.csproj
    ├── IOfflineCacheProvider.cs
    ├── FileCacheProvider.cs
    ├── IOfflineCacheSyncService.cs
    ├── OfflineCacheSyncService.cs
    ├── Models/
    │   ├── CacheManifest.cs
    │   ├── CachedProject.cs
    │   └── CacheSyncEventArgs.cs
    └── Internal/
        └── FileSystemHelpers.cs
```

### 6.2 Test Project: `Translaas.Caching.File.Tests`

```
tests/
└── Translaas.Caching.File.Tests/
    ├── Translaas.Caching.File.Tests.csproj
    ├── FileCacheProviderTests.cs
    ├── OfflineCacheSyncServiceTests.cs
    └── CacheManifestTests.cs
```

## 7. Behavior Specification

### 7.1 Cache Resolution Flow

```
GetEntryAsync("group", "entry", "en")
                │
                ▼
    ┌─────────────────────┐
    │ Check FallbackMode  │
    └─────────────────────┘
                │
    ┌───────────┴───────────┐
    │                       │
    ▼                       ▼
CacheFirst              ApiFirst
    │                       │
    ▼                       ▼
┌─────────┐           ┌─────────┐
│ Check   │           │ Call    │
│ Memory  │           │ API     │
│ Cache   │           │         │
└────┬────┘           └────┬────┘
     │ Miss                │ Success?
     ▼                     │
┌─────────┐                │ Yes──▶ Return + Update Cache
│ Check   │                │
│ File    │                │ No (offline/error)
│ Cache   │                │
└────┬────┘                ▼
     │ Miss          ┌─────────┐
     ▼               │ Check   │
┌─────────┐          │ File    │
│ Call    │          │ Cache   │
│ API     │          └────┬────┘
└────┬────┘               │
     │ Success?           │ Hit?
     │                    │
     │ Yes──▶ Return +    │ Yes──▶ Return
     │        Update      │
     │        Both Caches │ No──▶ Throw Exception
     │
     │ No──▶ Throw Exception
```

### 7.2 Pre-caching Behavior

On application startup (when `OfflineCache.Enabled = true` and `Projects` is configured):

1. Check if cache files exist for configured projects/languages
2. If `AutoSync = true` and cache is stale or missing:
   - Queue background sync for missing/stale items
   - Application can start immediately with whatever cache exists
3. Log warnings if no cache exists and API is unreachable

### 7.3 Background Sync Behavior

When `AutoSync = true`:

1. Start a background timer with `AutoSyncInterval`
2. On timer tick:
   - Check network connectivity (optional)
   - For each configured project/language:
     - Call API to get latest data
     - Update file cache
     - Update manifest
   - Raise `SyncCompleted` event
3. Handle errors gracefully (raise `SyncFailed`, continue with next project)

## 8. Error Handling

### 8.1 New Exception Types

```csharp
/// <summary>
/// Exception thrown when offline cache operations fail.
/// </summary>
public class TranslaasOfflineCacheException : TranslaasException
{
    public string? CacheDirectory { get; }
    public string? Project { get; }
    public string? Language { get; }
    
    // constructors...
}

/// <summary>
/// Exception thrown when translation is not found in offline cache.
/// </summary>
public class TranslaasOfflineCacheMissException : TranslaasOfflineCacheException
{
    // constructors...
}
```

### 8.2 Error Scenarios

| Scenario | Behavior |
|----------|----------|
| Cache directory not writable | Throw `TranslaasOfflineCacheException` on startup |
| Cache file corrupted | Log warning, delete file, re-sync if possible |
| API unreachable + cache miss | Throw `TranslaasOfflineCacheMissException` |
| API unreachable + cache hit | Return cached data (log info) |
| Sync failure | Raise `SyncFailed` event, keep existing cache |

## 9. Thread Safety and Concurrency

- File operations use file locks to prevent corruption
- Manifest updates are atomic (write to temp file, then rename)
- Background sync uses `SemaphoreSlim` to prevent overlapping syncs
- Read operations can proceed concurrently with background writes

## 10. Implementation Phases

### Phase 1: Core File Cache Provider
- `IOfflineCacheProvider` interface
- `FileCacheProvider` implementation
- Cache manifest management
- Basic read/write operations

### Phase 2: DI Integration
- Update `TranslaasOptions` with `OfflineCacheOptions`
- Update `ServiceCollectionExtensions` to wire up file cache
- Integrate with existing `ITranslaasClient` pipeline

### Phase 3: Sync Service
- `IOfflineCacheSyncService` interface
- `OfflineCacheSyncService` implementation
- Background sync with `IHostedService`
- Events for sync completion/failure

### Phase 4: Hybrid Caching
- Combine memory cache + file cache for optimal performance
- Memory cache acts as L1, file cache as L2

## 11. Questions for Approval

1. **Cache Directory Location**: Should we support environment variable expansion (e.g., `%APPDATA%\translaas-cache`)? Or keep it simple with relative/absolute paths only?

2. **Cache Encryption**: Should cached JSON files be encrypted for projects with sensitive translations? If yes, where does the encryption key come from?

3. **Cache Size Limits**: Should there be a maximum cache size? Or leave it unlimited (entire projects are usually small)?

4. **Stale Cache Policy**: When should cache be considered "stale"? Options:
   - Never (explicit sync only)
   - After `AutoSyncInterval`
   - After configurable `CacheExpiration` duration

5. **Group-Level Caching**: The current spec caches entire projects. Should we also support caching individual groups for more granular control?

6. **Compression**: Should cache files be compressed (gzip) to save disk space, or keep as plain JSON for easier debugging/inspection?

7. **Pre-cache Initialization**: Should pre-caching block application startup, or always run in background? Current spec assumes background.

