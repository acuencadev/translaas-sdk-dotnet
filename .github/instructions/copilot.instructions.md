# Translaas SDK - Copilot Instructions

## Repository Overview

This is an open-source .NET SDK for the Translaas Translation Delivery API SaaS. The SDK provides a strongly-typed, performant, and modular way to consume translation APIs in .NET applications. The backend API is proprietary, but this SDK is open-sourced to allow community contributions.

**Repository Type**: Multi-targeted .NET SDK library  
**Languages**: C#  
**Frameworks**: .NET Standard 2.0, .NET 8.0, .NET 10.0  
**Build System**: .NET SDK (MSBuild)  
**Test Framework**: xUnit  
**Package Management**: Central Package Management (Directory.Packages.props)

## High-Level Architecture

The SDK is organized into modular projects:

- **Translaas.Models** (`src/Translaas.Models/`) - Data transfer objects (DTOs) only, minimal dependencies
- **Translaas.Client** (`src/Translaas.Client/`) - Core HTTP client implementation
- **Translaas.Caching** (`src/Translaas.Caching/`) - Caching abstractions and implementations
- **Translaas.Extensions.Http** (`src/Translaas.Extensions.Http/`) - HttpClientFactory integration
- **Translaas.Extensions.DependencyInjection** (`src/Translaas.Extensions.DependencyInjection/`) - Full DI integration

Each source project has a corresponding test project in `tests/` directory following the naming pattern `{ProjectName}.Tests`.

## Build and Validation Instructions

### Prerequisites

- .NET SDK 8.0.x or later (required for building)
- .NET SDK 10.0.x (optional, for net10.0 target framework testing)

### Bootstrap/Restore

**Always run restore before building:**

```bash
dotnet restore
```

This restores NuGet packages using central package management from `Directory.Packages.props`.

### Build

**Build all projects:**

```bash
dotnet build
```

**Build specific framework:**

```bash
dotnet build -f net8.0
dotnet build -f net10.0
```

**Build in Release configuration:**

```bash
dotnet build --configuration Release
```

**Build without restore (faster after initial restore):**

```bash
dotnet build --no-restore
```

**Important**: The solution file is `Translaas.SDK.slnx` (Visual Studio 2022 format). Use `dotnet build` from the root directory - it will automatically discover all projects.

### Test

**Run all tests:**

```bash
dotnet test
```

**Run tests for specific project:**

```bash
dotnet test tests/Translaas.Client.Tests
```

**Run tests for specific framework:**

```bash
dotnet test -f net8.0
dotnet test -f net10.0
```

**Run tests with code coverage:**

```bash
dotnet test --collect:"XPlat Code Coverage"
```

**Run tests without building (faster after build):**

```bash
dotnet test --no-build
```

**Run tests with verbosity:**

```bash
dotnet test --verbosity normal
```

**Important**: Tests must pass for all target frameworks. Always run `dotnet test` before committing changes.

### Validation Steps

**Before committing, always:**

1. Restore dependencies: `dotnet restore`
2. Build solution: `dotnet build`
3. Run all tests: `dotnet test`
4. Verify tests pass for all frameworks: `dotnet test -f net8.0` and `dotnet test -f net10.0` (if SDK available)

**CI Pipeline Validation:**

The CI pipeline (`.github/workflows/ci.yml`) runs:
- Restore with specific framework: `dotnet restore -p:TargetFrameworks=net8.0`
- Build in Release: `dotnet build --no-restore --configuration Release -f net8.0`
- Test with coverage: `dotnet test --no-build --configuration Release --verbosity normal --collect:"XPlat Code Coverage"`

**Note**: CI currently tests on .NET 8.0 only (net10.0 SDK not yet available on GitHub Actions, net6.0 temporarily removed due to VSTest compatibility issues).

## Project Layout and File Management

### Critical File Synchronization Rules

**CRITICAL**: When files are added or removed, project files (`.csproj`) must be updated accordingly. When projects are added or removed, the solution file (`.slnx`) must be updated accordingly.

**When creating a new file:**
- `.cs` files in SDK-style projects are usually auto-included
- Other file types may need explicit inclusion in `.csproj`

**When creating a new project:**
- Add to solution: `dotnet sln add src/Translaas.NewProject/Translaas.NewProject.csproj`
- Create corresponding test project in `tests/` directory
- Add test project to solution: `dotnet sln add tests/Translaas.NewProject.Tests/Translaas.NewProject.Tests.csproj`

**When removing a project:**
- Remove from solution: `dotnet sln remove src/Translaas.OldProject/Translaas.OldProject.csproj`
- Remove test project: `dotnet sln remove tests/Translaas.OldProject.Tests/Translaas.OldProject.Tests.csproj`

### Directory Structure

