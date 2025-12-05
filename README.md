# Translaas SDK

![Tests](https://github.com/acuencadev/Translaas.SDK/workflows/CI/badge.svg)

A strongly-typed, performant, and modular .NET SDK for consuming the **Translaas Translation Delivery API**. This SDK provides a clean, dependency-injection friendly way to retrieve translations in your .NET applications.

## Features

- ✅ **Strongly-typed API** - Full IntelliSense support with strongly-typed models
- ✅ **Convenience API** - Simple `T()` method for quick translation lookups via `ITranslaasService`
- ✅ **Razor View Support** - Tag Helpers and HTML Helpers for easy translation in `.cshtml` files
- ✅ **Dependency Injection Ready** - Seamless integration with `IServiceCollection`
- ✅ **Flexible Caching** - Built-in memory caching with configurable cache modes
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
<PackageReference Include="Translaas.Extensions.DependencyInjection" Version="1.0.0" />
```

### Individual Packages

If you prefer to use individual packages:

- `Translaas.Client` - Core HTTP client
- `Translaas.Models` - Data transfer objects
- `Translaas.Caching` - Caching layer
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

**Option A: Using ITranslaasService (Recommended for simple lookups)**

```csharp
using Translaas.Extensions.DependencyInjection;

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
        return await _translaas.T("common", "welcome", "en");
    }

    public async Task<string> GetPluralMessageAsync(int count)
    {
        // With pluralization
        return await _translaas.T("messages", "item", "en", count);
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
services.AddTranslaas(options =>
{
    options.ApiKey = "your-api-key";
    options.BaseUrl = "https://api.translaas.com";
});
```

### Advanced Configuration

```csharp
services.AddTranslaas(options =>
{
    options.ApiKey = "your-api-key";
    options.BaseUrl = "https://api.translaas.com";
    
    // Caching
    options.CacheMode = CacheMode.Group;
    options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);
    options.CacheSlidingExpiration = TimeSpan.FromMinutes(15);
    
    // HTTP Client
    options.Timeout = TimeSpan.FromSeconds(30);
});
```

### Configuration from appsettings.json

```json
{
  "Translaas": {
    "ApiKey": "your-api-key",
    "BaseUrl": "https://api.translaas.com",
    "CacheMode": "Group",
    "Timeout": "00:00:30"
  }
}
```

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

## Usage Examples

### Get Single Translation Entry

**Using ITranslaasService (Convenience API):**

```csharp
// Basic usage with the convenient T() method
string translation = await _translaas.T("ui", "button.save", "en");

// With pluralization
string message = await _translaas.T("messages", "item.count", "en", 5);
```

**Using ITranslaasClient (Full API):**

```csharp
// Basic usage
string translation = await _client.GetEntryAsync(
    group: "ui",
    entry: "button.save",
    lang: "en"
);

// With pluralization
string message = await _client.GetEntryAsync(
    group: "messages",
    entry: "item.count",
    lang: "en",
    number: 5 // Used for pluralization rules
);
```

### Get All Translations for a Group

```csharp
TranslationGroup group = await _client.GetGroupAsync(
    project: "my-project",
    group: "ui",
    lang: "en"
);

// Access translations
foreach (var entry in group.Entries)
{
    Console.WriteLine($"{entry.Key}: {entry.Value}");
}
```

### Get All Translations for a Project

```csharp
TranslationProject project = await _client.GetProjectAsync(
    project: "my-project",
    lang: "en"
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
<!-- Basic usage -->
<h1><translaas group="common" entry="welcome" lang="en" /></h1>

<!-- With pluralization -->
<p><translaas group="messages" entry="item" lang="en" number="5" /></p>

<!-- In attributes -->
<button title="<translaas group="ui" entry="button.save.tooltip" lang="en" />">
    <translaas group="ui" entry="button.save" lang="en" />
</button>
```

**Option 2: Static Helper (Recommended - Consistent Naming)**

The `Translaas.T()` static helper provides consistent naming with the Tag Helper and service:

```razor
<!-- Basic usage - Html is available by default in Razor views -->
<h1>@Translaas.T(Html, "common", "welcome", "en")</h1>

<!-- With pluralization -->
<p>@Translaas.T(Html, "messages", "item", "en", 5)</p>

<!-- In code blocks -->
@{
    var greeting = Translaas.T(Html, "common", "greeting", "en");
}
<span>@greeting</span>
```

**Option 3: Direct Service Injection (Async Support)**

```razor
@inject ITranslaasService Translaas

<!-- Async usage -->
<h1>@await Translaas.T("common", "welcome", "en")</h1>
<p>@await Translaas.T("messages", "item", "en", 5)</p>
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

```csharp
services.AddSingleton<ITranslaasCacheProvider, MyCustomCacheProvider>();
services.AddTranslaas(options => { /* ... */ });
```

## API Reference

### ITranslaasService Interface (Convenience API)

`ITranslaasService` provides a simplified API for common translation lookups. It's automatically registered when you call `AddTranslaas()`.

```csharp
public interface ITranslaasService
{
    /// <summary>
    /// Gets a translation entry (shorthand for GetEntryAsync).
    /// </summary>
    /// <param name="group">The translation group name</param>
    /// <param name="entry">The translation entry key</param>
    /// <param name="lang">The language code (e.g., "en", "fr")</param>
    /// <param name="number">Optional number for pluralization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The translated text</returns>
    Task<string> T(string group, string entry, string lang, int? number = null, CancellationToken cancellationToken = default);
}
```

**Example Usage:**

```csharp
// Inject ITranslaasService
private readonly ITranslaasService _translaas;

// Simple translation lookup
string welcome = await _translaas.T("common", "welcome", "en");

// With pluralization
string items = await _translaas.T("messages", "item", "en", 5);
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

```html
<translaas group="common" entry="welcome" lang="en" />
<translaas group="messages" entry="item" lang="en" number="5" />
```

**Static Helper (Recommended - Consistent Naming):**

```csharp
@Translaas.T(Html, "common", "welcome", "en")
@Translaas.T(Html, "messages", "item", "en", 5)
```

**Note:** `Html` is available by default in Razor views (no injection needed).

**Direct Service Injection (Async Support):**

```csharp
@inject ITranslaasService Translaas
@await Translaas.T("common", "welcome", "en")
@await Translaas.T("messages", "item", "en", 5)
```

**Parameters:**

- `group` (required) - The translation group name
- `entry` (required) - The translation entry key
- `lang` (required) - The language code (e.g., "en", "fr")
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

```csharp
try
{
    string translation = await _client.GetEntryAsync("group", "entry", "en");
}
catch (TranslaasException ex)
{
    // Handle Translaas-specific errors
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
}
catch (HttpRequestException ex)
{
    // Handle HTTP errors
    Console.WriteLine($"HTTP Error: {ex.Message}");
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
