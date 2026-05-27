using System.Globalization;

namespace BosesApp.Presentation.Converters;

/// <summary>
/// Converts boolean to stroke color for selected/unselected states
/// </summary>
public class BoolToStrokeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            return Color.FromArgb("#2196F3"); // Blue for selected
        }
        return Color.FromArgb("#E0E0E0"); // Gray for unselected
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
