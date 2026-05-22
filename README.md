# Translaas SDK

![Tests](https://github.com/acuencadev/Translaas.SDK/workflows/CI/badge.svg)

A strongly-typed, performant, and modular .NET SDK for consuming the **Translaas Translation Delivery API**. This SDK provides a clean, dependency-injection friendly way to retrieve translations in your .NET applications.

## Features

- ✅ **Strongly-typed API** - Full IntelliSense support with strongly-typed models
- ✅ **Convenience API** - Simple `T()` method for quick translation lookups via `ITranslaasService`
- ✅ **Automatic Language Resolution** - Optional language parameter with configurable providers (HTTP request, culture, default)
- ✅ **Razor View Support** - Tag Helpers and HTML Helpers for easy translation in `.cshtml` files
- ✅ **Dependency Injection Ready** - Seamless integration with `IServiceCollection`
- ✅ **Flexible Caching** - Built-in memory caching with configurable cache modes
- ✅ **Offline Caching** - File-based caching for offline mode with automatic sync
- ✅ **Hybrid Caching** - Two-level caching (memory L1 + file L2) for optimal performance
- ✅ **Multiple Framework Support** - Compatible with .NET Standard 2.0, .NET 6, .NET 8, and .NET 10
- ✅ **Retry & Resilience** - Configurable retry policies and timeouts
- ✅ **Modular Design** - Use only what you need with separate NuGet packages
- ✅ **Async/Await** - Fully asynchronous API for optimal performance

## Installation

### Package Manager Console

```powershell
Install-Package Translaas.Extensions.DependencyInjection
```

### .NET CLI

```bash
dotnet add package Translaas.Extensions.DependencyInjection
```

### PackageReference

```xml
<PackageReference Include="Translaas.Extensions.DependencyInjection" Version="0.4.0-beta" />
```

### Individual Packages

If you prefer to use individual packages:

- `Translaas.Client` - Core HTTP client
- `Translaas.Models` - Data transfer objects
- `Translaas.Caching` - In-memory caching layer
- `Translaas.Caching.File` - File-based offline caching with hybrid caching support
- `Translaas.Extensions.Http` - HttpClientFactory integration
- `Translaas.Extensions.DependencyInjection` - Full DI integration (recommended)
- `Translaas.Extensions.Mvc` - Razor Tag Helpers and HTML Helpers for ASP.NET Core MVC

## Quick Start

### 1. Register Services

In your `Program.cs` or `Startup.cs`:

```csharp
using Translaas.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTranslaas(options =>
{
    options.ApiKey = "your-api-key-here";
    options.BaseUrl = "https://api.translaas.com"; // or your custom base URL
    options.CacheMode = CacheMode.Group; // Optional: configure caching
});
```

### 2. Inject and Use

You can use either `ITranslaasClient` (full API) or `ITranslaasService` (convenience wrapper):

**Service Lifetimes:**
- `ITranslaasClient` - Registered as **Scoped** (one instance per HTTP request in ASP.NET Core)
- `ITranslaasService` - Registered as **Scoped** (convenience wrapper around `ITranslaasClient`)
- `IOptions<TranslaasOptions>` - Registered as **Singleton** (configuration loaded once at startup)
- `IMemoryCache` - Registered as **Singleton** when caching is enabled
- `ITranslaasCacheProvider` - Registered as **Singleton** when caching is enabled

**Option A: Using ITranslaasService (Recommended for simple lookups)**

```csharp
using Translaas.Extensions.DependencyInjection;
using L = Translaas.Models.LanguageCodes;

public class MyService
{
    private readonly ITranslaasService _translaas;

    public MyService(ITranslaasService translaas)
    {
        _translaas = translaas;
    }

    public async Task<string> GetWelcomeMessageAsync()
    {
        // Use the convenient T() method
        // lang parameter is optional when language providers are configured
        return await _translaas.T("common", "welcome", L.English);
    }

    public async Task<string> GetWelcomeMessageAutoAsync()
    {
        // Automatic language resolution (requires providers configured)
        return await _translaas.T("common", "welcome"); // lang omitted
    }

    public async Task<string> GetPluralMessageAsync(int count)
    {
        // With pluralization
        return await _translaas.T("messages", "item", L.English, count);
    }
}
```

**Option B: Using ITranslaasClient (Full API access)**

```csharp
using Translaas.Client;

public class MyService
{
    private readonly ITranslaasClient _client;

    public MyService(ITranslaasClient client)
    {
        _client = client;
    }

    public async Task<string> GetWelcomeMessageAsync()
    {
        // Get a single translation entry
        return await _client.GetEntryAsync(
            group: "common",
            entry: "welcome",
            lang: "en"
        );
    }
}
```

## Configuration

### Basic Configuration

```csharp
using Translaas.Extensions.DependencyInjection;
using L = Translaas.Models.LanguageCodes;

services.AddTranslaas(options =>
{
    // Required: API key and base URL
    options.ApiKey = "your-api-key";
    options.BaseUrl = "https://api.translaas.com";
});
```

### Advanced Configuration

```csharp
using Translaas.Extensions.DependencyInjection;
using Translaas.Caching;
using L = Translaas.Models.LanguageCodes;

services.AddTranslaas(options =>
{
    // Required: API key and base URL
    options.ApiKey = "your-api-key";
    options.BaseUrl = "https://api.translaas.com";
    
    // Optional: Default language fallback
    options.DefaultLanguage = L.English;
    
    // Optional: Caching configuration
    options.CacheMode = CacheMode.Group;
    options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);
    options.CacheSlidingExpiration = TimeSpan.FromMinutes(15);
    
    // Optional: HTTP Client timeout
    options.Timeout = TimeSpan.FromSeconds(30);
});
```

**Configuration Options:**

| Option | Required | Description |
|--------|----------|-------------|
| `ApiKey` | ✅ **Required** | Your Translaas API key |
| `BaseUrl` | ✅ **Required** | Base URL for the Translaas API (do NOT include `/api`) |
| `DefaultLanguage` | ⚪ Optional | Default language code fallback (e.g., `L.English`) |
| `CacheMode` | ⚪ Optional | Caching mode (`None`, `Entry`, `Group`, `Project`) |
| `CacheAbsoluteExpiration` | ⚪ Optional | Absolute cache expiration time |
| `CacheSlidingExpiration` | ⚪ Optional | Sliding cache expiration time |
| `Timeout` | ⚪ Optional | HTTP client timeout |

### Configuration from appsettings.json

```json
{
  "Translaas": {
    "BaseUrl": "https://api.translaas.com",
    "DefaultLanguage": "en",
    "CacheMode": "Group",
    "CacheAbsoluteExpiration": "01:00:00",
    "CacheSlidingExpiration": "00:30:00",
    "Timeout": "00:00:30"
  }
}
```

**Note:** `ApiKey` should be stored in user secrets or environment variables, not in `appsettings.json`.

```csharp
using Microsoft.Extensions.Configuration;

// Option 1: Using the IConfiguration overload (recommended)
services.AddHttpClient();
services.AddTranslaas(builder.Configuration);

// Option 2: Manual binding
services.AddTranslaas(options =>
{
    builder.Configuration.GetSection("Translaas").Bind(options);
});
```

### Configuration from Environment Variables

You can also configure Translaas using environment variables:

```bash
# Windows PowerShell
$env:Translaas__ApiKey = "your-api-key"
$env:Translaas__BaseUrl = "https://api.translaas.com"
$env:Translaas__CacheMode = "Group"

# Linux/Mac
export Translaas__ApiKey="your-api-key"
export Translaas__BaseUrl="https://api.translaas.com"
export Translaas__CacheMode="Group"
```

```csharp
// Environment variables are automatically loaded by IConfiguration
services.AddHttpClient();
services.AddTranslaas(builder.Configuration);
```

### Configuration from User Secrets (Development)

For development, use .NET User Secrets:

```bash
dotnet user-secrets set "Translaas:ApiKey" "your-api-key"
dotnet user-secrets set "Translaas:BaseUrl" "https://api.translaas.com"
```

```csharp
// In Program.cs or Startup.cs
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

services.AddHttpClient();
services.AddTranslaas(builder.Configuration);
```

### Configuration Validation

The SDK validates configuration on startup. Invalid configuration will throw `TranslaasConfigurationException`:

```csharp
try
{
    services.AddTranslaas(options =>
    {
        options.ApiKey = ""; // Invalid: empty API key
        options.BaseUrl = "not-a-valid-url"; // Invalid: not a valid URL
    });
}
catch (TranslaasConfigurationException ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
}
```

### Language Resolution

The SDK supports automatic language resolution, making the `lang` parameter optional in `.T()` calls. Configure language providers to automatically determine the language from various sources.

**Basic Setup:**

```csharp
using System.Collections.Generic;
using Translaas.Extensions.DependencyInjection;
using Translaas.Extensions.Mvc; // For UseRequest() extension
using L = Translaas.Models.LanguageCodes;

services.AddTranslaas(options =>
{
    // Required: API key and base URL
    options.ApiKey = "your-api-key";
    options.BaseUrl = "https://api.translaas.com";
    
    // Optional: Default language fallback
    options.DefaultLanguage = L.English;
}, language =>
{
    // Configure language resolution providers
    // Providers are checked in the order they are registered.
    // The first provider that returns a non-null language wins.
    // 
    // Available providers:
    // - UseRequest() - Resolves from HTTP request (route, query string, header, cookie)
    // - UseCulture() - Resolves from CultureInfo.CurrentUICulture
    // - UseDefault() - Resolves from TranslaasOptions.DefaultLanguage
    // 
    // You can configure the order and which providers to use based on your needs.
    language
        .UseRequest(request =>
        {
            // Configure which HTTP request sources to check
            request.Sources = new List<RequestLanguageSource>
            {
                RequestLanguageSource.Route,      // e.g., /en/products
                RequestLanguageSource.QueryString, // e.g., ?lang=en
                RequestLanguageSource.Header,     // e.g., X-Language: en
                RequestLanguageSource.Cookie      // e.g., lang=en cookie
            };
        })
        .UseCulture()  // Resolves from thread culture (CultureInfo.CurrentUICulture)
        .UseDefault(); // Resolves from DefaultLanguage option (appsettings.json)
});
```

**Language Resolution:**

1. **Explicit `lang` parameter** (highest priority - always wins, bypasses all providers)
2. **Configured providers** (checked in registration order):
   - **RequestLanguageProvider** (for web apps): Route, query string, header, cookie
   - **CultureLanguageProvider**: `CultureInfo.CurrentUICulture`
   - **DefaultLanguageProvider**: `TranslaasOptions.DefaultLanguage`
   
   The first provider that returns a non-null language wins. You can configure the order and which providers to use.

**Usage:**

```csharp
using L = Translaas.Models.LanguageCodes;

// Explicit language (always works)
await translaasService.T("common", "welcome", L.English);

// Automatic resolution (requires providers configured)
await translaasService.T("common", "welcome"); // lang omitted
```

**Console Applications:**

For console apps (no HTTP context), use culture and default providers:

```csharp
using Translaas.Extensions.DependencyInjection;
using L = Translaas.Models.LanguageCodes;

services.AddTranslaas(options =>
{
    // Required: API key and base URL
    options.ApiKey = "your-api-key";
    options.BaseUrl = "https://api.translaas.com";
    
    // Optional: Default language fallback
    options.DefaultLanguage = L.English;
}, language =>
{
    // Configure language resolution providers
    // Providers are checked in the order they are registered.
    // You can configure the order based on your needs.
    language
        .UseCulture()  // Resolves from thread culture (CultureInfo.CurrentUICulture)
        .UseDefault(); // Resolves from DefaultLanguage option (appsettings.json)
});
{
    language
        .UseCulture()  // Uses thread culture
        .UseDefault(); // Falls back to DefaultLanguage
});
```

See the [sample projects](https://github.com/acuencadev/translaas-sdk-examples) for complete examples.

## Usage Examples

### Get Single Translation Entry

**Using ITranslaasService (Convenience API):**

```csharp
using L = Translaas.Models.LanguageCodes;

// Basic usage with explicit language
string translation = await _translaas.T("ui", "button.save", L.English);

// Automatic language resolution (requires providers configured)
string translation = await _translaas.T("ui", "button.save"); // lang omitted

// With pluralization
string message = await _translaas.T("messages", "item.count", L.English, 5);
```

**Using ITranslaasClient (Full API):**

```csharp
using L = Translaas.Models.LanguageCodes;

// Basic usage
string translation = await _client.GetEntryAsync(
    group: "ui",
    entry: "button.save",
    lang: L.English
);

// With pluralization
string message = await _client.GetEntryAsync(
    group: "messages",
    entry: "item.count",
    lang: L.English,
    number: 5 // Used for pluralization rules
);
```

### Get All Translations for a Group

```csharp
using L = Translaas.Models.LanguageCodes;

TranslationGroup group = await _client.GetGroupAsync(
    project: "my-project",
    group: "ui",
    lang: L.English
);

// Access translations
foreach (var entry in group.Entries)
{
    Console.WriteLine($"{entry.Key}: {entry.Value}");
}
```

### Get All Translations for a Project

```csharp
using L = Translaas.Models.LanguageCodes;

TranslationProject project = await _client.GetProjectAsync(
    project: "my-project",
    lang: L.English
);

// Access all groups and entries
foreach (var group in project.Groups)
{
    Console.WriteLine($"Group: {group.Name}");
    foreach (var entry in group.Entries)
    {
        Console.WriteLine($"  {entry.Key}: {entry.Value}");
    }
}
```

### Get Available Locales

```csharp
ProjectLocales locales = await _client.GetProjectLocalesAsync(
    project: "my-project"
);

foreach (string locale in locales.Locales)
{
    Console.WriteLine($"Available locale: {locale}");
}
```

### Using Translaas in Razor Views

For ASP.NET Core MVC applications, you can use Translaas directly in your `.cshtml` files using Tag Helpers or HTML Helpers.

**Installation:**

```bash
dotnet add package Translaas.Extensions.Mvc
```

**Setup:**

1. Register MVC services (in `Program.cs` or `Startup.cs`):

```csharp
using Translaas.Extensions.DependencyInjection;
using Translaas.Extensions.Mvc;

services.AddTranslaas(options =>
{
    options.ApiKey = "your-api-key";
    options.BaseUrl = "https://api.translaas.com";
});

// Optional: Explicitly register MVC helpers (Tag Helpers are auto-discovered)
services.AddTranslaasMvc();
```

2. Add to `_ViewImports.cshtml`:

```razor
@addTagHelper *, Translaas.Extensions.Mvc
@using Translaas.Extensions.Mvc
```

**Usage in Razor Views:**

**Option 1: Tag Helper (Declarative)**

```razor
@using L = Translaas.Models.LanguageCodes

<!-- Basic usage with explicit language -->
<h1><translaas group="common" entry="welcome" lang="@L.English" /></h1>

<!-- Automatic language resolution (requires providers configured) -->
<h1><translaas group="common" entry="welcome" /></h1>

<!-- With pluralization -->
<p><translaas group="messages" entry="item" lang="@L.English" number="5" /></p>

<!-- In attributes -->
<button title="@Translaas.T(Html, "ui", "button.save.tooltip", L.English)">
    <translaas group="ui" entry="button.save" lang="@L.English" />
</button>
```

**Option 2: Static Helper (Recommended - Consistent Naming)**

The `Translaas.T()` static helper provides consistent naming with the Tag Helper and service:

```razor
@using L = Translaas.Models.LanguageCodes

<!-- Basic usage with explicit language - Html is available by default in Razor views -->
<h1>@Translaas.T(Html, "common", "welcome", L.English)</h1>

<!-- Automatic language resolution (requires providers configured) -->
<h1>@Translaas.T(Html, "common", "welcome")</h1>

<!-- With pluralization -->
<p>@Translaas.T(Html, "messages", "item", L.English, 5)</p>

<!-- In code blocks -->
@{
    var greeting = Translaas.T(Html, "common", "greeting", L.English);
}
<span>@greeting</span>
```

**Option 3: Direct Service Injection (Async Support)**

```razor
@inject ITranslaasService Translaas
@using L = Translaas.Models.LanguageCodes

<!-- Async usage with explicit language -->
<h1>@await Translaas.T("common", "welcome", L.English)</h1>
<p>@await Translaas.T("messages", "item", L.English, 5)</p>

<!-- Async usage with automatic language resolution -->
<h1>@await Translaas.T("common", "welcome")</h1>
```

**All approaches:**
- Automatically resolve `ITranslaasService` from the DI container
- Support caching (if configured)
- Support pluralization via the `number` parameter
- Tag Helper and Static Helper are HTML-encoded by default
- Direct service injection allows async/await and gives you control over encoding

## Caching

The SDK supports multiple caching modes to optimize performance:

### Cache Modes

- `CacheMode.None` - No caching (default)
- `CacheMode.Entry` - Cache individual translation entries
- `CacheMode.Group` - Cache entire translation groups
- `CacheMode.Project` - Cache entire projects

### Example

```csharp
services.AddTranslaas(options =>
{
    options.ApiKey = "your-api-key";
    options.BaseUrl = "https://api.translaas.com";
    
    // Enable group-level caching
    options.CacheMode = CacheMode.Group;
    
    // Set cache expiration
    options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);
    options.CacheSlidingExpiration = TimeSpan.FromMinutes(15);
});
```

### Custom Cache Provider

You can implement a custom cache provider by implementing `ITranslaasCacheProvider`:

```csharp
using Translaas.Caching;

public class MyCustomCacheProvider : ITranslaasCacheProvider
{
    public bool TryGetValue<T>(string key, out T? value)
    {
        // Your custom cache logic
    }

    public void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        // Your custom cache logic
    }

    public void Remove(string key)
    {
        // Your custom cache logic
    }
}

// Register your custom provider
services.AddSingleton<ITranslaasCacheProvider, MyCustomCacheProvider>();
services.AddTranslaas(options => { /* ... */ });
```

### Cache Mode Comparison

| Cache Mode | Performance | Memory Usage | Use Case |
|------------|-------------|--------------|----------|
| `None` | Fastest API calls | None | Development, testing |
| `Entry` | Fast for repeated entries | Low | Few frequently accessed entries |
| `Group` | Fast for group access | Medium | Multiple entries from same group |
| `Project` | Fastest for project-wide access | High | Full project translations needed |

### Cache Expiration Strategies

```csharp
services.AddTranslaas(options =>
{
    options.ApiKey = "your-api-key";
    options.BaseUrl = "https://api.translaas.com";
    options.CacheMode = CacheMode.Group;
    
    // Absolute expiration: Cache expires after 1 hour regardless of access
    options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);
    
    // Sliding expiration: Cache expires after 15 minutes of inactivity
    options.CacheSlidingExpiration = TimeSpan.FromMinutes(15);
    
    // Both can be used together: Cache expires after 1 hour OR 15 minutes of inactivity, whichever comes first
});
```

## Offline Caching

The SDK supports file-based offline caching, allowing your application to work without network connectivity by caching translations locally in JSON files.

### Enabling Offline Cache

```csharp
services.AddTranslaas(options =>
{
    options.ApiKey = "your-api-key";
    options.BaseUrl = "https://api.translaas.com";
    
    // Enable offline caching
    options.OfflineCache.Enabled = true;
    options.OfflineCache.CacheDirectory = ".translaas-cache";
    options.OfflineCache.FallbackMode = OfflineFallbackMode.CacheFirst;
    
    // Configure which projects and languages to cache
    options.OfflineCache.Projects.Add("my-project");
    options.OfflineCache.Languages.AddRange(new[] { "en", "es", "fr" });
    
    // Enable automatic sync
    options.OfflineCache.AutoSync = true;
    options.OfflineCache.AutoSyncInterval = TimeSpan.FromHours(1);
});
```

### Fallback Modes

- `OfflineFallbackMode.CacheFirst` - Try cache first, fall back to API (default)
- `OfflineFallbackMode.ApiFirst` - Try API first, fall back to cache
- `OfflineFallbackMode.CacheOnly` - Only use cache, never call API

### Configuration from appsettings.json

```json
{
  "Translaas": {
    "ApiKey": "your-api-key",
    "BaseUrl": "https://api.translaas.com",
    "OfflineCache": {
      "Enabled": true,
      "CacheDirectory": ".translaas-cache",
      "FallbackMode": "CacheFirst",
      "AutoSync": true,
      "AutoSyncInterval": "01:00:00",
      "Projects": ["my-project"],
      "Languages": ["en", "es", "fr"],
      "DefaultProjectId": "my-project"
    }
  }
}
```

### Background Sync Service

For ASP.NET Core applications, you can enable automatic background synchronization:

```csharp
using Translaas.Caching.File;

// In Program.cs
services.AddTranslaas(options => { /* ... */ });
services.AddTranslaasOfflineCacheSyncHostedService();
```

This registers an `IHostedService` that will periodically sync your offline cache with the Translaas API.

### Hybrid Caching (Memory L1 + File L2)

For optimal performance, you can enable hybrid caching which combines fast in-memory access with persistent file storage:

```csharp
services.AddTranslaas(options =>
{
    options.ApiKey = "your-api-key";
    
    // Enable offline cache with hybrid mode
    options.OfflineCache.Enabled = true;
    options.OfflineCache.HybridCache.Enabled = true;
    options.OfflineCache.HybridCache.MemoryCacheExpiration = TimeSpan.FromMinutes(30);
    options.OfflineCache.HybridCache.MaxMemoryCacheEntries = 1000;
    options.OfflineCache.HybridCache.WarmupOnStartup = true;
});
```

With hybrid caching:
- **L1 (Memory)**: Fast access for frequently used translations
- **L2 (File)**: Persistent storage that survives application restarts

When a cache miss occurs in L1, the translation is automatically loaded from L2 and promoted to L1.

### Hybrid Cache Configuration from appsettings.json

```json
{
  "Translaas": {
    "ApiKey": "your-api-key",
    "OfflineCache": {
      "Enabled": true,
      "CacheDirectory": ".translaas-cache",
      "FallbackMode": "CacheFirst",
      "Projects": ["my-project"],
      "Languages": ["en", "es", "fr"],
      "HybridCache": {
        "Enabled": true,
        "MemoryCacheExpiration": "00:30:00",
        "MaxMemoryCacheEntries": 1000,
        "WarmupOnStartup": true
      }
    }
  }
}
```

## API Reference

### ITranslaasService Interface (Convenience API)

`ITranslaasService` provides a simplified API for common translation lookups. It's automatically registered when you call `AddTranslaas()`. The interface provides multiple overloaded methods to avoid nullable parameters.

```csharp
public interface ITranslaasService
{
    // Automatic language resolution (no lang parameter)
    Task<string> T(string group, string entry, CancellationToken cancellationToken = default);
    Task<string> T(string group, string entry, decimal number, CancellationToken cancellationToken = default);
    Task<string> T(string group, string entry, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);
    Task<string> T(string group, string entry, decimal number, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);
    
    // Explicit language override (bypasses all providers)
    Task<string> T(string group, string entry, string lang, CancellationToken cancellationToken = default);
    Task<string> T(string group, string entry, string lang, decimal number, CancellationToken cancellationToken = default);
    Task<string> T(string group, string entry, string lang, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);
    Task<string> T(string group, string entry, string lang, decimal number, Dictionary<string, string> parameters, CancellationToken cancellationToken = default);
}
```

**Example Usage:**

```csharp
using L = Translaas.Models.LanguageCodes;
using System.Collections.Generic;

// Inject ITranslaasService
private readonly ITranslaasService _translaas;

// Automatic language resolution (requires providers configured)
string welcome = await _translaas.T("common", "welcome");

// Explicit language override (bypasses all providers)
string welcome = await _translaas.T("common", "welcome", L.English);

// With pluralization (automatic resolution)
string items = await _translaas.T("messages", "item", 5);

// With pluralization (explicit language)
string items = await _translaas.T("messages", "item", L.English, 5);

// With named parameters (automatic resolution)
var parameters = new Dictionary<string, string> { { "userName", "John" } };
string greeting = await _translaas.T("messages", "greeting", parameters);

// With named parameters (explicit language)
string greeting = await _translaas.T("messages", "greeting", L.English, parameters);

// Combining pluralization and parameters
string items = await _translaas.T("messages", "items", L.English, 5, parameters);
```

### ITranslaasClient Interface (Full API)

`ITranslaasClient` provides complete access to all Translaas API features.

```csharp
public interface ITranslaasClient
{
    /// <summary>
    /// Gets a single translation entry.
    /// </summary>
    /// <param name="group">The translation group name</param>
    /// <param name="entry">The translation entry key</param>
    /// <param name="lang">The language code (e.g., "en", "fr")</param>
    /// <param name="number">Optional number for pluralization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The translated text</returns>
    Task<string> GetEntryAsync(string group, string entry, string lang, int? number = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all translations for a group.
    /// </summary>
    /// <param name="project">The project identifier</param>
    /// <param name="group">The translation group name</param>
    /// <param name="lang">The language code</param>
    /// <param name="format">Optional format parameter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A TranslationGroup containing all entries</returns>
    Task<TranslationGroup> GetGroupAsync(string project, string group, string lang, string? format = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all translations for a project.
    /// </summary>
    /// <param name="project">The project identifier</param>
    /// <param name="lang">The language code</param>
    /// <param name="format">Optional format parameter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A TranslationProject containing all groups and entries</returns>
    Task<TranslationProject> GetProjectAsync(string project, string lang, string? format = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available locales for a project.
    /// </summary>
    /// <param name="project">The project identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A ProjectLocales object containing available locales</returns>
    Task<ProjectLocales> GetProjectLocalesAsync(string project, CancellationToken cancellationToken = default);
}
```

**When to Use Which:**

- **Use `ITranslaasService`** when you only need simple translation lookups (`T()` method)
- **Use `ITranslaasClient`** when you need full API access (groups, projects, locales, etc.)

Both services are registered as scoped and share the same underlying client instance.

### Razor View Helpers

For ASP.NET Core MVC applications, use Tag Helpers or HTML Helpers in Razor views.

**Tag Helper (Declarative):**

```razor
@using L = Translaas.Models.LanguageCodes

<translaas group="common" entry="welcome" lang="@L.English" />
<translaas group="messages" entry="item" lang="@L.English" number="5" />

<!-- Automatic language resolution -->
<translaas group="common" entry="welcome" />
```

**Static Helper (Recommended - Consistent Naming):**

```razor
@using L = Translaas.Models.LanguageCodes

@Translaas.T(Html, "common", "welcome", L.English)
@Translaas.T(Html, "messages", "item", L.English, 5)

<!-- Automatic language resolution -->
@Translaas.T(Html, "common", "welcome")
```

**Note:** `Html` is available by default in Razor views (no injection needed).

**Direct Service Injection (Async Support):**

```razor
@inject ITranslaasService Translaas
@using L = Translaas.Models.LanguageCodes

@await Translaas.T("common", "welcome", L.English)
@await Translaas.T("messages", "item", L.English, 5)

<!-- Automatic language resolution -->
@await Translaas.T("common", "welcome")
```

**Parameters:**

- `group` (required) - The translation group name
- `entry` (required) - The translation entry key
- `lang` (optional) - The language code (e.g., "en", "fr"). If omitted, language is resolved from configured providers
- `number` (optional) - Number for pluralization

**Notes:**

- Tag Helper and Static Helper (`Translaas.T()`) are recommended for consistency
- `Html` is available by default in Razor views - no injection needed for `Translaas.T(Html, ...)`
- All helpers automatically resolve `ITranslaasService` from the DI container
- All helpers support caching if configured
- Direct service injection allows async/await usage and gives you control over HTML encoding

## Framework Compatibility

The SDK supports multiple .NET frameworks:

| Target Framework | Compatible With |
|-----------------|-----------------|
| .NET Standard 2.0 | .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+ |
| .NET 6 | .NET 6, .NET 7 |
| .NET 8 | .NET 8, .NET 9 |
| .NET 10 | .NET 10+ |

NuGet will automatically select the appropriate DLL for your project's target framework.

## Error Handling

The SDK provides comprehensive error handling with specific exception types for different scenarios.

### Exception Types

- **`TranslaasException`** - Base exception for all Translaas errors
- **`TranslaasApiException`** - Thrown when the API returns an error (includes HTTP status code)
- **`TranslaasConfigurationException`** - Thrown when configuration is invalid
- **`TranslaasOfflineCacheException`** - Base exception for offline cache errors
- **`TranslaasOfflineCacheMissException`** - Thrown when translation is not found in offline cache

### Basic Error Handling

```csharp
using Translaas.Models.Errors;
using Translaas.Models.LanguageCodes;

try
{
    string translation = await _client.GetEntryAsync("group", "entry", LanguageCodes.English);
}
catch (TranslaasApiException ex)
{
    // Handle API errors (400, 404, 500, etc.)
    Console.WriteLine($"API Error: {ex.Message}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
    Console.WriteLine($"Response: {ex.ResponseContent}");
    
    // Handle specific status codes
    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        // Translation not found
    }
    else if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
        // Invalid API key
    }
}
catch (TranslaasConfigurationException ex)
{
    // Handle configuration errors
    Console.WriteLine($"Configuration Error: {ex.Message}");
}
catch (TranslaasOfflineCacheMissException ex)
{
    // Handle offline cache miss
    Console.WriteLine($"Cache Miss: {ex.Message}");
    Console.WriteLine($"Project: {ex.Project}");
    Console.WriteLine($"Language: {ex.Language}");
}
catch (System.Net.Http.HttpRequestException ex)
{
    // Handle network errors
    Console.WriteLine($"Network Error: {ex.Message}");
}
catch (System.OperationCanceledException)
{
    // Handle cancellation
    Console.WriteLine("Operation was cancelled");
}
```

### Error Handling with ITranslaasService

```csharp
using Translaas.Extensions.DependencyInjection;
using Translaas.Models.Errors;

try
{
    // Automatic language resolution
    string translation = await _service.T("common", "welcome");
}
catch (System.InvalidOperationException ex)
{
    // Thrown when language resolution fails (no provider returns a language)
    Console.WriteLine($"Language resolution failed: {ex.Message}");
    // Fallback to explicit language
    translation = await _service.T("common", "welcome", "en");
}
catch (TranslaasApiException ex)
{
    // Handle API errors
    Console.WriteLine($"API Error: {ex.Message}");
}
```

### Error Handling Best Practices

1. **Always catch specific exceptions first** - Catch `TranslaasApiException` before `TranslaasException`
2. **Handle offline cache misses gracefully** - Provide fallback behavior when cache misses occur
3. **Log errors appropriately** - Include context (project, group, entry, language) in error logs
4. **Provide user-friendly messages** - Translate error messages for end users when appropriate

```csharp
public async Task<string> GetTranslationSafely(string group, string entry, string lang)
{
    try
    {
        return await _client.GetEntryAsync(group, entry, lang);
    }
    catch (TranslaasApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        // Return a fallback message or log the missing translation
        _logger.LogWarning("Translation not found: {Group}.{Entry} ({Lang})", group, entry, lang);
        return $"[{group}.{entry}]"; // Fallback display
    }
    catch (TranslaasApiException ex)
    {
        _logger.LogError(ex, "API error retrieving translation: {Group}.{Entry} ({Lang})", group, entry, lang);
        throw; // Re-throw for other API errors
    }
}
```

## Development

### Building from Source

```bash
git clone https://github.com/your-org/Translaas.SDK.git
cd Translaas.SDK
dotnet restore
dotnet build
```

### Running Tests

```bash
dotnet test
```

## API Endpoints

The SDK communicates with the following Translaas API endpoints:

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/translations/text` | GET | Get single translation entry |
| `/api/translations/group` | GET | Get all translations for a group |
| `/api/translations/project` | GET | Get all translations for a project |
| `/api/translations/locales` | GET | Get available locales for a project |

**Note:** All endpoints use GET requests with JSON request bodies.

## Authentication

The SDK uses API key authentication via the `X-Api-Key` header. Provide your API key during service registration:

```csharp
services.AddTranslaas(options =>
{
    options.ApiKey = "your-api-key-here";
});
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Copyright (c) 2025 Translaas SDK Contributors

## Support

- **Documentation**: [Link to full documentation]
- **Issues**: [https://github.com/acuencadev/Translaas.SDK/issues]
- **API Reference**: [Swagger/API Docs URL]

## Contributing

We welcome contributions to the Translaas SDK! Please read our [Contributing Guidelines](CONTRIBUTING.md) for details on:

- How to get started
- Development guidelines and code style
- Pull request process
- Commit message conventions
- Reporting issues

For more information, see [CONTRIBUTING.md](CONTRIBUTING.md).

---

**Made with ❤️ for the .NET community**
