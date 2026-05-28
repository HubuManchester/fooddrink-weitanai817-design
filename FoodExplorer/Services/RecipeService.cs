using FoodExplorer.Models;

namespace FoodExplorer.Services;

public class RecipeService : IRecipeService
{
    private readonly List<Recipe> _recipes = CreateSampleRecipes();

    public Task<IReadOnlyList<Recipe>> GetAllRecipesAsync() =>
        Task.FromResult<IReadOnlyList<Recipe>>(_recipes);

    public Task<IReadOnlyList<Recipe>> GetFeaturedRecipesAsync() =>
        Task.FromResult<IReadOnlyList<Recipe>>(_recipes.Where(r => r.IsFeatured).ToList());

    public Task<IReadOnlyList<RecipeCategory>> GetCategoriesAsync()
    {
        var categories = _recipes
            .GroupBy(r => r.Category)
            .Select(g => new RecipeCategory
            {
                Name = g.Key,
                IconEmoji = GetCategoryEmoji(g.Key),
                BackgroundColor = GetCategoryColor(g.Key),
                RecipeCount = g.Count()
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<RecipeCategory>>(categories);
    }

    public Task<Recipe?> GetRecipeByIdAsync(int id) =>
        Task.FromResult(_recipes.FirstOrDefault(r => r.Id == id));

    public Task<IReadOnlyList<RecipeSummary>> GetRecipeSummariesAsync() =>
        Task.FromResult<IReadOnlyList<RecipeSummary>>(
            _recipes.Select(RecipeSummary.FromRecipe).ToList());

    private static string GetCategoryEmoji(string category) => category switch
    {
        "Italian" => "🍝",
        "Asian" => "🍜",
        "Mexican" => "🌮",
        "Dessert" => "🍰",
        "Healthy" => "🥗",
        _ => "🍽️"
    };

    private static string GetCategoryColor(string category) => category switch
    {
        "Italian" => "#FF6B35",
        "Asian" => "#2EC4B6",
        "Mexican" => "#FFD166",
        "Dessert" => "#EF476F",
        "Healthy" => "#06D6A0",
        _ => "#FF6B35"
    };

    private static List<Recipe> CreateSampleRecipes() =>
    [
        new Recipe
        {
            Id = 1,
            Name = "Classic Margherita Pizza",
            Description = "A timeless Italian pizza with fresh basil and mozzarella.",
            FullDescription = "Originating from Naples, this pizza celebrates simplicity with quality ingredients and a blistered crust.",
            Category = "Italian",
            Cuisine = "Italian",
            ImageFileName = "dotnet_bot.png",
            PrepTimeMinutes = 20,
            CookTimeMinutes = 15,
            Servings = 4,
            Difficulty = DifficultyLevel.Medium,
            CaloriesPerServing = 280,
            Rating = 4.8,
            RatingCount = 324,
            IsFeatured = true,
            DietaryTags = ["Vegetarian"],
            Ingredients =
            [
                new Ingredient { Quantity = 1, Unit = "ball", Name = "pizza dough" },
                new Ingredient { Quantity = 200, Unit = "g", Name = "mozzarella", Note = "fresh" },
                new Ingredient { Quantity = 4, Unit = "tbsp", Name = "tomato sauce" },
                new Ingredient { Quantity = 10, Unit = "leaves", Name = "fresh basil" }
            ],
            Steps =
            [
                "Preheat oven to 250°C with a pizza stone if available.",
                "Roll out the dough on a floured surface.",
                "Spread tomato sauce evenly, leaving a border.",
                "Add torn mozzarella and bake for 12–15 minutes.",
                "Top with fresh basil and drizzle with olive oil."
            ]
        },
        new Recipe
        {
            Id = 2,
            Name = "Chicken Ramen Bowl",
            Description = "Rich broth, tender chicken, and springy noodles.",
            FullDescription = "A comforting Japanese-style ramen with a savoury chicken broth and classic toppings.",
            Category = "Asian",
            Cuisine = "Japanese",
            ImageFileName = "dotnet_bot.png",
            PrepTimeMinutes = 25,
            CookTimeMinutes = 40,
            Servings = 2,
            Difficulty = DifficultyLevel.Hard,
            CaloriesPerServing = 520,
            Rating = 4.6,
            RatingCount = 198,
            IsFeatured = true,
            DietaryTags = ["Dairy-Free"],
            Ingredients =
            [
                new Ingredient { Quantity = 2, Unit = "portions", Name = "ramen noodles" },
                new Ingredient { Quantity = 500, Unit = "ml", Name = "chicken stock" },
                new Ingredient { Quantity = 2, Unit = "", Name = "chicken thighs", Note = "boneless" },
                new Ingredient { Quantity = 2, Unit = "", Name = "soft-boiled eggs" }
            ],
            Steps =
            [
                "Simmer chicken stock with ginger and garlic for 30 minutes.",
                "Season broth with soy sauce and sesame oil.",
                "Cook noodles according to package instructions.",
                "Pan-sear chicken until golden and slice.",
                "Assemble bowls with noodles, broth, chicken, and eggs."
            ]
        },
        new Recipe
        {
            Id = 3,
            Name = "Street-Style Tacos",
            Description = "Spiced beef tacos with lime, onion, and cilantro.",
            FullDescription = "Inspired by Mexican street vendors — quick, bold flavours in every bite.",
            Category = "Mexican",
            Cuisine = "Mexican",
            ImageFileName = "dotnet_bot.png",
            PrepTimeMinutes = 15,
            CookTimeMinutes = 10,
            Servings = 4,
            Difficulty = DifficultyLevel.Easy,
            CaloriesPerServing = 340,
            Rating = 4.7,
            RatingCount = 412,
            IsFeatured = true,
            DietaryTags = ["Gluten-Free"],
            Ingredients =
            [
                new Ingredient { Quantity = 400, Unit = "g", Name = "ground beef" },
                new Ingredient { Quantity = 8, Unit = "", Name = "corn tortillas" },
                new Ingredient { Quantity = 1, Unit = "", Name = "lime", Note = "cut into wedges" },
                new Ingredient { Quantity = 1, Unit = "bunch", Name = "fresh cilantro" }
            ],
            Steps =
            [
                "Brown beef with cumin, paprika, and salt.",
                "Warm tortillas in a dry pan.",
                "Fill each tortilla with beef.",
                "Top with diced onion, cilantro, and a squeeze of lime."
            ]
        },
        new Recipe
        {
            Id = 4,
            Name = "Berry Cheesecake",
            Description = "Creamy no-bake cheesecake topped with mixed berries.",
            FullDescription = "A light dessert perfect for summer gatherings — no oven required.",
            Category = "Dessert",
            Cuisine = "American",
            ImageFileName = "dotnet_bot.png",
            PrepTimeMinutes = 30,
            CookTimeMinutes = 0,
            Servings = 8,
            Difficulty = DifficultyLevel.Medium,
            CaloriesPerServing = 380,
            Rating = 4.9,
            RatingCount = 567,
            IsFeatured = false,
            DietaryTags = ["Vegetarian"],
            Ingredients =
            [
                new Ingredient { Quantity = 250, Unit = "g", Name = "cream cheese" },
                new Ingredient { Quantity = 200, Unit = "g", Name = "digestive biscuits" },
                new Ingredient { Quantity = 300, Unit = "g", Name = "mixed berries" },
                new Ingredient { Quantity = 80, Unit = "g", Name = "butter", Note = "melted" }
            ],
            Steps =
            [
                "Crush biscuits and mix with melted butter.",
                "Press into a springform tin and chill.",
                "Beat cream cheese with sugar until smooth.",
                "Spread over the base and refrigerate 4 hours.",
                "Top with fresh berries before serving."
            ]
        },
        new Recipe
        {
            Id = 5,
            Name = "Quinoa Buddha Bowl",
            Description = "Colourful bowl packed with protein, greens, and tahini dressing.",
            FullDescription = "A nourishing plant-forward meal that's as beautiful as it is satisfying.",
            Category = "Healthy",
            Cuisine = "Mediterranean",
            ImageFileName = "dotnet_bot.png",
            PrepTimeMinutes = 20,
            CookTimeMinutes = 15,
            Servings = 2,
            Difficulty = DifficultyLevel.Easy,
            CaloriesPerServing = 420,
            Rating = 4.5,
            RatingCount = 143,
            IsFeatured = false,
            DietaryTags = ["Vegan", "Gluten-Free"],
            Ingredients =
            [
                new Ingredient { Quantity = 1, Unit = "cup", Name = "quinoa" },
                new Ingredient { Quantity = 1, Unit = "can", Name = "chickpeas", Note = "drained" },
                new Ingredient { Quantity = 2, Unit = "cups", Name = "mixed greens" },
                new Ingredient { Quantity = 3, Unit = "tbsp", Name = "tahini" }
            ],
            Steps =
            [
                "Cook quinoa according to package directions.",
                "Roast chickpeas with olive oil and spices.",
                "Arrange greens, quinoa, and chickpeas in a bowl.",
                "Drizzle with tahini-lemon dressing."
            ]
        },
        new Recipe
        {
            Id = 6,
            Name = "Spaghetti Carbonara",
            Description = "Silky egg sauce with crispy pancetta and pecorino.",
            FullDescription = "The authentic Roman recipe — no cream, just technique and great ingredients.",
            Category = "Italian",
            Cuisine = "Italian",
            ImageFileName = "dotnet_bot.png",
            PrepTimeMinutes = 10,
            CookTimeMinutes = 15,
            Servings = 4,
            Difficulty = DifficultyLevel.Medium,
            CaloriesPerServing = 450,
            Rating = 4.7,
            RatingCount = 289,
            IsFeatured = false,
            DietaryTags = [],
            Ingredients =
            [
                new Ingredient { Quantity = 400, Unit = "g", Name = "spaghetti" },
                new Ingredient { Quantity = 200, Unit = "g", Name = "pancetta" },
                new Ingredient { Quantity = 4, Unit = "", Name = "egg yolks" },
                new Ingredient { Quantity = 80, Unit = "g", Name = "pecorino cheese", Note = "grated" }
            ],
            Steps =
            [
                "Cook spaghetti in salted boiling water.",
                "Crisp pancetta in a pan over medium heat.",
                "Whisk egg yolks with pecorino and pepper.",
                "Toss hot pasta with pancetta, then off heat mix in egg mixture.",
                "Serve immediately with extra cheese."
            ]
        }
    ];
}
