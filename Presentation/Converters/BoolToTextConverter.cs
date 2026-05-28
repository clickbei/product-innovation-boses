using Microsoft.Maui.Controls;
using System.Globalization;

namespace BosesApp.Presentation.Converters;

/// <summary>
/// Converts a boolean to one of two text strings.
/// Usage in XAML (inline):
///   &lt;converters:BoolToTextConverter TrueText="??" FalseText="??"/&gt;
/// </summary>
public class BoolToTextConverter : IValueConverter
{
    /// <summary>Text returned when the value is <c>true</c>.</summary>
    public string TrueText  { get; set; } = "True";

    /// <summary>Text returned when the value is <c>false</c>.</summary>
    public string FalseText { get; set; } = "False";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? TrueText : FalseText;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
