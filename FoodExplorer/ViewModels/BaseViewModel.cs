using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodExplorer.Services;

namespace FoodExplorer.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    protected readonly INavigationService NavigationService;
    protected readonly IDialogService DialogService;

    protected BaseViewModel(INavigationService navigationService, IDialogService dialogService)
    {
        NavigationService = navigationService;
        DialogService = dialogService;
    }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    protected async Task ExecuteAsync(Func<Task> action, string? errorMessage = null)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;
            await action();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = errorMessage ?? "Something went wrong. Please try again.";
            System.Diagnostics.Debug.WriteLine($"[FoodExplorer] {ex}");
            await DialogService.DisplayAlertAsync("Error", ErrorMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
