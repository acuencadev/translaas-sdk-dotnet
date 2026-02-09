using System.Text.Json;
using FluentAssertions;
using Translaas.Models.Responses;

namespace Translaas.Models.Tests.Responses;

public class ProjectLocalesTests
{
    [Fact]
    public void ProjectLocales_ShouldHaveLocalesProperty()
    {
        // Arrange & Act
        var locales = new ProjectLocales
        {

<<<<<<< TODO: Unmerged change from project 'Translaas.Models.Tests(net10.0)', Before:
            Locales = new List<string> { "en", "fr", "es" }
=======
            Locales = ["en", "fr", "es"]
>>>>>>> After
            Locales = ["en", "fr", "es"]
        };

        // Assert
        locales.Locales.Should().NotBeNull();
        locales.Locales.Should().HaveCount(3);
        locales.Locales.Should().Contain("en");
        locales.Locales.Should().Contain("fr");
        locales.Locales.Should().Contain("es");
    }

    [Fact]
    public void ProjectLocales_ShouldDeserializeFromJson()
    {
        // Arrange
        var json = """{"locales":["en","fr","es","de"]}""";

        // Act
        var locales = JsonSerializer.Deserialize<ProjectLocales>(json);

        // Assert
        locales.Should().NotBeNull();
        locales!.Locales.Should().NotBeNull();
        locales.Locales.Should().HaveCount(4);
        locales.Locales.Should().Contain("en");
        locales.Locales.Should().Contain("fr");
        locales.Locales.Should().Contain("es");
        locales.Locales.Should().Contain("de");
    }

    [Fact]
    public void ProjectLocales_ShouldSerializeToJson()
    {
        // Arrange
        var locales = new ProjectLocales
        {

<<<<<<< TODO: Unmerged change from project 'Translaas.Models.Tests(net10.0)', Before:
            Locales = new List<string> { "en", "fr", "es" }
=======
            Locales = ["en", "fr", "es"]
>>>>>>> After
            Locales = ["en", "fr", "es"]
        };

        // Act
        var json = JsonSerializer.Serialize(locales);

        // Assert
        json.Should().Contain("\"en\"");
        json.Should().Contain("\"fr\"");
        json.Should().Contain("\"es\"");
    }

    [Fact]
    public void ProjectLocales_ShouldHandleEmptyLocales()
    {
        // Arrange & Act
        var locales = new ProjectLocales
        {

<<<<<<< TODO: Unmerged change from project 'Translaas.Models.Tests(net10.0)', Before:
            Locales = new List<string>()
=======
            Locales = []
>>>>>>> After
            Locales = []
        };

        // Assert
        locales.Locales.Should().NotBeNull();
        locales.Locales.Should().BeEmpty();
    }
}
