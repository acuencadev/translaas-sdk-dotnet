using FluentAssertions;
using System.Text.Json;
using Translaas.Models.Responses;
using Xunit;

namespace Translaas.Models.Tests.Responses;

public class TranslationGroupTests
{
    [Fact]
    public void TranslationGroup_ShouldDeserializeFromJson()
    {
        // Arrange
        var json = """{"button.save":"Save","button.cancel":"Cancel"}""";

        // Act
        var group = JsonSerializer.Deserialize<TranslationGroup>(json);

        // Assert
        group.Should().NotBeNull();
        group!.Entries.Should().NotBeNull();
        group.Entries.Should().HaveCount(2);
        group.Entries.Should().ContainKey("button.save");
        group.Entries.Should().ContainKey("button.cancel");
    }

    [Fact]
    public void TranslationGroup_GetValue_ShouldReturnCorrectTranslation()
    {
        // Arrange
        var json = """{"button.save":"Save","button.cancel":"Cancel"}""";
        var group = JsonSerializer.Deserialize<TranslationGroup>(json);

        // Act
        var saveValue = group!.GetValue("button.save");
        var cancelValue = group.GetValue("button.cancel");
        var missingValue = group.GetValue("button.missing");

        // Assert
        saveValue.Should().Be("Save");
        cancelValue.Should().Be("Cancel");
        missingValue.Should().BeNull();
    }

    [Fact]
    public void TranslationGroup_ShouldSerializeToJson()
    {
        // Arrange
        var json = """{"button.save":"Save","button.cancel":"Cancel"}""";
        var group = JsonSerializer.Deserialize<TranslationGroup>(json);

        // Act
        var serialized = JsonSerializer.Serialize(group);

        // Assert
        serialized.Should().Contain("\"button.save\"");
        serialized.Should().Contain("\"Save\"");
        serialized.Should().Contain("\"button.cancel\"");
        serialized.Should().Contain("\"Cancel\"");
    }

    [Fact]
    public void TranslationGroup_ShouldHandleEmptyEntries()
    {
        // Arrange
        var json = """{}""";

        // Act
        var group = JsonSerializer.Deserialize<TranslationGroup>(json);

        // Assert
        group.Should().NotBeNull();
        group!.Entries.Should().NotBeNull();
        group.Entries.Should().BeEmpty();
    }
}
