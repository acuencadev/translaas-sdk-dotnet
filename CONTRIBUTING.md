# Contributing to Translaas SDK

We welcome contributions to the Translaas SDK! This document provides guidelines and instructions for contributing.

## Getting Started

1. **Fork the repository** and clone your fork locally
2. **Create a branch** for your feature or bug fix:
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/your-bug-fix
   ```

3. **Set up your development environment**:
   ```bash
   dotnet restore
   dotnet build
   ```

## Development Guidelines

### Code Style

- Follow C# coding conventions and style guidelines
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and single-purpose
- Use `async`/`await` for asynchronous operations

### Optional: Auto-format on commit (pre-commit hook)

This repo includes an **opt-in** pre-commit hook that will:

- Run `dotnet format` on **staged `*.cs` files**
- Remove unused `using` directives
- Normalize whitespace (including trimming trailing whitespace and ensuring a final newline)
- Remove extra blank lines at end-of-file (ensures exactly one final newline)
- Re-stage formatted files so your commit stays consistent

Note: Running `dotnet format` against the entire repo/solution can occasionally fail due to upstream workspace/linking issues. The pre-commit hook runs `dotnet format` **per-project** (only for projects that contain staged files) to avoid those solution-wide problems.

Enable it once per clone:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/setup-githooks.ps1
```

Or (bash):

```bash
./scripts/setup-githooks.sh
```

### Project Structure

- Keep code organized within the appropriate project:
  - `Translaas.Models` - Data transfer objects only
  - `Translaas.Client` - Core HTTP client implementation
  - `Translaas.Caching` - In-memory caching layer
  - `Translaas.Caching.File` - File-based offline caching with hybrid caching support
  - `Translaas.Extensions.*` - Extension methods and DI integration

#### Test Project Structure

All test projects must be placed in the `tests/` directory:

```
tests/
├── Translaas.Models.Tests/
├── Translaas.Client.Tests/
├── Translaas.Caching.Tests/
├── Translaas.Caching.File.Tests/
├── Translaas.Extensions.Http.Tests/
└── Translaas.Extensions.DependencyInjection.Tests/
```

Each test project should:
- Be named `{ProjectName}.Tests`
- Reference the corresponding source project
- Use xUnit as the testing framework
- Include Moq or NSubstitute for mocking
- Use FluentAssertions for readable assertions

### Multi-Targeting

All projects in the Translaas.SDK solution are configured to target multiple .NET frameworks:
- `netstandard2.0` - Maximum compatibility (supports .NET Framework 4.6.1+, .NET Core 2.0+, etc.)
- `net8.0` - .NET 8 LTS
- `net10.0` - .NET 10 (latest)

**Note**: .NET 6.0 is temporarily removed due to VSTest test discovery compatibility issues.

#### Project File Configuration

Each `.csproj` file uses `TargetFrameworks` (plural) instead of `TargetFramework`:

```xml
<PropertyGroup>
  <TargetFrameworks>netstandard2.0;net8.0;net10.0</TargetFrameworks>
  <ImplicitUsings Condition="'$(TargetFramework)' != 'netstandard2.0'">enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <LangVersion>latest</LangVersion>
</PropertyGroup>
```

#### Key Points

1. **ImplicitUsings**: Conditionally enabled only for frameworks that support it (not netstandard2.0)
2. **Build Output**: Each project builds separate DLLs for each target framework
3. **NuGet Package**: When packaged, the NuGet package will contain all framework-specific DLLs

#### Framework Compatibility

When writing code, be aware of framework differences:

- **netstandard2.0**: 
  - No implicit usings
  - May need explicit `using` statements
  - Some newer APIs may not be available

- **net8.0+**: 
  - Implicit usings enabled
  - Access to newer APIs
  - Better performance optimizations

#### Conditional Compilation

If you need framework-specific code:

```csharp
#if NETSTANDARD2_0
    // Code for netstandard2.0
#elif NET8_0_OR_GREATER
    // Code for .NET 8+
#endif
```

#### Framework Compatibility Matrix

NuGet uses a compatibility matrix to select the best matching DLL:

| Customer's Target Framework | NuGet Will Use | Reason |
|----------------------------|----------------|--------|
| .NET Framework 4.6.1+ | `netstandard2.0` | Compatible with netstandard2.0 |
| .NET Core 2.0 - 5.0 | `netstandard2.0` | Compatible with netstandard2.0 |
| .NET 6 | `netstandard2.0` | Compatible with netstandard2.0 |
| .NET 7 | `netstandard2.0` | Compatible with netstandard2.0 |
| .NET 8 | `net8.0` | Exact match |
| .NET 9 | `net8.0` | .NET 9 is compatible with .NET 8 DLLs |
| .NET 10+ | `net10.0` | Exact match or latest compatible |

**Important Notes:**
- ✅ **.NET 6/7 customers**: Will automatically use the `netstandard2.0` DLL - this works perfectly!
- ✅ **.NET 9 customers**: Will automatically use the `net8.0` DLL - this works perfectly!
- .NET maintains backward compatibility within major versions
- NuGet automatically selects the highest compatible framework version

