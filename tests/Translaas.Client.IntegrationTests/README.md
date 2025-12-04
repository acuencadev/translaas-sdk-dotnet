# Translaas Client Integration Tests

This project contains integration tests for the Translaas Client SDK. These tests are designed to run against a real development API instance.

## Prerequisites

- A running Translaas API instance (development environment)
- Valid API key for the development environment

## Configuration

Integration tests are configured via environment variables:

- **TRANSLAAS_API_KEY** (required): Your API key for the development environment
- **TRANSLAAS_BASE_URL** (optional): Base URL for the API. Defaults to `https://sdkapi.translaas.local/api`

## Running Integration Tests

### Windows (PowerShell)

```powershell
$env:TRANSLAAS_API_KEY = "your-api-key-here"
$env:TRANSLAAS_BASE_URL = "https://api-dev.translaas.com/api"  # Optional
dotnet test tests/Translaas.Client.IntegrationTests
```

### Linux/macOS (Bash)

```bash
export TRANSLAAS_API_KEY="your-api-key-here"
export TRANSLAAS_BASE_URL="https://api-dev.translaas.com/api"  # Optional
dotnet test tests/Translaas.Client.IntegrationTests
```

### Running Specific Tests

```bash
# Run only GetEntryAsync tests
dotnet test tests/Translaas.Client.IntegrationTests --filter "FullyQualifiedName~GetEntryAsync"

# Run only error scenario tests
dotnet test tests/Translaas.Client.IntegrationTests --filter "FullyQualifiedName~ErrorScenarios"
```

## Test Behavior

- **If TRANSLAAS_API_KEY is not set**: Tests will be skipped automatically (no failures)
- **If TRANSLAAS_API_KEY is set**: Tests will run against the configured API

## Test Data Requirements

The integration tests expect certain test data to exist in your development API:

- **Project**: `test-project`
- **Group**: `ui`
- **Entries**: `button.save`, `button.cancel`, `items.count`
- **Locales**: At least `en` (and optionally `fr`, `es`, `de`)

Adjust the test values in the test files to match your actual development API test data.

## CI/CD Integration

These tests are **optional** and should **not** run automatically in CI/CD pipelines unless:

1. A development API instance is available
2. A valid API key is configured as a CI/CD secret
3. The tests are explicitly enabled via environment variables

To exclude integration tests from CI/CD, ensure `TRANSLAAS_API_KEY` is not set in your CI/CD environment.
