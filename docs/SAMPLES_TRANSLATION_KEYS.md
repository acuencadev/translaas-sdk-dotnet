# Translaas SDK Samples - Translation Keys Reference

This document lists all translation groups and entries used across all sample applications. Use this as a reference when cleaning up your Translaas project.

## Project ID

All samples use the project ID: **`translaas-sdk-samples`**

## Translation Groups and Entries

All samples now use only **two translation groups**: `common` and `messages`. This streamlined approach simplifies the samples and makes them easier to maintain.

### Group: `common`

**Used in:** All samples (Console, WebApp, Blazor, MAUI, Offline, WebApi)

**Entries:**
- `app.name` - Application name
- `welcome` - Welcome message
- `welcome.message` - Extended welcome message
- `error` - Generic error message
- `footer.rights` - Footer copyright text
- `loading` - Loading indicator text

**Sample Usage:**
```csharp
await translaasService.T("common", "welcome");
await translaasService.T("common", "app.name");
await translaasService.T("common", "welcome.message");
```

---

### Group: `messages`

**Used in:** All samples (Console, WebApp, Blazor, MAUI, Offline, WebApi)

**Entries:**
- `item` - Plural form entry for items (has `one` and `other` plural forms)
- `items` - Plural form entry combining number and parameters (has `one` and `other` plural forms)
- `greeting` - Greeting message with parameters (has `one` and `other` plural forms)
- `user.online` - Users online message (has `other` plural form)
- `notification` - Notification count message (has `other` plural form)

**Sample Usage:**
```csharp
// Pluralization
await translaasService.T("messages", "item", 1);  // "1 item"
await translaasService.T("messages", "item", 5);  // "5 items"

// With parameters
var params = new Dictionary<string, string> { { "userName", "John" }, { "itemCount", "5" } };
await translaasService.T("messages", "greeting", parameters: params);

// Combined number and parameters
await translaasService.T("messages", "items", 5, params);
```

**Plural Forms:**
- `item`: `one` = "1 item", `other` = "{N} items"
- `items`: `one` = "Hello {userName}, you have 1 item", `other` = "Hello {userName}, you have {N} items"
- `greeting`: `one` = "Hello {userName}, you have 1 item", `other` = "Hello {userName}, you have {itemCount} items"
- `user.online`: `other` = "{N} users online"
- `notification`: `other` = "You have {N} notifications"

---

## Summary by Sample

### Translaas.Samples.Console
- **Groups:** `common`, `messages`
- **Entries:** `common.welcome`, `common.app.name`, `common.welcome.message`, `messages.item`, `messages.greeting`, `messages.items`
- **Bulk Operations:** `GetGroupAsync("common")`, `GetProjectLocalesAsync()`

### Translaas.Samples.WebApp
- **Groups:** `common`, `messages`
- **Entries:** `common.welcome`, `common.welcome.message`, `common.app.name`, `common.footer.rights`, `messages.item`, `messages.greeting`, `messages.items`
- **Usage:** Tag helpers, static helpers, service injection

### Translaas.Samples.Blazor
- **Groups:** `common`, `messages`
- **Entries:** `common.welcome`, `common.welcome.message`, `messages.item`, `messages.greeting`, `messages.items`
- **Bulk Operations:** `GetGroupAsync("common")`

### Translaas.Samples.Maui
- **Groups:** `common`, `messages`
- **Entries:** `common.welcome`, `common.welcome.message`, `common.app.name`, `messages.item`, `messages.greeting`, `messages.items`
- **Bulk Operations:** `GetGroupAsync("common")`, `GetProjectLocalesAsync()`

### Translaas.Samples.Offline
- **Groups:** `common`, `messages`
- **Entries:** `common.welcome`, `common.app.name`, `common.welcome.message`, `messages.item`, `messages.greeting`, `messages.items`
- **Bulk Operations:** `GetGroupAsync("common")`, `GetProjectLocalesAsync()`

### Translaas.Samples.WebApi
- **Groups:** `common`, `messages`
- **TranslationController:** Uses dynamic groups/entries (API endpoint accepts any group/entry)
- **StatsController:** Uses `common.app.name` and `messages.*` entries
- **DashboardController:** Uses `common.*` and `messages.*` entries
- **ProductsController:** Uses `common.*` and `messages.*` entries

---

## Complete Entry List

### Simple String Entries

| Group | Entry | Used In |
|-------|-------|---------|
| `common` | `app.name` | Console, MAUI, Offline, WebApi |
| `common` | `welcome` | All samples |
| `common` | `welcome.message` | Console, WebApp, Blazor, MAUI, Offline, WebApi |
| `common` | `error` | WebApi (error handling) |
| `common` | `footer.rights` | WebApp |
| `common` | `loading` | Cache files (available for use) |

### Plural Form Entries

| Group | Entry | Plural Categories | Used In |
|-------|-------|-------------------|---------|
| `messages` | `item` | `one`, `other` | All samples |
| `messages` | `items` | `one`, `other` | Console, WebApp, Blazor, MAUI, Offline |
| `messages` | `greeting` | `one`, `other` | Console, WebApp, Blazor, MAUI, Offline, WebApi |
| `messages` | `user.online` | `other` | WebApi |
| `messages` | `notification` | `other` | WebApi |

---

## Notes

1. **TranslationController** in WebApi accepts **any group/entry** dynamically, so it can access any translation in your project. However, all sample controllers now use only `common` and `messages` groups.

2. **Bulk Operations** (`GetGroupAsync`, `GetProjectAsync`) retrieve entire groups/projects, so they include all entries in those groups.

3. **Plural Forms** are stored as objects with plural category keys (`one`, `other`, `few`, `many`, `two`, `zero`).

---

## Cleanup Recommendations

### Required Groups (Used in All Samples)
- ✅ `common` - Used in all samples
- ✅ `messages` - Used in all samples

### Required Languages

Based on sample usage, ensure these languages are available:
- ✅ `en` (English) - Used in all samples
- ✅ `fr` (French) - Used in WebApp, Blazor, MAUI, Offline samples
- ✅ `es` (Spanish) - Used in WebApp, Blazor, MAUI samples
- ✅ `ru` (Russian) - Listed in locales but not actively used in samples

---

## Quick Reference

**Most Used Entries:**
1. `common.welcome` - Used in all samples
2. `messages.item` - Used in all samples (plural form)
3. `messages.greeting` - Used in Console, WebApp, Blazor, MAUI, Offline, WebApi
4. `common.app.name` - Used in Console, MAUI, Offline, WebApi
5. `common.welcome.message` - Used in Console, WebApp, Blazor, MAUI, Offline, WebApi

---

## Migration Notes

If you're cleaning up your Translaas project, you can safely remove these groups that are no longer used in samples:
- ❌ `stats` - Removed from samples (now using `common` and `messages`)
- ❌ `dashboard` - Removed from samples (now using `common` and `messages`)
- ❌ `products` - Removed from samples (now using `common` and `messages`)
- ❌ `api` - Removed from samples (now using `common` and `messages`)
- ❌ `navigation` - Removed from samples (now using `common`)
- ❌ `privacy` - Removed from samples (now using `common`)
- ❌ `pluralization-tests` - Removed from samples (testing purposes only)

All samples have been updated to use only `common` and `messages` groups for consistency and simplicity.
