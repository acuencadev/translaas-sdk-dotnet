namespace Translaas.Client.IntegrationTests;

/// <summary>
/// Base class for integration tests.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly IntegrationTestConfiguration Configuration;
    protected readonly HttpClient HttpClient;
    protected readonly TranslaasClient Client;

    protected IntegrationTestBase()
    {
        Configuration = new IntegrationTestConfiguration();
        
        if (!Configuration.IsEnabled)
        {
            return; // Skip initialization if tests are disabled
        }

        Configuration.Validate();

        var options = new TranslaasClientOptions
        {
            ApiKey = Configuration.ApiKey,
            BaseUrl = Configuration.BaseUrl,
            Timeout = TimeSpan.FromSeconds(30)
        };

        HttpClient = new HttpClient();
        Client = new TranslaasClient(HttpClient, options);
    }

    public void Dispose()
    {
        HttpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
