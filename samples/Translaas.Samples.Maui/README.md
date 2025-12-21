# Translaas SDK - .NET MAUI Sample

A cross-platform mobile and desktop application demonstrating how to integrate and use the Translaas SDK in .NET MAUI applications.

## Overview

This sample showcases:
- **Dependency Injection** setup in MAUI applications
- **MVVM Pattern** integration with CommunityToolkit.Mvvm
- **Translation Service** usage with data binding
- **Language Switching** functionality
- **Caching Configuration** optimized for mobile scenarios
- **Pluralization** support
- **Bulk Operations** with translation groups
- **Error Handling** patterns for network failures

## Features Demonstrated

### 1. Basic Translations
Using `ITranslaasService.T()` for simple translation lookups with data binding.

### 2. Pluralization
Automatic plural form selection based on count values.

### 3. Named Parameters
Using placeholders like `{userName}` and `{itemCount}` in translations.

### 4. Translation Groups
Bulk retrieval of translations using `ITranslaasClient.GetGroupAsync()`.

### 5. Available Locales
Querying available languages from the API.

### 6. Caching Performance
Demonstration of cache hits vs. cache misses.

### 7. Language Switching
Dynamic language selection with UI refresh.

## Prerequisites

### Required
- .NET 8.0 SDK or later
- .NET MAUI workload installed
- A valid Translaas API key

### Platform-Specific

#### Windows
- Visual Studio 2022 with MAUI workload
- Windows 10 SDK (version 10.0.19041.0 or higher)

#### Android
- Android SDK (API level 21+)
- Android emulator or physical device

#### iOS/macOS
- Xcode 15 or later (macOS only)
- Apple Developer account for device testing

## Setup

### 1. Install MAUI Workload

```bash
dotnet workload install maui
```

### 2. Configure API Key

Edit `appsettings.json` and replace `your-api-key-here` with your actual API key:

```json
{
  "Translaas": {
    "ApiKey": "your-api-key-here",
    "BaseUrl": "https://sdk-api.translaas.local"
  }
}
```

