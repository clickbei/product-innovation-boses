using Microsoft.Maui.Graphics;
using System.Globalization;

namespace BosesApp.Presentation.Converters;

/// <summary>
/// Converts a boolean to one of two colors.
/// Usage in XAML (inline):
///   &lt;converters:BoolToColorConverter TrueColor="#3498DB" FalseColor="#2ECC71"/&gt;
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    /// <summary>Color returned when the value is <c>true</c>.</summary>
    public Color TrueColor  { get; set; } = Color.FromArgb("#E3F2FD");

    /// <summary>Color returned when the value is <c>false</c>.</summary>
    public Color FalseColor { get; set; } = Colors.White;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? TrueColor : FalseColor;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

