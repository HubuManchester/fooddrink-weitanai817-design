namespace FoodExplorer.Services;

public class DeviceLayoutService : IDeviceLayoutService
{
    private const double TabletBreakpoint = 600;
    private const double LargeTabletBreakpoint = 900;

    public bool IsTablet(double pageWidth) => pageWidth >= TabletBreakpoint;

    public int GetRecipeGridSpan(double pageWidth) => pageWidth switch
    {
        >= LargeTabletBreakpoint => 4,
        >= TabletBreakpoint => 3,
        _ => 2
    };

    public double GetFeaturedCardWidth(double pageWidth) => pageWidth switch
    {
        >= LargeTabletBreakpoint => 280,
        >= TabletBreakpoint => 240,
        _ => 200
    };

    public double GetContentMaxWidth(double pageWidth) =>
        IsTablet(pageWidth) ? 720 : double.PositiveInfinity;
}
