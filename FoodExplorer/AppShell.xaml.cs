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
            Icon = "dotnet_bot.png",
            Route = "HomePage",
            Content = homePage
        });

        tabBar.Items.Add(new ShellContent
        {
            Title = "Recipes",
            Icon = "dotnet_bot.png",
            Route = "RecipeListPage",
            Content = recipeListPage
        });

        tabBar.Items.Add(new ShellContent
        {
            Title = "Settings",
            Icon = "dotnet_bot.png",
            Route = "SettingsPage",
            Content = settingsPage
        });

        Items.Add(tabBar);
    }
}
