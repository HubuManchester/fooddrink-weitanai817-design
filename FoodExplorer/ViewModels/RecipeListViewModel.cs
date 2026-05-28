using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodExplorer.Models;
using FoodExplorer.Services;

namespace FoodExplorer.ViewModels;

[QueryProperty(nameof(CategoryFilter), "category")]
public partial class RecipeListViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;
    private List<RecipeSummary> _allRecipes = [];

    public RecipeListViewModel(
        IRecipeService recipeService,
        INavigationService navigationService,
        IDialogService dialogService)
        : base(navigationService, dialogService)
    {
        _recipeService = recipeService;
        Title = "Recipes";
    }

    public ObservableCollection<RecipeSummary> Recipes { get; } = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string? _categoryFilter;

    [ObservableProperty]
    private string _resultCountText = string.Empty;

    partial void OnSearchQueryChanged(string value) => ApplyFilter();

    partial void OnCategoryFilterChanged(string? value)
    {
        Title = string.IsNullOrWhiteSpace(value) ? "Recipes" : value;
        ApplyFilter();
    }

    [RelayCommand]
    private async Task LoadRecipesAsync()
    {
        await ExecuteAsync(async () =>
        {
            _allRecipes = (await _recipeService.GetRecipeSummariesAsync()).ToList();
            ApplyFilter();
        });
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
    private void ClearSearch()
    {
        SearchQuery = string.Empty;
    }

    private void ApplyFilter()
    {
        Recipes.Clear();

        var filtered = _allRecipes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(CategoryFilter))
            filtered = filtered.Where(r =>
                r.Category.Equals(CategoryFilter, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var query = SearchQuery.Trim();
            filtered = filtered.Where(r =>
                r.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                r.Category.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        var results = filtered.ToList();
        foreach (var recipe in results)
            Recipes.Add(recipe);

        ResultCountText = results.Count switch
        {
            0 => "No recipes found",
            1 => "1 recipe found",
            _ => $"{results.Count} recipes found"
        };
    }
}
