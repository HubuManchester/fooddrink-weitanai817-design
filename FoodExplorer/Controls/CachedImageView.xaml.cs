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

    private static void OnImageFileNameChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not CachedImageView view)
            return;

        var fileName = newValue as string;
        var cache = Application.Current?.Handler?.MauiContext?.Services.GetService<IImageCacheService>();
        view.Photo.Source = cache?.GetImage(fileName ?? string.Empty) ?? ImageSource.FromFile("dotnet_bot.png");
    }
}
