# Translaas SDK Offline Mode Sample

This sample demonstrates how to use the Translaas SDK with **offline caching** and supports all three fallback modes:
- **CacheOnly** - Only use cache, never call API (fully offline)
- **CacheFirst** - Check cache first, fall back to API on miss
- **ApiFirst** - Call API first, fall back to cache on API failure

## Key Features

- ✅ **Multiple Fallback Modes** - Test all three offline fallback modes
- ✅ **Interactive Mode Selection** - Choose mode at runtime or via command line
- ✅ **True Offline Mode** - CacheOnly mode works entirely without network connectivity
- ✅ **Cache + API Hybrid** - CacheFirst and ApiFirst modes demonstrate cache/API interaction
- ✅ **Error Handling** - Shows how to handle `TranslaasOfflineCacheMissException`
- ✅ **Complete Examples** - Demonstrates all SDK features (translations, pluralization, parameters, groups, locales)

## Prerequisites

- .NET 8.0 SDK or later
- Cache files must exist in the `cache` directory (included in this sample)

## Cache Files

This sample includes pre-populated cache files in the `cache` directory:

```
cache/
├── manifest.json
└── translaas-sdk-samples/
    ├── locales.json
    └── en/
        └── project.json
```

The cache files contain sample translations for:
- **Project**: `translaas-sdk-samples`
- **Language**: `en` (English)
- **Groups**: `common`, `messages`, `errors`

## Configuration

The sample is configured via `appsettings.json`:

```json
{
  "Translaas": {
    "DefaultLanguage": "ru",
    "ApiKey": "your-api-key-here",
    "BaseUrl": "https://api.translaas.com",
    "OfflineCache": {
      "Enabled": true,
      "CacheDirectory": "./cache",
      "FallbackMode": "CacheOnly",
      "AutoSync": false,
      "DefaultProjectId": "translaas-sdk-samples"
    }
  }
}
```

**Important Configuration Points:**

- `FallbackMode` - **Note**: This value in `appsettings.json` is ignored. The mode is selected at runtime via the interactive menu or command line argument.
- `AutoSync: false` - Disables background synchronization (prevents automatic cache updates)
- `CacheDirectory: "./cache"` - Points to the local cache directory
- `DefaultProjectId` - **Required**: The project ID that matches your cache files (must match the directory name in cache folder)
- `DefaultLanguage` - The default language to use (e.g., "en", "fr", "es", "ru")
- `ApiKey` - **Required** for CacheFirst and ApiFirst modes. Must be set to your actual API key. **Optional** in CacheOnly mode (not used since API is never called)
- `BaseUrl` - **Required** for CacheFirst and ApiFirst modes. Defaults to `https://api.translaas.com` if not specified. **Optional** in CacheOnly mode (not used since API is never called)

**Note:** When running CacheFirst or ApiFirst modes, the sample will throw an exception if `ApiKey` or `BaseUrl` are missing or empty. Make sure to set your actual API key in `appsettings.json` or use user secrets for production scenarios.

### Mode-Specific Requirements

| Mode | ApiKey Required | BaseUrl Required | Network Required |
|------|----------------|------------------|------------------|
| **CacheOnly** | ❌ No | ❌ No | ❌ No |
| **CacheFirst** | ✅ Yes | ✅ Yes | ✅ Yes (for cache misses) |
| **ApiFirst** | ✅ Yes | ✅ Yes | ✅ Yes (primary) |

**Language Resolution:**

The sample is configured to use only `UseDefault()` provider (not `UseCulture()`). This ensures that the configured `DefaultLanguage` from `appsettings.json` is always used, regardless of the thread's current culture. This is important for offline mode where you want predictable language selection based on configuration rather than system settings.

## Running the Sample

### Interactive Mode Selection

Run without arguments to see an interactive menu:

```bash
dotnet run --project samples/Translaas.Samples.Offline
```

You'll be prompted to select a fallback mode:
1. **CacheOnly** - Only use cache, never call API (fully offline)
2. **CacheFirst** - Check cache first, fall back to API on miss
3. **ApiFirst** - Call API first, fall back to cache on API failure

### Command Line Mode Selection

You can also specify the mode directly:

```bash
# CacheOnly mode
dotnet run --project samples/Translaas.Samples.Offline -- CacheOnly

# CacheFirst mode
dotnet run --project samples/Translaas.Samples.Offline -- CacheFirst

# ApiFirst mode
dotnet run --project samples/Translaas.Samples.Offline -- ApiFirst
```

Accepted values: `CacheOnly`, `CacheFirst`, `ApiFirst` (case-insensitive), or `1`, `2`, `3`.

### What the Sample Does

The sample will:
1. Verify cache files exist (for CacheOnly and CacheFirst modes)
2. Demonstrate various translation scenarios
3. Show error handling for cache misses
4. Display mode-specific verification messages

## Examples Demonstrated

All three modes demonstrate the same examples:

1. **Basic Translation** - Simple translation lookup
2. **Pluralization** - Handling plural forms (1 item vs 5 items)
3. **Named Parameters** - Translations with parameter substitution
4. **Number + Parameters** - Combining pluralization with parameters
5. **Multiple Entries** - Fetching multiple translations
6. **Translation Groups** - Getting entire groups at once
7. **Project Locales** - Getting available locales
8. **Language Resolution** - Automatic language resolution
9. **Explicit Override** - Overriding language explicitly
10. **Mode Verification** - Confirming mode-specific behavior

