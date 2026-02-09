using FluentAssertions;

using Moq;

namespace Translaas.Extensions.DependencyInjection.Tests.Language;

/// <summary>
/// Tests for LanguageResolver.
/// </summary>
public class LanguageResolverTests
{
    [Fact]
    public void Resolve_ReturnsFirstNonNullValue_FromProviders()
    {
        // Arrange
        var provider1 = new Mock<ILanguageProvider>();
        provider1.Setup(p => p.GetLanguage()).Returns((string?)null);

        var provider2 = new Mock<ILanguageProvider>();
        provider2.Setup(p => p.GetLanguage()).Returns("en");

        var provider3 = new Mock<ILanguageProvider>();
        // Should not be called due to short-circuit

        var providers = new List<ILanguageProvider> { provider1.Object, provider2.Object, provider3.Object };
        var resolver = new LanguageResolver(providers);

        // Act
        var result = resolver.Resolve();

        // Assert
        result.Should().Be("en");
        provider1.Verify(p => p.GetLanguage(), Times.Once);
        provider2.Verify(p => p.GetLanguage(), Times.Once);
        provider3.Verify(p => p.GetLanguage(), Times.Never);
    }

    [Fact]
    public void Resolve_ReturnsNull_WhenAllProvidersReturnNull()
    {
        // Arrange
        var provider1 = new Mock<ILanguageProvider>();
        provider1.Setup(p => p.GetLanguage()).Returns((string?)null);

        var provider2 = new Mock<ILanguageProvider>();
        provider2.Setup(p => p.GetLanguage()).Returns((string?)null);

        var providers = new List<ILanguageProvider> { provider1.Object, provider2.Object };
        var resolver = new LanguageResolver(providers);

        // Act
        var result = resolver.Resolve();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Resolve_ReturnsNull_WhenNoProvidersRegistered()
    {
        // Arrange
        var providers = new List<ILanguageProvider>();
        var resolver = new LanguageResolver(providers);

        // Act
        var result = resolver.Resolve();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Resolve_IgnoresEmptyStrings_AndContinuesToNextProvider()
    {
        // Arrange
        var provider1 = new Mock<ILanguageProvider>();
        provider1.Setup(p => p.GetLanguage()).Returns("");

        var provider2 = new Mock<ILanguageProvider>();
        provider2.Setup(p => p.GetLanguage()).Returns("   ");

        var provider3 = new Mock<ILanguageProvider>();
        provider3.Setup(p => p.GetLanguage()).Returns("en");

        var providers = new List<ILanguageProvider> { provider1.Object, provider2.Object, provider3.Object };
        var resolver = new LanguageResolver(providers);

        // Act
        var result = resolver.Resolve();

        // Assert
        result.Should().Be("en");
    }

    [Fact]
    public void Resolve_HandlesProviderException_Gracefully()
    {
        // Arrange
        var provider1 = new Mock<ILanguageProvider>();
        provider1.Setup(p => p.GetLanguage()).Throws(new System.Exception("Provider error"));

        var provider2 = new Mock<ILanguageProvider>();
        provider2.Setup(p => p.GetLanguage()).Returns("en");

        var providers = new List<ILanguageProvider> { provider1.Object, provider2.Object };
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<LanguageResolver>>();
        var resolver = new LanguageResolver(providers, logger.Object);

        // Act
        var result = resolver.Resolve();

        // Assert
        result.Should().Be("en");
        provider2.Verify(p => p.GetLanguage(), Times.Once);
    }

    [Fact]
    public void Resolve_ReturnsNull_WhenAllProvidersThrow()
    {
        // Arrange
        var provider1 = new Mock<ILanguageProvider>();
        provider1.Setup(p => p.GetLanguage()).Throws(new System.Exception("Provider error 1"));

        var provider2 = new Mock<ILanguageProvider>();
        provider2.Setup(p => p.GetLanguage()).Throws(new System.Exception("Provider error 2"));

        var providers = new List<ILanguageProvider> { provider1.Object, provider2.Object };
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<LanguageResolver>>();
        var resolver = new LanguageResolver(providers, logger.Object);

        // Act
        var result = resolver.Resolve();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenProvidersIsNull()
    {
        // Arrange & Act
        var act = () => new LanguageResolver(null!);

        // Assert
        act.Should().Throw<System.ArgumentNullException>()
            .WithParameterName("providers");
    }
}
