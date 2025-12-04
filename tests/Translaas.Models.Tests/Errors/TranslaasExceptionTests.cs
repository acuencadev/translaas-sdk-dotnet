using FluentAssertions;

using Translaas.Models.Errors;

namespace Translaas.Models.Tests.Errors;

public class TranslaasExceptionTests
{
    [Fact]
    public void TranslaasException_ShouldHaveMessage()
    {
        // Arrange & Act
        var exception = new TranslaasException("Test error message");

        // Assert
        exception.Message.Should().Be("Test error message");
    }

    [Fact]
    public void TranslaasException_ShouldHaveInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new TranslaasException("Outer error", innerException);

        // Assert
        exception.Message.Should().Be("Outer error");
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void TranslaasException_ShouldBeSerializable()
    {
        // Arrange
        var exception = new TranslaasException("Test error");

        // Act & Assert
        exception.Should().BeAssignableTo<Exception>();
    }
}

