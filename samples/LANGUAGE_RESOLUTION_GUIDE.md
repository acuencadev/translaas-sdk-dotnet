# Language Resolution Testing Guide

This guide shows you how to test the new automatic language resolution feature in the Translaas SDK.

## Quick Start

The sample projects (`Translaas.Samples.WebApp` and `Translaas.Samples.Blazor`) have been updated to demonstrate language resolution.

### 1. Configure Language Resolution

In `Program.cs`, language resolution is configured like this:

```csharp
builder.Services.AddTranslaas(options =>
{
    options.ApiKey = "your-api-key";
    options.BaseUrl = "https://api.translaas.local";
    options.DefaultLanguage = "en"; // Fallback language
}, language =>
{
    // Configure language resolution providers (checked in order)
    language
        .UseRequest(request =>
        {
            // Check HTTP request sources
            request.Sources = new List<RequestLanguageSource>
            {
                RequestLanguageSource.Route,      // e.g., /en/products
                RequestLanguageSource.QueryString, // e.g., ?lang=en
                RequestLanguageSource.Header,     // e.g., X-Language: en
                RequestLanguageSource.Cookie      // e.g., lang=en cookie
            };
        })
        .UseCulture()  // Fallback to thread culture
        .UseDefault(); // Final fallback to DefaultLanguage
});
```

### 2. Use Optional Language Parameter

Once configured, you can omit the `lang` parameter:

**Tag Helper:**
```razor
<!-- Explicit language (always works) -->
<translaas group="common" entry="welcome" lang="en" />

<!-- Automatic resolution (requires providers configured) -->
<translaas group="common" entry="welcome" />
```

**Static Helper:**
```csharp
// Explicit language
@Translaas.T(Html, "common", "welcome", "en")

// Automatic resolution
@Translaas.T(Html, "common", "welcome")
```

**Service:**
```csharp
// Explicit language
await translaasService.T("common", "welcome", "en");

// Automatic resolution
await translaasService.T("common", "welcome");
```

## Testing Scenarios

### Scenario 1: Query String Parameter

1. Navigate to: `https://localhost:5001/?lang=fr`
2. All translations without explicit `lang` will use French
3. Try: `https://localhost:5001/?lang=es` for Spanish

### Scenario 2: Route Parameter

The WebApp sample includes a language route:
- `https://localhost:5001/en/Home/Index` - English
- `https://localhost:5001/fr/Home/Index` - French
- `https://localhost:5001/es/Home/Index` - Spanish

### Scenario 3: HTTP Header

Use a tool like Postman or curl:

```bash
curl -H "X-Language: fr" https://localhost:5001/
```

### Scenario 4: Cookie

Set a cookie named `lang` with value `fr`:

```javascript
document.cookie = "lang=fr; path=/";
```

### Scenario 5: Thread Culture

If no HTTP sources provide a language, the system falls back to `CultureInfo.CurrentUICulture`:

```csharp
Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
```

### Scenario 6: Default Language

If no provider returns a language, the `DefaultLanguage` from options is used (defaults to `"en"`).

## Language Resolution Order

The system checks providers in this order:

1. **Explicit `lang` parameter** (highest priority - always wins)
2. **RequestLanguageProvider** (checks in order):
   - Route parameter (e.g., `/en/products`)
   - Query string (e.g., `?lang=en`)
   - HTTP header (e.g., `X-Language: en`)
   - Cookie (e.g., `lang=en`)
3. **CultureLanguageProvider** (`CultureInfo.CurrentUICulture`)
4. **DefaultLanguageProvider** (`TranslaasOptions.DefaultLanguage`)

## Example: Testing in WebApp Sample

1. **Build and run the WebApp sample:**
   ```bash
   cd samples/Translaas.Samples.WebApp
   dotnet run
   ```

2. **Navigate to the home page:**
   - Default: `https://localhost:5001/`
   - With query: `https://localhost:5001/?lang=fr`
   - With route: `https://localhost:5001/fr/Home/Index`

3. **Check the "Automatic Language Resolution" section** on the page to see examples

4. **Try different language sources:**
   - Add `?lang=es` to URL for Spanish
   - Add `?lang=fr` to URL for French
   - Change browser language settings to test culture fallback

## Example: Testing in Blazor Sample

1. **Build and run the Blazor sample:**
   ```bash
   cd samples/Translaas.Samples.Blazor
   dotnet run
   ```