The difference between modes is in how they handle cache hits/misses and API calls, which is demonstrated in the verification step.

## Cache File Format

The cache files follow the Translaas SDK cache format:

### manifest.json
```json
{
  "version": "1.0",
  "sdkVersion": "1.0.0",
  "createdAt": "2025-01-27T10:00:00Z",
  "lastSyncAt": "2025-01-27T10:00:00Z",
  "projects": {
    "translaas-sdk-samples": {
      "languages": ["en", "es", "fr"],
      "lastSyncAt": "2025-01-27T10:00:00Z",
      "status": "synced"
    }
  }
}
```

### locales.json
```json
{
  "cachedAt": "2025-01-27T10:00:00Z",
  "expiresAt": null,
  "data": {
    "locales": ["en", "es", "fr"]
  }
}
```

### project.json
```json
{
  "cachedAt": "2025-01-27T10:00:00Z",
  "expiresAt": null,
  "data": {
    "common": {
      "welcome": "Welcome",
      "app.name": "Translaas SDK Sample"
    },
    "messages": {
      "greeting": "Hello, {userName}!",
      "item": {
        "one": "1 item",
        "other": "{count} items"
      }
    }
  }
}
```

## Error Handling

When a translation is not found in the cache, the SDK throws `TranslaasOfflineCacheMissException`:

```csharp
try
{
    var translation = await translaasService.T("group", "entry");
}
catch (TranslaasOfflineCacheMissException ex)
{
    Console.WriteLine($"Cache miss: Project={ex.Project}, Language={ex.Language}, Group={ex.Group}, Entry={ex.Entry}");
}
```

**Mode-Specific Behavior:**

- **CacheOnly**: Missing translations cannot be fetched from the API. You must ensure all required translations are present in the cache files.
- **CacheFirst**: Missing translations will be fetched from the API and cached for future use.
- **ApiFirst**: API failures will fall back to cache. Successful API calls update the cache.

## Creating Your Own Cache Files

To use this sample with your own translations:

1. **Create the directory structure:**
   ```
   cache/
   └── {your-project-id}/
       └── {lang}/
           └── project.json
   ```

2. **Create project.json** with your translations:
   ```json
   {
     "cachedAt": "2025-01-27T10:00:00Z",
     "expiresAt": null,
     "data": {
       "your-group": {
         "your-entry": "Your translation"
       }
     }
   }
   ```

3. **Update appsettings.json** to use your project ID and language

4. **Run the sample** - It will use your cache files

## Testing Different Modes

### Testing CacheOnly Mode

1. **Disconnect from network** (or use an invalid API key/BaseUrl)
2. **Select CacheOnly mode** when running the sample
3. **Run the sample** - It should work perfectly from cache
4. **Request a non-existent translation** - Should throw `TranslaasOfflineCacheMissException`
5. **Check logs** - No HTTP requests should be logged

### Testing CacheFirst Mode

1. **Ensure network connectivity** and valid API credentials
2. **Select CacheFirst mode** when running the sample
3. **Run the sample** - First requests may call API, subsequent requests use cache
4. **Disconnect network** - Cached translations should still work
5. **Request new translation** - Should call API (if online) or throw exception (if offline)

### Testing ApiFirst Mode

1. **Ensure network connectivity** and valid API credentials
2. **Select ApiFirst mode** when running the sample
3. **Run the sample** - All requests call API first, cache is updated in background
4. **Disconnect network** - Should fall back to cache for previously fetched translations
5. **Request new translation** - Should fail (API unavailable) and fall back to cache if available

## Related Documentation

- [Offline Mode Guide](../../docs/OFFLINE_MODE.md) - Complete offline mode documentation
- [Caching Documentation](../../docs/CACHING.md) - Caching system overview
- [Offline Cache Specification](../../docs/specs/OFFLINE_CACHE_SPEC.md) - Technical specification

## Sample Structure

The sample is organized into separate classes:

- **`OfflineSampleBase`** - Base class containing shared example logic
- **`CacheOnlySample`** - Implementation for CacheOnly mode
- **`CacheFirstSample`** - Implementation for CacheFirst mode
- **`ApiFirstSample`** - Implementation for ApiFirst mode
- **`Program.cs`** - Entry point with mode selection logic

This structure allows easy comparison between modes and makes it simple to add new modes in the future.

## Differences from Regular Console Sample

This sample differs from `Translaas.Samples.Console` in:

- **Multiple Modes**: Supports all three offline fallback modes (CacheOnly, CacheFirst, ApiFirst)
- **Mode Selection**: Interactive menu or command-line argument to choose mode
- **Cache Files**: Includes pre-populated cache files
- **Error Handling**: Demonstrates `TranslaasOfflineCacheMissException`
- **Mode Verification**: Confirms mode-specific behavior (cache-only vs cache+API)

## Troubleshooting

### Cache Not Found

**Error:** `TranslaasOfflineCacheMissException`

**Solution:** Ensure cache files exist in the `cache` directory with the correct structure.

### Invalid JSON Format

**Error:** `TranslaasOfflineCacheException` with inner `JsonException`

**Solution:** Validate JSON syntax in cache files. Use a JSON validator.

### Wrong Project ID or Language

**Error:** Cache miss for expected translations

**Solution:** Verify the project ID and language code in `appsettings.json` match the cache directory structure.
