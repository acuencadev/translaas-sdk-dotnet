using System.Globalization;
using System.Threading;

using FluentAssertions;

using Xunit;

namespace Translaas.Extensions.DependencyInjection.Tests.Language;

/// <summary>
/// Tests for CultureLanguageProvider.
/// </summary>
public class CultureLanguageProviderTests
{
    [Fact]
    public void GetLanguage_ReturnsTwoLetterISOLanguageName_ByDefault()
    {
        // Arrange
        var options = new CultureLanguageOptions { UseFullCultureName = false };
        var provider = new CultureLanguageProvider(options);

        // Set current culture to English (US)
        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("en-US");

        try
        {
            // Act
            var result = provider.GetLanguage();

            // Assert
            result.Should().Be("en");
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    [Fact]
    public void GetLanguage_ReturnsFullCultureName_WhenConfigured()
    {
        // Arrange
        var options = new CultureLanguageOptions { UseFullCultureName = true };
        var provider = new CultureLanguageProvider(options);

        // Set current culture to English (US)
        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("en-US");

        try
        {
            // Act
            var result = provider.GetLanguage();

            // Assert
            result.Should().Be("en-US");
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    [Fact]
    public void GetLanguage_ReturnsNull_ForInvariantCulture()
    {
        // Arrange
        var options = new CultureLanguageOptions();
        var provider = new CultureLanguageProvider(options);

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        try
        {
            // Act
            var result = provider.GetLanguage();

            // Assert
            result.Should().BeNull();
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    [Fact]
    public void GetLanguage_ReturnsTwoLetterCode_ForFrenchCulture()
    {
        // Arrange
        var options = new CultureLanguageOptions { UseFullCultureName = false };
        var provider = new CultureLanguageProvider(options);

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

        try
        {
            // Act
            var result = provider.GetLanguage();

            // Assert
            result.Should().Be("fr");
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    [Fact]
    public void GetLanguage_ReturnsFullCultureName_ForFrenchCulture_WhenConfigured()
    {
        // Arrange
        var options = new CultureLanguageOptions { UseFullCultureName = true };
        var provider = new CultureLanguageProvider(options);

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("fr-CA");

        try
        {
            // Act
            var result = provider.GetLanguage();

            // Assert
            result.Should().Be("fr-CA");
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange & Act
        var act = () => new CultureLanguageProvider(null!);

        // Assert
        act.Should().Throw<System.ArgumentNullException>()
            .WithParameterName("options");
    }
}
