# Translaas SDK Console Sample

This sample demonstrates how to use the Translaas SDK in a console application with dependency injection and configuration.

## Configuration

Configuration is loaded from multiple sources in order of precedence (later sources override earlier ones):

1. `appsettings.json` - Base configuration
2. `appsettings.{Environment}.json` - Environment-specific configuration (e.g., `appsettings.Development.json`)
3. User secrets - For sensitive values like API keys (development only)
4. Environment variables - For production deployments

Sensitive values like the API key should be stored in user secrets (development) or environment variables (production).

### Best Practices for Secrets in Console Apps

For .NET console applications, there are several options for storing secrets:

1. **User Secrets (Recommended for Development)**
   - Stored in your user profile, never committed to source control
   - Perfect for local development
   - See setup instructions below

2. **Environment Variables (Recommended for Production)**
   - Set via system environment variables or launch settings
   - Works well in containers and CI/CD pipelines
   - Example: `set TRANSLAAS__APIKEY=your-key` (Windows) or `export TRANSLAAS__APIKEY=your-key` (Linux/Mac)
   - Note: Use double underscore `__` instead of colon `:` for nested configuration

3. **appsettings.json (Not Recommended for Secrets)**
   - Can be used for non-sensitive configuration
   - Should be gitignored if it contains secrets (not recommended)

### Setting up User Secrets (Development)

1. User secrets are already initialized (UserSecretsId is set in the .csproj file)

2. Set your API key:
   ```bash
   dotnet user-secrets set "Translaas:ApiKey" "your-api-key-here" --project samples/Translaas.Samples.Console
   ```

3. Optionally set a custom base URL:
   ```bash
   dotnet user-secrets set "Translaas:BaseUrl" "https://your-api-url.com" --project samples/Translaas.Samples.Console
   ```

4. Verify your secrets are set:
   ```bash
   dotnet user-secrets list --project samples/Translaas.Samples.Console
   ```

**Note:** User secrets are stored in your user profile and never committed to source control. They're perfect for local development.

### Using Environment Variables (Production)

For production deployments, use environment variables instead:

**Windows:**
```cmd
set TRANSLAAS__APIKEY=your-api-key-here
set TRANSLAAS__BASEURL=https://your-api-url.com
```

**Linux/Mac:**
```bash
export TRANSLAAS__APIKEY=your-api-key-here
export TRANSLAAS__BASEURL=https://your-api-url.com
```

**PowerShell:**
```powershell
$env:TRANSLAAS__APIKEY = "your-api-key-here"
$env:TRANSLAAS__BASEURL = "https://your-api-url.com"
```

**Important:** Use double underscore `__` instead of colon `:` for nested configuration keys in environment variables.

### Configuration Files

**appsettings.json** - Base configuration (committed to source control):
```json
{
  "Translaas": {
    "BaseUrl": "https://sdk-api.translaas.local",
    "DefaultLanguage": "en",
    "CacheMode": "Group",
    "CacheAbsoluteExpiration": "01:00:00",
    "CacheSlidingExpiration": "00:30:00",
    "Timeout": "00:00:30"
  }
}
```

**Note:** `ApiKey` should be stored in user secrets or environment variables, not in `appsettings.json`.

**appsettings.Development.json** - Development-specific overrides (committed to source control):
```json
{
  "Translaas": {
    "BaseUrl": "https://sdk-api.translaas.local"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

**User Secrets** - Sensitive values (NOT committed to source control):
- `Translaas:ApiKey` - Your API key (required)

### Setting the Environment

To use `appsettings.Development.json`, set the environment variable:

**Windows (Command Prompt):**
```cmd
set ASPNETCORE_ENVIRONMENT=Development
```

**Windows (PowerShell):**
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
```

**Linux/Mac:**
```bash
export ASPNETCORE_ENVIRONMENT=Development
```

Or run with the environment variable:
```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project samples/Translaas.Samples.Console
```

### Running the Sample

```bash
dotnet run --project samples/Translaas.Samples.Console
```

## Examples

The sample demonstrates:
1. Using `ITranslaasService.T()` for simple translations
2. Using `ITranslaasClient.GetEntryAsync()` for direct API access
3. Pluralization support
4. Bulk operations with `GetGroupAsync()`
5. Getting available locales
6. Caching demonstration
7. Automatic language resolution (from thread culture and default language)
