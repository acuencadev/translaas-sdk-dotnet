# Translaas SDK Web API Sample

This sample demonstrates how to use the Translaas SDK in an ASP.NET Core Web API application with dependency injection, MVC, and caching.

## Overview

This Web API sample shows:
- How to configure Translaas services in an ASP.NET Core Web API
- How to inject `ITranslaasService` and `ITranslaasClient` into controllers
- How to use both convenience service and full client API
- How to configure caching for improved performance
- How to handle errors and logging
- How to expose translation endpoints via REST API

## Prerequisites

- .NET 8.0 SDK or later
- A valid Translaas API key
- Access to a Translaas API endpoint

## Configuration

### appsettings.json

Configure Translaas in `appsettings.json`:

```json
{
  "Translaas": {
    "BaseUrl": "https://sdk-api.translaas.local",
    "DefaultLanguage": "en",
    "CacheMode": "Group",
    "CacheAbsoluteExpiration": "01:00:00",
    "CacheSlidingExpiration": "00:30:00",
    "Timeout": "00:00:30"
  }
}
```

**Note:** `ApiKey` should be stored in user secrets or environment variables, not in `appsettings.json`.

### Environment Variables

Alternatively, use environment variables:

- `TRANSLAAS_API_KEY`: Your Translaas API key (required)
- `TRANSLAAS_BASE_URL`: The base URL for the Translaas API (defaults to `https://sdk-api.translaas.local`)
  - **Note**: Do NOT include `/api` in the BaseUrl - the client adds `/api/` to all endpoints automatically

### Code Configuration

The configuration is set up in `Program.cs`:

```csharp
using System.Collections.Generic;
using Translaas.Extensions.DependencyInjection;
using Translaas.Extensions.Mvc;
using Translaas.Caching;
using L = Translaas.Models.LanguageCodes;

builder.Services.AddTranslaas(options =>
{
    // Required: API key and base URL
    options.ApiKey = builder.Configuration["Translaas:ApiKey"] 
        ?? Environment.GetEnvironmentVariable("TRANSLAAS_API_KEY") 
        ?? "your-api-key-here";
    
    options.BaseUrl = builder.Configuration["Translaas:BaseUrl"] 
        ?? Environment.GetEnvironmentVariable("TRANSLAAS_BASE_URL") 
        ?? "https://sdk-api.translaas.local";
    
    // Note: Do NOT include /api in the BaseUrl - the client adds /api/ to all endpoints
    
    // Optional: Default language fallback
    options.DefaultLanguage = builder.Configuration["Translaas:DefaultLanguage"] ?? L.English;
    
    // Optional: Cache settings (read from configuration)
    options.CacheMode = builder.Configuration.GetValue<CacheMode?>("Translaas:CacheMode") ?? CacheMode.Group;
    options.CacheAbsoluteExpiration = TimeSpan.TryParse(builder.Configuration["Translaas:CacheAbsoluteExpiration"], out var absoluteExpiration)
        ? absoluteExpiration
        : TimeSpan.FromHours(1);
    options.CacheSlidingExpiration = TimeSpan.TryParse(builder.Configuration["Translaas:CacheSlidingExpiration"], out var slidingExpiration)
        ? slidingExpiration
        : TimeSpan.FromMinutes(30);
    options.Timeout = TimeSpan.TryParse(builder.Configuration["Translaas:Timeout"], out var timeout)
        ? timeout
        : TimeSpan.FromSeconds(30);
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
                RequestLanguageSource.Route,
                RequestLanguageSource.QueryString,
                RequestLanguageSource.Header,
                RequestLanguageSource.Cookie
            };
        })
        .UseCulture()  // Resolves from thread culture (CultureInfo.CurrentUICulture)
        .UseDefault(); // Resolves from DefaultLanguage option (appsettings.json)
});
```

## Running the Sample

1. Configure your API key in `appsettings.json` or set environment variables:
   ```bash
   # Windows PowerShell
   $env:TRANSLAAS_API_KEY = "your-api-key"
   $env:TRANSLAAS_BASE_URL = "https://sdk-api.translaas.local"  # Do NOT include /api

   # Linux/macOS
   export TRANSLAAS_API_KEY="your-api-key"
   export TRANSLAAS_BASE_URL="https://sdk-api.translaas.local"  # Do NOT include /api
   ```

