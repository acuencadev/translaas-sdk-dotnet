# Translaas SDK WebApp Sample

This sample demonstrates how to use the Translaas SDK in an ASP.NET Core MVC Web Application with Razor views, tag helpers, and all features.

## Overview

This WebApp sample shows:
- How to configure Translaas services in an ASP.NET Core MVC application
- How to use Translaas tag helpers in Razor views
- How to use the `Translaas.T()` static helper
- How to inject `ITranslaasService` directly in views
- How to configure caching for improved performance
- How to handle pluralization
- How to switch between languages

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

builder.Services.AddTranslaasMvc();
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
   dotnet run --project samples/Translaas.Samples.WebApp
   ```

3. Open your browser to `https://localhost:5001` (or the port shown in the console)

## Features Demonstrated

### 1. Tag Helper Usage

Use the `<translaas>` tag helper directly in Razor views:

```razor
@using L = Translaas.Models.LanguageCodes

<translaas group="common" entry="welcome" lang="@L.English" />
<translaas group="messages" entry="item" lang="@L.English" number="5" />

<!-- Automatic language resolution -->
<translaas group="common" entry="welcome" />
```

**Required attributes:**
- `group`: The translation group name
- `entry`: The translation entry key

**Optional attributes:**
- `lang`: The language code (e.g., "en", "fr"). If omitted, language is resolved from configured providers
- `number`: Number for pluralization

### 2. Static Helper Usage

Use the `Translaas.T()` static helper:

```razor
@using L = Translaas.Models.LanguageCodes

@Translaas.T(Html, "common", "welcome", L.English)
@Translaas.T(Html, "messages", "item", L.English, 5)

<!-- Automatic language resolution -->
@Translaas.T(Html, "common", "welcome")
```

**Note:** `Html` is available by default in Razor views (no injection needed).

### 3. Direct Service Injection

Inject `ITranslaasService` directly in views (already done in `_ViewImports.cshtml`):

```razor
@inject ITranslaasService Translaas
@using L = Translaas.Models.LanguageCodes

@await Translaas.T("common", "welcome", L.English)
@await Translaas.T("messages", "item", L.English, 5)

<!-- Automatic language resolution -->
@await Translaas.T("common", "welcome")
```

### 4. View Imports Configuration

The `_ViewImports.cshtml` file configures tag helpers and service injection:

```html
@addTagHelper *, Translaas.Extensions.Mvc
@using Translaas.Extensions.Mvc
@inject ITranslaasService Translaas
```

### 5. Pluralization

Support for plural forms using the `number` attribute:

```razor
@using L = Translaas.Models.LanguageCodes

<translaas group="messages" entry="item" lang="@L.English" number="1" />
<translaas group="messages" entry="item" lang="@L.English" number="5" />
```

### 6. Multiple Languages

Switch between languages by changing the `lang` attribute or using automatic resolution:

```razor
@using L = Translaas.Models.LanguageCodes

<!-- Explicit language -->
<translaas group="common" entry="welcome" lang="@L.English" />
<translaas group="common" entry="welcome" lang="@L.French" />
<translaas group="common" entry="welcome" lang="@L.Spanish" />

<!-- Automatic language resolution (from HTTP request, culture, or default) -->
<translaas group="common" entry="welcome" />
```

### 7. Automatic Language Resolution

With language providers configured, you can omit the `lang` parameter:

```html
<!-- Language resolved from configured providers -->
<translaas group="common" entry="welcome" />

<!-- Or using static helper -->
@Translaas.T(Html, "common", "welcome")
```

**Language Resolution:**
1. **Explicit `lang` parameter** (highest priority - always wins, bypasses all providers)
2. **Configured providers** (checked in registration order):
   - **RequestLanguageProvider** (for web apps): Route, query string, header, cookie
   - **CultureLanguageProvider**: `CultureInfo.CurrentUICulture`
   - **DefaultLanguageProvider**: `TranslaasOptions.DefaultLanguage`
   
   The first provider that returns a non-null language wins. You can configure the order and which providers to use.

### 7. Caching

Caching is configured at the service level:

```csharp
options.CacheMode = CacheMode.Group; // Cache at group level
options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);
options.CacheSlidingExpiration = TimeSpan.FromMinutes(30);
```

## Project Structure

```
Translaas.Samples.WebApp/
├── Controllers/
│   └── HomeController.cs          # MVC controller
├── Views/
│   ├── _ViewImports.cshtml        # Tag helpers and service injection
│   ├── _ViewStart.cshtml          # Layout configuration
│   ├── Shared/
│   │   └── _Layout.cshtml         # Main layout with tag helpers
│   └── Home/
│       ├── Index.cshtml           # Home page with examples
│       └── Privacy.cshtml         # Privacy page
├── wwwroot/                       # Static files
├── Program.cs                     # Application startup
├── appsettings.json              # Configuration
└── README.md                      # This file
```

## Usage Examples

### Example 1: Navigation Menu

```razor
@using L = Translaas.Models.LanguageCodes

<nav>
    <a href="/">
        <translaas group="navigation" entry="home" lang="@L.English" />
    </a>
    <a href="/privacy">
        <translaas group="navigation" entry="privacy" lang="@L.English" />
    </a>
</nav>
```

### Example 2: Dynamic Content

```razor
@using L = Translaas.Models.LanguageCodes

<h1>
    <translaas group="common" entry="welcome" lang="@L.English" />
</h1>
<p>
    <translaas group="common" entry="welcome.message" lang="@L.English" />
</p>
```

### Example 3: Pluralization in Lists

```razor
@using L = Translaas.Models.LanguageCodes

<ul>
    <li>
        <translaas group="messages" entry="item" lang="@L.English" number="1" />
    </li>
    <li>
        <translaas group="messages" entry="item" lang="@L.English" number="5" />
    </li>
</ul>
```

### Example 4: Conditional Translation

```razor
@using L = Translaas.Models.LanguageCodes

@{
    var itemCount = 5;
    var lang = L.English;
}

<p>
    <translaas group="messages" entry="item" lang="@lang" number="@itemCount" />
</p>
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

## Best Practices

1. **Use Tag Helpers for Static Content**: Tag helpers are the most declarative and readable for static translations.

2. **Use Static Helper for Dynamic Content**: When you need to compute parameters dynamically, use `Translaas.T()`.

3. **Use Direct Service for Async Operations**: When you need async/await in views, inject `ITranslaasService` directly.

4. **Cache at Group Level**: Use `CacheMode.Group` for best performance in web applications.

5. **Centralize Language Selection**: Consider creating a helper or service to manage the current language based on user preferences, URL, or headers.

## Next Steps

- Explore the [Console sample](../Translaas.Samples.Console/README.md) for simple console application usage
- Explore the [Web API sample](../Translaas.Samples.WebApi/README.md) for REST API usage
- Explore the [Blazor sample](../Translaas.Samples.Blazor/README.md) for Blazor component usage
