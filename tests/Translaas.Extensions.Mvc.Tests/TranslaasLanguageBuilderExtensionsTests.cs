using System;
using System.Linq;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Translaas.Extensions.DependencyInjection;

using Xunit;

namespace Translaas.Extensions.Mvc.Tests;

/// <summary>
/// Tests for TranslaasLanguageBuilderExtensions.
/// </summary>
public class TranslaasLanguageBuilderExtensionsTests
{
    [Fact]
    public void UseRequest_RegistersRequestLanguageProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        builder.UseRequest();

        // Assert
        var descriptors = services.Where(s => s.ServiceType == typeof(ILanguageProvider)).ToList();
        descriptors.Should().HaveCount(1);
        descriptors[0].Lifetime.Should().Be(ServiceLifetime.Scoped);
        
        // Verify the actual type by resolving the service
        var serviceProvider = services.BuildServiceProvider();
        using (var scope = serviceProvider.CreateScope())
        {
            var provider = scope.ServiceProvider.GetService<ILanguageProvider>();
            provider.Should().NotBeNull();
            provider.Should().BeOfType<RequestLanguageProvider>();
        }
    }

    [Fact]
    public void UseRequest_RegistersRequestLanguageOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        builder.UseRequest();

        // Assert
        var descriptors = services.Where(s => s.ServiceType == typeof(RequestLanguageOptions)).ToList();
        descriptors.Should().HaveCount(1);
        descriptors[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void UseRequest_RegistersIHttpContextAccessor_WhenNotPresent()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        builder.UseRequest();

        // Assert
        var descriptors = services.Where(s => s.ServiceType == typeof(IHttpContextAccessor)).ToList();
        descriptors.Should().HaveCount(1);
    }

    [Fact]
    public void UseRequest_DoesNotDuplicateIHttpContextAccessor_WhenAlreadyRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        builder.UseRequest();

        // Assert
        var descriptors = services.Where(s => s.ServiceType == typeof(IHttpContextAccessor)).ToList();
        descriptors.Should().HaveCount(1); // Should not add duplicate
    }

    [Fact]
    public void UseRequest_AppliesConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);
        bool configCalled = false;

        // Act
        builder.UseRequest(options =>
        {
            options.RouteParameterName = "culture";
            options.QueryParameterName = "locale";
            options.HeaderName = "X-Custom-Language";
            options.CookieName = "language";
            configCalled = true;
        });

        // Assert
        configCalled.Should().BeTrue();
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<RequestLanguageOptions>();
        options.RouteParameterName.Should().Be("culture");
        options.QueryParameterName.Should().Be("locale");
        options.HeaderName.Should().Be("X-Custom-Language");
        options.CookieName.Should().Be("language");
    }

    [Fact]
    public void UseRequest_ReturnsBuilderForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        var result = builder.UseRequest();

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void UseRequest_CanBeChained_WithOtherProviders()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        var result = builder
            .UseRequest()
            .UseCulture()
            .UseDefault();

        // Assert
        result.Should().BeSameAs(builder);
        var descriptors = services.Where(s => s.ServiceType == typeof(ILanguageProvider)).ToList();
        descriptors.Should().HaveCount(3);
    }

    [Fact]
    public void UseRequest_ThrowsArgumentNullException_WhenBuilderIsNull()
    {
        // Arrange & Act
        ITranslaasLanguageBuilder? builder = null;
        var act = () => builder!.UseRequest();

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("builder");
    }

    [Fact]
    public void UseRequest_ThrowsArgumentException_WhenBuilderIsNotTranslaasLanguageBuilder()
    {
        // Arrange
        var mockBuilder = new Mock<ITranslaasLanguageBuilder>();

        // Act
        var act = () => mockBuilder.Object.UseRequest();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*TranslaasLanguageBuilder*")
            .WithParameterName("builder");
    }

    [Fact]
    public void UseRequest_UsesDefaultSources_WhenNotConfigured()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        builder.UseRequest();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<RequestLanguageOptions>();
        options.Sources.Should().HaveCount(4);
        options.Sources[0].Should().Be(RequestLanguageSource.Route);
        options.Sources[1].Should().Be(RequestLanguageSource.QueryString);
        options.Sources[2].Should().Be(RequestLanguageSource.Header);
        options.Sources[3].Should().Be(RequestLanguageSource.Cookie);
    }

    [Fact]
    public void UseRequest_UsesCustomSources_WhenConfigured()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        builder.UseRequest(options =>
        {
            options.Sources = new System.Collections.Generic.List<RequestLanguageSource>
            {
                RequestLanguageSource.AcceptLanguage,
                RequestLanguageSource.Cookie
            };
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<RequestLanguageOptions>();
        options.Sources.Should().HaveCount(2);
        options.Sources[0].Should().Be(RequestLanguageSource.AcceptLanguage);
        options.Sources[1].Should().Be(RequestLanguageSource.Cookie);
    }

    [Fact]
    public void UseRequest_CreatesProvider_WithCorrectDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        builder.UseRequest();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // Should be able to resolve RequestLanguageProvider with its dependencies
        using (var scope = serviceProvider.CreateScope())
        {
            var provider = scope.ServiceProvider.GetService<ILanguageProvider>();
            provider.Should().NotBeNull();
            provider.Should().BeOfType<RequestLanguageProvider>();
        }
    }
}
