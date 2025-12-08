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
                    options.BaseUrl = Environment.GetEnvironmentVariable("TRANSLAAS_BASE_URL") 
                        ?? "https://sdkapi.translaas.local/api";

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

        Console.WriteLine("=== Translaas SDK Console Sample ===\n");

        try
        {
            // Example 1: Using ITranslaasService (convenience wrapper)
            Console.WriteLine("Example 1: Using ITranslaasService.T()");
            var translation1 = await translaasService.T("common", "welcome", "en");
            Console.WriteLine($"Translation: {translation1}\n");

            // Example 2: Using ITranslaasClient.GetEntryAsync (full API)
            Console.WriteLine("Example 2: Using ITranslaasClient.GetEntryAsync()");
            var translation2 = await translaasClient.GetEntryAsync("common", "welcome", "en");
            Console.WriteLine($"Translation: {translation2}\n");

            // Example 3: Pluralization
            Console.WriteLine("Example 3: Pluralization");
            var translation3a = await translaasService.T("messages", "item", "en", 1);
            var translation3b = await translaasService.T("messages", "item", "en", 5);
            Console.WriteLine($"1 item: {translation3a}");
            Console.WriteLine($"5 items: {translation3b}\n");

            // Example 4: Get entire translation group
            Console.WriteLine("Example 4: Get entire translation group");
            var group = await translaasClient.GetGroupAsync("my-project", "common", "en");
            Console.WriteLine($"Group '{group.Group}' contains {group.Entries.Count} entries:");
            foreach (var entry in group.Entries)
            {
                Console.WriteLine($"  {entry.Key}: {entry.Value}");
            }
            Console.WriteLine();

            // Example 5: Get project locales
            Console.WriteLine("Example 5: Get available locales");
            var locales = await translaasClient.GetProjectLocalesAsync("my-project");
            Console.WriteLine($"Available locales: {string.Join(", ", locales.Locales)}\n");

            // Example 6: Caching demonstration
            Console.WriteLine("Example 6: Caching demonstration");
            Console.WriteLine("First call (cache miss):");
            var start1 = DateTime.UtcNow;
            await translaasService.T("common", "welcome", "en");
            var duration1 = DateTime.UtcNow - start1;
            Console.WriteLine($"Duration: {duration1.TotalMilliseconds}ms");

            Console.WriteLine("Second call (cache hit):");
            var start2 = DateTime.UtcNow;
            await translaasService.T("common", "welcome", "en");
            var duration2 = DateTime.UtcNow - start2;
            Console.WriteLine($"Duration: {duration2.TotalMilliseconds}ms");
            Console.WriteLine($"Cache speedup: {(duration1.TotalMilliseconds / duration2.TotalMilliseconds):F2}x faster\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
