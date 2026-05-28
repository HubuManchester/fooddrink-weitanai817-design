namespace FoodExplorer.Services;

public class DialogService : IDialogService
{
    public Task DisplayAlertAsync(string title, string message, string cancel = "OK")
    {
        var page = Shell.Current?.CurrentPage ?? Application.Current?.MainPage;
        return page is not null
            ? page.DisplayAlert(title, message, cancel)
            : Task.CompletedTask;
    }

    public Task<bool> DisplayConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No")
    {
        var page = Shell.Current?.CurrentPage ?? Application.Current?.MainPage;
        return page is not null
            ? page.DisplayAlert(title, message, accept, cancel)
            : Task.FromResult(false);
    }
}
