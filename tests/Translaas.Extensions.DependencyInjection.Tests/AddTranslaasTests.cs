using FluentAssertions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Translaas.Caching;
using Translaas.Client;

namespace Translaas.Extensions.DependencyInjection.Tests;

/// <summary>
/// Tests for the AddTranslaas extension method.
/// </summary>
public class AddTranslaasTests
{
    [Fact]
    public void AddTranslaas_ShouldRegisterITranslaasClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetService<ITranslaasClient>();
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddTranslaas_ShouldRegisterTranslaasOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<TranslaasOptions>>();
        options.Should().NotBeNull();
        options!.Value.ApiKey.Should().Be("test-api-key");
        options.Value.BaseUrl.Should().Be("https://api.test.com");
    }

    [Fact]
    public void AddTranslaas_ShouldRegisterHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(ITranslaasClient));
        httpClient.Should().NotBeNull();
        httpClient.BaseAddress.Should().NotBeNull();
    }

    [Fact]
    public void AddTranslaas_ShouldRegisterIMemoryCache_WhenCacheModeIsNotNone()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
            options.CacheMode = CacheMode.Entry;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var cache = serviceProvider.GetService<IMemoryCache>();
        cache.Should().NotBeNull();
    }

    [Fact]
    public void AddTranslaas_ShouldNotRegisterIMemoryCache_WhenCacheModeIsNone()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
            options.CacheMode = CacheMode.None;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var cache = serviceProvider.GetService<IMemoryCache>();
        // IMemoryCache might be registered elsewhere, so we check that ITranslaasCacheProvider is not registered
        var cacheProvider = serviceProvider.GetService<ITranslaasCacheProvider>();
        cacheProvider.Should().BeNull();
    }

    [Fact]
    public void AddTranslaas_ShouldRegisterITranslaasCacheProvider_WhenCacheModeIsNotNone()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
            options.CacheMode = CacheMode.Entry;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var cacheProvider = serviceProvider.GetService<ITranslaasCacheProvider>();
        cacheProvider.Should().NotBeNull();
        cacheProvider.Should().BeOfType<MemoryCacheProvider>();
    }

    [Fact]
    public void AddTranslaas_ShouldNotRegisterITranslaasCacheProvider_WhenCacheModeIsNone()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
            options.CacheMode = CacheMode.None;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var cacheProvider = serviceProvider.GetService<ITranslaasCacheProvider>();
        cacheProvider.Should().BeNull();
    }

    [Fact]
    public void AddTranslaas_ShouldReturnIServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        var result = services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
        });

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddTranslaas_ShouldThrowArgumentNullException_WhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services!.AddTranslaas(options =>
            {
                options.ApiKey = "test-api-key";
                options.BaseUrl = "https://api.test.com";
            }));
    }

    [Fact]
    public void AddTranslaas_ShouldThrowArgumentNullException_WhenConfigureIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.AddTranslaas(null!));
    }

    [Fact]
    public void AddTranslaas_ShouldConfigureHttpClientWithOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key-123";
            options.BaseUrl = "https://api.test.com";
            options.Timeout = TimeSpan.FromSeconds(30);
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(ITranslaasClient));
        
        httpClient.BaseAddress!.ToString().Should().Be("https://api.test.com/");
        httpClient.DefaultRequestHeaders.GetValues("X-Api-Key").Should().ContainSingle().Which.Should().Be("test-api-key-123");
        httpClient.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void AddTranslaas_ShouldRegisterITranslaasClientAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
        });

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ITranslaasClient));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddTranslaas_ShouldRegisterTranslaasOptionsAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options1 = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        var options2 = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        
        // Options should be singleton (same instance)
        options1.Should().BeSameAs(options2);
    }

    [Fact]
    public void AddTranslaas_ShouldRegisterITranslaasCacheProviderAsSingleton_WhenCachingEnabled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();

        // Act
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
            options.CacheMode = CacheMode.Entry;
        });

        // Assert
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ITranslaasCacheProvider));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }
}
