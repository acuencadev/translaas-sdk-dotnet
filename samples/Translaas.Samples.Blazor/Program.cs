using Translaas.Caching;
using Translaas.Caching.File;
using Translaas.Extensions.DependencyInjection;
using Translaas.Extensions.Mvc;
using L = Translaas.Models.LanguageCodes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add HttpClient support (required for Translaas)
builder.Services.AddHttpClient();

// Configure Translaas with options and language resolution
builder.Services.AddTranslaas(options =>
{
    // Required: Set your API key
    options.ApiKey = !string.IsNullOrWhiteSpace(builder.Configuration["Translaas:ApiKey"])
        ? builder.Configuration["Translaas:ApiKey"]!
        : Environment.GetEnvironmentVariable("TRANSLAAS_API_KEY") 
        ?? "your-api-key-here";

    // Required: Set the base URL
    // Note: Do NOT include /api in the BaseUrl - the client adds /api/ to all endpoints
    options.BaseUrl = builder.Configuration["Translaas:BaseUrl"] 
        ?? Environment.GetEnvironmentVariable("TRANSLAAS_BASE_URL") 
        ?? "https://sdk-api.translaas.local";

    // Optional: Configure caching (read from appsettings.json)
    options.CacheMode = builder.Configuration.GetValue<CacheMode?>("Translaas:CacheMode") ?? CacheMode.Group;
    options.CacheAbsoluteExpiration = TimeSpan.TryParse(builder.Configuration["Translaas:CacheAbsoluteExpiration"], out var absoluteExpiration)
        ? absoluteExpiration
        : TimeSpan.FromHours(1);
    options.CacheSlidingExpiration = TimeSpan.TryParse(builder.Configuration["Translaas:CacheSlidingExpiration"], out var slidingExpiration)
        ? slidingExpiration
        : TimeSpan.FromMinutes(30);

    // Optional: Configure timeout (read from appsettings.json)
    options.Timeout = TimeSpan.TryParse(builder.Configuration["Translaas:Timeout"], out var timeout)
        ? timeout
        : TimeSpan.FromSeconds(30);

    // Optional: Set default language fallback (read from appsettings.json, fallback to English)
    options.DefaultLanguage = builder.Configuration["Translaas:DefaultLanguage"] ?? L.English;

    // Optional: Configure offline cache (enabled by default with ApiFirst mode)
    var offlineCacheEnabled = builder.Configuration.GetValue<bool?>("Translaas:OfflineCache:Enabled") ?? true;
    if (offlineCacheEnabled)
    {
        options.OfflineCache.Enabled = true;
        options.OfflineCache.CacheDirectory = builder.Configuration["Translaas:OfflineCache:CacheDirectory"] ?? "./cache";
        
        // Parse FallbackMode from configuration, default to ApiFirst
        var fallbackModeStr = builder.Configuration["Translaas:OfflineCache:FallbackMode"] ?? "ApiFirst";
        if (Enum.TryParse<OfflineFallbackMode>(fallbackModeStr, ignoreCase: true, out var fallbackMode))
        {
            options.OfflineCache.FallbackMode = fallbackMode;
        }
        else
        {
            options.OfflineCache.FallbackMode = OfflineFallbackMode.ApiFirst;
        }
        
        options.OfflineCache.AutoSync = builder.Configuration.GetValue<bool?>("Translaas:OfflineCache:AutoSync") ?? false;
        options.OfflineCache.DefaultProjectId = builder.Configuration["Translaas:OfflineCache:DefaultProjectId"] ?? "translaas-sdk-samples";
        
        // Validate API settings for CacheFirst and ApiFirst modes
        if (options.OfflineCache.FallbackMode != OfflineFallbackMode.CacheOnly)
        {
            if (string.IsNullOrWhiteSpace(options.ApiKey) || options.ApiKey == "your-api-key-here")
            {
                throw new InvalidOperationException(
                    $"ApiKey is required for {options.OfflineCache.FallbackMode} mode. " +
                    "Please configure it in appsettings.json or set the TRANSLAAS_API_KEY environment variable.");
            }
            
            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException(
                    $"BaseUrl is required for {options.OfflineCache.FallbackMode} mode. " +
                    "Please configure it in appsettings.json or set the TRANSLAAS_BASE_URL environment variable.");
            }
        }
    }
}, language =>
{
    // Configure language resolution providers
    // Providers are checked in the order they are registered.
    // The first provider that returns a non-null language wins.
    // 
    // Priority order:
    // 1. UseRequest() - Resolves from HTTP request (route, query string, header, cookie)
    // 2. UseDefault() - Resolves from DefaultLanguage option (appsettings.json)
    // 3. UseCulture() - Resolves from thread culture (CultureInfo.CurrentUICulture) as fallback
    // 
    // This ensures DefaultLanguage from appsettings.json is prioritized over thread culture,
    // but still allows web-specific language selection via request parameters.
    language
        .UseRequest(request =>
        {
            // Configure which HTTP request sources to check
            request.Sources =
            [
                RequestLanguageSource.Route,      // e.g., /en/products
                RequestLanguageSource.QueryString, // e.g., ?lang=en
                RequestLanguageSource.Header,     // e.g., X-Language: en
                RequestLanguageSource.Cookie      // e.g., lang=en cookie
            ];
        })
        .UseDefault()  // Resolves from DefaultLanguage option (appsettings.json)
        .UseCulture(); // Resolves from thread culture (CultureInfo.CurrentUICulture) as fallback
});

// Add Translaas MVC services (for tag helpers and view helpers)
builder.Services.AddTranslaasMvc();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
