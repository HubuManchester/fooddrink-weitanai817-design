using FoodExplorer.Models;

namespace FoodExplorer.Services;

/// <summary>
/// Contract for all recipe data operations including CRUD and favourites management.
/// </summary>
public interface IRecipeService
{
    Task<IReadOnlyList<Recipe>> GetAllRecipesAsync();
    Task<IReadOnlyList<Recipe>> GetFeaturedRecipesAsync();
    Task<IReadOnlyList<RecipeCategory>> GetCategoriesAsync();
    Task<Recipe?> GetRecipeByIdAsync(int id);
    Task<IReadOnlyList<RecipeSummary>> GetRecipeSummariesAsync();

    Task<IReadOnlyList<RecipeSummary>> SearchRecipesAsync(
        string? query,
        string? category,
        DifficultyLevel? difficulty,
        bool favouritesOnly);

    Task<bool> ToggleFavouriteAsync(int recipeId);
    Task<IReadOnlyList<int>> GetFavouriteIdsAsync();
    bool IsFavourite(int recipeId);
}
