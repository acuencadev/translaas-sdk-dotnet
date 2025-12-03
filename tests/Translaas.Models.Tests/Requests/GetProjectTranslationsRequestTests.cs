using FluentAssertions;

using System.Text.Json;

using Translaas.Models.Requests;

namespace Translaas.Models.Tests.Requests;

public class GetProjectTranslationsRequestTests
{
    [Fact]
    public void GetProjectTranslationsRequest_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var request = new GetProjectTranslationsRequest
        {
            Project = "my-project",
            Lang = "en",
            Format = "json"
        };

        // Assert
        request.Project.Should().Be("my-project");
        request.Lang.Should().Be("en");
        request.Format.Should().Be("json");
    }

    [Fact]
    public void GetProjectTranslationsRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new GetProjectTranslationsRequest
        {
            Project = "my-project",
            Lang = "en",
            Format = "json"
        };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        json.Should().Contain("\"project\":\"my-project\"");
        json.Should().Contain("\"lang\":\"en\"");
        json.Should().Contain("\"format\":\"json\"");
    }

    [Fact]
    public void GetProjectTranslationsRequest_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """{"project":"my-project","lang":"en","format":"json"}""";

        // Act
        var request = JsonSerializer.Deserialize<GetProjectTranslationsRequest>(json);

        // Assert
        request.Should().NotBeNull();
        request!.Project.Should().Be("my-project");
        request.Lang.Should().Be("en");
        request.Format.Should().Be("json");
    }
}
