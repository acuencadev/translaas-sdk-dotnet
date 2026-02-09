using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

namespace Translaas.Extensions.DependencyInjection.Tests.Language;

/// <summary>
/// Tests for DefaultLanguageProvider.
/// </summary>
public class DefaultLanguageProviderTests
{
    [Fact]
    public void GetLanguage_ReturnsDefaultLanguage_WhenConfigured()
    {
        // Arrange
        var options = new TranslaasOptions { DefaultLanguage = "en" };
        var optionsMonitor = Mock.Of<IOptions<TranslaasOptions>>(o => o.Value == options);
        var provider = new DefaultLanguageProvider(optionsMonitor);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().Be("en");
    }

    [Fact]
    public void GetLanguage_ReturnsNull_WhenDefaultLanguageNotConfigured()
    {
        // Arrange
        var options = new TranslaasOptions { DefaultLanguage = null };
        var optionsMonitor = Mock.Of<IOptions<TranslaasOptions>>(o => o.Value == options);
        var provider = new DefaultLanguageProvider(optionsMonitor);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetLanguage_ReturnsNull_WhenDefaultLanguageIsEmpty()
    {
        // Arrange
        var options = new TranslaasOptions { DefaultLanguage = string.Empty };
        var optionsMonitor = Mock.Of<IOptions<TranslaasOptions>>(o => o.Value == options);
        var provider = new DefaultLanguageProvider(optionsMonitor);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetLanguage_ReturnsNull_WhenDefaultLanguageIsWhitespace()
    {
        // Arrange
        var options = new TranslaasOptions { DefaultLanguage = "   " };
        var optionsMonitor = Mock.Of<IOptions<TranslaasOptions>>(o => o.Value == options);
        var provider = new DefaultLanguageProvider(optionsMonitor);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetLanguage_ReturnsConfiguredLanguage_ForDifferentLanguages()
    {
        // Arrange
        var options = new TranslaasOptions { DefaultLanguage = "fr" };
        var optionsMonitor = Mock.Of<IOptions<TranslaasOptions>>(o => o.Value == options);
        var provider = new DefaultLanguageProvider(optionsMonitor);

        // Act
        var result = provider.GetLanguage();

        // Assert
        result.Should().Be("fr");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange & Act
        var act = () => new DefaultLanguageProvider(null!);

        // Assert
        act.Should().Throw<System.ArgumentNullException>()
            .WithParameterName("options");
    }
}
