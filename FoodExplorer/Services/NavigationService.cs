namespace FoodExplorer.Services;

public class NavigationService : INavigationService
{
    public async Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        if (parameters is null || parameters.Count == 0)
        {
            await Shell.Current.GoToAsync(route);
            return;
        }

        await Shell.Current.GoToAsync(route, parameters);
    }

    public async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
