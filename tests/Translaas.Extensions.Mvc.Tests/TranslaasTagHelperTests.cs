using FluentAssertions;

using Microsoft.AspNetCore.Razor.TagHelpers;

using Moq;

using Translaas.Extensions.DependencyInjection;

namespace Translaas.Extensions.Mvc.Tests;

/// <summary>
/// Tests for the TranslaasTagHelper class.
/// </summary>
public class TranslaasTagHelperTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenServiceIsNull()
    {
        // Arrange & Act
        var act = () => new TranslaasTagHelper(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("translaasService");
    }

    [Fact]
    public async Task ProcessAsync_ThrowsArgumentException_WhenGroupIsNullOrWhiteSpace()
    {
        // Arrange
        var mockService = new Mock<ITranslaasService>();
        var tagHelper = new TranslaasTagHelper(mockService.Object);
        tagHelper.Group = string.Empty;
        tagHelper.Entry = "entry";
        tagHelper.Lang = "en";

        var context = new TagHelperContext(
            "translaas",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString());

        var output = new TagHelperOutput(
            "translaas",
            new TagHelperAttributeList(),
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        var act = async () => await tagHelper.ProcessAsync(context, output);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("Group");
    }

    [Fact]
    public async Task ProcessAsync_ThrowsArgumentException_WhenEntryIsNullOrWhiteSpace()
    {
        // Arrange
        var mockService = new Mock<ITranslaasService>();
        var tagHelper = new TranslaasTagHelper(mockService.Object);
        tagHelper.Group = "group";
        tagHelper.Entry = string.Empty;
        tagHelper.Lang = "en";

        var context = new TagHelperContext(
            "translaas",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString());

        var output = new TagHelperOutput(
            "translaas",
            new TagHelperAttributeList(),
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        var act = async () => await tagHelper.ProcessAsync(context, output);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("Entry");
    }

    [Fact]
    public async Task ProcessAsync_ThrowsArgumentException_WhenLangIsNullOrWhiteSpace()
    {
        // Arrange
        var mockService = new Mock<ITranslaasService>();
        var tagHelper = new TranslaasTagHelper(mockService.Object);
        tagHelper.Group = "group";
        tagHelper.Entry = "entry";
        tagHelper.Lang = string.Empty;

        var context = new TagHelperContext(
            "translaas",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString());

        var output = new TagHelperOutput(
            "translaas",
            new TagHelperAttributeList(),
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        var act = async () => await tagHelper.ProcessAsync(context, output);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("Lang");
    }

    [Fact]
    public async Task ProcessAsync_ThrowsArgumentNullException_WhenOutputIsNull()
    {
        // Arrange
        var mockService = new Mock<ITranslaasService>();
        var tagHelper = new TranslaasTagHelper(mockService.Object);
        tagHelper.Group = "group";
        tagHelper.Entry = "entry";
        tagHelper.Lang = "en";

        var context = new TagHelperContext(
            "translaas",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString());

        // Act
        var act = async () => await tagHelper.ProcessAsync(context, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("output");
    }

    [Fact]
    public async Task ProcessAsync_SetsOutputContent_WhenValid()
    {
        // Arrange
        var mockService = new Mock<ITranslaasService>();
        var expectedTranslation = "Hello, World!";
        
        mockService
            .Setup(s => s.T("common", "welcome", "en", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTranslation);

        var tagHelper = new TranslaasTagHelper(mockService.Object);
        tagHelper.Group = "common";
        tagHelper.Entry = "welcome";
        tagHelper.Lang = "en";

        var context = new TagHelperContext(
            "translaas",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString());

        var output = new TagHelperOutput(
            "translaas",
            new TagHelperAttributeList(),
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        output.TagName.Should().BeNull();
        output.Content.GetContent().Should().Be(expectedTranslation);
        
        mockService.Verify(
            s => s.T("common", "welcome", "en", null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_SetsOutputContent_WithPluralization()
    {
        // Arrange
        var mockService = new Mock<ITranslaasService>();
        var expectedTranslation = "5 items";
        
        mockService
            .Setup(s => s.T("messages", "item", "en", 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTranslation);

        var tagHelper = new TranslaasTagHelper(mockService.Object);
        tagHelper.Group = "messages";
        tagHelper.Entry = "item";
        tagHelper.Lang = "en";
        tagHelper.Number = 5;

        var context = new TagHelperContext(
            "translaas",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString());

        var output = new TagHelperOutput(
            "translaas",
            new TagHelperAttributeList(),
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        output.TagName.Should().BeNull();
        output.Content.GetContent().Should().Be(expectedTranslation);
        
        mockService.Verify(
            s => s.T("messages", "item", "en", 5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_PassesCorrectParameters_ToService()
    {
        // Arrange
        var mockService = new Mock<ITranslaasService>();
        mockService
            .Setup(s => s.T(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("test");

        var tagHelper = new TranslaasTagHelper(mockService.Object);
        tagHelper.Group = "test-group";
        tagHelper.Entry = "test-entry";
        tagHelper.Lang = "fr";
        tagHelper.Number = 10;

        var context = new TagHelperContext(
            "translaas",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString());

        var output = new TagHelperOutput(
            "translaas",
            new TagHelperAttributeList(),
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        mockService.Verify(
            s => s.T("test-group", "test-entry", "fr", 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
