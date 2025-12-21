namespace Translaas.Models;

/// <summary>
/// Provides constants for ISO 639-1 language codes commonly used in translations.
/// </summary>
/// <remarks>
/// <para>
/// This class provides type-safe constants for language codes to improve code readability and reduce typos.
/// Language codes follow the ISO 639-1 standard (two-letter codes).
/// </para>
/// <para>
/// While these constants are provided for convenience, you can still use any string value for language codes.
/// The Translaas API accepts any valid language code supported by your project.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// // Direct usage
/// var translation = await client.GetEntryAsync("common", "welcome", LanguageCodes.English);
/// var french = await client.GetEntryAsync("common", "welcome", LanguageCodes.French);
/// 
/// // With using alias for shorter syntax
/// using L = Translaas.Models.LanguageCodes;
/// var translation = await client.GetEntryAsync("common", "welcome", L.English);
/// </code>
/// </para>
/// </remarks>
public static class LanguageCodes
{
    /// <summary>
    /// English language code (ISO 639-1: en).
    /// </summary>
    public const string English = "en";

    /// <summary>
    /// French language code (ISO 639-1: fr).
    /// </summary>
    public const string French = "fr";

    /// <summary>
    /// Spanish language code (ISO 639-1: es).
    /// </summary>
    public const string Spanish = "es";

    /// <summary>
    /// German language code (ISO 639-1: de).
    /// </summary>
    public const string German = "de";

    /// <summary>
    /// Italian language code (ISO 639-1: it).
    /// </summary>
    public const string Italian = "it";

    /// <summary>
    /// Portuguese language code (ISO 639-1: pt).
    /// </summary>
    public const string Portuguese = "pt";

    /// <summary>
    /// Russian language code (ISO 639-1: ru).
    /// </summary>
    public const string Russian = "ru";

    /// <summary>
    /// Japanese language code (ISO 639-1: ja).
    /// </summary>
    public const string Japanese = "ja";

    /// <summary>
    /// Chinese (Simplified) language code (ISO 639-1: zh).
    /// </summary>
    public const string Chinese = "zh";

    /// <summary>
    /// Korean language code (ISO 639-1: ko).
    /// </summary>
    public const string Korean = "ko";

    /// <summary>
    /// Arabic language code (ISO 639-1: ar).
    /// </summary>
    public const string Arabic = "ar";

    /// <summary>
    /// Dutch language code (ISO 639-1: nl).
    /// </summary>
    public const string Dutch = "nl";

    /// <summary>
    /// Swedish language code (ISO 639-1: sv).
    /// </summary>
    public const string Swedish = "sv";

    /// <summary>
    /// Norwegian language code (ISO 639-1: no).
    /// </summary>
    public const string Norwegian = "no";

    /// <summary>
    /// Danish language code (ISO 639-1: da).
    /// </summary>
    public const string Danish = "da";

    /// <summary>
    /// Finnish language code (ISO 639-1: fi).
    /// </summary>
    public const string Finnish = "fi";

    /// <summary>
    /// Polish language code (ISO 639-1: pl).
    /// </summary>
    public const string Polish = "pl";

    /// <summary>
    /// Turkish language code (ISO 639-1: tr).
    /// </summary>
    public const string Turkish = "tr";

    /// <summary>
    /// Greek language code (ISO 639-1: el).
    /// </summary>
    public const string Greek = "el";

    /// <summary>
    /// Hebrew language code (ISO 639-1: he).
    /// </summary>
    public const string Hebrew = "he";

    /// <summary>
    /// Hindi language code (ISO 639-1: hi).
    /// </summary>
    public const string Hindi = "hi";

    /// <summary>
    /// Thai language code (ISO 639-1: th).
    /// </summary>
    public const string Thai = "th";

    /// <summary>
    /// Vietnamese language code (ISO 639-1: vi).
    /// </summary>
    public const string Vietnamese = "vi";

    /// <summary>
    /// Indonesian language code (ISO 639-1: id).
    /// </summary>
    public const string Indonesian = "id";

    /// <summary>
    /// Czech language code (ISO 639-1: cs).
    /// </summary>
    public const string Czech = "cs";

    /// <summary>
    /// Romanian language code (ISO 639-1: ro).
    /// </summary>
    public const string Romanian = "ro";

    /// <summary>
    /// Hungarian language code (ISO 639-1: hu).
    /// </summary>
    public const string Hungarian = "hu";

    /// <summary>
    /// Bulgarian language code (ISO 639-1: bg).
    /// </summary>
    public const string Bulgarian = "bg";

    /// <summary>
    /// Croatian language code (ISO 639-1: hr).
    /// </summary>
    public const string Croatian = "hr";

    /// <summary>
    /// Slovak language code (ISO 639-1: sk).
    /// </summary>
    public const string Slovak = "sk";

    /// <summary>
    /// Slovenian language code (ISO 639-1: sl).
    /// </summary>
    public const string Slovenian = "sl";

    /// <summary>
    /// Ukrainian language code (ISO 639-1: uk).
    /// </summary>
    public const string Ukrainian = "uk";

    /// <summary>
    /// Norwegian Bokmål language code (ISO 639-1: nb).
    /// </summary>
    public const string NorwegianBokmal = "nb";

    /// <summary>
    /// Norwegian Nynorsk language code (ISO 639-1: nn).
    /// </summary>
    public const string NorwegianNynorsk = "nn";
}

