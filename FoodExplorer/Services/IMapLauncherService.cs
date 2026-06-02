namespace FoodExplorer.Services;

public record MapLaunchResult(bool Success, string? ErrorMessage)
{
    public static MapLaunchResult Ok() => new(true, null);
    public static MapLaunchResult Fail(string error) => new(false, error);
}

public interface IMapLauncherService
{
    Task<MapLaunchResult> OpenNearbySearchAsync(
        double latitude,
        double longitude,
        string searchQuery,
        CancellationToken cancellationToken = default);
}
