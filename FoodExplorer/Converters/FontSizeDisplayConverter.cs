using System.Globalization;
using FoodExplorer.Models;

namespace FoodExplorer.Converters;

public class FontSizeDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is FontSizeOption option ? option.GetDisplayName() : value?.ToString();

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
