using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodExplorer.Models;
using FoodExplorer.Services;

namespace FoodExplorer.ViewModels;

[QueryProperty(nameof(CategoryFilter), "category")]
[QueryProperty(nameof(FavouritesQuery), "favourites")]
public partial class RecipeListViewModel : BaseViewModel
{
    private const int MaxSearchLength = 100;

    private readonly IRecipeService _recipeService;
    private readonly IVoiceSearchService _voiceSearchService;
    private readonly IHapticService _hapticService;
    private readonly IDeviceLayoutService _deviceLayoutService;
    private readonly IImageCacheService _imageCacheService;
    private bool _imagesPreloaded;

    public RecipeListViewModel(
        IRecipeService recipeService,
        IVoiceSearchService voiceSearchService,
        IHapticService hapticService,
        IDeviceLayoutService deviceLayoutService,
        IImageCacheService imageCacheService,
        INavigationService navigationService,
        IDialogService dialogService)
        : base(navigationService, dialogService)
    {
        _recipeService = recipeService;
        _voiceSearchService = voiceSearchService;
        _hapticService = hapticService;
        _deviceLayoutService = deviceLayoutService;
        _imageCacheService = imageCacheService;
        Title = "Recipes";
    }

    public ObservableCollection<RecipeSummary> Recipes { get; } = new();
    public IList<string> DifficultyOptions { get; } = ["All", "Easy", "Medium", "Hard"];

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string? _categoryFilter;

    [ObservableProperty]
    private string _selectedDifficulty = "All";

    [ObservableProperty]
    private bool _showFavouritesOnly;

    [ObservableProperty]
    private string _resultCountText = string.Empty;

    [ObservableProperty]
    private bool _isListening;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _hasValidationMessage;

    [ObservableProperty]
    private string? _favouritesQuery;

    [ObservableProperty]
    private int _gridSpan = 2;

    public void UpdateLayout(double pageWidth) =>
        GridSpan = _deviceLayoutService.GetRecipeGridSpan(pageWidth);

    partial void OnFavouritesQueryChanged(string? value)
    {
        ShowFavouritesOnly = bool.TryParse(value, out var favouritesOnly) && favouritesOnly;
        if (ShowFavouritesOnly)
            Title = "Favourites";
    }

    partial void OnSearchQueryChanged(string value)
    {
        if (value.Length > MaxSearchLength)
        {
            SearchQuery = value[..MaxSearchLength];
            ShowValidationMessage("Search text cannot exceed 100 characters.");
            return;
        }

        ClearValidation();
        _ = ApplyFilterAsync();
    }

    partial void OnCategoryFilterChanged(string? value)
    {
        if (!ShowFavouritesOnly)
            Title = string.IsNullOrWhiteSpace(value) ? "Recipes" : value;

        _ = ApplyFilterAsync();
    }

    partial void OnSelectedDifficultyChanged(string value) => _ = ApplyFilterAsync();

    partial void OnShowFavouritesOnlyChanged(bool value)
    {
        Title = value ? "Favourites" : (string.IsNullOrWhiteSpace(CategoryFilter) ? "Recipes" : CategoryFilter!);
        _ = ApplyFilterAsync();
    }

    [RelayCommand]
    private async Task LoadRecipesAsync()
    {
        await ExecuteAsync(async () =>
        {
            if (!_imagesPreloaded)
            {
                var all = await _recipeService.GetAllRecipesAsync();
                _imageCacheService.Preload(all.Select(r => r.ImageUri));
                _imagesPreloaded = true;
            }

            await ApplyFilterAsync();
        }, "Unable to load recipes.");
    }

    [RelayCommand]
    private async Task OpenRecipeAsync(RecipeSummary recipe)
    {
        if (recipe is null)
            return;

        await NavigationService.GoToAsync(
            $"{nameof(Views.RecipeDetailPage)}?id={recipe.Id}");
    }

    [RelayCommand]
    private async Task ToggleFavouriteAsync(RecipeSummary recipe)
    {
        if (recipe is null)
            return;

        await ExecuteAsync(async () =>
        {
            var isFavourite = await _recipeService.ToggleFavouriteAsync(recipe.Id);
            recipe.IsFavourite = isFavourite;
            _hapticService.PerformClick();

            if (ShowFavouritesOnly && !isFavourite)
                await ApplyFilterAsync();
        }, "Could not update favourite.");
    }

    [RelayCommand]
    private async Task VoiceSearchAsync()
    {
        if (IsListening)
            return;

        ClearValidation();

        try
        {
            IsListening = true;
            var result = await _voiceSearchService.ListenAsync();

            if (!result.Success)
            {
                if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                    ShowValidationMessage(result.ErrorMessage);
                return;
            }

            if (string.IsNullOrWhiteSpace(result.Text))
            {
                ShowValidationMessage("No speech detected. Please try again.");
                return;
            }

            SearchQuery = result.Text.Trim();
            _hapticService.PerformSuccess();
            SemanticScreenReader.Announce($"Searching for {SearchQuery}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeListViewModel] Voice search: {ex}");
            ShowValidationMessage("Voice search failed. Please try again.");
        }
        finally
        {
            IsListening = false;
        }
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchQuery = string.Empty;
        ClearValidation();
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchQuery = string.Empty;
        SelectedDifficulty = "All";
        ShowFavouritesOnly = false;
        CategoryFilter = null;
        ClearValidation();
    }

    private async Task ApplyFilterAsync()
    {
        DifficultyLevel? difficulty = SelectedDifficulty switch
        {
            "Easy" => DifficultyLevel.Easy,
            "Medium" => DifficultyLevel.Medium,
            "Hard" => DifficultyLevel.Hard,
            _ => null
        };

        var query = string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery.Trim();
        var results = await _recipeService.SearchRecipesAsync(
            query,
            CategoryFilter,
            difficulty,
            ShowFavouritesOnly);

        Recipes.Clear();
        foreach (var recipe in results)
            Recipes.Add(recipe);

        ResultCountText = results.Count switch
        {
            0 => "No recipes found",
            1 => "1 recipe found",
            _ => $"{results.Count} recipes found"
        };
    }

    public void ShowValidationMessage(string message)
    {
        ValidationMessage = message;
        HasValidationMessage = true;
    }

    private void ClearValidation()
    {
        ValidationMessage = string.Empty;
        HasValidationMessage = false;
    }
}
