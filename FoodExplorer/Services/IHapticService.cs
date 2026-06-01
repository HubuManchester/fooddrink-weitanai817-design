namespace FoodExplorer.Services;

public interface IHapticService
{
    void PerformClick();
    void PerformSuccess();
    void PerformError();
    Task VibrateAsync(TimeSpan duration);
}