**Security Note**: For production apps, store API keys securely using:
- [MAUI Secure Storage](https://learn.microsoft.com/dotnet/maui/platform-integration/storage/secure-storage)
- Environment variables
- Platform-specific key stores

### 3. Add Fonts (Optional)

Download OpenSans fonts from [Google Fonts](https://fonts.google.com/specimen/Open+Sans) and place them in `Resources/Fonts/`:
- OpenSans-Regular.ttf
- OpenSans-Semibold.ttf

Or use system fonts by updating `MauiProgram.cs`.

## Running the Sample

### Windows

```bash
cd samples/Translaas.Samples.Maui
dotnet build -f net10.0-windows10.0.19041.0 -r win-x64 --no-self-contained
dotnet run -f net10.0-windows10.0.19041.0 -r win-x64 --no-self-contained
```

Or use Visual Studio:
1. Open the solution in Visual Studio 2022
2. Set `Translaas.Samples.Maui` as the startup project
3. Select Windows Machine as the target
4. Press F5 to run

### Android

```bash
dotnet build -f net10.0-android
dotnet run -f net10.0-android
```

Or use Visual Studio with an Android emulator.

### iOS (macOS only)

```bash
dotnet build -f net10.0-ios
dotnet run -f net10.0-ios
```

### macOS (macOS only)

```bash
dotnet build -f net10.0-maccatalyst
dotnet run -f net10.0-maccatalyst
```

## Project Structure

```
Translaas.Samples.Maui/
├── App.xaml                    # Application resources
├── App.xaml.cs                 # Application entry point
├── AppShell.xaml               # Shell navigation
├── AppShell.xaml.cs
├── MauiProgram.cs              # DI and service configuration
├── appsettings.json            # Configuration file
├── ViewModels/
│   ├── MainViewModel.cs        # Main page view model
│   └── SettingsViewModel.cs    # Settings page view model
├── Views/
│   ├── MainPage.xaml           # Main translation demo page
│   ├── MainPage.xaml.cs
│   ├── SettingsPage.xaml       # Settings and testing page
│   └── SettingsPage.xaml.cs
└── Resources/
    ├── AppIcon/               # App icons
    ├── Fonts/                 # Custom fonts
    ├── Images/                # Navigation icons
    ├── Raw/                   # Raw assets
    ├── Splash/                # Splash screen
    └── Styles/                # Colors and styles
```

## Code Examples

### Dependency Injection Setup

```csharp
// MauiProgram.cs
builder.Services.AddHttpClient();

builder.Services.AddTranslaas(options =>
{
    options.ApiKey = configuration["Translaas:ApiKey"]!;
    options.BaseUrl = configuration["Translaas:BaseUrl"]!;
    options.CacheMode = CacheMode.Group;  // Recommended for mobile
    options.CacheAbsoluteExpiration = TimeSpan.FromHours(24);
});

// Register ViewModels and Views
builder.Services.AddTransient<MainViewModel>();
builder.Services.AddTransient<MainPage>();
```

### ViewModel with Translations

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly ITranslaasService _translaasService;
    
    [ObservableProperty]
    private string _welcomeText = "Loading...";
    
    [RelayCommand]
    public async Task LoadTranslationsAsync()
    {
        WelcomeText = await _translaasService.T("common", "welcome", "en");
    }
}
```

### XAML Data Binding

```xml
<Label Text="{Binding WelcomeText}" />
<Picker ItemsSource="{Binding AvailableLanguages}"
        SelectedItem="{Binding SelectedLanguage}" />
<Button Text="Refresh" Command="{Binding RefreshCommand}" />
```

### Language Switching

```csharp
partial void OnSelectedLanguageChanged(LanguageOption value)
{
    // Reload translations when language changes
    _ = LoadTranslationsAsync();
}
```

## Configuration Options

| Setting | Description | Recommended Value |
|---------|-------------|-------------------|
| `CacheMode` | Caching strategy | `Group` for mobile |
| `CacheAbsoluteExpiration` | Maximum cache lifetime | 24 hours |
| `CacheSlidingExpiration` | Sliding expiration window | 12 hours |
| `Timeout` | HTTP request timeout | 45 seconds |

### Why Group Caching for Mobile?

- **Reduced Network Calls**: Fetches all entries in a group at once
- **Offline Support**: Cached translations available without network
- **Battery Friendly**: Fewer network operations
- **Faster UI**: Instant cache hits after initial load

## Platform-Specific Considerations

### iOS
- App lifecycle events may clear memory cache
- Consider using file-based caching for persistence
- Test on physical device for accurate performance

### Android
- Background service limitations may affect background cache refresh
- Handle network connectivity changes gracefully
- Test on multiple device sizes

### Windows
- Desktop apps can use more aggressive caching
- Higher memory allowance available
- Test with different DPI settings

### macOS
- Similar to iOS but with desktop form factor
- Test app suspension/resume behavior

## Troubleshooting

### "API key not configured"
Ensure `appsettings.json` contains your valid API key.

### "Connection failed"
- Verify network connectivity
- Check `BaseUrl` is correct (without `/api` suffix)
- Ensure API endpoint is accessible from your network

### "Translation not found"
- Verify the group, entry, and language exist in your project
- Check the project ID in the sample code matches your Translaas project

### Build errors
- Ensure MAUI workload is installed: `dotnet workload install maui`
- Update NuGet packages to latest versions
- Clean and rebuild the solution

### Missing fonts
- Download OpenSans fonts from Google Fonts
- Place TTF files in `Resources/Fonts/` folder
- Rebuild the project

## Additional Resources

- [Main SDK README](../../README.md) - Complete SDK documentation
- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
- [MAUI Dependency Injection](https://learn.microsoft.com/dotnet/maui/fundamentals/dependency-injection)

## License

This sample is part of the Translaas SDK and is provided under the same license as the SDK.
