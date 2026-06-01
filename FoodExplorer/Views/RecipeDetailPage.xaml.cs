using FoodExplorer.ViewModels;

namespace FoodExplorer.Views;

public partial class RecipeDetailPage : ContentPage
{
    private readonly RecipeDetailViewModel _viewModel;

    public RecipeDetailPage(RecipeDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.StartHardwareFeatures();
    }

    protected override void OnDisappearing()
    {
        _viewModel.StopHardwareFeatures();
        base.OnDisappearing();
    }
}
