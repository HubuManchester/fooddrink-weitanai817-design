using System.Globalization;

namespace FoodExplorer.Converters;

public class StepHighlightConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isCurrent = value is true;
        return isCurrent ? Color.FromArgb("#FFD166") : Color.FromArgb("#FF6B35");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
