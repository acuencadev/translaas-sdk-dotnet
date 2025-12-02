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

### Project Structure

- Keep code organized within the appropriate project:
  - `Translaas.Models` - Data transfer objects only
  - `Translaas.Client` - Core HTTP client implementation
  - `Translaas.Caching` - Caching layer
  - `Translaas.Extensions.*` - Extension methods and DI integration

#### Test Project Structure

All test projects must be placed in the `tests/` directory:

```
tests/
├── Translaas.Models.Tests/
├── Translaas.Client.Tests/
├── Translaas.Caching.Tests/
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

- Ensure all code compiles for all target frameworks:
  - `netstandard2.0`
  - `net6.0`
  - `net8.0`
  - `net10.0`
- Use conditional compilation if framework-specific code is needed:
  ```csharp
  #if NETSTANDARD2_0
      // netstandard2.0 specific code
  #elif NET6_0_OR_GREATER
      // .NET 6+ specific code
  #endif
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
   dotnet test -f net6.0
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
dotnet test -f net6.0
```

## Additional Resources

- [Multi-Targeting Guide](docs/MULTI-TARGETING.md) - Understanding framework compatibility
- [Technical Specification](.specs/translaas-sdk.spec.md) - Detailed SDK architecture
- [SDK Guidelines](.cursor/rules/sdk-guidelines.mdc) - Comprehensive development guidelines including TDD practices

Thank you for contributing to Translaas SDK! 🎉
