using FoodExplorer.Models;

namespace FoodExplorer.Services;

public interface IRecipeService
{
    Task<IReadOnlyList<Recipe>> GetAllRecipesAsync();
    Task<IReadOnlyList<Recipe>> GetFeaturedRecipesAsync();
    Task<IReadOnlyList<RecipeCategory>> GetCategoriesAsync();
    Task<Recipe?> GetRecipeByIdAsync(int id);
    Task<IReadOnlyList<RecipeSummary>> GetRecipeSummariesAsync();
}
