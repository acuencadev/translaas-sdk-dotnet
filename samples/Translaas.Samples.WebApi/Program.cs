using Microsoft.AspNetCore.Mvc;
using Translaas.Caching;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;
using Translaas.Extensions.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient support (required for Translaas)
builder.Services.AddHttpClient();

// Configure Translaas with options
builder.Services.AddTranslaas(options =>
{
    // Required: Set your API key
    options.ApiKey = builder.Configuration["Translaas:ApiKey"] 
        ?? Environment.GetEnvironmentVariable("TRANSLAAS_API_KEY") 
        ?? "your-api-key-here";

    // Required: Set the base URL
    options.BaseUrl = builder.Configuration["Translaas:BaseUrl"] 
        ?? Environment.GetEnvironmentVariable("TRANSLAAS_BASE_URL") 
        ?? "https://sdk-api.translaas.local/api";

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
