# Translaas SDK Samples - Translation Structure

This document defines the translation groups and strings used across all SDK samples (Console, WebApi, WebApp, Blazor). All samples use the same Translaas project to demonstrate cross-platform translation sharing.

## Project Information

- **Project ID**: `translaas-sdk-samples`
- **Description**: Sample project demonstrating Translaas SDK usage across .NET platforms

## Translation Groups and Strings

### Group: `common`
Common UI elements and messages used across all applications.

| Entry Key | English (en) | French (fr) | Spanish (es) |
|-----------|--------------|-------------|---------------|
| `app.name` | Translaas SDK Samples | Échantillons SDK Translaas | Muestras SDK Translaas |
| `welcome` | Welcome | Bienvenue | Bienvenido |
| `welcome.message` | This is a sample application demonstrating the Translaas SDK across different .NET platforms. | Ceci est une application d'exemple démontrant le SDK Translaas sur différentes plateformes .NET. | Esta es una aplicación de ejemplo que demuestra el SDK Translaas en diferentes plataformas .NET. |
| `footer.rights` | All rights reserved | Tous droits réservés | Todos los derechos reservados |
| `loading` | Loading... | Chargement... | Cargando... |
| `error` | An error occurred | Une erreur s'est produite | Ocurrió un error |

### Group: `navigation`
Navigation menu items.

| Entry Key | English (en) | French (fr) | Spanish (es) |
|-----------|--------------|-------------|---------------|
| `home` | Home | Accueil | Inicio |
| `privacy` | Privacy | Confidentialité | Privacidad |
| `about` | About | À propos | Acerca de |

### Group: `messages`
User-facing messages with pluralization support.

| Entry Key | English (en) | French (fr) | Spanish (es) |
|-----------|--------------|-------------|---------------|
| `item` | {0} item | {0} article | {0} artículo |
| `item.plural` | {0} items | {0} articles | {0} artículos |
| `notification` | You have {0} notification | Vous avez {0} notification | Tienes {0} notificación |
| `notification.plural` | You have {0} notifications | Vous avez {0} notifications | Tienes {0} notificaciones |
| `user.online` | {0} user online | {0} utilisateur en ligne | {0} usuario en línea |
| `user.online.plural` | {0} users online | {0} utilisateurs en ligne | {0} usuarios en línea |

### Group: `privacy`
Privacy policy content.

| Entry Key | English (en) | French (fr) | Spanish (es) |
|-----------|--------------|-------------|---------------|
| `title` | Privacy Policy | Politique de confidentialité | Política de privacidad |
| `description` | Your privacy is important to us. This sample application demonstrates how to use the Translaas SDK for managing translations. | Votre vie privée est importante pour nous. Cette application d'exemple démontre comment utiliser le SDK Translaas pour gérer les traductions. | Su privacidad es importante para nosotros. Esta aplicación de ejemplo demuestra cómo usar el SDK Translaas para gestionar traducciones. |
| `details` | This is a demonstration application. No personal data is collected or stored. | Il s'agit d'une application de démonstration. Aucune donnée personnelle n'est collectée ou stockée. | Esta es una aplicación de demostración. No se recopilan ni almacenan datos personales. |

### Group: `api`
API-related messages and responses.

| Entry Key | English (en) | French (fr) | Spanish (es) |
|-----------|--------------|-------------|---------------|
| `translation.retrieved` | Translation retrieved successfully | Traduction récupérée avec succès | Traducción recuperada exitosamente |
| `translation.not.found` | Translation not found | Traduction introuvable | Traducción no encontrada |
| `group.retrieved` | Translation group retrieved successfully | Groupe de traduction récupéré avec succès | Grupo de traducción recuperado exitosamente |
| `project.retrieved` | Translation project retrieved successfully | Projet de traduction récupéré avec succès | Proyecto de traducción recuperado exitosamente |

### Group: `console`
Console application specific messages.

| Entry Key | English (en) | French (fr) | Spanish (es) |
|-----------|--------------|-------------|---------------|
| `example.title` | Example {0} | Exemple {0} | Ejemplo {0} |
| `cache.miss` | First call (cache miss) | Premier appel (cache manqué) | Primera llamada (fallo de caché) |
| `cache.hit` | Second call (cache hit) | Deuxième appel (cache réussi) | Segunda llamada (acierto de caché) |
| `duration` | Duration: {0}ms | Durée : {0}ms | Duración: {0}ms |
| `cache.speedup` | Cache speedup: {0}x faster | Accélération du cache : {0}x plus rapide | Aceleración de caché: {0}x más rápido |
| `press.key` | Press any key to exit... | Appuyez sur une touche pour quitter... | Presione cualquier tecla para salir... |

## Pluralization Rules

The SDK supports pluralization through the `number` parameter. For entries that support pluralization:

- When `number = 1`: Use the singular form (e.g., `item`)
- When `number != 1`: Use the plural form (e.g., `item.plural`)

**Note**: The pluralization logic is handled by the Translaas API. The SDK simply passes the `number` parameter to the API, which returns the appropriate form based on the language's pluralization rules.

## Language Codes

The samples use the following language codes:
- `en` - English
- `fr` - French
- `es` - Spanish

## Usage in Samples

All samples use the same project ID (`translaas-sdk-samples`) and the same group/entry keys, demonstrating how a single Translaas project can serve multiple applications and platforms.

### Console Sample
- Uses: `common`, `messages`, `console` groups
- Demonstrates: Basic translation, pluralization, caching

### WebApi Sample
- Uses: `common`, `messages`, `api` groups
- Demonstrates: API endpoints returning translations

### WebApp Sample
- Uses: `common`, `navigation`, `messages`, `privacy` groups
- Demonstrates: Tag helpers, static helpers, Razor view integration

### Blazor Sample
- Uses: `common`, `messages`, `privacy` groups
- Demonstrates: Service injection, dynamic language switching, component integration
