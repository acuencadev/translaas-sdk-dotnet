# Translaas.Extensions.DependencyInjection

Full **dependency injection** integration for the Translaas SDK: `AddTranslaas`, options binding, `ITranslaasClient` / `ITranslaasService`, caching, HttpClientFactory, and file-based caching support.

## Installation

```powershell
dotnet add package Translaas.Extensions.DependencyInjection
```

This is the **recommended** package for ASP.NET Core and other apps using `Microsoft.Extensions.DependencyInjection`.

## When to use this package

- You want one call to register Translaas with configuration from `IConfiguration` or delegates.
- You need the convenience API (`ITranslaasService` / `T()`-style helpers) and integrated caching.

## Documentation

- [Translaas SDK repository](https://github.com/acuencadev/translaas-sdk-dotnet)
- [Root README (full guide)](https://github.com/acuencadev/translaas-sdk-dotnet#readme)

## Feedback

- [Report issues](https://github.com/acuencadev/translaas-sdk-dotnet/issues)
