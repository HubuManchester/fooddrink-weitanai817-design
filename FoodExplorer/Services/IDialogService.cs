namespace FoodExplorer.Services;

public interface IDialogService
{
    Task DisplayAlertAsync(string title, string message, string cancel = "OK");
    Task<bool> DisplayConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No");
}
