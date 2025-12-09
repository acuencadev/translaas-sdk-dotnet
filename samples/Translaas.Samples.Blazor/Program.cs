using Translaas.Caching;
using Translaas.Extensions.DependencyInjection;
using Translaas.Extensions.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add HttpClient support (required for Translaas)
builder.Services.AddHttpClient();

// Configure Translaas with options
builder.Services.AddTranslaas(options =>
{
    // Required: Set your API key
    options.ApiKey = builder.Configuration["Translaas:ApiKey"] 
        ?? Environment.GetEnvironmentVariable("TRANSLAAS_API_KEY") 
        ?? "fYMrUMSEt5NIEPiYDJQwzyTVW7YMILiv";

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