2. **Navigate to the home page** and check translations

3. **Try query string parameters** to change language dynamically

## Example: Testing in Console Sample

1. **Build and run the Console sample:**
   ```bash
   cd samples/Translaas.Samples.Console
   dotnet run
   ```

2. **Observe Example 8** which demonstrates automatic language resolution:
   - Uses thread culture (`CultureInfo.CurrentUICulture`) when `lang` is omitted
   - Falls back to `DefaultLanguage` if thread culture doesn't provide a language
   - Shows how changing thread culture affects automatic resolution

3. **Try changing thread culture** in your own code:
   ```csharp
   Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
   await translaasService.T("common", "welcome"); // Uses "fr" automatically
   ```

## Example: Testing in Web API Sample

1. **Build and run the Web API sample:**
   ```bash
   cd samples/Translaas.Samples.WebApi
   dotnet run
   ```

2. **Test with explicit language:**
   ```bash
   GET /api/translation/entry?group=common&entry=welcome&lang=en
   ```

3. **Test with automatic language resolution:**
   ```bash
   # Using query string
   GET /api/translation/entry?group=common&entry=welcome&lang=fr
   
   # Using header
   GET /api/translation/entry?group=common&entry=welcome
   Header: X-Language: fr
   
   # Using cookie
   GET /api/translation/entry?group=common&entry=welcome
   Cookie: lang=fr
   ```

4. **View Swagger documentation:**
   - Navigate to `https://localhost:5001/swagger`
   - Try the `/api/translation/entry` endpoint
   - Notice that `lang` parameter is now optional
   - The response includes `resolvedLanguage` showing how language was determined

5. **Test different language sources:**
   - Query string: `?lang=es`
   - HTTP header: `X-Language: es`
   - Cookie: `lang=es`
   - Route parameter: `/api/translation/{lang}/entry` (if route is configured)

## Customizing Language Resolution

### Custom Request Sources Order

```csharp
language.UseRequest(request =>
{
    // Only check query string and cookie
    request.Sources = new List<RequestLanguageSource>
    {
        RequestLanguageSource.QueryString,
        RequestLanguageSource.Cookie
    };
    
    // Custom parameter names
    request.QueryParameterName = "locale";  // ?locale=en
    request.CookieName = "language";        // language=en cookie
    request.HeaderName = "X-Custom-Lang";   // X-Custom-Lang: en
});
```

### Culture Provider Options

```csharp
language.UseCulture(options =>
{
    // Return full culture name (e.g., "en-US") instead of two-letter code (e.g., "en")
    options.UseFullCultureName = true;
});
```

### Custom Provider

```csharp
language.UseProvider<MyCustomLanguageProvider>();
```

## Troubleshooting

### Language Not Resolving

If language resolution fails, you'll get an `InvalidOperationException`:

```
Unable to determine language for translation request. 
Either provide the 'lang' parameter explicitly, or configure language providers 
using AddTranslaas(..., language => language.UseCulture().UseDefault()).
```

**Solution:** Ensure you've configured at least one language provider, or provide a `DefaultLanguage` in options.

### Testing Without HTTP Context

For console applications or background services, use:

```csharp
services.AddTranslaas(options =>
{
    options.ApiKey = "your-api-key";
    options.BaseUrl = "https://api.translaas.local";
    options.DefaultLanguage = "en"; // Fallback language
}, language =>
{
    // Console apps don't have HTTP context, so only use:
    language
        .UseCulture()  // Uses thread culture (CultureInfo.CurrentUICulture)
        .UseDefault(); // Falls back to DefaultLanguage
});
```

**Example: Console App Language Resolution**

```csharp
// Explicit language (always works)
await translaasService.T("common", "welcome", "en");

// Automatic resolution from thread culture
Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
await translaasService.T("common", "welcome"); // Uses "fr" from thread culture

// Falls back to DefaultLanguage if thread culture doesn't provide a language
await translaasService.T("common", "welcome"); // Uses "en" from DefaultLanguage
```

See `Translaas.Samples.Console/Program.cs` for a complete example.

## Next Steps

- Check the sample projects for complete examples
- Review the test files in `tests/Translaas.Extensions.Mvc.Tests/` for more scenarios
- Read `LANGUAGE_RESOLUTION_SPEC.md` for detailed specification

