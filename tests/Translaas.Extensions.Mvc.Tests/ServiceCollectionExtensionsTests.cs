using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace Translaas.Extensions.Mvc.Tests;

/// <summary>
/// Tests for the ServiceCollectionExtensions class.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTranslaasMvc_ThrowsArgumentNullException_WhenServicesIsNull()
    {
        // Arrange & Act
        var act = () => ServiceCollectionExtensions.AddTranslaasMvc(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddTranslaasMvc_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddTranslaasMvc();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddTranslaasMvc_DoesNotThrow_WhenCalled()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddTranslaasMvc();

        // Assert
        act.Should().NotThrow();
    }
}
