namespace FoodExplorer.Services;

public interface IAccessibilityService
{
    void ApplyFontScale(Page page, double scale);
    void ApplyHighContrast(Page page, bool enabled);
}
