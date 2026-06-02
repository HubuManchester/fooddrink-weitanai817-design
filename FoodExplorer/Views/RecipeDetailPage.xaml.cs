using FoodExplorer.Services;
using FoodExplorer.ViewModels;

namespace FoodExplorer.Views;

public partial class RecipeDetailPage : ContentPage
{
    private readonly RecipeDetailViewModel _viewModel;
    private readonly IDeviceLayoutService _deviceLayoutService;

    public RecipeDetailPage(
        RecipeDetailViewModel viewModel,
        IDeviceLayoutService deviceLayoutService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _deviceLayoutService = deviceLayoutService;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Shell.SetTabBarIsVisible(this, false);
        Shell.SetNavBarIsVisible(this, true);
        ApplyAccessibility();
        _viewModel.StartHardwareFeatures();
    }

    protected override bool OnBackButtonPressed()
    {
        _ = _viewModel.GoBackCommand.ExecuteAsync(null);
        return true;
    }

    protected override async void OnDisappearing()
    {
        try
        {
            await _viewModel.StopHardwareFeaturesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeDetailPage] OnDisappearing: {ex}");
        }

        base.OnDisappearing();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width <= 0)
            return;

        _viewModel.UpdateLayout(width);
        DetailContent.MaximumWidthRequest = _deviceLayoutService.GetContentMaxWidth(width);
        DetailContent.HorizontalOptions = _deviceLayoutService.IsTablet(width)
            ? LayoutOptions.Center
            : LayoutOptions.Fill;
    }

    private void ApplyAccessibility()
    {
        if (Application.Current is App app)
            app.ApplyAccessibilityToPage(this);
    }
}
