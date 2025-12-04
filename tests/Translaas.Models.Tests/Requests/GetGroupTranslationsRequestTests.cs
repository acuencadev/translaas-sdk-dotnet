using FluentAssertions;

using System.Text.Json;

using Translaas.Models.Requests;

namespace Translaas.Models.Tests.Requests;

public class GetGroupTranslationsRequestTests
{
    [Fact]
    public void GetGroupTranslationsRequest_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var request = new GetGroupTranslationsRequest
        {
            Project = "my-project",
            Group = "ui",
            Lang = "en",
            Format = "json"
        };

        // Assert
        request.Project.Should().Be("my-project");
        request.Group.Should().Be("ui");
        request.Lang.Should().Be("en");
        request.Format.Should().Be("json");
    }

    [Fact]
    public void GetGroupTranslationsRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new GetGroupTranslationsRequest
        {
            Project = "my-project",
            Group = "ui",
            Lang = "en",
            Format = "json"
        };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        json.Should().Contain("\"project\":\"my-project\"");
        json.Should().Contain("\"group\":\"ui\"");
        json.Should().Contain("\"lang\":\"en\"");
        json.Should().Contain("\"format\":\"json\"");
    }

    [Fact]
    public void GetGroupTranslationsRequest_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """{"project":"my-project","group":"ui","lang":"en","format":"json"}""";

        // Act
        var request = JsonSerializer.Deserialize<GetGroupTranslationsRequest>(json);

        // Assert
        request.Should().NotBeNull();
        request!.Project.Should().Be("my-project");
        request.Group.Should().Be("ui");
        request.Lang.Should().Be("en");
        request.Format.Should().Be("json");
    }

    [Fact]
    public void GetGroupTranslationsRequest_ShouldHandleNullFormat()
    {
        // Arrange
        var request = new GetGroupTranslationsRequest
        {
            Project = "my-project",
            Group = "ui",
            Lang = "en",
            Format = null
        };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        json.Should().Contain("\"format\":null");
    }
}
