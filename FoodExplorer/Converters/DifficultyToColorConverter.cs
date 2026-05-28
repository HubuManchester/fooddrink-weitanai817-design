using System.Globalization;
using FoodExplorer.Models;

namespace FoodExplorer.Converters;

public class DifficultyToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DifficultyLevel difficulty)
            return Colors.Gray;

        return difficulty switch
        {
            DifficultyLevel.Easy => Color.FromArgb("#06D6A0"),
            DifficultyLevel.Medium => Color.FromArgb("#FFD166"),
            DifficultyLevel.Hard => Color.FromArgb("#EF476F"),
            _ => Colors.Gray
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
