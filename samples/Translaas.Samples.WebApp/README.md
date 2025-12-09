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
    "ApiKey": "your-api-key-here",
    "BaseUrl": "https://sdk-api.translaas.local"
  }
}
```

### Environment Variables

Alternatively, use environment variables:

- `TRANSLAAS_API_KEY`: Your Translaas API key (required)
- `TRANSLAAS_BASE_URL`: The base URL for the Translaas API (defaults to `https://sdk-api.translaas.local`)
  - **Note**: Do NOT include `/api` in the BaseUrl - the client adds `/api/` to all endpoints automatically

### Code Configuration

The configuration is set up in `Program.cs`:

```csharp
builder.Services.AddTranslaas(options =>
{
    options.ApiKey = builder.Configuration["Translaas:ApiKey"] 
        ?? Environment.GetEnvironmentVariable("TRANSLAAS_API_KEY") 
        ?? "your-api-key-here";
    
    options.BaseUrl = builder.Configuration["Translaas:BaseUrl"] 
        ?? Environment.GetEnvironmentVariable("TRANSLAAS_BASE_URL") 
        ?? "https://sdk-api.translaas.local";
    
    // Note: Do NOT include /api in the BaseUrl - the client adds /api/ to all endpoints
    
    options.CacheMode = CacheMode.Group;
    options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);
    options.CacheSlidingExpiration = TimeSpan.FromMinutes(30);
    options.Timeout = TimeSpan.FromSeconds(30);
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

```html
<translaas group="common" entry="welcome" lang="en" />
<translaas group="messages" entry="item" lang="en" number="5" />
```

**Required attributes:**
- `group`: The translation group name
- `entry`: The translation entry key
- `lang`: The language code (e.g., "en", "fr")

**Optional attributes:**
- `number`: Number for pluralization

### 2. Static Helper Usage

Use the `Translaas.T()` static helper:

```csharp
@Translaas.T(Html, "common", "welcome", "en")
@Translaas.T(Html, "messages", "item", "en", 5)
```

**Note:** `Html` is available by default in Razor views (no injection needed).

### 3. Direct Service Injection

Inject `ITranslaasService` directly in views (already done in `_ViewImports.cshtml`):

```csharp
@inject ITranslaasService Translaas

@await Translaas.T("common", "welcome", "en")
@await Translaas.T("messages", "item", "en", 5)
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

```html
<translaas group="messages" entry="item" lang="en" number="1" />
<translaas group="messages" entry="item" lang="en" number="5" />
```

### 6. Multiple Languages

Switch between languages by changing the `lang` attribute:

```html
<translaas group="common" entry="welcome" lang="en" />
<translaas group="common" entry="welcome" lang="fr" />
<translaas group="common" entry="welcome" lang="es" />
```

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

```html
<nav>
    <a href="/">
        <translaas group="navigation" entry="home" lang="en" />
    </a>
    <a href="/privacy">
        <translaas group="navigation" entry="privacy" lang="en" />
    </a>
</nav>
```

### Example 2: Dynamic Content

```html
<h1>
    <translaas group="common" entry="welcome" lang="en" />
</h1>
<p>
    <translaas group="common" entry="welcome.message" lang="en" />
</p>
```

### Example 3: Pluralization in Lists

```html
<ul>
    <li>
        <translaas group="messages" entry="item" lang="en" number="1" />
    </li>
    <li>
        <translaas group="messages" entry="item" lang="en" number="5" />
    </li>
</ul>
```

### Example 4: Conditional Translation

```csharp
@{
    var itemCount = 5;
    var lang = "en";
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
