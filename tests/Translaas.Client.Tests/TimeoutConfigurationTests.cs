using FluentAssertions;

using Translaas.Models.Errors;

namespace Translaas.Client.Tests;

public class TimeoutConfigurationTests
{
    [Fact]
    public void TranslaasClient_ShouldApplyTimeout_WhenTimeoutIsSpecified()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com/api",
            Timeout = TimeSpan.FromSeconds(30)
        };

        var httpClient = new HttpClient();

        // Act
        _ = new TranslaasClient(httpClient, options);

        // Assert
        httpClient.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void TranslaasClient_ShouldNotChangeTimeout_WhenTimeoutIsNotSpecified()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com/api",
            Timeout = null
        };

        var httpClient = new HttpClient();
        var originalTimeout = httpClient.Timeout;

        // Act
        _ = new TranslaasClient(httpClient, options);

        // Assert
        httpClient.Timeout.Should().Be(originalTimeout);
    }

    [Fact]
    public void TranslaasClientOptions_Validate_ShouldThrowException_WhenTimeoutIsZero()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com/api",
            Timeout = TimeSpan.Zero
        };

        // Act & Assert
        var exception = Assert.Throws<TranslaasConfigurationException>(() => options.Validate());
        exception.Message.Should().Contain("Timeout must be greater than zero");
    }

    [Fact]
    public void TranslaasClientOptions_Validate_ShouldThrowException_WhenTimeoutIsNegative()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com/api",
            Timeout = TimeSpan.FromSeconds(-1)
        };

        // Act & Assert
        var exception = Assert.Throws<TranslaasConfigurationException>(() => options.Validate());
        exception.Message.Should().Contain("Timeout must be greater than zero");
    }

    [Fact]
    public void TranslaasClientOptions_Validate_ShouldNotThrowException_WhenTimeoutIsValid()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com/api",
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Act & Assert
        options.Invoking(o => o.Validate()).Should().NotThrow();
    }

    [Fact]
    public void TranslaasClientOptions_Validate_ShouldNotThrowException_WhenTimeoutIsNull()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.test.com/api",
            Timeout = null
        };

        // Act & Assert
        options.Invoking(o => o.Validate()).Should().NotThrow();
    }
}
