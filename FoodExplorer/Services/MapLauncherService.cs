using System.Globalization;

namespace FoodExplorer.Services;

/// <summary>
/// Opens the device maps app to search for nearby places (Hardware #9 — Geolocation follow-up).
/// </summary>
public class MapLauncherService : IMapLauncherService
{
    public async Task<MapLaunchResult> OpenNearbySearchAsync(
        double latitude,
        double longitude,
        string searchQuery,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            searchQuery = "restaurants";

        var encodedQuery = Uri.EscapeDataString(searchQuery.Trim());
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);

        var candidates = new[]
        {
            new Uri($"geo:{lat},{lon}?q={encodedQuery}"),
            new Uri($"https://www.google.com/maps/search/{encodedQuery}/@{lat},{lon},14z"),
            new Uri($"https://maps.google.com/?q={encodedQuery}&ll={lat},{lon}&z=14")
        };

        foreach (var uri in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (await Launcher.Default.CanOpenAsync(uri))
                {
                    await Launcher.Default.OpenAsync(uri);
                    return MapLaunchResult.Ok();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MapLauncherService] CanOpen {uri}: {ex.Message}");
            }
        }

        foreach (var uri in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await Launcher.Default.OpenAsync(uri);
                return MapLaunchResult.Ok();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MapLauncherService] Open {uri}: {ex.Message}");
            }
        }

        try
        {
            var location = new Location(latitude, longitude);
            var options = new MapLaunchOptions { Name = searchQuery };
            await Map.Default.OpenAsync(location, options);
            return MapLaunchResult.Ok();
        }
        catch (FeatureNotSupportedException)
        {
            return MapLaunchResult.Fail("No maps app found. Install Google Maps or another maps app.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MapLauncherService] Map.OpenAsync: {ex}");
            return MapLaunchResult.Fail(
                string.IsNullOrWhiteSpace(ex.Message)
                    ? "Could not open maps."
                    : $"Could not open maps: {ex.Message}");
        }
    }
}
