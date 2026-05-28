namespace FoodExplorer.Models;

public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FullDescription { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Cuisine { get; set; } = string.Empty;
    public string ImageFileName { get; set; } = string.Empty;
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public int CaloriesPerServing { get; set; }
    public double Rating { get; set; }
    public int RatingCount { get; set; }
    public bool IsFavourite { get; set; }
    public bool IsFeatured { get; set; }
    public List<Ingredient> Ingredients { get; set; } = new();
    public List<string> Steps { get; set; } = new();
    public List<string> DietaryTags { get; set; } = new();

    public int TotalTimeMinutes => PrepTimeMinutes + CookTimeMinutes;

    public string TotalTimeDisplay => TotalTimeMinutes >= 60
        ? $"{TotalTimeMinutes / 60}h {TotalTimeMinutes % 60}m"
        : $"{TotalTimeMinutes}m";

    public string RatingDisplay => $"★ {Rating:F1}";

    public string ImageUri => string.IsNullOrEmpty(ImageFileName)
        ? "dotnet_bot.png"
        : ImageFileName;
}

public class Ingredient
{
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Note { get; set; }

    public string DisplayText => string.IsNullOrEmpty(Unit)
        ? $"{Quantity} {Name}{(Note != null ? $" ({Note})" : "")}"
        : $"{Quantity} {Unit} {Name}{(Note != null ? $" ({Note})" : "")}";
}

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}

public class RecipeCategory
{
    public string Name { get; set; } = string.Empty;
    public string IconEmoji { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = "#FF6B35";
    public int RecipeCount { get; set; }
}

public class RecipeSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ImageUri { get; set; } = string.Empty;
    public string TotalTimeDisplay { get; set; } = string.Empty;
    public double Rating { get; set; }
    public bool IsFavourite { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public string DifficultyDisplay => Difficulty.ToString();

    public static RecipeSummary FromRecipe(Recipe recipe) => new()
    {
        Id = recipe.Id,
        Name = recipe.Name,
        Description = recipe.Description,
        Category = recipe.Category,
        ImageUri = recipe.ImageUri,
        TotalTimeDisplay = recipe.TotalTimeDisplay,
        Rating = recipe.Rating,
        IsFavourite = recipe.IsFavourite,
        Difficulty = recipe.Difficulty
    };
}
