namespace FoodExplorer.Services;

/// <summary>
/// In-memory cache for bundled recipe images to avoid repeated decoding.
/// Recipe JPGs are MauiAssets; other images use MauiImage (FromFile).
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

            var source = CreateSource(key);
            _cache[key] = source;
            return source;
        }
    }

    public void Preload(IEnumerable<string> imageFileNames)
    {
        _ = Task.Run(() =>
        {
            foreach (var name in imageFileNames.Distinct(StringComparer.OrdinalIgnoreCase))
                _ = GetImage(name);

            _ = GetImage(FallbackImage);
        });
    }

    public void Clear()
    {
        lock (_lock)
            _cache.Clear();
    }

    private static ImageSource CreateSource(string fileName)
    {
        if (fileName.StartsWith("recipe_", StringComparison.OrdinalIgnoreCase))
            return LoadRecipeAsset(fileName);

        return ImageSource.FromFile(fileName);
    }

    private static ImageSource LoadRecipeAsset(string fileName)
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync(fileName)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            var bytes = buffer.ToArray();

            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ImageCache] Asset load failed '{fileName}': {ex.Message}");
            return ImageSource.FromFile(FallbackImage);
        }
    }
}
