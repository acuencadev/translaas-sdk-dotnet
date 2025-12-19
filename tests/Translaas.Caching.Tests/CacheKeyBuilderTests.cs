using System;
using System.Collections.Generic;

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
    public void BuildProjectKey_ShouldNotIncludeFormat_WhenFormatIsWhitespace()
    {
        // Arrange
        var project = "my-project";
        var lang = "en";

        // Act
        var key = CacheKeyBuilder.BuildProjectKey(project, lang, "   ");

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

    [Fact]
    public void BuildEntryKey_ShouldIncludeParameters_WhenParametersProvided()
    {
        // Arrange
        var group = "messages";
        var entry = "greeting";
        var lang = "en";
        var parameters = new Dictionary<string, string>
        {
            { "userName", "John" },
            { "count", "5" }
        };

        // Act
        var key = CacheKeyBuilder.BuildEntryKey(group, entry, lang, null, parameters);

        // Assert
        // Keys are normalized to lowercase for consistent cache keys with case-insensitive parameter matching
        key.Should().Be("entry:messages:greeting:en:count=5:username=John");
    }

    [Fact]
    public void BuildEntryKey_ShouldIncludeNumberAndParameters_WhenBothProvided()
    {
        // Arrange
        var group = "messages";
        var entry = "greeting";
        var lang = "en";
        var number = 5m;
        var parameters = new Dictionary<string, string>
        {
            { "userName", "John" }
        };

        // Act
        var key = CacheKeyBuilder.BuildEntryKey(group, entry, lang, number, parameters);

        // Assert
        // Keys are normalized to lowercase for consistent cache keys with case-insensitive parameter matching
        key.Should().Be("entry:messages:greeting:en:5:username=John");
    }

    [Fact]
    public void BuildEntryKey_ShouldSortParameters_ForConsistentKeys()
    {
        // Arrange
        var group = "messages";
        var entry = "greeting";
        var lang = "en";
        var parameters1 = new Dictionary<string, string>
        {
            { "userName", "John" },
            { "count", "5" }
        };
        var parameters2 = new Dictionary<string, string>
        {
            { "count", "5" },
            { "userName", "John" }
        };

        // Act
        var key1 = CacheKeyBuilder.BuildEntryKey(group, entry, lang, null, parameters1);
        var key2 = CacheKeyBuilder.BuildEntryKey(group, entry, lang, null, parameters2);

        // Assert
        key1.Should().Be(key2);
        // Keys are normalized to lowercase for consistent cache keys with case-insensitive parameter matching
        key1.Should().Be("entry:messages:greeting:en:count=5:username=John");
    }

    [Fact]
    public void BuildEntryKey_ShouldNotIncludeParameters_WhenParametersIsNull()
    {
        // Arrange
        var group = "ui";
        var entry = "button.save";
        var lang = "en";

        // Act
        var key = CacheKeyBuilder.BuildEntryKey(group, entry, lang, null, null);

        // Assert
        key.Should().Be("entry:ui:button.save:en");
    }

    [Fact]
    public void BuildEntryKey_ShouldNotIncludeParameters_WhenParametersIsEmpty()
    {
        // Arrange
        var group = "ui";
        var entry = "button.save";
        var lang = "en";
        var parameters = new Dictionary<string, string>();

        // Act
        var key = CacheKeyBuilder.BuildEntryKey(group, entry, lang, null, parameters);

        // Assert
        key.Should().Be("entry:ui:button.save:en");
    }

    [Fact]
    public void BuildEntryKey_ShouldHandleSpecialCharacters_InParameterValues()
    {
        // Arrange
        var group = "messages";
        var entry = "greeting";
        var lang = "en";
        var parameters = new Dictionary<string, string>
        {
            { "userName", "John Doe" },
            { "message", "Hello & Welcome" }
        };

        // Act
        var key = CacheKeyBuilder.BuildEntryKey(group, entry, lang, null, parameters);

        // Assert
        // Keys are normalized to lowercase for consistent cache keys with case-insensitive parameter matching
        key.Should().Be("entry:messages:greeting:en:message=Hello & Welcome:username=John Doe");
    }

    [Fact]
    public void BuildEntryKey_ShouldProduceSameCacheKey_ForCaseInsensitiveParameterKeys()
    {
        // Arrange
        var group = "messages";
        var entry = "greeting";
        var lang = "en";
        var parameters1 = new Dictionary<string, string>
        {
            { "userName", "John" },
            { "count", "5" }
        };
        var parameters2 = new Dictionary<string, string>
        {
            { "USERNAME", "John" },
            { "COUNT", "5" }
        };

        // Act
        var key1 = CacheKeyBuilder.BuildEntryKey(group, entry, lang, null, parameters1);
        var key2 = CacheKeyBuilder.BuildEntryKey(group, entry, lang, null, parameters2);

        // Assert
        // Both should produce the same cache key since parameter keys are normalized to lowercase
        // This ensures case-insensitive parameter matching (as used in MergeNumberIntoParameters) 
        // produces consistent cache keys
        key1.Should().Be(key2);
        key1.Should().Be("entry:messages:greeting:en:count=5:username=John");
    }
}
