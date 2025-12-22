---
name: Feature
about: Track new features and enhancements
title: "[Feature] Implement offline file-based caching for translation projects"
labels: 'enhancement'
assignees: ''
---

**Description:**

Implement a file-based offline caching system that allows users to cache entire translation projects locally in JSON files. This enables applications to function without network connectivity by falling back to cached translations when the Translaas API is unavailable.

**Current Behavior:**

- The SDK only works in online mode, requiring network connectivity to fetch translations
- In-memory caching exists but does not persist across application restarts
- If the API is unreachable, translation requests fail with no fallback option
- Users cannot pre-download translations for known offline scenarios

**Expected Behavior:**

- Users can enable file-based offline caching via DI configuration
- Translation projects are cached locally in JSON files that persist across restarts
- When offline, the SDK automatically falls back to cached translations
- Users can configure fallback behavior (CacheFirst, ApiFirst, CacheOnly, ApiOnlyWithBackup)
- Background synchronization keeps cache up-to-date when online
- Users can pre-configure specific projects and languages to cache

**Acceptance Criteria:**

- [ ] New `OfflineCacheOptions` class with `Enabled`, `CacheDirectory`, `FallbackMode`, `AutoSync`, `AutoSyncInterval`, `Projects`, and `Languages` properties
- [ ] New `OfflineFallbackMode` enum with `CacheFirst`, `ApiFirst`, `CacheOnly`, and `ApiOnlyWithBackup` values
- [ ] `IOfflineCacheProvider` interface for file-based cache operations
- [ ] `FileCacheProvider` implementation with read/write/clear operations
- [ ] Cache manifest (`manifest.json`) tracking cached projects and sync status
- [ ] `IOfflineCacheSyncService` interface for cache synchronization
- [ ] `OfflineCacheSyncService` implementation with background sync support
- [ ] DI integration via `TranslaasOptions.OfflineCache` property
- [ ] Support for both fluent API and `appsettings.json` configuration
- [ ] Thread-safe file operations with proper locking
- [ ] Atomic manifest updates (write to temp, then rename)
- [ ] All public APIs have XML documentation
- [ ] Unit tests for all new components (80%+ coverage)
- [ ] Integration tests for cache sync scenarios

**Technical Notes:**

- Must work across all target frameworks: `netstandard2.0`, `net6.0`, `net8.0`, `net10.0`
- Use `System.Text.Json` for all serialization (no Newtonsoft.Json)
- File operations should use `async`/`await` with proper `CancellationToken` support
- Cache directory defaults to `.translaas-cache` relative to application base directory
- Manifest updates must be atomic to prevent corruption
- Background sync should use `IHostedService` pattern for ASP.NET Core integration
- Memory cache (L1) + File cache (L2) hybrid mode for optimal performance
- File locks prevent concurrent write corruption
- `SemaphoreSlim` prevents overlapping sync operations

**Cache File Structure:**
```
{CacheDirectory}/
├── manifest.json
├── {project-id}/
│   ├── locales.json
│   └── {lang}/
│       └── project.json
```

**Implementation Plan:**

**Phase 1 - Core File Cache Provider:**
- [ ] Create `Translaas.Caching.File` project
- [ ] Add project to solution file
- [ ] Implement `OfflineCacheOptions` class
- [ ] Implement `OfflineFallbackMode` enum
- [ ] Implement `IOfflineCacheProvider` interface
- [ ] Implement `FileCacheProvider` class
- [ ] Implement `CacheManifest` and `ProjectCacheInfo` models
- [ ] Implement `CacheSyncStatus` enum
- [ ] Add internal `FileSystemHelpers` for atomic operations
- [ ] Write unit tests for `FileCacheProvider`

**Phase 2 - DI Integration:**
- [ ] Add `OfflineCache` property to `TranslaasOptions`
- [ ] Update `ServiceCollectionExtensions` to register file cache services
- [ ] Integrate offline cache into `ITranslaasClient` resolution pipeline
- [ ] Support `appsettings.json` binding for `OfflineCacheOptions`
- [ ] Write unit tests for DI registration

**Phase 3 - Sync Service:**
- [ ] Implement `IOfflineCacheSyncService` interface
- [ ] Implement `OfflineCacheSyncService` class
- [ ] Implement `CacheSyncEventArgs` and `CacheSyncErrorEventArgs`
- [ ] Implement background sync using `IHostedService`
- [ ] Add `SyncCompleted` and `SyncFailed` events
- [ ] Write unit tests for sync service

**Phase 4 - Error Handling:**
- [ ] Implement `TranslaasOfflineCacheException` class
- [ ] Implement `TranslaasOfflineCacheMissException` class
- [ ] Add proper error handling for file I/O failures
- [ ] Add proper error handling for corrupted cache files
- [ ] Write unit tests for error scenarios

**Phase 5 - Integration & Documentation:**
- [ ] Create integration tests for full offline workflow
- [ ] Update README with offline caching documentation
- [ ] Add sample code demonstrating offline cache configuration
- [ ] Performance testing with large translation files

**Files to Modify:**

New files:
- `src/Translaas.Caching.File/Translaas.Caching.File.csproj`
- `src/Translaas.Caching.File/IOfflineCacheProvider.cs`
- `src/Translaas.Caching.File/FileCacheProvider.cs`
- `src/Translaas.Caching.File/IOfflineCacheSyncService.cs`
- `src/Translaas.Caching.File/OfflineCacheSyncService.cs`
- `src/Translaas.Caching.File/OfflineCacheOptions.cs`
- `src/Translaas.Caching.File/OfflineFallbackMode.cs`
- `src/Translaas.Caching.File/Models/CacheManifest.cs`
- `src/Translaas.Caching.File/Models/ProjectCacheInfo.cs`
- `src/Translaas.Caching.File/Models/CachedProject.cs`
- `src/Translaas.Caching.File/Models/CacheSyncEventArgs.cs`
- `src/Translaas.Caching.File/Internal/FileSystemHelpers.cs`
- `src/Translaas.Models/Errors/TranslaasOfflineCacheException.cs`
- `src/Translaas.Models/Errors/TranslaasOfflineCacheMissException.cs`
- `tests/Translaas.Caching.File.Tests/Translaas.Caching.File.Tests.csproj`
- `tests/Translaas.Caching.File.Tests/FileCacheProviderTests.cs`
- `tests/Translaas.Caching.File.Tests/OfflineCacheSyncServiceTests.cs`
- `tests/Translaas.Caching.File.Tests/CacheManifestTests.cs`

Modified files:
- `Translaas.SDK.slnx` (add new projects)
- `src/Translaas.Extensions.DependencyInjection/TranslaasOptions.cs` (add OfflineCache property)
- `src/Translaas.Extensions.DependencyInjection/ServiceCollectionExtensions.cs` (register file cache services)
- `README.md` (add offline caching documentation)

**Additional Context:**

- Priority level: **High** - Enables critical offline use cases
- Estimated effort: **Large** - Multiple phases with new project creation
- Related issues or PRs: None
- Spec document: `docs/specs/OFFLINE_CACHE_SPEC.md`

**Open Questions:**

1. Should we support environment variable expansion in `CacheDirectory` (e.g., `%APPDATA%`)?
2. Should cached JSON files support optional encryption for sensitive translations?
3. Should there be a maximum cache size limit, or leave unlimited?
4. When should cache be considered "stale" - after `AutoSyncInterval`, or configurable separately?
5. Should we support caching individual groups in addition to entire projects?
6. Should cache files be compressed (gzip) or plain JSON?
7. Should pre-caching block application startup or always run in background?
