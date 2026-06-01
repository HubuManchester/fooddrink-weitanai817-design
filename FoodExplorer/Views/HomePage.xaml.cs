using FoodExplorer.Services;
using FoodExplorer.ViewModels;

namespace FoodExplorer.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;
    private readonly IDeviceLayoutService _deviceLayoutService;

    public HomePage(
        HomeViewModel viewModel,
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
        _viewModel.StartShakeMonitoring();
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        _viewModel.StopShakeMonitoring();
        base.OnDisappearing();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width <= 0)
            return;

        _viewModel.UpdateLayout(width);
        ContentLayout.MaximumWidthRequest = _deviceLayoutService.GetContentMaxWidth(width);
        ContentLayout.HorizontalOptions = _deviceLayoutService.IsTablet(width)
            ? LayoutOptions.Center
            : LayoutOptions.Fill;
    }

    private void ApplyAccessibility()
    {
        if (Application.Current is App app)
            app.ApplyAccessibilityToPage(this);
    }
}
