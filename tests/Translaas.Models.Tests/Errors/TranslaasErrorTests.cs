using System.Text.Json;
using FluentAssertions;
using Translaas.Models.Errors;

namespace Translaas.Models.Tests.Errors;

public class TranslaasErrorTests
{
    [Fact]
    public void TranslaasError_ShouldHaveMessageProperty()
    {
        // Arrange & Act
        var error = new TranslaasError
        {
            Message = "Error message"
        };

        // Assert
        error.Message.Should().Be("Error message");
    }

    [Fact]
    public void TranslaasError_ShouldDeserializeFromJson()
    {
        // Arrange
        var json = """{"message":"Error message","code":"ERROR_CODE"}""";

        // Act
        var error = JsonSerializer.Deserialize<TranslaasError>(json);

        // Assert
        error.Should().NotBeNull();
        error!.Message.Should().Be("Error message");
        error.Code.Should().Be("ERROR_CODE");
    }

    [Fact]
    public void TranslaasError_ShouldSerializeToJson()
    {
        // Arrange
        var error = new TranslaasError
        {
            Message = "Error message",
            Code = "ERROR_CODE"
        };

        // Act
        var json = JsonSerializer.Serialize(error);

        // Assert
        json.Should().Contain("\"message\"");
        json.Should().Contain("\"Error message\"");
        json.Should().Contain("\"code\"");
        json.Should().Contain("\"ERROR_CODE\"");
    }

    [Fact]
    public void TranslaasError_ShouldHandleOptionalProperties()
    {
        // Arrange
        var json = """{"message":"Error message"}""";

        // Act
        var error = JsonSerializer.Deserialize<TranslaasError>(json);

        // Assert
        error.Should().NotBeNull();
        error!.Message.Should().Be("Error message");
        error.Code.Should().BeNull();
    }
}
