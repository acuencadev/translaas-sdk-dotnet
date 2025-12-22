using System.Collections.Generic;
using Translaas.Caching;
using Translaas.Extensions.DependencyInjection;
using Translaas.Extensions.Mvc;
using L = Translaas.Models.LanguageCodes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

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

    // Optional: Configure caching
    options.CacheMode = CacheMode.Group; // Cache at group level
    options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);
    options.CacheSlidingExpiration = TimeSpan.FromMinutes(30);

    // Optional: Configure timeout
    options.Timeout = TimeSpan.FromSeconds(30);

    // Optional: Set default language fallback (read from appsettings.json, fallback to English)
    options.DefaultLanguage = builder.Configuration["Translaas:DefaultLanguage"] ?? L.English;
}, language =>
{
    // Configure language resolution providers (checked in order)
    language
        .UseRequest(request =>
        {
            // Check HTTP request sources (route, query string, header, cookie)
            request.Sources = new List<RequestLanguageSource>
            {
                RequestLanguageSource.Route,      // e.g., /en/products
                RequestLanguageSource.QueryString, // e.g., ?lang=en
                RequestLanguageSource.Header,     // e.g., X-Language: en
                RequestLanguageSource.Cookie      // e.g., lang=en cookie
            };
        })
        .UseCulture()  // Fallback to thread culture (CultureInfo.CurrentUICulture)
        .UseDefault(); // Final fallback to DefaultLanguage from options
});

// Add Translaas MVC services (for tag helpers and view helpers)
builder.Services.AddTranslaasMvc();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
// Add language route support (optional - for /en/Home/Index style URLs)
app.MapControllerRoute(
    name: "language",
    pattern: "{lang}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
