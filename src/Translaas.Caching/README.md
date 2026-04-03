# Translaas.Caching

Caching **abstractions and implementations** for the Translaas SDK: `CacheMode`, `ITranslaasCacheProvider`, cache key building, and an in-memory provider built on **Microsoft.Extensions.Caching.Memory**.

## Installation

```powershell
dotnet add package Translaas.Caching
```

References **Translaas.Models**.

## When to use this package

- You use **Translaas.Client** and want to configure or extend how translations are cached.
- You implement a custom `ITranslaasCacheProvider`.

For file-based or offline scenarios, see **Translaas.Caching.File**.

## Documentation

- [Translaas SDK repository](https://github.com/acuencadev/translaas-sdk-dotnet)
- [Root README (full guide)](https://github.com/acuencadev/translaas-sdk-dotnet#readme)

## Feedback

- [Report issues](https://github.com/acuencadev/translaas-sdk-dotnet/issues)
