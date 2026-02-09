using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace Translaas.Extensions.DependencyInjection.Tests.Language;

/// <summary>
/// Tests for TranslaasLanguageBuilder.
/// </summary>
public class TranslaasLanguageBuilderTests
{
    [Fact]
    public void UseCulture_RegistersCultureLanguageProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        builder.UseCulture();

        // Assert
        var descriptors = services.Where(s => s.ServiceType == typeof(ILanguageProvider)).ToList();
        descriptors.Should().HaveCount(1);
        descriptors[0].Lifetime.Should().Be(ServiceLifetime.Transient);
        
        // Verify the actual type by resolving the service
        var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetService<ILanguageProvider>();
        provider.Should().NotBeNull();
        provider.Should().BeOfType<CultureLanguageProvider>();
    }

    [Fact]
    public void UseCulture_RegistersCultureLanguageOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        builder.UseCulture();

        // Assert
        var descriptors = services.Where(s => s.ServiceType == typeof(CultureLanguageOptions)).ToList();
        descriptors.Should().HaveCount(1);
        descriptors[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void UseCulture_AppliesConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);
        bool configCalled = false;

        // Act
        builder.UseCulture(options =>
        {
            options.UseFullCultureName = true;
            configCalled = true;
        });

        // Assert
        configCalled.Should().BeTrue();
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<CultureLanguageOptions>();
        options.UseFullCultureName.Should().BeTrue();
    }

    [Fact]
    public void UseDefault_RegistersDefaultLanguageProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        builder.UseDefault();

        // Assert
        var descriptors = services.Where(s => s.ServiceType == typeof(ILanguageProvider)).ToList();
        descriptors.Should().HaveCount(1);
        descriptors[0].ImplementationType.Should().Be<DefaultLanguageProvider>();
        descriptors[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void UseProvider_WithType_RegistersProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        builder.UseProvider<MockLanguageProvider>();

        // Assert
        var descriptors = services.Where(s => s.ServiceType == typeof(ILanguageProvider)).ToList();
        descriptors.Should().HaveCount(1);
        descriptors[0].ImplementationType.Should().Be<MockLanguageProvider>();
    }

    [Fact]
    public void UseProvider_WithInstance_RegistersProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);
        var provider = new Mock<ILanguageProvider>().Object;

        // Act
        builder.UseProvider(provider);

        // Assert
        var descriptors = services.Where(s => s.ServiceType == typeof(ILanguageProvider)).ToList();
        descriptors.Should().HaveCount(1);
        descriptors[0].ImplementationInstance.Should().Be(provider);
        descriptors[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void UseProvider_WithFactory_RegistersProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);
        var provider = new Mock<ILanguageProvider>().Object;

        // Act
        builder.UseProvider(sp => provider);

        // Assert
        var descriptors = services.Where(s => s.ServiceType == typeof(ILanguageProvider)).ToList();
        descriptors.Should().HaveCount(1);
        descriptors[0].Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void UseProvider_WithInstance_ThrowsArgumentNullException_WhenProviderIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        var act = () => builder.UseProvider((ILanguageProvider)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("provider");
    }

    [Fact]
    public void UseProvider_WithFactory_ThrowsArgumentNullException_WhenFactoryIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        var act = () => builder.UseProvider((Func<IServiceProvider, ILanguageProvider>)null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("factory");
    }

    [Fact]
    public void Builder_Methods_ReturnBuilderForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new TranslaasLanguageBuilder(services);

        // Act
        var result = builder
            .UseCulture()
            .UseDefault()
            .UseProvider<MockLanguageProvider>();

        // Assert
        result.Should().BeSameAs(builder);
        var descriptors = services.Where(s => s.ServiceType == typeof(ILanguageProvider)).ToList();
        descriptors.Should().HaveCount(3);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenServicesIsNull()
    {
        // Arrange & Act
        var act = () => new TranslaasLanguageBuilder(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    private class MockLanguageProvider : ILanguageProvider
    {
        public string? GetLanguage() => "en";
    }
}
