using FoodExplorer.ViewModels;

namespace FoodExplorer.Views;

public partial class RecipeListPage : ContentPage
{
    private readonly RecipeListViewModel _viewModel;

    public RecipeListPage(RecipeListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadRecipesCommand.ExecuteAsync(null);
    }
}
