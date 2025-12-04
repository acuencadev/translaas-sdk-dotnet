using FluentAssertions;

namespace Translaas.Client.Tests;

public class ITranslaasClientTests
{
    [Fact]
    public void ITranslaasClient_ShouldDefineGetEntryAsync()
    {
        // Arrange & Act
        var method = typeof(ITranslaasClient).GetMethod(nameof(ITranslaasClient.GetEntryAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<string>));
        method.GetParameters().Should().HaveCount(5); // group, entry, lang, number, cancellationToken
        method.GetParameters()[3].ParameterType.Should().Be(typeof(int?)); // number is nullable
    }

    [Fact]
    public void ITranslaasClient_ShouldDefineGetGroupAsync()
    {
        // Arrange & Act
        var method = typeof(ITranslaasClient).GetMethod(nameof(ITranslaasClient.GetGroupAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<>).MakeGenericType(typeof(Translaas.Models.Responses.TranslationGroup)));
        method.GetParameters().Should().HaveCount(5); // project, group, lang, format, cancellationToken
    }

    [Fact]
    public void ITranslaasClient_ShouldDefineGetProjectAsync()
    {
        // Arrange & Act
        var method = typeof(ITranslaasClient).GetMethod(nameof(ITranslaasClient.GetProjectAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<>).MakeGenericType(typeof(Translaas.Models.Responses.TranslationProject)));
        method.GetParameters().Should().HaveCount(4); // project, lang, format, cancellationToken
    }

    [Fact]
    public void ITranslaasClient_ShouldDefineGetProjectLocalesAsync()
    {
        // Arrange & Act
        var method = typeof(ITranslaasClient).GetMethod(nameof(ITranslaasClient.GetProjectLocalesAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<>).MakeGenericType(typeof(Translaas.Models.Responses.ProjectLocales)));
        method.GetParameters().Should().HaveCount(2); // project, cancellationToken
    }

    [Fact]
    public void ITranslaasClient_AllMethods_ShouldHaveCancellationToken()
    {
        // Arrange
        var methods = typeof(ITranslaasClient).GetMethods();

        // Act & Assert
        foreach (var method in methods)
        {
            var parameters = method.GetParameters();
            var lastParam = parameters[parameters.Length - 1];
            lastParam.ParameterType.Should().Be(typeof(CancellationToken));
            lastParam.Name.Should().Be("cancellationToken");
        }
    }
}
