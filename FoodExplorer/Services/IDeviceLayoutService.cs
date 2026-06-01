namespace FoodExplorer.Services;

public interface IDeviceLayoutService
{
    bool IsTablet(double pageWidth);
    int GetRecipeGridSpan(double pageWidth);
    double GetFeaturedCardWidth(double pageWidth);
    double GetContentMaxWidth(double pageWidth);
}
