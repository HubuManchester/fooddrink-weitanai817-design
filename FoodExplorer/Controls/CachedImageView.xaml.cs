using FoodExplorer.Services;

namespace FoodExplorer.Controls;

public partial class CachedImageView : ContentView
{
    public static readonly BindableProperty ImageFileNameProperty =
        BindableProperty.Create(
            nameof(ImageFileName),
            typeof(string),
            typeof(CachedImageView),
            string.Empty,
            propertyChanged: OnImageFileNameChanged);

    public CachedImageView()
    {
        InitializeComponent();
    }

    public string ImageFileName
    {
        get => (string)GetValue(ImageFileNameProperty);
        set => SetValue(ImageFileNameProperty, value);
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        UpdateImageSource();
    }

    private static void OnImageFileNameChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CachedImageView view)
            view.UpdateImageSource();
    }

    private void UpdateImageSource()
    {
        var fileName = ImageFileName;
        if (string.IsNullOrWhiteSpace(fileName))
            fileName = "dotnet_bot.png";

        var cache = Handler?.MauiContext?.Services.GetService<IImageCacheService>();
        Photo.Source = cache?.GetImage(fileName) ?? ImageSource.FromFile(fileName);
    }
}
