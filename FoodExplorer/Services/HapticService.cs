namespace FoodExplorer.Services;

public class HapticService : IHapticService
{
    public void PerformClick()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[HapticService] Click: {ex.Message}"); }
    }

    public void PerformSuccess()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[HapticService] Success: {ex.Message}"); }
    }

    public void PerformError()
    {
        try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250)); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[HapticService] Error: {ex.Message}"); }
    }

    public Task VibrateAsync(TimeSpan duration)
    {
        try { Vibration.Default.Vibrate(duration); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[HapticService] Vibrate: {ex.Message}"); }
        return Task.CompletedTask;
    }
}
