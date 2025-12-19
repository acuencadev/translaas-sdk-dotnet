namespace Translaas.Models;

/// <summary>
/// Provides convenient access to Translaas SDK constants and utilities.
/// </summary>
/// <remarks>
/// <para>
/// This class provides convenient shortcuts for commonly used Translaas SDK constants.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var translation = await client.GetEntryAsync("common", "welcome", Translaas.L.English);
/// var french = await client.GetEntryAsync("common", "welcome", Translaas.L.French);
/// </code>
/// </para>
/// </remarks>
public static class Translaas
{
    /// <summary>
    /// Provides convenient access to language codes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This nested class provides a shorter alias for <see cref="LanguageCodes"/>.
    /// Use <c>Translaas.L.English</c> instead of <c>LanguageCodes.English</c> for improved readability.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// var translation = await client.GetEntryAsync("common", "welcome", Translaas.L.English);
    /// </code>
    /// </para>
    /// </remarks>
    public static class L
    {
        /// <summary>
        /// English language code (ISO 639-1: en).
        /// </summary>
        public const string English = LanguageCodes.English;

        /// <summary>
        /// French language code (ISO 639-1: fr).
        /// </summary>
        public const string French = LanguageCodes.French;

        /// <summary>
        /// Spanish language code (ISO 639-1: es).
        /// </summary>
        public const string Spanish = LanguageCodes.Spanish;

        /// <summary>
        /// German language code (ISO 639-1: de).
        /// </summary>
        public const string German = LanguageCodes.German;

        /// <summary>
        /// Italian language code (ISO 639-1: it).
        /// </summary>
        public const string Italian = LanguageCodes.Italian;

        /// <summary>
        /// Portuguese language code (ISO 639-1: pt).
        /// </summary>
        public const string Portuguese = LanguageCodes.Portuguese;

        /// <summary>
        /// Russian language code (ISO 639-1: ru).
        /// </summary>
        public const string Russian = LanguageCodes.Russian;

        /// <summary>
        /// Japanese language code (ISO 639-1: ja).
        /// </summary>
        public const string Japanese = LanguageCodes.Japanese;

        /// <summary>
        /// Chinese (Simplified) language code (ISO 639-1: zh).
        /// </summary>
        public const string Chinese = LanguageCodes.Chinese;

        /// <summary>
        /// Korean language code (ISO 639-1: ko).
        /// </summary>
        public const string Korean = LanguageCodes.Korean;

        /// <summary>
        /// Arabic language code (ISO 639-1: ar).
        /// </summary>
        public const string Arabic = LanguageCodes.Arabic;

        /// <summary>
        /// Dutch language code (ISO 639-1: nl).
        /// </summary>
        public const string Dutch = LanguageCodes.Dutch;

        /// <summary>
        /// Swedish language code (ISO 639-1: sv).
        /// </summary>
        public const string Swedish = LanguageCodes.Swedish;

        /// <summary>
        /// Norwegian language code (ISO 639-1: no).
        /// </summary>
        public const string Norwegian = LanguageCodes.Norwegian;

        /// <summary>
        /// Danish language code (ISO 639-1: da).
        /// </summary>
        public const string Danish = LanguageCodes.Danish;

        /// <summary>
        /// Finnish language code (ISO 639-1: fi).
        /// </summary>
        public const string Finnish = LanguageCodes.Finnish;

        /// <summary>
        /// Polish language code (ISO 639-1: pl).
        /// </summary>
        public const string Polish = LanguageCodes.Polish;

        /// <summary>
        /// Turkish language code (ISO 639-1: tr).
        /// </summary>
        public const string Turkish = LanguageCodes.Turkish;

        /// <summary>
        /// Greek language code (ISO 639-1: el).
        /// </summary>
        public const string Greek = LanguageCodes.Greek;

        /// <summary>
        /// Hebrew language code (ISO 639-1: he).
        /// </summary>
        public const string Hebrew = LanguageCodes.Hebrew;

        /// <summary>
        /// Hindi language code (ISO 639-1: hi).
        /// </summary>
        public const string Hindi = LanguageCodes.Hindi;

        /// <summary>
        /// Thai language code (ISO 639-1: th).
        /// </summary>
        public const string Thai = LanguageCodes.Thai;

        /// <summary>
        /// Vietnamese language code (ISO 639-1: vi).
        /// </summary>
        public const string Vietnamese = LanguageCodes.Vietnamese;

        /// <summary>
        /// Indonesian language code (ISO 639-1: id).
        /// </summary>
        public const string Indonesian = LanguageCodes.Indonesian;

        /// <summary>
        /// Czech language code (ISO 639-1: cs).
        /// </summary>
        public const string Czech = LanguageCodes.Czech;

        /// <summary>
        /// Romanian language code (ISO 639-1: ro).
        /// </summary>
        public const string Romanian = LanguageCodes.Romanian;

        /// <summary>
        /// Hungarian language code (ISO 639-1: hu).
        /// </summary>
        public const string Hungarian = LanguageCodes.Hungarian;

        /// <summary>
        /// Bulgarian language code (ISO 639-1: bg).
        /// </summary>
        public const string Bulgarian = LanguageCodes.Bulgarian;

        /// <summary>
        /// Croatian language code (ISO 639-1: hr).
        /// </summary>
        public const string Croatian = LanguageCodes.Croatian;

        /// <summary>
        /// Slovak language code (ISO 639-1: sk).
        /// </summary>
        public const string Slovak = LanguageCodes.Slovak;

        /// <summary>
        /// Slovenian language code (ISO 639-1: sl).
        /// </summary>
        public const string Slovenian = LanguageCodes.Slovenian;

        /// <summary>
        /// Ukrainian language code (ISO 639-1: uk).
        /// </summary>
        public const string Ukrainian = LanguageCodes.Ukrainian;

        /// <summary>
        /// Norwegian Bokmål language code (ISO 639-1: nb).
        /// </summary>
        public const string NorwegianBokmal = LanguageCodes.NorwegianBokmal;

        /// <summary>
        /// Norwegian Nynorsk language code (ISO 639-1: nn).
        /// </summary>
        public const string NorwegianNynorsk = LanguageCodes.NorwegianNynorsk;
    }
}
