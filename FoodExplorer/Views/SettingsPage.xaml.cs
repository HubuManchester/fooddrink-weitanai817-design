using FoodExplorer.Services;
using FoodExplorer.ViewModels;

namespace FoodExplorer.Views;

public partial class SettingsPage : ContentPage
{
    private readonly IDeviceLayoutService _deviceLayoutService;

    public SettingsPage(
        SettingsViewModel viewModel,
        IDeviceLayoutService deviceLayoutService)
    {
        InitializeComponent();
        _deviceLayoutService = deviceLayoutService;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (Application.Current is App app)
            app.ApplyAccessibilityToPage(this);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width <= 0)
            return;

        ContentLayout.MaximumWidthRequest = _deviceLayoutService.GetContentMaxWidth(width);
        ContentLayout.HorizontalOptions = _deviceLayoutService.IsTablet(width)
            ? LayoutOptions.Center
            : LayoutOptions.Fill;
    }
}
