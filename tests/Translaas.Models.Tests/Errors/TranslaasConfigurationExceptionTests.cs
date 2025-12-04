using FluentAssertions;

using Translaas.Models.Errors;

namespace Translaas.Models.Tests.Errors;

public class TranslaasConfigurationExceptionTests
{
    [Fact]
    public void TranslaasConfigurationException_ShouldHaveMessage()
    {
        // Arrange & Act
        var exception = new TranslaasConfigurationException("Configuration error");

        // Assert
        exception.Message.Should().Be("Configuration error");
    }

    [Fact]
    public void TranslaasConfigurationException_ShouldHaveInnerException()
    {
        // Arrange
        var innerException = new ArgumentException("Invalid argument");

        // Act
        var exception = new TranslaasConfigurationException("Configuration error", innerException);

        // Assert
        exception.Message.Should().Be("Configuration error");
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void TranslaasConfigurationException_ShouldInheritFromTranslaasException()
    {
        // Arrange & Act
        var exception = new TranslaasConfigurationException("Configuration error");

        // Assert
        exception.Should().BeAssignableTo<TranslaasException>();
    }
}
