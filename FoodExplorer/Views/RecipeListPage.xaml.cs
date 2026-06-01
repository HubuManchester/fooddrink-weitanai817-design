using FoodExplorer.Services;
using FoodExplorer.ViewModels;

namespace FoodExplorer.Views;

public partial class RecipeListPage : ContentPage
{
    private readonly RecipeListViewModel _viewModel;
    private readonly IDeviceLayoutService _deviceLayoutService;

    public RecipeListPage(
        RecipeListViewModel viewModel,
        IDeviceLayoutService deviceLayoutService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _deviceLayoutService = deviceLayoutService;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ApplyAccessibility();
        await _viewModel.LoadRecipesCommand.ExecuteAsync(null);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width <= 0)
            return;

        _viewModel.UpdateLayout(width);
        ContentGrid.MaximumWidthRequest = _deviceLayoutService.GetContentMaxWidth(width);
        ContentGrid.HorizontalOptions = _deviceLayoutService.IsTablet(width)
            ? LayoutOptions.Center
            : LayoutOptions.Fill;

        if (RecipeCollection.ItemsLayout is GridItemsLayout gridLayout)
            gridLayout.Span = _viewModel.GridSpan;
    }

    private void ApplyAccessibility()
    {
        if (Application.Current is App app)
            app.ApplyAccessibilityToPage(this);
    }

    private void OnSearchBarSearchButtonPressed(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_viewModel.SearchQuery))
            _viewModel.ShowValidationMessage("Please enter a search term.");
    }
}
