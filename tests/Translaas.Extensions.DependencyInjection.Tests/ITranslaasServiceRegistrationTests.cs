using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace Translaas.Extensions.DependencyInjection.Tests;

/// <summary>
/// Tests for ITranslaasService registration in dependency injection.
/// </summary>
public class ITranslaasServiceRegistrationTests
{
    [Fact]
    public void AddTranslaas_ShouldRegisterITranslaasService()
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
        var service = serviceProvider.GetService<ITranslaasService>();
        service.Should().NotBeNull();
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

    [Fact]
    public void ITranslaasService_ShouldResolveTranslaasService()
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

        // Assert
        var service = serviceProvider.GetRequiredService<ITranslaasService>();
        service.Should().BeOfType<TranslaasService>();
    }
}
