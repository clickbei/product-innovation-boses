using System.Globalization;

namespace BosesApp.Presentation.Converters;

/// <summary>
/// Converts boolean to color for selected/unselected states
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            return Color.FromArgb("#E3F2FD"); // Light blue for selected
        }
        return Colors.White; // White for unselected
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
