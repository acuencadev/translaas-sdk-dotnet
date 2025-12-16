using FluentAssertions;

using System.Text.Json;

using Translaas.Models.Responses;

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

    [Fact]
    public void TranslationGroup_ShouldHandlePluralForms()
    {
        // Arrange
        var json = """{"simple-count":{"one":"There is 1 record","other":"There are {0} records"}}""";

        // Act
        var group = JsonSerializer.Deserialize<TranslationGroup>(json);

        // Assert
        group.Should().NotBeNull();
        group!.Entries.Should().ContainKey("simple-count");
        group.HasPluralForms("simple-count").Should().BeTrue();
        group.GetValue("simple-count").Should().BeNull(); // Plural forms return null from GetValue
    }

    [Fact]
    public void TranslationGroup_GetPluralForms_ShouldReturnCorrectPluralForms()
    {
        // Arrange
        var json = """{"simple-count":{"one":"There is 1 record","other":"There are {0} records"}}""";
        var group = JsonSerializer.Deserialize<TranslationGroup>(json);

        // Act
        var pluralForms = group!.GetPluralForms("simple-count");

        // Assert
        pluralForms.Should().NotBeNull();
        pluralForms!.Should().HaveCount(2);
        pluralForms[Models.PluralCategory.One].Should().Be("There is 1 record");
        pluralForms[Models.PluralCategory.Other].Should().Be("There are {0} records");
    }

    [Fact]
    public void TranslationGroup_GetPluralForm_ShouldReturnSpecificPluralForm()
    {
        // Arrange
        var json = """{"simple-count":{"one":"There is 1 record","other":"There are {0} records"}}""";
        var group = JsonSerializer.Deserialize<TranslationGroup>(json);

        // Act
        var oneForm = group!.GetPluralForm("simple-count", Models.PluralCategory.One);
        var otherForm = group.GetPluralForm("simple-count", Models.PluralCategory.Other);
        var missingForm = group.GetPluralForm("simple-count", Models.PluralCategory.Few);

        // Assert
        oneForm.Should().Be("There is 1 record");
        otherForm.Should().Be("There are {0} records");
        missingForm.Should().BeNull();
    }

    [Fact]
    public void TranslationGroup_GetPluralForms_ShouldReturnNull_WhenEntryIsString()
    {
        // Arrange
        var json = """{"button.save":"Save"}""";
        var group = JsonSerializer.Deserialize<TranslationGroup>(json);

        // Act
        var pluralForms = group!.GetPluralForms("button.save");

        // Assert
        pluralForms.Should().BeNull();
        group.HasPluralForms("button.save").Should().BeFalse();
    }

    [Fact]
    public void TranslationGroup_ShouldHandleMixedEntries_WithPluralFormsAndStrings()
    {
        // Arrange
        var json = """{"button.save":"Save","simple-count":{"one":"There is 1 record","other":"There are {0} records"}}""";
        var group = JsonSerializer.Deserialize<TranslationGroup>(json);

        // Act & Assert
        group.Should().NotBeNull();
        group!.GetValue("button.save").Should().Be("Save");
        group.HasPluralForms("button.save").Should().BeFalse();
        
        group.GetValue("simple-count").Should().BeNull();
        group.HasPluralForms("simple-count").Should().BeTrue();
        var pluralForms = group.GetPluralForms("simple-count");
        pluralForms.Should().NotBeNull();
        pluralForms![Models.PluralCategory.Other].Should().Be("There are {0} records");
    }

    [Fact]
    public void TranslationGroup_GetPluralForms_ShouldHandleComplexPluralRules()
    {
        // Arrange - Arabic has zero, one, two, few, many, other
        var json = """{"item":{"zero":"0 items","one":"1 item","two":"2 items","few":"3-10 items","many":"11-99 items","other":"{0} items"}}""";
        var group = JsonSerializer.Deserialize<TranslationGroup>(json);

        // Act
        var pluralForms = group!.GetPluralForms("item");

        // Assert
        pluralForms.Should().NotBeNull();
        pluralForms!.Should().HaveCount(6);
        pluralForms[Models.PluralCategory.Zero].Should().Be("0 items");
        pluralForms[Models.PluralCategory.One].Should().Be("1 item");
        pluralForms[Models.PluralCategory.Two].Should().Be("2 items");
        pluralForms[Models.PluralCategory.Few].Should().Be("3-10 items");
        pluralForms[Models.PluralCategory.Many].Should().Be("11-99 items");
        pluralForms[Models.PluralCategory.Other].Should().Be("{0} items");
    }
}
