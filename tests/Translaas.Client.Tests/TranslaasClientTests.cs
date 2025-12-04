using FluentAssertions;

using Translaas.Models.Errors;

namespace Translaas.Client.Tests;

public class TranslaasClientTests
{
    [Fact]
    public void TranslaasClient_Constructor_ShouldAcceptHttpClientAndOptions()
    {
        // Arrange
        var httpClient = new HttpClient();
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://api.test.com"
        };

        // Act
        var client = new TranslaasClient(httpClient, options);

        // Assert
        client.Should().NotBeNull();
        client.Should().BeAssignableTo<ITranslaasClient>();
    }

    [Fact]
    public void TranslaasClient_Constructor_ShouldThrowWhenHttpClientIsNull()
    {
        // Arrange
        var options = new TranslaasClientOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://api.test.com"
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TranslaasClient(null!, options));
    }

    [Fact]
    public void TranslaasClient_Constructor_ShouldThrowWhenOptionsIsNull()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TranslaasClient(httpClient, null!));
    }

    [Fact]
    public void TranslaasClient_Constructor_ShouldValidateOptions()
    {
        // Arrange
        var httpClient = new HttpClient();
        var options = new TranslaasClientOptions
        {
            ApiKey = string.Empty, // Invalid
            BaseUrl = "https://api.test.com"
        };

        // Act & Assert
        Assert.Throws<TranslaasConfigurationException>(() => new TranslaasClient(httpClient, options));
    }
}
