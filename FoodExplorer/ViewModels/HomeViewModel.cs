using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using FoodExplorer.Models;
using FoodExplorer.Services;

namespace FoodExplorer.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;

    public HomeViewModel(
        IRecipeService recipeService,
        INavigationService navigationService,
        IDialogService dialogService)
        : base(navigationService, dialogService)
    {
        _recipeService = recipeService;
        Title = "FoodExplorer";
    }

    public ObservableCollection<Recipe> FeaturedRecipes { get; } = new();
    public ObservableCollection<RecipeCategory> Categories { get; } = new();

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            FeaturedRecipes.Clear();
            Categories.Clear();

            var featured = await _recipeService.GetFeaturedRecipesAsync();
            var categories = await _recipeService.GetCategoriesAsync();

            foreach (var recipe in featured)
                FeaturedRecipes.Add(recipe);

            foreach (var category in categories)
                Categories.Add(category);
        });
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
}
