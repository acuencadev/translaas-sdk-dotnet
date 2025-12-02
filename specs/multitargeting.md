# Multi-Targeting Setup Guide

## Overview

All projects in the Translaas.SDK solution are configured to target multiple .NET frameworks:
- `netstandard2.0` - Maximum compatibility (supports .NET Framework 4.6.1+, .NET Core 2.0+, etc.)
- `net6.0` - .NET 6 LTS
- `net8.0` - .NET 8 LTS
- `net10.0` - .NET 10 (latest)

## How It Works

### Project File Configuration

Each `.csproj` file uses `TargetFrameworks` (plural) instead of `TargetFramework`:

```xml
<PropertyGroup>
  <TargetFrameworks>netstandard2.0;net6.0;net8.0;net10.0</TargetFrameworks>
  <ImplicitUsings Condition="'$(TargetFramework)' != 'netstandard2.0'">enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <LangVersion>latest</LangVersion>
</PropertyGroup>
```

### Key Points

1. **ImplicitUsings**: Conditionally enabled only for frameworks that support it (not netstandard2.0)
2. **Build Output**: Each project builds separate DLLs for each target framework
3. **NuGet Package**: When packaged, the NuGet package will contain all framework-specific DLLs

## Creating Projects

### Option 1: Create with .NET 10 and Modify (What We Did)

1. Create project with single framework:
   ```bash
   dotnet new classlib -n ProjectName -f net10.0 -o src/ProjectName
   ```

2. Update `.csproj` to use multi-targeting:
   - Change `<TargetFramework>` to `<TargetFrameworks>`
   - Add all target frameworks: `netstandard2.0;net6.0;net8.0;net10.0`
   - Add conditional `ImplicitUsings`

3. Restore packages:
   ```bash
   dotnet restore
   ```

### Option 2: Create with Multi-Targeting from Start

You can also manually create the `.csproj` file with multi-targeting from the beginning.

## Building

### Build All Frameworks
```bash
dotnet build
```

### Build Specific Framework
```bash
dotnet build -f net6.0
```

### Build Release
```bash
dotnet build -c Release
```

## Testing Compatibility

When writing code, be aware of framework differences:

- **netstandard2.0**: 
  - No implicit usings
  - May need explicit `using` statements
  - Some newer APIs may not be available

- **net6.0+**: 
  - Implicit usings enabled
  - Access to newer APIs
  - Better performance optimizations

### Conditional Compilation

If you need framework-specific code:

```csharp
#if NETSTANDARD2_0
    // Code for netstandard2.0
#elif NET6_0_OR_GREATER
    // Code for .NET 6+
#endif
```

## NuGet Packaging

When you create NuGet packages, they will automatically include all framework-specific DLLs. NuGet will select the appropriate DLL based on the consuming project's target framework.

### Framework Compatibility Matrix

NuGet uses a compatibility matrix to select the best matching DLL. Here's how it works:

| Customer's Target Framework | NuGet Will Use | Reason |
|----------------------------|----------------|--------|
| .NET Framework 4.6.1+ | `netstandard2.0` | Compatible with netstandard2.0 |
| .NET Core 2.0 - 5.0 | `netstandard2.0` | Compatible with netstandard2.0 |
| **.NET 6** | `net6.0` | Exact match |
| **.NET 7** | `net6.0` | .NET 7 is compatible with .NET 6 DLLs |
| **.NET 8** | `net8.0` | Exact match |
| **.NET 9** | `net8.0` | .NET 9 is compatible with .NET 8 DLLs |
| **.NET 10+** | `net10.0` | Exact match or latest compatible |

### Important Notes

✅ **.NET 7 customers**: Will automatically use the `net6.0` DLL - this works perfectly!  
✅ **.NET 9 customers**: Will automatically use the `net8.0` DLL - this works perfectly!

**Why this works:**
- .NET maintains backward compatibility within major versions
- .NET 7 can run code compiled for .NET 6
- .NET 9 can run code compiled for .NET 8
- NuGet automatically selects the highest compatible framework version

### Should You Add net7.0 and net9.0?

**Generally, NO** - It's not necessary because:
- .NET 7 can use net6.0 DLLs (backward compatible)
- .NET 9 can use net8.0 DLLs (backward compatible)
- Adding more targets increases build time and package size
- LTS versions (6, 8) are more commonly used

**However, you CAN add them if:**
- You need to use .NET 7/9-specific APIs
- You want to optimize specifically for those versions
- You have customers explicitly requesting it

To add them, simply update `TargetFrameworks`:
```xml
<TargetFrameworks>netstandard2.0;net6.0;net7.0;net8.0;net9.0;net10.0</TargetFrameworks>
```

## Project Structure

```
src/
├── Translaas.Models/              # DTOs and request/response models
├── Translaas.Client/              # Core HTTP client
├── Translaas.Caching/             # Caching layer
├── Translaas.Extensions.Http/     # HttpClientFactory extensions
└── Translaas.Extensions.DependencyInjection/  # DI extensions
```

## Verification

All projects successfully build for all target frameworks. You can verify by running:

```bash
dotnet build
```

You should see output for each framework:
- `netstandard2.0` succeeded
- `net6.0` succeeded
- `net8.0` succeeded
- `net10.0` succeeded
