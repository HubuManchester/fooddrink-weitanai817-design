using System.Text.Json;
using FoodExplorer.Models;

namespace FoodExplorer.Services;

/// <summary>
/// Recipe service that loads data from a local JSON file bundled as a raw asset.
/// Favourite state is persisted via Preferences so it survives app restarts.
/// </summary>
public class RecipeService : IRecipeService
{
    private const string FavouritesKey = "recipe_favourites";
    private const string RecipesFileName = "recipes.json";

    private List<Recipe>? _recipes;
    private HashSet<int> _favouriteIds = new();
    private bool _initialised;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<Recipe>> GetAllRecipesAsync()
    {
        await EnsureInitialisedAsync();
        return _recipes!.AsReadOnly();
    }

    public async Task<IReadOnlyList<Recipe>> GetFeaturedRecipesAsync()
    {
        await EnsureInitialisedAsync();
        return _recipes!.Where(r => r.IsFeatured).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<RecipeCategory>> GetCategoriesAsync()
    {
        await EnsureInitialisedAsync();
        var categories = _recipes!
            .GroupBy(r => r.Category)
            .Select(g => new RecipeCategory
            {
                Name = g.Key,
                IconEmoji = GetCategoryEmoji(g.Key),
                BackgroundColor = GetCategoryColor(g.Key),
                RecipeCount = g.Count()
            })
            .ToList();
        return categories.AsReadOnly();
    }

    public async Task<Recipe?> GetRecipeByIdAsync(int id)
    {
        await EnsureInitialisedAsync();
        var recipe = _recipes!.FirstOrDefault(r => r.Id == id);
        if (recipe is not null)
            recipe.IsFavourite = _favouriteIds.Contains(recipe.Id);
        return recipe;
    }

    public async Task<IReadOnlyList<RecipeSummary>> GetRecipeSummariesAsync()
    {
        await EnsureInitialisedAsync();
        return MapToSummaries(_recipes!);
    }

    public async Task<IReadOnlyList<RecipeSummary>> SearchRecipesAsync(
        string? query,
        string? category,
        DifficultyLevel? difficulty,
        bool favouritesOnly)
    {
        await EnsureInitialisedAsync();

        var filtered = _recipes!.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(category))
            filtered = filtered.Where(r =>
                r.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        if (difficulty.HasValue)
            filtered = filtered.Where(r => r.Difficulty == difficulty.Value);

        if (favouritesOnly)
            filtered = filtered.Where(r => _favouriteIds.Contains(r.Id));

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();
            filtered = filtered.Where(r =>
                r.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                r.Category.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                r.Cuisine.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                r.DietaryTags.Any(t => t.Contains(q, StringComparison.OrdinalIgnoreCase)));
        }

        return MapToSummaries(filtered.ToList());
    }

    public async Task<bool> ToggleFavouriteAsync(int recipeId)
    {
        await EnsureInitialisedAsync();

        bool isNowFavourite;
        if (_favouriteIds.Contains(recipeId))
        {
            _favouriteIds.Remove(recipeId);
            isNowFavourite = false;
        }
        else
        {
            _favouriteIds.Add(recipeId);
            isNowFavourite = true;
        }

        PersistFavourites();

        var recipe = _recipes!.FirstOrDefault(r => r.Id == recipeId);
        if (recipe is not null)
            recipe.IsFavourite = isNowFavourite;

        return isNowFavourite;
    }

    public async Task<IReadOnlyList<int>> GetFavouriteIdsAsync()
    {
        await EnsureInitialisedAsync();
        return _favouriteIds.ToList().AsReadOnly();
    }

    public bool IsFavourite(int recipeId) => _favouriteIds.Contains(recipeId);

    public async Task<Recipe?> GetRandomRecipeAsync()
    {
        await EnsureInitialisedAsync();
        if (_recipes is null || _recipes.Count == 0)
            return null;

        var index = Random.Shared.Next(_recipes.Count);
        var recipe = _recipes[index];
        recipe.IsFavourite = _favouriteIds.Contains(recipe.Id);
        return recipe;
    }

