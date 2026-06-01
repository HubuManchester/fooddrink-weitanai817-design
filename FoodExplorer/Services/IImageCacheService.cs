namespace FoodExplorer.Services;

public interface IImageCacheService
{
    ImageSource GetImage(string imageFileName);
    void Preload(IEnumerable<string> imageFileNames);
    void Clear();
}
