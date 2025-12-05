using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Translaas.Client;
using Translaas.Models.Errors;

namespace Translaas.Extensions.Http.Tests;

/// <summary>
/// Tests for the AddTranslaasHttpClient extension method.
/// </summary>
public class AddTranslaasHttpClientTests
{
    [Fact]
    public void AddTranslaasHttpClient_ShouldRegisterHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = services.AddTranslaasHttpClient(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
        });

        // Assert
        builder.Should().NotBeNull();
        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        httpClientFactory.Should().NotBeNull();
    }

    [Fact]
    public void AddTranslaasHttpClient_ShouldConfigureBaseAddress()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedBaseUrl = "https://api.test.com";

        // Act
        services.AddTranslaasHttpClient(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = expectedBaseUrl;
        });

        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(ITranslaasClient));

        // Assert
        httpClient.BaseAddress.Should().NotBeNull();
        httpClient.BaseAddress!.ToString().Should().Be(expectedBaseUrl + "/");
    }

    [Fact]
    public void AddTranslaasHttpClient_ShouldConfigureApiKeyHeader()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedApiKey = "test-api-key-123";

        // Act
        services.AddTranslaasHttpClient(options =>
        {
            options.ApiKey = expectedApiKey;
            options.BaseUrl = "https://api.test.com";
        });

        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(ITranslaasClient));

        // Assert
        httpClient.DefaultRequestHeaders.Contains("X-Api-Key").Should().BeTrue();
        httpClient.DefaultRequestHeaders.GetValues("X-Api-Key").Should().ContainSingle().Which.Should().Be(expectedApiKey);
    }

    [Fact]
    public void AddTranslaasHttpClient_ShouldConfigureTimeout()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedTimeout = TimeSpan.FromSeconds(30);

        // Act
        services.AddTranslaasHttpClient(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
            options.Timeout = expectedTimeout;
        });

        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(ITranslaasClient));

        // Assert
        httpClient.Timeout.Should().Be(expectedTimeout);
    }

    [Fact]
    public void AddTranslaasHttpClient_ShouldUseDefaultTimeout_WhenTimeoutNotSpecified()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTranslaasHttpClient(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
            // Timeout not set
        });

        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(ITranslaasClient));

        // Assert
        httpClient.Timeout.Should().Be(TimeSpan.FromSeconds(100)); // HttpClient default timeout
    }

    [Fact]
    public void AddTranslaasHttpClient_ShouldReturnIHttpClientBuilder()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = services.AddTranslaasHttpClient(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
        });

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeAssignableTo<IHttpClientBuilder>();
    }

    [Fact]
    public void AddTranslaasHttpClient_ShouldAllowFurtherConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = services.AddTranslaasHttpClient(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
        });

        // Configure additional headers via builder
        builder.ConfigureHttpClient(client =>
        {
            client.DefaultRequestHeaders.Add("Custom-Header", "custom-value");
        });

        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(ITranslaasClient));

        // Assert
        httpClient.DefaultRequestHeaders.Contains("Custom-Header").Should().BeTrue();
        httpClient.DefaultRequestHeaders.GetValues("Custom-Header").Should().ContainSingle().Which.Should().Be("custom-value");
    }

    [Fact]
    public void AddTranslaasHttpClient_ShouldThrowArgumentNullException_WhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services!.AddTranslaasHttpClient(options =>
            {
                options.ApiKey = "test-api-key";
                options.BaseUrl = "https://api.test.com";
            }));
    }

    [Fact]
    public void AddTranslaasHttpClient_ShouldThrowArgumentNullException_WhenConfigureIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.AddTranslaasHttpClient(null!));
    }

    [Fact]
    public void AddTranslaasHttpClient_ShouldValidateOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Validation happens during registration (fail fast)
        var exception = Assert.Throws<TranslaasConfigurationException>(() =>
            services.AddTranslaasHttpClient(options =>
            {
                // Invalid options - empty ApiKey
                options.ApiKey = string.Empty;
                options.BaseUrl = "https://api.test.com";
            }));

        exception.Message.Should().Contain("ApiKey is required and cannot be null or empty");
    }
}
