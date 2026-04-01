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
        method.GetParameters().Should().HaveCount(7); // group, entry, lang, number, parameters, requestContext, cancellationToken
        method.GetParameters()[3].ParameterType.Should().Be(typeof(decimal?)); // number is nullable
    }

    [Fact]
    public void ITranslaasClient_ShouldDefineGetGroupAsync()
    {
        // Arrange & Act
        var method = typeof(ITranslaasClient).GetMethod(nameof(ITranslaasClient.GetGroupAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<>).MakeGenericType(typeof(Translaas.Models.Responses.TranslationGroup)));
        method.GetParameters().Should().HaveCount(6); // project, group, lang, format, requestContext, cancellationToken
    }

    [Fact]
    public void ITranslaasClient_ShouldDefineGetProjectAsync()
    {
        // Arrange & Act
        var method = typeof(ITranslaasClient).GetMethod(nameof(ITranslaasClient.GetProjectAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<>).MakeGenericType(typeof(Translaas.Models.Responses.TranslationProject)));
        method.GetParameters().Should().HaveCount(5); // project, lang, format, requestContext, cancellationToken
    }

    [Fact]
    public void ITranslaasClient_ShouldDefineGetProjectLocalesAsync()
    {
        // Arrange & Act
        var method = typeof(ITranslaasClient).GetMethod(nameof(ITranslaasClient.GetProjectLocalesAsync));

        // Assert
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<>).MakeGenericType(typeof(Translaas.Models.Responses.ProjectLocales)));
        method.GetParameters().Should().HaveCount(3); // project, requestContext, cancellationToken
    }

    [Fact]
    public void ITranslaasClient_ShouldDefineGetOfflineCacheAsync()
    {
        var method = typeof(ITranslaasClient).GetMethod(nameof(ITranslaasClient.GetOfflineCacheAsync));
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<>).MakeGenericType(typeof(Translaas.Models.Responses.OfflineCacheDownloadResult)));
        method.GetParameters().Should().HaveCount(3);
    }

    [Fact]
    public void ITranslaasClient_ShouldDefineReportMissingKeysAsync()
    {
        var method = typeof(ITranslaasClient).GetMethod(nameof(ITranslaasClient.ReportMissingKeysAsync));
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task));
        method.GetParameters().Should().HaveCount(2);
    }

    [Fact]
    public void ITranslaasClient_ShouldDefineValidateApiKeyAsync()
    {
        var method = typeof(ITranslaasClient).GetMethod(nameof(ITranslaasClient.ValidateApiKeyAsync));
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task<>).MakeGenericType(typeof(Translaas.Models.Responses.ValidateApiKeyResponse)));
        method.GetParameters().Should().HaveCount(1);
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
            var lastParam = parameters[^1];
            lastParam.ParameterType.Should().Be(typeof(CancellationToken));
            lastParam.Name.Should().Be("cancellationToken");
        }
    }
}
