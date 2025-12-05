using System.Linq;

using FluentAssertions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Translaas.Caching;
using Translaas.Client;

using Xunit;

namespace Translaas.Extensions.DependencyInjection.Tests;

/// <summary>
/// Tests for verifying correct service lifetimes.
/// </summary>
public class ServiceLifetimeTests
{
    [Fact]
    public void ITranslaasClient_ShouldBeRegisteredAsScoped()
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
    public void TranslaasOptions_ShouldBeRegisteredAsSingleton()
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

        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify that IOptions<TranslaasOptions> can be resolved
        // and that it's singleton (same instance returned multiple times)
        var options1 = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        var options2 = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        
        options1.Should().NotBeNull();
        options2.Should().NotBeNull();
        options1.Should().BeSameAs(options2); // Singleton verification
    }

    [Fact]
    public void IMemoryCache_ShouldBeRegisteredAsSingleton_WhenCachingEnabled()
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
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IMemoryCache));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void ITranslaasCacheProvider_ShouldBeRegisteredAsSingleton_WhenCachingEnabled()
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

    [Fact]
    public void ITranslaasClient_ShouldCreateNewInstancePerScope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        ITranslaasClient client1;
        ITranslaasClient client2;
        ITranslaasClient client3;

        using (var scope1 = serviceProvider.CreateScope())
        {
            client1 = scope1.ServiceProvider.GetRequiredService<ITranslaasClient>();
            client2 = scope1.ServiceProvider.GetRequiredService<ITranslaasClient>();
        }

        using (var scope2 = serviceProvider.CreateScope())
        {
            client3 = scope2.ServiceProvider.GetRequiredService<ITranslaasClient>();
        }

        // Assert
        // Same scope should return same instance (scoped services are reused within scope)
        client1.Should().BeSameAs(client2);
        
        // Different scopes should return different instances
        client1.Should().NotBeSameAs(client3);
    }

    [Fact]
    public void TranslaasOptions_ShouldBeSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options1 = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();
        var options2 = serviceProvider.GetRequiredService<IOptions<TranslaasOptions>>();

        // Assert
        options1.Should().BeSameAs(options2);
    }

    [Fact]
    public void IMemoryCache_ShouldBeSingleton_WhenCachingEnabled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
            options.CacheMode = CacheMode.Entry;
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var cache1 = serviceProvider.GetRequiredService<IMemoryCache>();
        var cache2 = serviceProvider.GetRequiredService<IMemoryCache>();

        // Assert
        cache1.Should().BeSameAs(cache2);
    }

    [Fact]
    public void ITranslaasCacheProvider_ShouldBeSingleton_WhenCachingEnabled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
            options.CacheMode = CacheMode.Entry;
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var cacheProvider1 = serviceProvider.GetRequiredService<ITranslaasCacheProvider>();
        var cacheProvider2 = serviceProvider.GetRequiredService<ITranslaasCacheProvider>();

        // Assert
        cacheProvider1.Should().BeSameAs(cacheProvider2);
    }

    [Fact]
    public void ITranslaasService_ShouldBeRegisteredAsScoped()
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
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ITranslaasService));
        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void ITranslaasService_ShouldCreateNewInstancePerScope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddTranslaas(options =>
        {
            options.ApiKey = "test-api-key";
            options.BaseUrl = "https://api.test.com";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        ITranslaasService service1;
        ITranslaasService service2;
        ITranslaasService service3;

        using (var scope1 = serviceProvider.CreateScope())
        {
            service1 = scope1.ServiceProvider.GetRequiredService<ITranslaasService>();
            service2 = scope1.ServiceProvider.GetRequiredService<ITranslaasService>();
        }

        using (var scope2 = serviceProvider.CreateScope())
        {
            service3 = scope2.ServiceProvider.GetRequiredService<ITranslaasService>();
        }

        // Assert
        // Same scope should return same instance (scoped services are reused within scope)
        service1.Should().BeSameAs(service2);
        
        // Different scopes should return different instances
        service1.Should().NotBeSameAs(service3);
    }
}