    private async Task EnsureInitialisedAsync()
    {
        if (_initialised) return;
        await _initLock.WaitAsync();
        try
        {
            if (_initialised) return;
            _recipes = await LoadRecipesFromJsonAsync();
            LoadFavouritesFromPreferences();
            SyncFavouritesOntoRecipes();
            _initialised = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private static async Task<List<Recipe>> LoadRecipesFromJsonAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(RecipesFileName);
            using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
            var json = await reader.ReadToEndAsync();
            var recipes = JsonSerializer.Deserialize<List<Recipe>>(json, JsonOptions);
            return recipes ?? GetFallbackRecipes();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeService] JSON load failed: {ex.Message}");
            return GetFallbackRecipes();
        }
    }

    private void LoadFavouritesFromPreferences()
    {
        try
        {
            var raw = Preferences.Default.Get(FavouritesKey, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
            {
                _favouriteIds = new HashSet<int>();
                return;
            }

            _favouriteIds = raw
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var id) ? (int?)id : null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();
        }
        catch
        {
            _favouriteIds = new HashSet<int>();
        }
    }

    private void PersistFavourites()
    {
        try
        {
            Preferences.Default.Set(FavouritesKey, string.Join(",", _favouriteIds));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeService] Persist failed: {ex.Message}");
        }
    }

    private void SyncFavouritesOntoRecipes()
    {
        if (_recipes is null) return;
        foreach (var recipe in _recipes)
            recipe.IsFavourite = _favouriteIds.Contains(recipe.Id);
    }

    private IReadOnlyList<RecipeSummary> MapToSummaries(IEnumerable<Recipe> recipes)
    {
        return recipes
            .Select(r =>
            {
                var s = RecipeSummary.FromRecipe(r);
                s.IsFavourite = _favouriteIds.Contains(r.Id);
                return s;
            })
            .ToList()
            .AsReadOnly();
    }

    private static string GetCategoryEmoji(string category) => category switch
    {
        "Italian" => "🍝",
        "Asian" => "🍜",
        "Mexican" => "🌮",
        "Dessert" => "🍰",
        "Healthy" => "🥗",
        "Indian" => "🍛",
        "American" => "🍔",
        "Breakfast" => "🥞",
        "Korean" => "🍚",
        "Greek" => "🥙",
        "Chinese" => "🥡",
        "British" => "🐟",
        "Spanish" => "🥘",
        "Japanese" => "🍣",
        "Middle Eastern" => "🧆",
        "French" => "🥖",
        _ => "🍽️"
    };

    private static string GetCategoryColor(string category) => category switch
    {
        "Italian" => "#FF6B35",
        "Asian" => "#2EC4B6",
        "Mexican" => "#FFD166",
        "Dessert" => "#EF476F",
        "Healthy" => "#06D6A0",
        "Indian" => "#E07A5F",
        "American" => "#D62828",
        "Breakfast" => "#F4A261",
        "Korean" => "#E63946",
        "Greek" => "#457B9D",
        "Chinese" => "#E76F51",
        "British" => "#264653",
        "Spanish" => "#F77F00",
        "Japanese" => "#2A9D8F",
        "Middle Eastern" => "#6A994E",
        "French" => "#9B5DE5",
        _ => "#FF6B35"
    };

    private static List<Recipe> GetFallbackRecipes() =>
    [
        new Recipe
        {
            Id = 1,
            Name = "Classic Margherita Pizza",
            Description = "A timeless Italian pizza with fresh basil and mozzarella.",
            FullDescription = "Originating from Naples, this pizza celebrates simplicity.",
            Category = "Italian",
            Cuisine = "Italian",
            PrepTimeMinutes = 20,
            CookTimeMinutes = 15,
            Servings = 4,
            Difficulty = DifficultyLevel.Medium,
            CaloriesPerServing = 280,
            Rating = 4.8,
            RatingCount = 324,
            IsFeatured = true,
            DietaryTags = ["Vegetarian"],
            Ingredients = [new Ingredient { Quantity = 1, Unit = "ball", Name = "pizza dough" }],
            Steps = ["Preheat oven to 250 degrees C.", "Roll out dough.", "Add toppings.", "Bake 12-15 min."]
        }
    ];
}
