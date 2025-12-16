namespace Translaas.Models;

/// <summary>
/// Represents CLDR (Common Locale Data Repository) plural categories used for pluralization.
/// </summary>
public enum PluralCategory
{
    /// <summary>
    /// Zero quantity (e.g., Arabic: 0 items).
    /// </summary>
    Zero,

    /// <summary>
    /// Singular quantity (e.g., English: 1 item, Russian: 1, 21, 31...).
    /// </summary>
    One,

    /// <summary>
    /// Dual quantity (e.g., Arabic: 2 items).
    /// </summary>
    Two,

    /// <summary>
    /// Few quantity (e.g., Russian: 2-4, 22-24..., Arabic: 3-10).
    /// </summary>
    Few,

    /// <summary>
    /// Many quantity (e.g., Russian: 5-20, 25-30..., Arabic: 11-99).
    /// </summary>
    Many,

    /// <summary>
    /// Other/plural quantity (e.g., English: 0, 2+, Russian: fractional, Arabic: fractional).
    /// This is the default fallback category and is required for all languages.
    /// </summary>
    Other
}
