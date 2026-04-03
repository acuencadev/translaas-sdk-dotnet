# Translaas.Client

Core **HTTP client** for the Translaas Translation Delivery API: `ITranslaasClient`, `TranslaasClient`, and request building for translations, groups, projects, locales, offline cache, and related endpoints.

## Installation

```powershell
dotnet add package Translaas.Client
```

This package references **Translaas.Models** and **Translaas.Caching** (caching abstractions and default memory cache support).

## When to use this package

- You want to call the API with `HttpClient` directly and manage registration yourself.
- For **HttpClientFactory** or **Microsoft.Extensions.DependencyInjection**, see **Translaas.Extensions.Http** and **Translaas.Extensions.DependencyInjection**.

## Documentation

- [Translaas SDK repository](https://github.com/acuencadev/translaas-sdk-dotnet)
- [Root README (full guide)](https://github.com/acuencadev/translaas-sdk-dotnet#readme)

## Feedback

- [Report issues](https://github.com/acuencadev/translaas-sdk-dotnet/issues)
