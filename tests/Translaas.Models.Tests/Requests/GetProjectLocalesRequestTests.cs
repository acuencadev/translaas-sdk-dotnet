using FluentAssertions;

using System.Text.Json;

using Translaas.Models.Requests;

namespace Translaas.Models.Tests.Requests;

public class GetProjectLocalesRequestTests
{
    [Fact]
    public void GetProjectLocalesRequest_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var request = new GetProjectLocalesRequest
        {
            Project = "my-project"
        };

        // Assert
        request.Project.Should().Be("my-project");
    }

    [Fact]
    public void GetProjectLocalesRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new GetProjectLocalesRequest
        {
            Project = "my-project"
        };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        json.Should().Contain("\"project\":\"my-project\"");
    }

    [Fact]
    public void GetProjectLocalesRequest_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """{"project":"my-project"}""";

        // Act
        var request = JsonSerializer.Deserialize<GetProjectLocalesRequest>(json);

        // Assert
        request.Should().NotBeNull();
        request!.Project.Should().Be("my-project");
    }
}
