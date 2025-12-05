using System;

using FluentAssertions;

using Translaas.Caching;

using Xunit;

namespace Translaas.Caching.Tests;

/// <summary>
/// Tests for the CacheKeyBuilder class.
/// </summary>
public class CacheKeyBuilderTests
{
    [Fact]
    public void BuildEntryKey_ShouldReturnCorrectFormat_WhenAllParametersProvided()
    {
        // Arrange
        var group = "ui";
        var entry = "button.save";
        var lang = "en";

        // Act
        var key = CacheKeyBuilder.BuildEntryKey(group, entry, lang);

        // Assert
        key.Should().Be("entry:ui:button.save:en");
    }

    [Fact]
    public void BuildEntryKey_ShouldIncludeNumber_WhenNumberProvided()
    {
        // Arrange
        var group = "messages";
        var entry = "item.count";
        var lang = "en";
        var number = 5;

        // Act
        var key = CacheKeyBuilder.BuildEntryKey(group, entry, lang, number);

        // Assert
        key.Should().Be("entry:messages:item.count:en:5");
    }

    [Fact]
    public void BuildEntryKey_ShouldThrowArgumentNullException_WhenGroupIsNull()
    {
        // Arrange & Act
        Action act = () => CacheKeyBuilder.BuildEntryKey(null!, "entry", "en");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("group");
    }

    [Fact]
    public void BuildEntryKey_ShouldThrowArgumentNullException_WhenEntryIsNull()
    {
        // Arrange & Act
        Action act = () => CacheKeyBuilder.BuildEntryKey("group", null!, "en");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("entry");
    }

    [Fact]
    public void BuildEntryKey_ShouldThrowArgumentNullException_WhenLangIsNull()
    {
        // Arrange & Act
        Action act = () => CacheKeyBuilder.BuildEntryKey("group", "entry", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("lang");
    }

    [Fact]
    public void BuildGroupKey_ShouldReturnCorrectFormat_WhenAllParametersProvided()
    {
        // Arrange
        var project = "my-project";
        var group = "ui";
        var lang = "en";

        // Act
        var key = CacheKeyBuilder.BuildGroupKey(project, group, lang);

        // Assert
        key.Should().Be("group:my-project:ui:en");
    }

    [Fact]
    public void BuildGroupKey_ShouldIncludeFormat_WhenFormatProvided()
    {
        // Arrange
        var project = "my-project";
        var group = "ui";
        var lang = "en";
        var format = "json";

        // Act
        var key = CacheKeyBuilder.BuildGroupKey(project, group, lang, format);

        // Assert
        key.Should().Be("group:my-project:ui:en:json");
    }

    [Fact]
    public void BuildGroupKey_ShouldNotIncludeFormat_WhenFormatIsNull()
    {
        // Arrange
        var project = "my-project";
        var group = "ui";
        var lang = "en";

        // Act
        var key = CacheKeyBuilder.BuildGroupKey(project, group, lang, null);

        // Assert
        key.Should().Be("group:my-project:ui:en");
    }

    [Fact]
    public void BuildGroupKey_ShouldNotIncludeFormat_WhenFormatIsWhitespace()
    {
        // Arrange
        var project = "my-project";
        var group = "ui";
        var lang = "en";

        // Act
        var key = CacheKeyBuilder.BuildGroupKey(project, group, lang, "   ");

        // Assert
        key.Should().Be("group:my-project:ui:en");
    }

    [Fact]
    public void BuildGroupKey_ShouldThrowArgumentNullException_WhenProjectIsNull()
    {
        // Arrange & Act
        Action act = () => CacheKeyBuilder.BuildGroupKey(null!, "group", "en");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("project");
    }

    [Fact]
    public void BuildGroupKey_ShouldThrowArgumentNullException_WhenGroupIsNull()
    {
        // Arrange & Act
        Action act = () => CacheKeyBuilder.BuildGroupKey("project", null!, "en");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("group");
    }

    [Fact]
    public void BuildGroupKey_ShouldThrowArgumentNullException_WhenLangIsNull()
    {
        // Arrange & Act
        Action act = () => CacheKeyBuilder.BuildGroupKey("project", "group", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("lang");
    }

    [Fact]
    public void BuildProjectKey_ShouldReturnCorrectFormat_WhenAllParametersProvided()
    {
        // Arrange
        var project = "my-project";
        var lang = "en";

        // Act
        var key = CacheKeyBuilder.BuildProjectKey(project, lang);

        // Assert
        key.Should().Be("project:my-project:en");
    }

    [Fact]
    public void BuildProjectKey_ShouldIncludeFormat_WhenFormatProvided()
    {
        // Arrange
        var project = "my-project";
        var lang = "en";
        var format = "json";

        // Act
        var key = CacheKeyBuilder.BuildProjectKey(project, lang, format);

        // Assert
        key.Should().Be("project:my-project:en:json");
    }

    [Fact]
    public void BuildProjectKey_ShouldNotIncludeFormat_WhenFormatIsNull()
    {
        // Arrange
        var project = "my-project";
        var lang = "en";

        // Act
        var key = CacheKeyBuilder.BuildProjectKey(project, lang, null);

        // Assert
        key.Should().Be("project:my-project:en");
    }

    [Fact]
    public void BuildProjectKey_ShouldThrowArgumentNullException_WhenProjectIsNull()
    {
        // Arrange & Act
        Action act = () => CacheKeyBuilder.BuildProjectKey(null!, "en");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("project");
    }

    [Fact]
    public void BuildProjectKey_ShouldThrowArgumentNullException_WhenLangIsNull()
    {
        // Arrange & Act
        Action act = () => CacheKeyBuilder.BuildProjectKey("project", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("lang");
    }

    [Fact]
    public void BuildLocalesKey_ShouldReturnCorrectFormat()
    {
        // Arrange
        var project = "my-project";

        // Act
        var key = CacheKeyBuilder.BuildLocalesKey(project);

        // Assert
        key.Should().Be("locales:my-project");
    }

    [Fact]
    public void BuildLocalesKey_ShouldThrowArgumentNullException_WhenProjectIsNull()
    {
        // Arrange & Act
        Action act = () => CacheKeyBuilder.BuildLocalesKey(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("project");
    }

    [Theory]
    [InlineData("group1", "entry1", "en", "entry:group1:entry1:en")]
    [InlineData("group2", "entry2", "fr", "entry:group2:entry2:fr")]
    public void BuildEntryKey_ShouldGenerateConsistentKeys(string group, string entry, string lang, string expectedKey)
    {
        // Act
        var key = CacheKeyBuilder.BuildEntryKey(group, entry, lang);

        // Assert
        key.Should().Be(expectedKey);
    }

    [Theory]
    [InlineData("project1", "group1", "en", "group:project1:group1:en")]
    [InlineData("project2", "group2", "fr", "group:project2:group2:fr")]
    public void BuildGroupKey_ShouldGenerateConsistentKeys(string project, string group, string lang, string expectedKey)
    {
        // Act
        var key = CacheKeyBuilder.BuildGroupKey(project, group, lang);

        // Assert
        key.Should().Be(expectedKey);
    }
}
