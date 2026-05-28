using Microsoft.Maui.Controls;
using System.Globalization;

namespace BosesApp.Presentation.Converters;

/// <summary>
/// Returns <c>true</c> (visible) when the integer value is greater than zero.
/// Usage: IsVisible="{Binding GuardianAlertsToday, Converter={StaticResource IntGtZero}}"
/// </summary>
public class IntGreaterThanZeroConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i > 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