2. Run the application:
   ```bash
   dotnet run --project samples/Translaas.Samples.WebApi
   ```

3. Open Swagger UI at `https://localhost:5001/swagger` (or the port shown in the console)

## API Endpoints

### Get Translation Entry (using ITranslaasService)

```
GET /api/translation/entry?group=common&entry=welcome&lang=en
GET /api/translation/entry?group=messages&entry=item&lang=en&number=5
GET /api/translation/entry?group=common&entry=welcome  # lang omitted - uses automatic resolution
```

**Note:** The `lang` parameter is optional. If omitted, language is resolved from HTTP request (query string, header, cookie, route), thread culture, or default language.

### Get Translation Entry (using ITranslaasClient)

```
GET /api/translation/entry/client?group=common&entry=welcome&lang=en
```

### Get Translation Group

```
GET /api/translation/group?project=my-project&group=common&lang=en
```

### Get Translation Project

```
GET /api/translation/project?project=my-project&lang=en
```

### Get Project Locales

```
GET /api/translation/locales?project=my-project
```

## Features Demonstrated

### 1. Dependency Injection

Both `ITranslaasService` and `ITranslaasClient` are injected into controllers:

```csharp
public TranslationController(
    ITranslaasService translaasService,
    ITranslaasClient translaasClient,
    ILogger<TranslationController> logger)
{
    _translaasService = translaasService;
    _translaasClient = translaasClient;
    _logger = logger;
}
```

### 2. Using ITranslaasService

The convenience service provides a simple `T()` method:

```csharp
var translation = await _translaasService.T(group, entry, lang, number);
```

### 3. Using ITranslaasClient

The full client provides access to all API methods:

```csharp
var translation = await _translaasClient.GetEntryAsync(group, entry, lang, number);
var group = await _translaasClient.GetGroupAsync(project, group, lang);
var project = await _translaasClient.GetProjectAsync(project, lang);
var locales = await _translaasClient.GetProjectLocalesAsync(project);
```

### 4. Caching

Caching is configured at the service level:

```csharp
options.CacheMode = CacheMode.Group; // Cache at group level
options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);
options.CacheSlidingExpiration = TimeSpan.FromMinutes(30);
```

### 5. Error Handling

The controller includes error handling and logging:

```csharp
try
{
    var translation = await _translaasService.T(group, entry, lang, number);
    return Ok(translation);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error retrieving translation entry");
    return StatusCode(500, new { error = ex.Message });
}
```

### 6. MVC Integration

The sample includes MVC services for tag helpers (useful if you add views later):

```csharp
builder.Services.AddTranslaasMvc();
```

## Caching Modes

- `CacheMode.None`: No caching (default)
- `CacheMode.Entry`: Cache individual entries
- `CacheMode.Group`: Cache entire translation groups (recommended)
- `CacheMode.Project`: Cache entire projects

## Service Lifetime

- `ITranslaasClient`: Scoped (one instance per HTTP request)
- `ITranslaasService`: Scoped (one instance per HTTP request)
- `IMemoryCache`: Singleton (shared across all requests)
- `ITranslaasCacheProvider`: Singleton (shared across all requests)

## Testing the API

### Using curl

```bash
# Get a translation entry
curl "https://localhost:5001/api/translation/entry?group=common&entry=welcome&lang=en"

# Get a translation group
curl "https://localhost:5001/api/translation/group?project=my-project&group=common&lang=en"

# Get project locales
curl "https://localhost:5001/api/translation/locales?project=my-project"
```

### Using Swagger UI

1. Navigate to `https://localhost:5001/swagger`
2. Expand the `Translation` controller
3. Click "Try it out" on any endpoint
4. Fill in the parameters
5. Click "Execute"

## Next Steps

- Explore the [Console sample](../Translaas.Samples.Console/README.md) for simple console application usage
- Explore the [WebApp sample](../Translaas.Samples.WebApp/README.md) for Razor views and tag helpers
- Explore the [Blazor sample](../Translaas.Samples.Blazor/README.md) for Blazor component usage
