# Translaas SDK Samples

This directory contains sample applications demonstrating how to use the Translaas SDK in various .NET application types.

## Overview

The Translaas SDK provides a comprehensive solution for integrating translation services into .NET applications. These samples demonstrate different integration patterns and use cases.

## Sample Projects

### 1. [Console Application](./Translaas.Samples.Console/)

A simple console application demonstrating basic Translaas SDK usage with dependency injection.

**Key Features:**
- Service configuration with dependency injection
- Using `ITranslaasService` for simple translation lookups
- Using `ITranslaasClient` for full API access
- Caching configuration and demonstration
- Pluralization support
- Translation group and locale retrieval

**Best For:**
- Learning the basics of the SDK
- Understanding dependency injection setup
- Testing translation functionality
- Command-line tools and utilities

**Run:**
```bash
dotnet run --project samples/Translaas.Samples.Console
```

---

### 2. [ASP.NET Core Web API](./Translaas.Samples.WebApi/)

A RESTful Web API demonstrating Translaas SDK integration in ASP.NET Core Web API applications.

**Key Features:**
- Dependency injection in controllers
- RESTful API endpoints for translations
- Both `ITranslaasService` and `ITranslaasClient` usage
- Caching configuration
- Error handling and logging
- Swagger/OpenAPI integration

**Best For:**
- Building REST APIs that serve translations
- Microservices architecture
- Backend services requiring translation functionality
- API-first applications

**Run:**
```bash
dotnet run --project samples/Translaas.Samples.WebApi
```

Then navigate to `https://localhost:5001/swagger` to explore the API.

---

### 3. [ASP.NET Core WebApp](./Translaas.Samples.WebApp/)

An MVC web application demonstrating Razor views, tag helpers, and all Translaas SDK features.

**Key Features:**
- Razor view integration
- Tag helper usage (`<translaas>`)
- Static helper usage (`Translaas.T()`)
- Direct service injection in views
- Pluralization in views
- Multiple language support
- Caching configuration

**Best For:**
- Traditional MVC web applications
- Server-rendered web pages
- Applications using Razor views
- Multi-language web sites

**Run:**
```bash
dotnet run --project samples/Translaas.Samples.WebApp
```

Then navigate to `https://localhost:5001` to see the application.

---

### 4. [Blazor Server Application](./Translaas.Samples.Blazor/)

A Blazor Server application demonstrating Translaas SDK usage in Blazor components.

**Key Features:**
- Service injection in Blazor components
- Async translation loading
- Dynamic language switching
- Translation group retrieval
- Pluralization support
- Component lifecycle integration
- Caching configuration

**Best For:**
- Blazor Server applications
- Interactive web applications
- Real-time translation updates
- Component-based architectures

**Run:**
```bash
dotnet run --project samples/Translaas.Samples.Blazor
```

Then navigate to `https://localhost:5001` to see the application.

---

### 5. [Offline Mode Console Application](./Translaas.Samples.Offline/)

A console application demonstrating **offline mode** using `OfflineFallbackMode.CacheOnly`. This sample works entirely without network connectivity by reading translations from local cache files.

**Key Features:**
- True offline mode (no API calls)
- Cache-only operation (`CacheOnly` fallback mode)
- Pre-populated cache files included
- Error handling for cache misses
- Complete examples of all SDK features

**Best For:**
- Understanding offline mode configuration
- Testing cache file format
- Learning how to create and manage cache files
- Applications that need to work without network connectivity
- Pre-populated translation scenarios

**Run:**
```bash
dotnet run --project samples/Translaas.Samples.Offline
```

**Note:** This sample includes pre-populated cache files and does not require an API key or network connectivity.

---

### 6. [.NET MAUI Application](./Translaas.Samples.Maui/)

A cross-platform mobile and desktop application demonstrating Translaas SDK usage in .NET MAUI.

**Key Features:**
- MVVM pattern with CommunityToolkit.Mvvm
- Dependency injection in ViewModels
- XAML data binding for translations
- Dynamic language switching UI
- Caching optimized for mobile scenarios
- Pluralization support
- Translation group retrieval
- Connection testing and diagnostics
- Shell-based navigation

**Best For:**
- Cross-platform mobile applications (iOS, Android)
- Desktop applications (Windows, macOS)
- Native UI with translation support
- Offline-capable applications
- Apps requiring MVVM architecture

