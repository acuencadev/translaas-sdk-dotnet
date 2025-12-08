# Translaas SDK Console Sample

This sample demonstrates how to use the Translaas SDK in a simple console application with dependency injection.

## Overview

This console application shows:
- How to configure Translaas services using dependency injection
- How to use `ITranslaasService` for simple translation lookups
- How to use `ITranslaasClient` for full API access
- How to configure caching
- How to handle pluralization
- How to retrieve translation groups and project locales

## Prerequisites

- .NET 8.0 SDK or later
- A valid Translaas API key
- Access to a Translaas API endpoint

## Configuration

### Environment Variables

The application can be configured using environment variables:

- `TRANSLAAS_API_KEY`: Your Translaas API key (required)
- `TRANSLAAS_BASE_URL`: The base URL for the Translaas API (defaults to `https://sdkapi.translaas.local/api`)

### Code Configuration

Alternatively, you can modify the configuration directly in `Program.cs`:

```csharp
services.AddTranslaas(options =>
{
    options.ApiKey = "your-api-key-here";
    options.BaseUrl = "https://sdkapi.translaas.local/api";
    options.CacheMode = CacheMode.Group;
    options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);
    options.CacheSlidingExpiration = TimeSpan.FromMinutes(30);
    options.Timeout = TimeSpan.FromSeconds(30);
});
```

## Running the Sample

1. Set your environment variables:
   ```bash
   # Windows PowerShell
   $env:TRANSLAAS_API_KEY = "your-api-key"
   $env:TRANSLAAS_BASE_URL = "https://sdkapi.translaas.local/api"

   # Linux/macOS
   export TRANSLAAS_API_KEY="your-api-key"
   export TRANSLAAS_BASE_URL="https://sdkapi.translaas.local/api"
   ```

2. Run the application:
   ```bash
   dotnet run --project samples/Translaas.Samples.Console
   ```

## Features Demonstrated

### 1. Using ITranslaasService

The `ITranslaasService` provides a convenient `T()` method for simple translation lookups:

```csharp
var translation = await translaasService.T("common", "welcome", "en");
```

### 2. Using ITranslaasClient

The `ITranslaasClient` provides full API access:

```csharp
var translation = await translaasClient.GetEntryAsync("common", "welcome", "en");
```

### 3. Pluralization

Support for plural forms using the `number` parameter:

```csharp
var singular = await translaasService.T("messages", "item", "en", 1);
var plural = await translaasService.T("messages", "item", "en", 5);
```

### 4. Translation Groups

Retrieve all translations for a group:

```csharp
var group = await translaasClient.GetGroupAsync("my-project", "common", "en");
foreach (var entry in group.Entries)
{
    Console.WriteLine($"{entry.Key}: {entry.Value}");
}
```

### 5. Project Locales

Get available locales for a project:

```csharp
var locales = await translaasClient.GetProjectLocalesAsync("my-project");
Console.WriteLine($"Available locales: {string.Join(", ", locales.Locales)}");
```

### 6. Caching

The sample demonstrates caching configuration and its performance benefits:

```csharp
options.CacheMode = CacheMode.Group; // Cache at group level
options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);
options.CacheSlidingExpiration = TimeSpan.FromMinutes(30);
```

## Caching Modes

- `CacheMode.None`: No caching (default)
- `CacheMode.Entry`: Cache individual entries
- `CacheMode.Group`: Cache entire translation groups (recommended)
- `CacheMode.Project`: Cache entire projects

## Error Handling

The sample includes basic error handling. In production applications, you should:

- Handle `TranslaasApiException` for API errors
- Handle `HttpRequestException` for network errors
- Handle `TranslaasConfigurationException` for configuration errors
- Implement retry logic for transient failures
- Log errors appropriately

## Next Steps

- Explore the [Web API sample](../Translaas.Samples.WebApi/README.md) for ASP.NET Core Web API usage
- Explore the [WebApp sample](../Translaas.Samples.WebApp/README.md) for Razor views and tag helpers
- Explore the [Blazor sample](../Translaas.Samples.Blazor/README.md) for Blazor component usage
