using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Translaas.Caching;
using Translaas.Extensions.DependencyInjection;
using Translaas.Samples.Maui.ViewModels;
using Translaas.Samples.Maui.Views;

namespace Translaas.Samples.Maui;

/// <summary>
/// MAUI application entry point with dependency injection configuration.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the MAUI application.
    /// </summary>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Custom fonts - these files should be placed in Resources/Fonts/
                // If not available, the app will fall back to system fonts
                // Download OpenSans from: https://fonts.google.com/specimen/Open+Sans
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Configure embedded appsettings.json
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("appsettings.json");
        
        if (stream != null)
        {
            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();
            
            builder.Configuration.AddConfiguration(config);
        }

        // Add HttpClient support (required for Translaas)
        builder.Services.AddHttpClient();

        // Configure Translaas with options from configuration
        ConfigureTranslaas(builder.Services, builder.Configuration);

        // Register ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Register Views
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    /// <summary>
    /// Configures Translaas services with appropriate settings for mobile/desktop scenarios.
    /// </summary>
    private static void ConfigureTranslaas(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTranslaas(options =>
        {
            // Required: Set your API key
            // For production, consider using secure storage instead of appsettings
            options.ApiKey = !string.IsNullOrWhiteSpace(configuration["Translaas:ApiKey"])
                ? configuration["Translaas:ApiKey"]!
                : Environment.GetEnvironmentVariable("TRANSLAAS_API_KEY")
                ?? "your-api-key-here";

            // Required: Set the base URL
            // Note: Do NOT include /api in the BaseUrl - the client adds /api/ to all endpoints
            options.BaseUrl = configuration["Translaas:BaseUrl"]
                ?? Environment.GetEnvironmentVariable("TRANSLAAS_BASE_URL")
                ?? "https://sdk-api.translaas.local";

            // Configure caching - Group mode is recommended for mobile apps
            // This minimizes network calls and supports offline scenarios
            options.CacheMode = configuration.GetValue<CacheMode?>("Translaas:CacheMode")
                ?? CacheMode.Group;

            // Cache settings - longer expiration for mobile apps to reduce network usage
            options.CacheAbsoluteExpiration = TimeSpan.TryParse(
                configuration["Translaas:CacheAbsoluteExpiration"], out var absoluteExpiration)
                ? absoluteExpiration
                : TimeSpan.FromHours(24); // 24 hours for mobile

            options.CacheSlidingExpiration = TimeSpan.TryParse(
                configuration["Translaas:CacheSlidingExpiration"], out var slidingExpiration)
                ? slidingExpiration
                : TimeSpan.FromHours(12); // 12 hours sliding for mobile

            // Configure timeout - slightly longer for mobile networks
            options.Timeout = TimeSpan.TryParse(
                configuration["Translaas:Timeout"], out var timeout)
                ? timeout
                : TimeSpan.FromSeconds(45);
        });
    }
}
