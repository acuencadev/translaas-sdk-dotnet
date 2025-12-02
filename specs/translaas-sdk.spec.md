# Translaas SDK – Technical Specification

Version: **0.1 — Initial Draft**
Status: **Active / In Progress**
Scope: **Client SDK for consuming Translaas Public API**
Applies to: **NuGet packages inside Translaas.SDK.sln**

---

# **1. Goals Overview**

This SDK provides a strongly-typed, performant, and modular way for any .NET application to interact with the **Translaas Translation Delivery API**.

### **Primary Goals**
1. Provide a clean HTTP client for retrieving translations.
2. Support single and batch translation retrieval.
3. Support project → group → entry resolution.
4. Provide caching mechanisms (memory + pluggable).
5. Provide DI-friendly registration + automatic configuration.
6. Provide models and client errors.
7. Provide full NuGet packages.

---

# **2. Project Structure**

The SDK consists of several NuGet packages contained in the `Translaas.SDK.sln` solution.

### **2.1 Project List**

| Project | Type | Purpose |
|--------|------|---------|
| **Translaas.Client** | Class Library | Core HTTP client + authentication, retries, serialization |
| **Translaas.Models** | Class Library | DTOs for responses / requests / errors |
| **Translaas.Caching** | Class Library | Optional caching layer with IMemoryCache & extensibility points |
| **Translaas.Extensions.Http** | Class Library | HttpClientFactory service registration |
| **Translaas.Extensions.DependencyInjection** | Class Library | `IServiceCollection` integration & options pattern |

---

# **3. Target Frameworks**

Each project should target:

<TargetFrameworks>netstandard2.0;net6.0;net8.0;net10.0</TargetFrameworks>

---

# **4. API Authentication**

Uses **X-Api-Key** header.

**Security Scheme:** API Key authentication via `X-Api-Key` header (as defined in Swagger security scheme).

**API Reference:**
- Swagger JSON: `https://sdk-api.translaas.local/swagger/v1/swagger.json`
- Development Base URL: `https://sdkapi.translaas.local/api`
- Production Base URL: `https://api.translaas.com` (assumed, update when confirmed)

---

# 5. Core HTTP Client

Base responsibilities:

### API Endpoints

| Endpoint | Method | Request Model | Purpose |
|----------|--------|---------------|---------|
| `/api/translations/text` | GET | `GetTranslationRequest` | Get single translation entry |
| `/api/translations/group` | GET | `GetGroupTranslationsRequest` | Get all translations for a group |
| `/api/translations/project` | GET | `GetProjectTranslationsRequest` | Get all translations for a project |
| `/api/translations/locales` | GET | `GetProjectLocalesRequest` | Get available locales for a project |

**Important:** All endpoints use GET requests with JSON request bodies (unusual REST pattern but matches the actual API design).

### Must support:
- `GetEntryAsync(group, entry, lang, number)` - GET `/api/translations/text`
  - Required: `group`, `entry`, `lang`
  - Optional: `n` (number for pluralization)
- `GetGroupAsync(project, group, lang, format)` - GET `/api/translations/group`
  - Required: `project`, `group`, `lang`
  - Optional: `format`
- `GetProjectAsync(project, lang, format)` - GET `/api/translations/project`
  - Required: `project`, `lang`
  - Optional: `format`
- `GetProjectLocalesAsync(project)` - GET `/api/translations/locales`
  - Required: `project`
- Raw text response mode (preferred)
- JSON fallback mode

### Should include:
- Configurable retry policy  
- Configurable timeout  
- Configurable base URL (default: `https://sdkapi.translaas.local/api` for development, production: `https://api.translaas.com`)  
- Partial caching integration hooks

---

# 6. Caching Layer

### Cache Modes:
- `None`  
- `Entry`  
- `Group`  
- `Project`  

### Requirements:
- Use `IMemoryCache` by default  
- Allow custom cache provider injection  
- Support absolute + sliding expiration  

---

# 7. DI Integration

Service registration:

```csharp
services.AddTranslaas(options =>
{
    options.ApiKey = "<value>";
    options.BaseUrl = "https://sdkapi.translaas.local/api"; // Development URL
    options.CacheMode = CacheMode.Group;
});

# **8. Models**

Required Models:

**Request Models:**
- `GetTranslationRequest` - { group, entry, lang, n? }
- `GetGroupTranslationsRequest` - { project, group, lang, format? }
- `GetProjectTranslationsRequest` - { project, lang, format? }
- `GetProjectLocalesRequest` - { project }

**Response Models:**
- TranslationEntry (for single entry responses)
- TranslationGroup (for group responses)
- TranslationProject (for project responses)
- ProjectLocales (for locales response)
- TranslaasError (for error responses)
- Internal HttpResponse models

**Requirements:**
- Minimal serialization attributes
- System.Text.Json only
- No unnecessary allocations
- Match API schema exactly (nullable strings, optional parameters)

# **9. Public API**

```csharp
// Get single translation entry (returns raw text by default, JSON object if requested)
Task<string> GetEntryAsync(string group, string entry, string lang, int? number = null);

// Get all translations for a group
Task<TranslationGroup> GetGroupAsync(string project, string group, string lang, string? format = null);

// Get all translations for a project
Task<TranslationProject> GetProjectAsync(string project, string lang, string? format = null);

// Get available locales for a project
Task<ProjectLocales> GetProjectLocalesAsync(string project);
```

**Notes:**
- All `project` parameters are strings (not integers)
- `format` parameter is optional and may control response format (text vs JSON)
- `number` parameter (n) is optional and used for pluralization

# **10. NuGet Packages**

Final published packages will be:
- Translaas.Client
- Translaas.Models
- Translaas.Caching
- Translaas.Extensions.Http
- Translaas.Extensions.DependencyInjection