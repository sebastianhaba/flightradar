using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace FlightRadar.UI.Converters;

public class StaleOpacityConverter : IValueConverter
{
    public static readonly StaleOpacityConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? 0.5 : 1.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
