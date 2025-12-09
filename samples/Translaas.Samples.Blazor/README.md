# Translaas SDK Blazor Sample

This sample demonstrates how to use the Translaas SDK in a Blazor Server application with all features including dependency injection, caching, and dynamic translations.

## Overview

This Blazor sample shows:
- How to configure Translaas services in a Blazor Server application
- How to inject `ITranslaasService` and `ITranslaasClient` into Blazor components
- How to use translations in Blazor components
- How to handle pluralization
- How to switch between languages dynamically
- How to retrieve translation groups
- How to configure caching for improved performance

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
    "BaseUrl": "https://sdk-api.translaas.local/api"
  }
}
```

### Environment Variables

Alternatively, use environment variables:

- `TRANSLAAS_API_KEY`: Your Translaas API key (required)
- `TRANSLAAS_BASE_URL`: The base URL for the Translaas API (defaults to `https://sdk-api.translaas.local/api`)

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
        ?? "https://sdk-api.translaas.local/api";
    
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
   $env:TRANSLAAS_BASE_URL = "https://sdk-api.translaas.local/api"

   # Linux/macOS
   export TRANSLAAS_API_KEY="your-api-key"
   export TRANSLAAS_BASE_URL="https://sdk-api.translaas.local/api"
   ```

2. Run the application:
   ```bash
   dotnet run --project samples/Translaas.Samples.Blazor
   ```

3. Open your browser to `https://localhost:5001` (or the port shown in the console)

## Features Demonstrated

### 1. Service Injection in Components

Inject `ITranslaasService` or `ITranslaasClient` into Blazor components:

```razor
@inject ITranslaasService Translaas
@inject ITranslaasClient TranslaasClient

<h1>@await Translaas.T("common", "welcome", "en")</h1>
```

### 2. Using ITranslaasService

The convenience service provides a simple `T()` method:

```razor
@inject ITranslaasService Translaas

<p>@await Translaas.T("common", "welcome", "en")</p>
```

### 3. Using ITranslaasClient

The full client provides access to all API methods:

```razor
@inject ITranslaasClient TranslaasClient

@code {
    private TranslationGroup? translationGroup;

    protected override async Task OnInitializedAsync()
    {
        translationGroup = await TranslaasClient.GetGroupAsync("my-project", "common", "en");
    }
}
```

### 4. Pluralization

Support for plural forms using the `number` parameter:

```razor
<p>@await Translaas.T("messages", "item", "en", 1)</p>
<p>@await Translaas.T("messages", "item", "en", 5)</p>
```

### 5. Dynamic Language Switching

Change the language dynamically based on component state:

```razor
@code {
    private string currentLang = "en";

    private async Task ToggleLanguage()
    {
        currentLang = currentLang == "en" ? "fr" : "en";
        StateHasChanged();
    }
}

<p>@await Translaas.T("common", "welcome", currentLang)</p>
<button @onclick="ToggleLanguage">Toggle Language</button>
```

### 6. Translation Groups

Retrieve and display entire translation groups:

```razor
@code {
    private TranslationGroup? translationGroup;

    protected override async Task OnInitializedAsync()
    {
        translationGroup = await TranslaasClient.GetGroupAsync("my-project", "common", "en");
    }
}

@if (translationGroup != null)
{
    <ul>
        @foreach (var entry in translationGroup.Entries)
        {
            <li><strong>@entry.Key</strong>: @entry.Value</li>
        }
    </ul>
}
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
Translaas.Samples.Blazor/
├── Pages/
│   ├── _Host.cshtml              # Host page
│   ├── Index.razor               # Home page with examples
│   ├── Counter.razor             # Counter page
│   └── FetchData.razor           # Weather forecast page
├── Shared/
│   ├── MainLayout.razor          # Main layout component
│   └── NavMenu.razor             # Navigation menu component
├── App.razor                     # Root component
├── Program.cs                    # Application startup
├── appsettings.json             # Configuration
└── README.md                     # This file
```

## Usage Examples

### Example 1: Basic Translation

```razor
@page "/"
@inject ITranslaasService Translaas

<h1>@await Translaas.T("common", "welcome", "en")</h1>
```

### Example 2: Pluralization

```razor
@code {
    private int itemCount = 5;
}

<p>@await Translaas.T("messages", "item", "en", itemCount)</p>
```

### Example 3: Dynamic Language

```razor
@code {
    private string lang = "en";
    
    private void SwitchLanguage()
    {
        lang = lang == "en" ? "fr" : "en";
        StateHasChanged();
    }
}

<h1>@await Translaas.T("common", "welcome", lang)</h1>
<button @onclick="SwitchLanguage">Switch Language</button>
```

### Example 4: Loading Translation Groups

```razor
@inject ITranslaasClient TranslaasClient

@code {
    private TranslationGroup? group;

    protected override async Task OnInitializedAsync()
    {
        group = await TranslaasClient.GetGroupAsync("my-project", "common", "en");
    }
}

@if (group != null)
{
    @foreach (var entry in group.Entries)
    {
        <p><strong>@entry.Key</strong>: @entry.Value</p>
    }
}
```

### Example 5: Error Handling

```razor
@code {
    private string? translation;
    private string? error;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            translation = await Translaas.T("common", "welcome", "en");
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
    }
}

@if (error != null)
{
    <div class="alert alert-danger">@error</div>
}
else if (translation != null)
{
    <p>@translation</p>
}
```

## Caching Modes

- `CacheMode.None`: No caching (default)
- `CacheMode.Entry`: Cache individual entries
- `CacheMode.Group`: Cache entire translation groups (recommended)
- `CacheMode.Project`: Cache entire projects

## Service Lifetime

- `ITranslaasClient`: Scoped (one instance per HTTP request/Blazor circuit)
- `ITranslaasService`: Scoped (one instance per HTTP request/Blazor circuit)
- `IMemoryCache`: Singleton (shared across all requests)
- `ITranslaasCacheProvider`: Singleton (shared across all requests)

## Best Practices

1. **Use Async/Await**: Always use `await` when calling translation methods in Blazor components.

2. **Handle Errors**: Wrap translation calls in try-catch blocks or use error boundaries.

3. **Cache at Group Level**: Use `CacheMode.Group` for best performance in Blazor applications.

4. **Centralize Language Management**: Consider creating a service or state management solution to manage the current language across components.

5. **Use StateHasChanged**: When changing language dynamically, call `StateHasChanged()` to trigger UI updates.

6. **Load Translations Early**: Load translations in `OnInitializedAsync()` or `OnParametersSetAsync()` lifecycle methods.

## Next Steps

- Explore the [Console sample](../Translaas.Samples.Console/README.md) for simple console application usage
- Explore the [Web API sample](../Translaas.Samples.WebApi/README.md) for REST API usage
- Explore the [WebApp sample](../Translaas.Samples.WebApp/README.md) for Razor views and tag helpers
