# Translaas SDK

![Tests](https://github.com/acuencadev/Translaas.SDK/workflows/CI/badge.svg)

A strongly-typed, performant, and modular .NET SDK for consuming the **Translaas Translation Delivery API**. This SDK provides a clean, dependency-injection friendly way to retrieve translations in your .NET applications.

## Features

- ✅ **Strongly-typed API** - Full IntelliSense support with strongly-typed models
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
    options.RetryPolicy = RetryPolicy.ExponentialBackoff(
        maxRetries: 3,
        initialDelay: TimeSpan.FromSeconds(1)
    );
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
services.AddTranslaas(options =>
{
    var config = builder.Configuration.GetSection("Translaas");
    config.Bind(options);
});
```

## Usage Examples

### Get Single Translation Entry

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

### ITranslaasClient Interface

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
    /// <returns>The translated text</returns>
    Task<string> GetEntryAsync(string group, string entry, string lang, int? number = null);

    /// <summary>
    /// Gets all translations for a group.
    /// </summary>
    /// <param name="project">The project identifier</param>
    /// <param name="group">The translation group name</param>
    /// <param name="lang">The language code</param>
    /// <param name="format">Optional format parameter</param>
    /// <returns>A TranslationGroup containing all entries</returns>
    Task<TranslationGroup> GetGroupAsync(string project, string group, string lang, string? format = null);

    /// <summary>
    /// Gets all translations for a project.
    /// </summary>
    /// <param name="project">The project identifier</param>
    /// <param name="lang">The language code</param>
    /// <param name="format">Optional format parameter</param>
    /// <returns>A TranslationProject containing all groups and entries</returns>
    Task<TranslationProject> GetProjectAsync(string project, string lang, string? format = null);

    /// <summary>
    /// Gets available locales for a project.
    /// </summary>
    /// <param name="project">The project identifier</param>
    /// <returns>A ProjectLocales object containing available locales</returns>
    Task<ProjectLocales> GetProjectLocalesAsync(string project);
}
```

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
- **Issues**: [GitHub Issues URL]
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
