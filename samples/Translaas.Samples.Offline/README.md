# Translaas SDK Offline Mode Sample

This sample demonstrates how to use the Translaas SDK in **offline mode** using `OfflineFallbackMode.CacheOnly`. In this mode, the SDK **never attempts to connect to the backend API** and reads all translations exclusively from local cache files.

## Key Features

- ✅ **True Offline Mode** - Works entirely without network connectivity
- ✅ **No API Calls** - All translations are read from local JSON cache files
- ✅ **Cache-Only Operation** - Demonstrates `OfflineFallbackMode.CacheOnly` configuration
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
    "DefaultLanguage": "en",
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

- `FallbackMode: "CacheOnly"` - **Critical**: This prevents all API calls
- `AutoSync: false` - Disables background synchronization (prevents API calls)
- `CacheDirectory: "./cache"` - Points to the local cache directory
- `DefaultProjectId` - **Required**: The project ID that matches your cache files (must match the directory name in cache folder)
- `DefaultLanguage` - The default language to use (e.g., "en", "fr", "es")
- `ApiKey` and `BaseUrl` - **Optional** in CacheOnly mode (not used since API is never called). Required for other `OfflineFallbackMode` values (CacheFirst, ApiFirst, ApiOnlyWithBackup)

**Language Resolution:**

The sample is configured to use only `UseDefault()` provider (not `UseCulture()`). This ensures that the configured `DefaultLanguage` from `appsettings.json` is always used, regardless of the thread's current culture. This is important for offline mode where you want predictable language selection based on configuration rather than system settings.

## Running the Sample

```bash
dotnet run --project samples/Translaas.Samples.Offline
```

The sample will:
1. Verify cache files exist
2. Demonstrate various translation scenarios
3. Show error handling for cache misses
4. Confirm that no API calls were made

## Examples Demonstrated

1. **Basic Translation** - Simple translation lookup
2. **Pluralization** - Handling plural forms (1 item vs 5 items)
3. **Named Parameters** - Translations with parameter substitution
4. **Number + Parameters** - Combining pluralization with parameters
5. **Multiple Entries** - Fetching multiple translations
6. **Translation Groups** - Getting entire groups at once
7. **Project Locales** - Getting available locales
8. **Language Resolution** - Automatic language resolution
9. **Explicit Override** - Overriding language explicitly
10. **Offline Verification** - Confirming no API calls were made

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

In `CacheOnly` mode, missing translations cannot be fetched from the API, so you must ensure all required translations are present in the cache files.

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

## Testing Offline Mode

To verify offline mode is working:

1. **Disconnect from network** (or use an invalid API key/BaseUrl)
2. **Run the sample** - It should work perfectly from cache
3. **Request a non-existent translation** - Should throw `TranslaasOfflineCacheMissException`
4. **Check logs** - No HTTP requests should be logged

## Related Documentation

- [Offline Mode Guide](../../docs/OFFLINE_MODE.md) - Complete offline mode documentation
- [Caching Documentation](../../docs/CACHING.md) - Caching system overview
- [Offline Cache Specification](../../docs/specs/OFFLINE_CACHE_SPEC.md) - Technical specification

## Differences from Regular Console Sample

This sample differs from `Translaas.Samples.Console` in:

- **Configuration**: Uses `OfflineFallbackMode.CacheOnly` instead of online mode
- **No User Secrets**: API key is not required (set to dummy value)
- **Cache Files**: Includes pre-populated cache files
- **Error Handling**: Demonstrates `TranslaasOfflineCacheMissException`
- **Verification**: Confirms no API calls were made

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
