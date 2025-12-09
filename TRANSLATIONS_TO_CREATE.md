# Translations to Create in Translaas

This document lists all translation groups and strings that need to be created in your Translaas project for the SDK samples to work correctly.

## Project Setup

**Project ID**: `translaas-sdk-samples`

**Languages**: 
- `en` (English)
- `fr` (French) 
- `es` (Spanish)

---

## Group: `common`

Common UI elements and messages used across all applications.

| Entry Key | English (en) | French (fr) | Spanish (es) |
|-----------|--------------|-------------|---------------|
| `app.name` | Translaas SDK Samples | Échantillons SDK Translaas | Muestras SDK Translaas |
| `welcome` | Welcome | Bienvenue | Bienvenido |
| `welcome.message` | This is a sample application demonstrating the Translaas SDK across different .NET platforms. | Ceci est une application d'exemple démontrant le SDK Translaas sur différentes plateformes .NET. | Esta es una aplicación de ejemplo que demuestra el SDK Translaas en diferentes plataformas .NET. |
| `footer.rights` | All rights reserved | Tous droits réservés | Todos los derechos reservados |
| `loading` | Loading... | Chargement... | Cargando... |
| `error` | An error occurred | Une erreur s'est produite | Ocurrió un error |

---

## Group: `navigation`

Navigation menu items.

| Entry Key | English (en) | French (fr) | Spanish (es) |
|-----------|--------------|-------------|---------------|
| `home` | Home | Accueil | Inicio |
| `privacy` | Privacy | Confidentialité | Privacidad |
| `about` | About | À propos | Acerca de |

---

## Group: `messages`

User-facing messages with pluralization support.

**Important**: These entries support pluralization. The Translaas API will automatically select the correct form based on the `number` parameter passed by the SDK.

| Entry Key | English (en) | French (fr) | Spanish (es) |
|-----------|--------------|-------------|---------------|
| `item` | {0} item | {0} article | {0} artículo |
| `item.plural` | {0} items | {0} articles | {0} artículos |
| `notification` | You have {0} notification | Vous avez {0} notification | Tienes {0} notificación |
| `notification.plural` | You have {0} notifications | Vous avez {0} notifications | Tienes {0} notificaciones |
| `user.online` | {0} user online | {0} utilisateur en ligne | {0} usuario en línea |
| `user.online.plural` | {0} users online | {0} utilisateurs en ligne | {0} usuarios en línea |

**Pluralization Notes**:
- When `number = 1`: Use the singular form (e.g., `item`)
- When `number != 1`: Use the plural form (e.g., `item.plural`)
- The SDK passes the `number` parameter to the API, which handles pluralization based on language rules

---

## Group: `privacy`

Privacy policy content.

| Entry Key | English (en) | French (fr) | Spanish (es) |
|-----------|--------------|-------------|---------------|
| `title` | Privacy Policy | Politique de confidentialité | Política de privacidad |
| `description` | Your privacy is important to us. This sample application demonstrates how to use the Translaas SDK for managing translations. | Votre vie privée est importante pour nous. Cette application d'exemple démontre comment utiliser le SDK Translaas pour gérer les traductions. | Su privacidad es importante para nosotros. Esta aplicación de ejemplo demuestra cómo usar el SDK Translaas para gestionar traducciones. |
| `details` | This is a demonstration application. No personal data is collected or stored. | Il s'agit d'une application de démonstration. Aucune donnée personnelle n'est collectée ou stockée. | Esta es una aplicación de demostración. No se recopilan ni almacenan datos personales. |

---

## Group: `api`

API-related messages and responses.

| Entry Key | English (en) | French (fr) | Spanish (es) |
|-----------|--------------|-------------|---------------|
| `translation.retrieved` | Translation retrieved successfully | Traduction récupérée avec succès | Traducción recuperada exitosamente |
| `translation.not.found` | Translation not found | Traduction introuvable | Traducción no encontrada |
| `group.retrieved` | Translation group retrieved successfully | Groupe de traduction récupéré avec succès | Grupo de traducción recuperado exitosamente |
| `project.retrieved` | Translation project retrieved successfully | Projet de traduction récupéré avec succès | Proyecto de traducción recuperado exitosamente |

---

## Quick Setup Checklist

- [ ] Create project with ID: `translaas-sdk-samples`
- [ ] Add languages: `en`, `fr`, `es`
- [ ] Create group: `common` (6 entries)
- [ ] Create group: `navigation` (3 entries)
- [ ] Create group: `messages` (6 entries with pluralization)
- [ ] Create group: `privacy` (3 entries)
- [ ] Create group: `api` (4 entries)

**Total**: 5 groups, 22 entries, 3 languages = 66 translations

---

## Usage in Samples

All samples use the same project ID (`translaas-sdk-samples`) and the same group/entry keys, demonstrating how a single Translaas project can serve multiple applications and platforms:

- **Console Sample**: Uses `common`, `messages` groups
- **WebApi Sample**: Uses `common`, `messages`, `api` groups  
- **WebApp Sample**: Uses `common`, `navigation`, `messages`, `privacy` groups
- **Blazor Sample**: Uses `common`, `messages`, `privacy` groups

This unified approach allows you to:
- Share translations across different .NET applications
- Maintain consistency across platforms
- Easily add new platforms (e.g., Kotlin, iOS) that use the same project