**Platforms:**
- Windows 10+ (net8.0-windows10.0.19041.0)
- Android (net8.0-android, API 21+)
- iOS (net8.0-ios, iOS 11+)
- macOS (net8.0-maccatalyst)

**Run (Windows):**
```bash
dotnet run --project samples/Translaas.Samples.Maui -f net10.0-windows10.0.19041.0 -r win-x64 --no-self-contained
```

**Run (Android):**
```bash
dotnet run --project samples/Translaas.Samples.Maui -f net10.0-android
```

---

## Common Configuration

All samples require configuration of the Translaas API key and base URL. This can be done in several ways:

### 1. Environment Variables (Recommended)

```bash
# Windows PowerShell
$env:TRANSLAAS_API_KEY = "your-api-key"
$env:TRANSLAAS_BASE_URL = "https://sdk-api.translaas.local"  # Do NOT include /api

# Linux/macOS
export TRANSLAAS_API_KEY="your-api-key"
export TRANSLAAS_BASE_URL="https://sdk-api.translaas.local"  # Do NOT include /api
```

### 2. Configuration Files

For Web API, WebApp, and Blazor samples, configure in `appsettings.json`:

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

### 3. Code Configuration

All samples show how to configure Translaas in code:

```csharp
using System.Collections.Generic;
using Translaas.Extensions.DependencyInjection;
using Translaas.Extensions.Mvc;
using Translaas.Caching;
using L = Translaas.Models.LanguageCodes;

services.AddTranslaas(options =>
{
    // Required: API key and base URL
    options.ApiKey = "your-api-key-here";
    options.BaseUrl = "https://sdk-api.translaas.local";
    // Note: Do NOT include /api in the BaseUrl - the client adds /api/ to all endpoints
    
    // Optional: Default language fallback
    options.DefaultLanguage = L.English;
    
    // Optional: Cache configuration
    options.CacheMode = CacheMode.Group;
    options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);
    options.CacheSlidingExpiration = TimeSpan.FromMinutes(30);
    options.Timeout = TimeSpan.FromSeconds(30);
}, language =>
{
    // Optional: Configure language resolution providers
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
        .UseRequest()  // For web apps - resolves from HTTP request
        .UseCulture()   // Resolves from thread culture
        .UseDefault();  // Resolves from DefaultLanguage option
});
```

**Configuration Options:**

| Option | Required | Description |
|--------|----------|-------------|
| `ApiKey` | ✅ **Required** | Your Translaas API key (store in user secrets or environment variables) |
| `BaseUrl` | ✅ **Required** | Base URL for the Translaas API (do NOT include `/api`) |
| `DefaultLanguage` | ⚪ Optional | Default language code fallback (e.g., `L.English`) |
| `CacheMode` | ⚪ Optional | Caching mode (`None`, `Entry`, `Group`, `Project`) |
| `CacheAbsoluteExpiration` | ⚪ Optional | Absolute cache expiration time |
| `CacheSlidingExpiration` | ⚪ Optional | Sliding cache expiration time |
| `Timeout` | ⚪ Optional | HTTP client timeout |
| Language Providers | ⚪ Optional | Configure automatic language resolution |

## Common Features

All samples demonstrate:

- **Dependency Injection**: How to register and use Translaas services
- **Language Resolution**: How to configure automatic language resolution from HTTP request, culture, or default
- **Caching**: How to configure and use caching for improved performance
- **Error Handling**: How to handle translation errors gracefully
- **Pluralization**: How to handle plural forms
- **Multiple Languages**: How to switch between languages
- **Configuration**: How to read settings from `appsettings.json` with proper fallbacks

## Getting Started

1. **Choose a sample** that matches your application type
2. **Read the sample's README** for detailed instructions
3. **Configure your API key** using environment variables or configuration files
4. **Run the sample** and explore the code
5. **Adapt the patterns** to your own application

## Prerequisites

- .NET 8.0 SDK or later
- A valid Translaas API key
- Access to a Translaas API endpoint

## Additional Resources

- [Main SDK README](../README.md) - Complete SDK documentation
- [Contributing Guide](../CONTRIBUTING.md) - Guidelines for contributing to the SDK
- [API Reference](../README.md#api-reference) - Complete API documentation

## Support

For issues, questions, or contributions, please refer to the main project repository.
