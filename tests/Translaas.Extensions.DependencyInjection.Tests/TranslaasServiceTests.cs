using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using Translaas.Client;

using Xunit;

namespace Translaas.Extensions.DependencyInjection.Tests;

/// <summary>
/// Tests for ITranslaasService and TranslaasService implementation.
/// </summary>
public class TranslaasServiceTests
{
    [Fact]
    public void TranslaasService_Constructor_ThrowsArgumentNullException_WhenClientIsNull()
    {
        // Arrange & Act
        var act = () => new TranslaasService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("client");
    }

    [Fact]
    public async Task T_DelegatesToClientGetEntryAsync()
    {
        // Arrange
        var mockClient = new Mock<ITranslaasClient>();
        var expectedResult = "Hello, World!";
        
        mockClient
            .Setup(c => c.GetEntryAsync("common", "welcome", "en", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var service = new TranslaasService(mockClient.Object);

        // Act
        var result = await service.T("common", "welcome", "en");

        // Assert
        result.Should().Be(expectedResult);
        mockClient.Verify(
            c => c.GetEntryAsync("common", "welcome", "en", null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task T_DelegatesToClientGetEntryAsync_WithNumber()
    {
        // Arrange
        var mockClient = new Mock<ITranslaasClient>();
        var expectedResult = "5 items";
        decimal? number = 5;
        
        mockClient
            .Setup(c => c.GetEntryAsync("messages", "item", "en", number, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var service = new TranslaasService(mockClient.Object);

        // Act
        var result = await service.T("messages", "item", "en", number);

        // Assert
        result.Should().Be(expectedResult);
        mockClient.Verify(
            c => c.GetEntryAsync("messages", "item", "en", number, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task T_PassesCancellationToken_ToClient()
    {
        // Arrange
        var mockClient = new Mock<ITranslaasClient>();
        var cancellationToken = new CancellationToken();
        
        mockClient
            .Setup(c => c.GetEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<Dictionary<string, string>?>(), cancellationToken))
            .ReturnsAsync("test");

        var service = new TranslaasService(mockClient.Object);

        // Act
        await service.T("group", "entry", "en", null, null, cancellationToken);

        // Assert
        mockClient.Verify(
            c => c.GetEntryAsync("group", "entry", "en", null, null, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task T_DelegatesToClientGetEntryAsync_WithParameters()
    {
        // Arrange
        var mockClient = new Mock<ITranslaasClient>();
        var expectedResult = "Hello John, you have 5 items";
        var parameters = new Dictionary<string, string>
        {
            { "userName", "John" },
            { "itemCount", "5" }
        };
        
        mockClient
            .Setup(c => c.GetEntryAsync("messages", "greeting", "en", null, parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var service = new TranslaasService(mockClient.Object);

        // Act
        var result = await service.T("messages", "greeting", "en", parameters: parameters);

        // Assert
        result.Should().Be(expectedResult);
        mockClient.Verify(
            c => c.GetEntryAsync("messages", "greeting", "en", null, parameters, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task T_DelegatesToClientGetEntryAsync_WithNumberAndParameters()
    {
        // Arrange
        var mockClient = new Mock<ITranslaasClient>();
        var expectedResult = "Hello John, you have 5 items";
        decimal? number = 5;
        var parameters = new Dictionary<string, string>
        {
            { "userName", "John" }
        };
        
        mockClient
            .Setup(c => c.GetEntryAsync("messages", "items", "en", number, parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var service = new TranslaasService(mockClient.Object);

        // Act
        var result = await service.T("messages", "items", "en", number, parameters);

        // Assert
        result.Should().Be(expectedResult);
        mockClient.Verify(
            c => c.GetEntryAsync("messages", "items", "en", number, parameters, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task T_PropagatesException_FromClient()
    {
        // Arrange
        var mockClient = new Mock<ITranslaasClient>();
        var expectedException = new Models.Errors.TranslaasApiException("API Error", System.Net.HttpStatusCode.BadRequest);
        
        mockClient
            .Setup(c => c.GetEntryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var service = new TranslaasService(mockClient.Object);

        // Act
        var act = async () => await service.T("group", "entry", "en");

        // Assert
        await act.Should().ThrowAsync<Models.Errors.TranslaasApiException>()
            .WithMessage("API Error");
    }
}
