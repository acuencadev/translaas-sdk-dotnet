using System.Text.Json;
using FluentAssertions;
using Translaas.Models.Responses;

namespace Translaas.Models.Tests.Responses;

public class TranslationProjectTests
{
    [Fact]
    public void TranslationProject_ShouldDeserializeFromJson()
    {
        // Arrange
        var json = """{"ui":{"button.save":"Save","button.cancel":"Cancel"},"common":{"welcome":"Welcome"}}""";

        // Act
        var project = JsonSerializer.Deserialize<TranslationProject>(json);

        // Assert
        project.Should().NotBeNull();
        project!.Groups.Should().NotBeNull();
        project.Groups.Should().HaveCount(2);
        project.Groups.Should().ContainKey("ui");
        project.Groups.Should().ContainKey("common");
    }

    [Fact]
    public void TranslationProject_GetGroup_ShouldReturnCorrectGroup()
    {
        // Arrange
        var json = """{"ui":{"button.save":"Save","button.cancel":"Cancel"},"common":{"welcome":"Welcome"}}""";
        var project = JsonSerializer.Deserialize<TranslationProject>(json);

        // Act
        var uiGroup = project!.GetGroup("ui");
        var commonGroup = project.GetGroup("common");
        var missingGroup = project.GetGroup("missing");

        // Assert
        uiGroup.Should().NotBeNull();
        uiGroup!.GetValue("button.save").Should().Be("Save");
        uiGroup.GetValue("button.cancel").Should().Be("Cancel");
        
        commonGroup.Should().NotBeNull();
        commonGroup!.GetValue("welcome").Should().Be("Welcome");
        
        missingGroup.Should().BeNull();
    }

    [Fact]
    public void TranslationProject_ShouldSerializeToJson()
    {
        // Arrange
        var json = """{"ui":{"button.save":"Save"},"common":{"welcome":"Welcome"}}""";
        var project = JsonSerializer.Deserialize<TranslationProject>(json);

        // Act
        var serialized = JsonSerializer.Serialize(project);

        // Assert
        serialized.Should().Contain("\"ui\"");
        serialized.Should().Contain("\"button.save\"");
        serialized.Should().Contain("\"common\"");
        serialized.Should().Contain("\"welcome\"");
    }

    [Fact]
    public void TranslationProject_ShouldHandleEmptyGroups()
    {
        // Arrange
        var json = """{}""";

        // Act
        var project = JsonSerializer.Deserialize<TranslationProject>(json);

        // Assert
        project.Should().NotBeNull();
        project!.Groups.Should().NotBeNull();
        project.Groups.Should().BeEmpty();
    }
}