```
Translaas.SDK/
├── .github/
│   ├── workflows/
│   │   └── ci.yml              # CI/CD pipeline
│   └── copilot-instructions.md # This file
├── src/                        # Source projects
│   ├── Translaas.Models/
│   ├── Translaas.Client/
│   ├── Translaas.Caching/
│   ├── Translaas.Extensions.Http/
│   └── Translaas.Extensions.DependencyInjection/
├── tests/                      # Test projects
│   ├── Translaas.Models.Tests/
│   ├── Translaas.Client.Tests/
│   ├── Translaas.Client.IntegrationTests/
│   ├── Translaas.Extensions.Http.Tests/
│   └── [other test projects]
├── Directory.Build.props       # Common build properties
├── Directory.Packages.props    # Central package management
├── Translaas.SDK.slnx         # Solution file
├── README.md                   # Project documentation
├── CONTRIBUTING.md             # Contribution guidelines
├── IMPLEMENTATION.md           # Implementation tracking
└── LICENSE                     # MIT License
```

### Key Configuration Files

- **Directory.Build.props**: Common build properties, target frameworks (netstandard2.0;net8.0;net10.0), nullable reference types, code analysis
- **Directory.Packages.props**: Central package version management for all NuGet packages
- **.github/workflows/ci.yml**: CI/CD pipeline configuration
- **.cursor/rules/translaas-sdk-rules.mdc**: Comprehensive development rules and guidelines

## Development Guidelines

### Test-Driven Development (TDD) - MANDATORY

**TDD is mandatory for all code changes.** Follow the Red-Green-Refactor cycle:

1. **Red**: Write a failing test that describes the desired behavior
2. **Green**: Write the minimum code necessary to make the test pass
3. **Refactor**: Improve the code while keeping tests green

**Rules:**
- Write tests BEFORE implementation
- Every project MUST have a corresponding test project in `tests/`
- Test project naming: `{ProjectName}.Tests`
- Test naming convention: `{MethodName}_{Scenario}_{ExpectedBehavior}`
- All public APIs must have tests
- Test both success and failure scenarios

### Code Style

- Use `async`/`await` for all I/O operations
- Use `string?` for nullable strings, `int?` for nullable integers
- Use `readonly` fields where possible
- Prefer expression-bodied members for simple properties/methods
- Use `nameof()` for parameter names in exceptions
- All public APIs require XML documentation comments

### Multi-Targeting

- Code must compile for all target frameworks: netstandard2.0, net8.0, net10.0
- Use conditional compilation sparingly - prefer runtime checks when possible
- netstandard2.0 requires explicit `using` statements (no implicit usings)
- Test code against all frameworks when possible

### Dependencies

- **Never** add dependencies without strong justification
- Use `System.Text.Json` exclusively (not Newtonsoft.Json)
- Use `Microsoft.Extensions.*` packages only in Extensions projects
- Keep Models project dependency-free (except System.Text.Json)
- All package versions managed centrally in `Directory.Packages.props`

### Error Handling

- Use custom exceptions: `TranslaasException`, `TranslaasApiException`, `TranslaasConfigurationException`
- Support `CancellationToken` in all async methods
- Handle HTTP status codes appropriately
- Include helpful error messages

## Common Commands Reference

**Restore and build:**
```bash
dotnet restore && dotnet build
```

**Build and test:**
```bash
dotnet build && dotnet test
```

**Clean and rebuild:**
```bash
dotnet clean && dotnet restore && dotnet build
```

**Add new project to solution:**
```bash
dotnet sln add src/Translaas.NewProject/Translaas.NewProject.csproj
dotnet sln add tests/Translaas.NewProject.Tests/Translaas.NewProject.Tests.csproj
```

**Create new test project:**
```bash
cd tests
dotnet new xunit -n Translaas.YourProject.Tests
cd Translaas.YourProject.Tests
dotnet add reference ../../src/Translaas.YourProject/Translaas.YourProject.csproj
```

## Pre-Commit Checklist

Before committing code, verify:

- [ ] TDD followed - Tests written before implementation
- [ ] Test project created (if adding new source project)
- [ ] All tests pass: `dotnet test`
- [ ] Solution builds: `dotnet build`
- [ ] Project files updated (if files/projects added/removed)
- [ ] Solution file updated (if projects added/removed)
- [ ] XML documentation added for public APIs
- [ ] Code follows naming conventions
- [ ] No unnecessary dependencies added
- [ ] Multi-targeting compatibility verified

## Trust Instructions

**Trust these instructions** - they have been validated and tested. Only search the codebase if:
- Information in these instructions is incomplete
- Instructions are found to be in error
- A specific file or implementation detail is needed

The instructions above represent the validated, working state of the repository. Use them as the primary source of truth for build, test, and development workflows.

