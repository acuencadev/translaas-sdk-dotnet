using System.Globalization;

namespace Translaas.Samples.Maui.Converters;

/// <summary>
/// A value converter that inverts a boolean value.
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    /// <summary>
    /// Inverts the boolean value.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value;
    }

    /// <summary>
    /// Inverts the boolean value back.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return value;
    }
}