#### Building for Specific Frameworks

```bash
# Build all frameworks
dotnet build

# Build specific framework
dotnet build -f net8.0

# Build Release
dotnet build -c Release
```

### Test-Driven Development (TDD)

We follow **Test-Driven Development (TDD)** practices. This means:

1. **Write tests first** - Before implementing any feature, write a failing test
2. **Make it pass** - Write the minimum code to make the test pass
3. **Refactor** - Improve the code while keeping tests green

#### TDD Workflow

```
Red → Green → Refactor
```

- **Red**: Write a failing test that describes the desired behavior
- **Green**: Write the minimum code to make the test pass
- **Refactor**: Improve code quality while keeping tests passing

### Testing

- **Follow TDD** - Write tests before implementation
- **Every project must have tests** - Test projects are located in `tests/` directory
- **Test project naming**: `{ProjectName}.Tests` (e.g., `Translaas.Client.Tests`)
- Write unit tests for all public APIs
- Test both success and failure scenarios
- Ensure all tests pass before submitting a pull request
- Maintain or improve code coverage (aim for 80%+)
- Test against all target frameworks when possible
- Use proper test naming: `{MethodName}_{Scenario}_{ExpectedBehavior}`

### Dependencies

- Minimize external dependencies
- Use `System.Text.Json` for serialization (not Newtonsoft.Json)
- Prefer built-in .NET types over third-party libraries
- Document any new dependencies and their justification

## Pull Request Process

1. **Follow TDD workflow**:
   - Write failing tests first (Red)
   - Implement code to make tests pass (Green)
   - Refactor while keeping tests green
2. **Create test project** if adding a new source project:
   - Create test project in `tests/` directory
   - Name it `{ProjectName}.Tests`
   - Add appropriate test dependencies (xUnit, Moq, FluentAssertions)
3. **Update documentation** if you're adding features or changing behavior
4. **Ensure test coverage**:
   - All public APIs have tests
   - Both success and failure scenarios are tested
   - Tests follow naming convention: `{MethodName}_{Scenario}_{ExpectedBehavior}`
5. **Run the build** to ensure everything compiles:
   ```bash
   dotnet build
   ```
6. **Run tests** and ensure all pass:
   ```bash
   dotnet test
   ```
7. **Run tests for all frameworks**:
   ```bash
   dotnet test -f net8.0
   dotnet test -f net10.0
   ```
8. **Update the README** if you're adding new features or changing usage
9. **Write a clear PR description**:
   - What changes were made
   - Why the changes were made
   - How to test the changes
   - Any breaking changes
   - Test coverage information

## Commit Messages

Use clear, descriptive commit messages following conventional commits:

```
feat: Add support for custom cache providers
fix: Resolve timeout issue in retry policy
docs: Update README with caching examples
refactor: Simplify HTTP client configuration
test: Add unit tests for retry policy
chore: Update dependencies
```

### Commit Types

- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks, dependency updates, etc.

## Reporting Issues

When reporting bugs or requesting features:

- Use the GitHub issue tracker
- Provide a clear description of the issue
- Include steps to reproduce (for bugs)
- Specify the .NET version and target framework you're using
- Include relevant code samples or error messages
- Use appropriate labels if you have permission

### Issue Templates

When creating an issue, please use the appropriate template:
- **Bug Report**: For reporting bugs
- **Feature Request**: For requesting new features
- **Question**: For asking questions about usage or implementation

## Code Review Process

1. All pull requests require at least one approval
2. Ensure CI/CD checks pass
3. Address review feedback promptly
4. Keep pull requests focused and reasonably sized
5. Rebase on main branch if requested

## Code of Conduct

- Be respectful and inclusive
- Welcome newcomers and help them get started
- Focus on constructive feedback
- Respect different viewpoints and experiences
- Be patient with questions and learning curves

## Questions?

If you have questions about contributing, please:
- Open an issue with the `question` label
- Check existing issues and discussions
- Review the codebase to understand patterns and conventions

## Testing Resources

### Creating a Test Project

To create a new test project:

```bash
# Navigate to tests directory
cd tests

# Create test project
dotnet new xunit -n Translaas.YourProject.Tests

# Add project reference
cd Translaas.YourProject.Tests
dotnet add reference ../../src/Translaas.YourProject/Translaas.YourProject.csproj

# Add test dependencies
dotnet add package Moq
dotnet add package FluentAssertions
```

### Example Test Structure

```csharp
using FluentAssertions;
using Moq;
using Xunit;

namespace Translaas.Client.Tests;

public class TranslaasClientTests
{
    [Fact]
    public async Task GetEntryAsync_ReturnsTranslation_WhenEntryExists()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var result = await client.GetEntryAsync("ui", "button.save", "en");
        
        // Assert
        result.Should().Be("Save");
    }
    
    [Fact]
    public async Task GetEntryAsync_ThrowsException_WhenApiReturnsError()
    {
        // Arrange
        var client = CreateClientWithFailingHttpClient();
        
        // Act & Assert
        await Assert.ThrowsAsync<TranslaasApiException>(
            () => client.GetEntryAsync("ui", "button.save", "en"));
    }
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests for specific project
dotnet test tests/Translaas.Client.Tests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests for specific framework
dotnet test -f net8.0
```

