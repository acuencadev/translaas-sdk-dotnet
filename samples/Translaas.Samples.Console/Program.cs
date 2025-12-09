using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Translaas.Caching;
using Translaas.Client;
using Translaas.Extensions.DependencyInjection;

namespace Translaas.Samples.Console;

/// <summary>
/// Console application demonstrating Translaas SDK usage with dependency injection.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Build the host with dependency injection
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Add HttpClient support (required for Translaas)
                services.AddHttpClient();

                // Configure Translaas with options
                services.AddTranslaas(options =>
                {
                    // Required: Set your API key
                    options.ApiKey = Environment.GetEnvironmentVariable("TRANSLAAS_API_KEY") 
                        ?? "your-api-key-here";

                    // Required: Set the base URL
                    // Note: Do NOT include /api in the BaseUrl - the client adds /api/ to all endpoints
                    options.BaseUrl = Environment.GetEnvironmentVariable("TRANSLAAS_BASE_URL") 
                        ?? "https://sdk-api.translaas.local";

                    // Optional: Configure caching
                    options.CacheMode = CacheMode.Group; // Cache at group level
                    options.CacheAbsoluteExpiration = TimeSpan.FromHours(1);
                    options.CacheSlidingExpiration = TimeSpan.FromMinutes(30);

                    // Optional: Configure timeout
                    options.Timeout = TimeSpan.FromSeconds(30);
                });
            })
            .Build();

        // Get the service from DI container
        var translaasService = host.Services.GetRequiredService<ITranslaasService>();
        var translaasClient = host.Services.GetRequiredService<ITranslaasClient>();

        System.Console.WriteLine("=== Translaas SDK Console Sample ===\n");

        try
        {
            const string projectId = "translaas-sdk-samples";

            // Example 1: Using ITranslaasService (convenience wrapper)
            System.Console.WriteLine("Example 1: Using ITranslaasService.T()");
            var translation1 = await translaasService.T("common", "welcome", "en");
            System.Console.WriteLine($"Translation: {translation1}\n");

            // Example 2: Using ITranslaasClient.GetEntryAsync (full API)
            System.Console.WriteLine("Example 2: Using ITranslaasClient.GetEntryAsync()");
            var translation2 = await translaasClient.GetEntryAsync("common", "welcome", "en");
            System.Console.WriteLine($"Translation: {translation2}\n");

            // Example 3: Pluralization
            System.Console.WriteLine("Example 3: Pluralization");
            var translation3a = await translaasService.T("messages", "item", "en", 1);
            var translation3b = await translaasService.T("messages", "item", "en", 5);
            System.Console.WriteLine($"1 item: {translation3a}");
            System.Console.WriteLine($"5 items: {translation3b}\n");

            // Example 4: Get multiple entries using .T() helper
            System.Console.WriteLine("Example 4: Get multiple entries using .T() helper");
            var appName = await translaasService.T("common", "app.name", "en");
            var welcome = await translaasService.T("common", "welcome", "en");
            var welcomeMessage = await translaasService.T("common", "welcome.message", "en");
            System.Console.WriteLine($"App Name: {appName}");
            System.Console.WriteLine($"Welcome: {welcome}");
            System.Console.WriteLine($"Message: {welcomeMessage}\n");

            // Example 5: Get entire translation group (bulk operation)
            System.Console.WriteLine("Example 5: Get entire translation group (bulk operation)");
            System.Console.WriteLine("Note: GetGroupAsync() retrieves all entries in a group at once.");
            System.Console.WriteLine("Use this when you need multiple entries, or use .T() for individual entries.\n");
            const string groupName = "common";
            var group = await translaasClient.GetGroupAsync(projectId, groupName, "en");
            System.Console.WriteLine($"Group '{groupName}' contains {group.Entries.Count} entries:");
            foreach (var entry in group.Entries)
            {
                var value = entry.Value.GetString() ?? entry.Value.ToString();
                System.Console.WriteLine($"  {entry.Key}: {value}");
            }
            System.Console.WriteLine();

            // Example 6: Get project locales
            System.Console.WriteLine("Example 6: Get available locales");
            var locales = await translaasClient.GetProjectLocalesAsync(projectId);
            System.Console.WriteLine($"Available locales: {string.Join(", ", locales.Locales)}\n");

            // Example 7: Caching demonstration
            System.Console.WriteLine("Example 7: Caching demonstration");
            System.Console.WriteLine("First call (cache miss):");
            var start1 = DateTime.UtcNow;
            await translaasService.T("common", "welcome", "en");
            var duration1 = DateTime.UtcNow - start1;
            System.Console.WriteLine($"Duration: {duration1.TotalMilliseconds:F2}ms");

            System.Console.WriteLine("Second call (cache hit):");
            var start2 = DateTime.UtcNow;
            await translaasService.T("common", "welcome", "en");
            var duration2 = DateTime.UtcNow - start2;
            System.Console.WriteLine($"Duration: {duration2.TotalMilliseconds:F2}ms");
            var speedup = duration1.TotalMilliseconds / duration2.TotalMilliseconds;
            System.Console.WriteLine($"Cache speedup: {speedup:F2}x faster\n");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
            System.Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        System.Console.WriteLine("Press any key to exit...");
        System.Console.ReadKey();
    }
}
