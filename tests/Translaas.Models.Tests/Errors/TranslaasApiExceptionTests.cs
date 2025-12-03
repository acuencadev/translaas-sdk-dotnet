using FluentAssertions;
using System.Net;
using System;
using Translaas.Models.Errors;
using Xunit;

namespace Translaas.Models.Tests.Errors;

public class TranslaasApiExceptionTests
{
    [Fact]
    public void TranslaasApiException_ShouldHaveStatusCode()
    {
        // Arrange & Act
        var exception = new TranslaasApiException(
            "API error",
            HttpStatusCode.BadRequest);

        // Assert
        exception.Message.Should().Be("API error");
        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void TranslaasApiException_ShouldHaveStatusCodeAndInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new TranslaasApiException(
            "API error",
            HttpStatusCode.InternalServerError,
            innerException);

        // Assert
        exception.Message.Should().Be("API error");
        exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void TranslaasApiException_ShouldHaveResponseContent()
    {
        // Arrange & Act
        var exception = new TranslaasApiException(
            "API error",
            HttpStatusCode.BadRequest,
            responseContent: "Error details");

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        exception.ResponseContent.Should().Be("Error details");
    }

    [Fact]
    public void TranslaasApiException_ShouldInheritFromTranslaasException()
    {
        // Arrange & Act
        var exception = new TranslaasApiException(
            "API error",
            HttpStatusCode.BadRequest);

        // Assert
        exception.Should().BeAssignableTo<TranslaasException>();
    }
}
