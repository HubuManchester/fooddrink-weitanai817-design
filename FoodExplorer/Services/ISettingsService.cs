using FoodExplorer.Models;

namespace FoodExplorer.Services;

public interface ISettingsService
{
    bool IsDarkMode { get; set; }
    FontSizeOption FontSize { get; set; }
    double FontScale { get; }
    bool ReduceMotion { get; set; }
    bool HighContrast { get; set; }

    event EventHandler? SettingsChanged;
}
