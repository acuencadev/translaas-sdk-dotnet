# Translaas Client Integration Tests

This project contains integration tests for the Translaas Client SDK. These tests are designed to run against a real development API instance.

## Prerequisites

- A running Translaas API instance (development environment)
- Valid API key for the development environment

## Configuration

Integration tests are configured via environment variables:

- **TRANSLAAS_API_KEY** (required): Your API key for the development environment
- **TRANSLAAS_BASE_URL** (optional): Base URL for the API. Defaults to `https://sdk-api.translaas.local`
  - **Note**: Do NOT include `/api` in the BaseUrl - the client adds `/api/` to all endpoints automatically

## Running Integration Tests

### Windows (PowerShell)

```powershell
$env:TRANSLAAS_API_KEY = "your-api-key-here"
$env:TRANSLAAS_BASE_URL = "https://api-dev.translaas.com"  # Optional - do NOT include /api
dotnet test tests/Translaas.Client.IntegrationTests
```

### Linux/macOS (Bash)

```bash
export TRANSLAAS_API_KEY="your-api-key-here"
export TRANSLAAS_BASE_URL="https://api-dev.translaas.com"  # Optional - do NOT include /api
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

- **Project**: `test-project` (must exist and contain translation data)
- **Group**: `ui` (must exist within the project)
- **Entries**: `button.save`, `button.cancel`, `items.count` (must exist within the group)
- **Locales**: At least `en` (and optionally `fr`, `es`, `de`)

**Important**: If your API doesn't have this test data, the tests will fail. You have two options:

1. **Create the test data** in your API to match the test expectations
2. **Update the test files** to use data that exists in your API (modify the hardcoded values in the test files)

### API Behavior Notes

- The API returns **204 No Content** for non-existent resources (not 404 errors)
- The client handles 204 responses by returning:
  - **GetEntryAsync**: Returns the entry key as fallback (common i18n pattern)
  - **GetGroupAsync**: Returns empty `TranslationGroup`
  - **GetProjectAsync**: Returns empty `TranslationProject`
  - **GetProjectLocalesAsync**: Returns empty `ProjectLocales`
- Tests that expect data will fail if the test data doesn't exist in your API
- Tests for "not found" scenarios expect empty data, not exceptions

## CI/CD Integration

These tests are **optional** and should **not** run automatically in CI/CD pipelines unless:

1. A development API instance is available
2. A valid API key is configured as a CI/CD secret
3. The tests are explicitly enabled via environment variables

To exclude integration tests from CI/CD, ensure `TRANSLAAS_API_KEY` is not set in your CI/CD environment.
