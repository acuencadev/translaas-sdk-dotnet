using FluentAssertions;

using Translaas.Models.Errors;

namespace Translaas.Client.Tests;

public class TranslaasClientOptionsTests
{
    [Fact]
    public void TranslaasClientOptions_ShouldHaveApiKeyProperty()
    {
        // Arrange & Act
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key"
        };

        // Assert
        options.ApiKey.Should().Be("test-api-key");
    }

    [Fact]
    public void TranslaasClientOptions_ShouldHaveBaseUrlProperty()
    {
        // Arrange & Act
        var options = new TranslaasClientOptions
        {
            BaseUrl = "https://api.test.com"
        };

        // Assert
        options.BaseUrl.Should().Be("https://api.test.com");
    }

    [Fact]
    public void TranslaasClientOptions_ShouldHaveDefaultBaseUrl()
    {
        // Arrange & Act
        var options = new TranslaasClientOptions();

        // Assert
        options.BaseUrl.Should().Be("https://sdk-api.translaas.local");
    }

    [Fact]
    public void TranslaasClientOptions_ShouldHaveTimeoutProperty()
    {
        // Arrange & Act
        var options = new TranslaasClientOptions
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Assert
        options.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void TranslaasClientOptions_Timeout_ShouldBeNullable()
    {
        // Arrange & Act
        var options = new TranslaasClientOptions
        {
            Timeout = null
        };

        // Assert
        options.Timeout.Should().BeNull();
    }

    [Fact]
    public void TranslaasClientOptions_Validate_ShouldThrowWhenApiKeyIsNull()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = null!
        };

        // Act & Assert
        var exception = Assert.Throws<TranslaasConfigurationException>(() => options.Validate());
        exception.Message.Should().Contain("ApiKey");
    }

    [Fact]
    public void TranslaasClientOptions_Validate_ShouldThrowWhenApiKeyIsEmpty()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = string.Empty
        };

        // Act & Assert
        var exception = Assert.Throws<TranslaasConfigurationException>(() => options.Validate());
        exception.Message.Should().Contain("ApiKey");
    }

    [Fact]
    public void TranslaasClientOptions_Validate_ShouldThrowWhenBaseUrlIsNull()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-key",
            BaseUrl = null!
        };

        // Act & Assert
        var exception = Assert.Throws<TranslaasConfigurationException>(() => options.Validate());
        exception.Message.Should().Contain("BaseUrl");
    }

    [Fact]
    public void TranslaasClientOptions_Validate_ShouldThrowWhenBaseUrlIsEmpty()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-key",
            BaseUrl = string.Empty
        };

        // Act & Assert
        var exception = Assert.Throws<TranslaasConfigurationException>(() => options.Validate());
        exception.Message.Should().Contain("BaseUrl");
    }

    [Fact]
    public void TranslaasClientOptions_Validate_ShouldThrowWhenBaseUrlIsInvalid()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-key",
            BaseUrl = "not-a-valid-url"
        };

        // Act & Assert
        var exception = Assert.Throws<TranslaasConfigurationException>(() => options.Validate());
        exception.Message.Should().Contain("BaseUrl");
    }

    [Fact]
    public void TranslaasClientOptions_Validate_ShouldSucceedWithValidOptions()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com",
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Act & Assert
        options.Validate(); // Should not throw
    }
}
