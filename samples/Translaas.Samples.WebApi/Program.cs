using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Translaas.Caching;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;
using Translaas.Extensions.Mvc;
using L = Translaas.Models.LanguageCodes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    // Web APIs can use HTTP request sources since they have HTTP context
    language
        .UseRequest(request =>
        {
            // Check HTTP request sources (route, query string, header, cookie)
            request.Sources = new List<RequestLanguageSource>
            {
                RequestLanguageSource.Route,      // e.g., /api/translation/en/entry
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
