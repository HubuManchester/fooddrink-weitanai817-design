namespace FoodExplorer.Models;

public enum FontSizeOption
{
    Small,
    Medium,
    Large,
    ExtraLarge
}

public static class FontSizeOptionExtensions
{
    public static double GetScale(this FontSizeOption option) => option switch
    {
        FontSizeOption.Small => 0.85,
        FontSizeOption.Medium => 1.0,
        FontSizeOption.Large => 1.15,
        FontSizeOption.ExtraLarge => 1.3,
        _ => 1.0
    };

    public static string GetDisplayName(this FontSizeOption option) => option switch
    {
        FontSizeOption.Small => "Small",
        FontSizeOption.Medium => "Medium",
        FontSizeOption.Large => "Large",
        FontSizeOption.ExtraLarge => "Extra Large",
        _ => "Medium"
    };
}