## Additional Resources

- [SDK Guidelines](.cursor/rules/sdk-guidelines.mdc) - Comprehensive development guidelines including TDD practices

## Release Notes and Version Management

### Version Management

#### How to Update the Version

The SDK uses a centralized version management approach. To update the version for all packages:

1. **Update the version in `Directory.Build.props`**:
   ```xml
   <PropertyGroup Condition="'$(IsPackable)' != 'false'">
     <Version>X.Y.Z</Version>
     <!-- other metadata -->
   </PropertyGroup>
   ```

2. **Update the release notes section in this file (`CONTRIBUTING.md`)**:
   - Add a new version section at the top of the "Release Notes" section
   - Update package version numbers in the "Packages Included" section
   - Document new features, changes, fixes, etc.

3. **Rebuild and repack all packages**:
   ```bash
   # Clean previous packages (optional)
   Remove-Item -Path "nupkgs\*" -Force
   
   # Build all projects
   dotnet build --configuration Release
   
   # Pack all packages
   Get-ChildItem -Path src -Filter *.csproj -Recurse | ForEach-Object { 
     dotnet pack -Path $_.FullName --configuration Release --output ./nupkgs
   }
   ```

#### Version Numbering

We follow [Semantic Versioning](https://semver.org/) (SemVer):
- **MAJOR** (X.0.0): Breaking changes
- **MINOR** (0.Y.0): New features, backward compatible
- **PATCH** (0.0.Z): Bug fixes, backward compatible

#### Pre-Release Versions

For pre-release versions, use version numbers like:
- `0.1.0` - Initial pre-release
- `0.2.0` - Pre-release with new features
- `0.1.1` - Pre-release bug fix

Once stable, release `1.0.0` as the first stable version.

---

### Release Notes

## Version 0.1.0 (Pre-Release)

### Initial Pre-Release

This is the initial pre-release of the Translaas SDK for .NET. This version is still under active development and may have breaking changes before the 1.0.0 release.

### Packages Included

- **Translaas.Models** (0.1.0) - Data transfer objects (DTOs) for the Translaas Translation Delivery API
- **Translaas.Client** (0.1.0) - Core HTTP client implementation with caching support
- **Translaas.Caching** (0.1.0) - In-memory caching abstractions and implementations
- **Translaas.Caching.File** (0.1.0) - File-based offline caching with hybrid caching support
- **Translaas.Extensions.Http** (0.1.0) - HttpClientFactory integration extensions
- **Translaas.Extensions.DependencyInjection** (0.1.0) - Full dependency injection integration
- **Translaas.Extensions.Mvc** (0.1.0) - ASP.NET Core MVC/Razor integration with Tag Helpers

### Features

- ✅ Strongly-typed API with full IntelliSense support
- ✅ Convenience API via `ITranslaasService` with `T()` method
- ✅ Razor View Support with Tag Helpers and static helpers
- ✅ Dependency Injection ready with seamless `IServiceCollection` integration
- ✅ Flexible caching with configurable cache modes (None, Entry, Group, Project)
- ✅ Offline caching with file-based storage for offline mode
- ✅ Hybrid caching (memory L1 + file L2) for optimal performance
- ✅ Multiple framework support (.NET Standard 2.0, .NET 8, .NET 10)
- ✅ Fully asynchronous API for optimal performance
- ✅ Modular design - use only what you need

### Supported Frameworks

- .NET Standard 2.0
- .NET 8.0
- .NET 10.0

### Installation

```bash
# Full DI integration (recommended)
dotnet add package Translaas.Extensions.DependencyInjection

# Or install individual packages
dotnet add package Translaas.Client
dotnet add package Translaas.Models
dotnet add package Translaas.Caching
dotnet add package Translaas.Caching.File
dotnet add package Translaas.Extensions.Http
dotnet add package Translaas.Extensions.Mvc
```

### Documentation

- [README.md](README.md) - Getting started guide
- [CONTRIBUTING.md](CONTRIBUTING.md) - Contribution guidelines
- [GitHub Repository](https://github.com/acuencadev/Translaas.SDK)

### Breaking Changes

None - This is the initial pre-release.

### Known Issues

None at this time.

---

### Template for Future Releases

When adding a new release, add it at the top of the "Release Notes" section above, following this template:

```markdown
## Version X.Y.Z

### Added
- New features added in this release

### Changed
- Changes to existing functionality

### Fixed
- Bug fixes

### Deprecated
- Features that will be removed in a future release

### Removed
- Features removed in this release

### Security
- Security fixes
```

Thank you for contributing to Translaas SDK! 🎉
