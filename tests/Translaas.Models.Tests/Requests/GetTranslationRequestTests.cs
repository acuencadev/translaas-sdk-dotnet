using System.Text.Json;
using FluentAssertions;
using Translaas.Models.Requests;

namespace Translaas.Models.Tests.Requests;

public class GetTranslationRequestTests
{
    [Fact]
    public void GetTranslationRequest_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var request = new GetTranslationRequest
        {
            Group = "ui",
            Entry = "button.save",
            Lang = "en",
            Number = 5
        };

        // Assert
        request.Group.Should().Be("ui");
        request.Entry.Should().Be("button.save");
        request.Lang.Should().Be("en");
        request.Number.Should().Be(5);
    }

    [Fact]
    public void GetTranslationRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new GetTranslationRequest
        {
            Group = "ui",
            Entry = "button.save",
            Lang = "en",
            Number = 5
        };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        json.Should().Contain("\"group\":\"ui\"");
        json.Should().Contain("\"entry\":\"button.save\"");
        json.Should().Contain("\"lang\":\"en\"");
        json.Should().Contain("\"n\":5");
    }

    [Fact]
    public void GetTranslationRequest_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """{"group":"ui","entry":"button.save","lang":"en","n":5}""";

        // Act
        var request = JsonSerializer.Deserialize<GetTranslationRequest>(json);

        // Assert
        request.Should().NotBeNull();
        request!.Group.Should().Be("ui");
        request.Entry.Should().Be("button.save");
        request.Lang.Should().Be("en");
        request.Number.Should().Be(5);
    }

    [Fact]
    public void GetTranslationRequest_ShouldHandleNullNumber()
    {
        // Arrange
        var request = new GetTranslationRequest
        {
            Group = "ui",
            Entry = "button.save",
            Lang = "en",
            Number = null
        };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        json.Should().Contain("\"n\":null");
    }

    [Fact]
    public void GetTranslationRequest_ShouldHandleNullableProperties()
    {
        // Arrange
        var request = new GetTranslationRequest
        {
            Group = null,
            Entry = null,
            Lang = null,
            Number = null
        };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        json.Should().NotBeNull();
    }
}
