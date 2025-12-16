using System;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Translaas.Extensions.DependencyInjection;

using Xunit;

namespace Translaas.Extensions.Mvc.Tests;

/// <summary>
/// Tests for the Translaas static helper class.
/// </summary>
public class TranslaasHelperTests
{
    [Fact]
    public void T_ThrowsArgumentNullException_WhenHtmlHelperIsNull()
    {
        // Arrange & Act
        var act = () => Translaas.T(null!, "group", "entry", "lang");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("htmlHelper");
    }

    [Fact]
    public void T_ThrowsInvalidOperationException_WhenServiceNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        
        var viewContext = new ViewContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                RequestServices = serviceProvider
            }
        };
        
        var htmlHelper = Mock.Of<IHtmlHelper>(h => h.ViewContext == viewContext);

        // Act
        var act = () => Translaas.T(htmlHelper, "group", "entry", "lang");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ITranslaasService is not registered*");
    }

    [Fact]
    public void T_ReturnsHtmlContent_WhenServiceIsRegistered()
    {
        // Arrange
        var mockService = new Mock<ITranslaasService>();
        var expectedTranslation = "Hello, World!";
        
        mockService
            .Setup(s => s.T("common", "welcome", "en", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTranslation);

        var services = new ServiceCollection();
        services.AddSingleton(mockService.Object);
        var serviceProvider = services.BuildServiceProvider();
        
        var viewContext = new ViewContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                RequestServices = serviceProvider
            }
        };
        
        var htmlHelper = Mock.Of<IHtmlHelper>(h => h.ViewContext == viewContext);

        // Act
        var result = Translaas.T(htmlHelper, "common", "welcome", "en");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<HtmlString>();
        result.ToString().Should().Be(expectedTranslation);
        
        mockService.Verify(
            s => s.T("common", "welcome", "en", null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void T_ReturnsHtmlContent_WithPluralization()
    {
        // Arrange
        var mockService = new Mock<ITranslaasService>();
        var expectedTranslation = "5 items";
        
        mockService
            .Setup(s => s.T("messages", "item", "en", 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTranslation);

        var services = new ServiceCollection();
        services.AddSingleton(mockService.Object);
        var serviceProvider = services.BuildServiceProvider();
        
        var viewContext = new ViewContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                RequestServices = serviceProvider
            }
        };
        
        var htmlHelper = Mock.Of<IHtmlHelper>(h => h.ViewContext == viewContext);

        // Act
        var result = Translaas.T(htmlHelper, "messages", "item", "en", 5);

        // Assert
        result.Should().NotBeNull();
        result.ToString().Should().Be(expectedTranslation);
        
        mockService.Verify(
            s => s.T("messages", "item", "en", 5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void T_PassesCorrectParameters_ToService()
    {
        // Arrange
        var mockService = new Mock<ITranslaasService>();
        mockService
            .Setup(s => s.T(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("test");

        var services = new ServiceCollection();
        services.AddSingleton(mockService.Object);
        var serviceProvider = services.BuildServiceProvider();
        
        var viewContext = new ViewContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                RequestServices = serviceProvider
            }
        };
        
        var htmlHelper = Mock.Of<IHtmlHelper>(h => h.ViewContext == viewContext);

        // Act
        Translaas.T(htmlHelper, "test-group", "test-entry", "fr", 10);

        // Assert
        mockService.Verify(
            s => s.T("test-group", "test-entry", "fr", 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
