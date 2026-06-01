using FoodExplorer.ViewModels;

namespace FoodExplorer.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.StartShakeMonitoring();
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        _viewModel.StopShakeMonitoring();
        base.OnDisappearing();
    }
}
