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

### Testing

- Write unit tests for new features and bug fixes
- Ensure all tests pass before submitting a pull request
- Maintain or improve code coverage
- Test against all target frameworks when possible

### Dependencies

- Minimize external dependencies
- Use `System.Text.Json` for serialization (not Newtonsoft.Json)
- Prefer built-in .NET types over third-party libraries
- Document any new dependencies and their justification

## Pull Request Process

1. **Update documentation** if you're adding features or changing behavior
2. **Update tests** - add tests for new features, update tests for bug fixes
3. **Run the build** to ensure everything compiles:
   ```bash
   dotnet build
   ```
4. **Run tests** (when available):
   ```bash
   dotnet test
   ```
5. **Update the README** if you're adding new features or changing usage
6. **Write a clear PR description**:
   - What changes were made
   - Why the changes were made
   - How to test the changes
   - Any breaking changes

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

## Additional Resources

- [Multi-Targeting Guide](docs/MULTI-TARGETING.md) - Understanding framework compatibility
- [Technical Specification](.specs/translaas-sdk.spec.md) - Detailed SDK architecture

Thank you for contributing to Translaas SDK! 🎉
