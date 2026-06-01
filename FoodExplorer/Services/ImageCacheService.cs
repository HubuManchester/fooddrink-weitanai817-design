namespace FoodExplorer.Services;

/// <summary>
/// In-memory cache for bundled recipe images to avoid repeated decoding.
/// </summary>
public class ImageCacheService : IImageCacheService
{
    private const string FallbackImage = "dotnet_bot.png";
    private readonly Dictionary<string, ImageSource> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public ImageSource GetImage(string imageFileName)
    {
        var key = string.IsNullOrWhiteSpace(imageFileName) ? FallbackImage : imageFileName;

        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var source = ImageSource.FromFile(key);
            _cache[key] = source;
            return source;
        }
    }

    public void Preload(IEnumerable<string> imageFileNames)
    {
        foreach (var name in imageFileNames.Distinct(StringComparer.OrdinalIgnoreCase))
            _ = GetImage(name);

        _ = GetImage(FallbackImage);
    }

    public void Clear()
    {
        lock (_lock)
            _cache.Clear();
    }
}
