using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodExplorer.Models;
using FoodExplorer.Services;

namespace FoodExplorer.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;
    private readonly IShakeDetectionService _shakeDetectionService;
    private readonly IHapticService _hapticService;
    private readonly ISettingsService _settingsService;
    private readonly IDeviceLayoutService _deviceLayoutService;
    private readonly IImageCacheService _imageCacheService;
    private bool _dataLoaded;

    public HomeViewModel(
        IRecipeService recipeService,
        IShakeDetectionService shakeDetectionService,
        IHapticService hapticService,
        ISettingsService settingsService,
        IDeviceLayoutService deviceLayoutService,
        IImageCacheService imageCacheService,
        INavigationService navigationService,
        IDialogService dialogService)
        : base(navigationService, dialogService)
    {
        _recipeService = recipeService;
        _shakeDetectionService = shakeDetectionService;
        _hapticService = hapticService;
        _settingsService = settingsService;
        _deviceLayoutService = deviceLayoutService;
        _imageCacheService = imageCacheService;
        Title = "FoodExplorer";
        ShakeHint = _settingsService.ReduceMotion
            ? "Shake disabled (Reduce Motion is on). Use the button below."
            : _shakeDetectionService.IsSupported
                ? "📱 Shake your phone for a random recipe!"
                : "Shake detection is not available on this device.";
    }

    public ObservableCollection<Recipe> FeaturedRecipes { get; } = new();
    public ObservableCollection<RecipeCategory> Categories { get; } = new();

    [ObservableProperty]
    private string _shakeHint = string.Empty;

    [ObservableProperty]
    private double _featuredCardWidth = 200;

    public void UpdateLayout(double pageWidth) =>
        FeaturedCardWidth = _deviceLayoutService.GetFeaturedCardWidth(pageWidth);

    /// <summary>Hardware #3: Accelerometer — shake detection for random recipe discovery.</summary>
    public void StartShakeMonitoring()
    {
        if (_settingsService.ReduceMotion)
            return;

        // Hardware #3: Accelerometer — subscribe to shake events
        _shakeDetectionService.ShakeDetected += OnShakeDetected;
        _shakeDetectionService.StartMonitoring();
    }

    public void StopShakeMonitoring()
    {
        _shakeDetectionService.ShakeDetected -= OnShakeDetected;
        _shakeDetectionService.StopMonitoring();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (_dataLoaded)
            return;

        await ExecuteAsync(async () =>
        {
            FeaturedRecipes.Clear();
            Categories.Clear();

            var featured = await _recipeService.GetFeaturedRecipesAsync();
            var categories = await _recipeService.GetCategoriesAsync();
            var all = await _recipeService.GetAllRecipesAsync();

            _imageCacheService.Preload(all.Select(r => r.ImageUri));

            foreach (var recipe in featured)
                FeaturedRecipes.Add(recipe);

            foreach (var category in categories)
                Categories.Add(category);

            _dataLoaded = true;
        });
    }

    [RelayCommand]
    private async Task RandomRecipeAsync()
    {
        await HandleShakeAsync();
    }

    [RelayCommand]
    private async Task OpenRecipeAsync(Recipe recipe)
    {
        if (recipe is null)
            return;

        await NavigationService.GoToAsync(
            $"{nameof(Views.RecipeDetailPage)}?id={recipe.Id}");
    }

    [RelayCommand]
    private async Task OpenCategoryAsync(RecipeCategory category)
    {
        if (category is null)
            return;

        await NavigationService.GoToAsync(
            $"//{nameof(Views.RecipeListPage)}?category={Uri.EscapeDataString(category.Name)}");
    }

    [RelayCommand]
    private async Task BrowseAllAsync()
    {
        await NavigationService.GoToAsync($"//{nameof(Views.RecipeListPage)}");
    }

    [RelayCommand]
    private async Task BrowseFavouritesAsync()
    {
        await NavigationService.GoToAsync($"//{nameof(Views.RecipeListPage)}?favourites=true");
    }

    private async void OnShakeDetected(object? sender, EventArgs e)
    {
        await MainThread.InvokeOnMainThreadAsync(HandleShakeAsync);
    }

    private async Task HandleShakeAsync()
    {
        if (IsBusy)
            return;

        await ExecuteAsync(async () =>
        {
            var recipe = await _recipeService.GetRandomRecipeAsync();
            if (recipe is null)
            {
                ShakeHint = "No recipes available for random pick.";
                return;
            }

            _hapticService.PerformSuccess();
            await _hapticService.VibrateAsync(TimeSpan.FromMilliseconds(300));
            ShakeHint = $"🎲 Random pick: {recipe.Name}";
            SemanticScreenReader.Announce($"Random recipe: {recipe.Name}");

            await NavigationService.GoToAsync(
                $"{nameof(Views.RecipeDetailPage)}?id={recipe.Id}");
        }, "Could not pick a random recipe.");
    }
}
