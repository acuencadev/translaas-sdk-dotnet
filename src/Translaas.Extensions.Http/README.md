# Translaas.Extensions.Http

**HttpClientFactory** integration for the Translaas SDK: register a typed `HttpClient` for `TranslaasClient` via `IHttpClientBuilder` extensions.

## Installation

```powershell
dotnet add package Translaas.Extensions.Http
```

References **Translaas.Client** and **Translaas.Models**.

## When to use this package

- ASP.NET Core or generic host apps that already use `AddHttpClient` and want a resilient, pooled `HttpClient` for Translaas.

For full **IServiceCollection** setup (options, caching, `ITranslaasService`), use **Translaas.Extensions.DependencyInjection**.

## Documentation

- [Translaas SDK repository](https://github.com/acuencadev/translaas-sdk-dotnet)
- [Root README (full guide)](https://github.com/acuencadev/translaas-sdk-dotnet#readme)

## Feedback

- [Report issues](https://github.com/acuencadev/translaas-sdk-dotnet/issues)
