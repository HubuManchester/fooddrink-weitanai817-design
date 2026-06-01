using FoodExplorer.Views;

namespace FoodExplorer;

public partial class AppShell : Shell
{
    public AppShell(HomePage homePage, RecipeListPage recipeListPage, SettingsPage settingsPage)
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(RecipeDetailPage), typeof(RecipeDetailPage));

        var tabBar = new TabBar();

        tabBar.Items.Add(new ShellContent
        {
            Title = "Home",
            Icon = "tab_home.svg",
            Route = "HomePage",
            Content = homePage
        });

        tabBar.Items.Add(new ShellContent
        {
            Title = "Recipes",
            Icon = "tab_recipes.svg",
            Route = "RecipeListPage",
            Content = recipeListPage
        });

        tabBar.Items.Add(new ShellContent
        {
            Title = "Settings",
            Icon = "tab_settings.svg",
            Route = "SettingsPage",
            Content = settingsPage
        });

        Items.Add(tabBar);
    }
}
