namespace FoodExplorer.Services;

/// <summary>
/// Hardware #7 and #8 — Haptic feedback and Vibration.
/// Uses platform-specific feedback on Windows and MAUI Essentials elsewhere.
/// </summary>
public class HapticService : IHapticService
{
    public void PerformClick()
    {
#if WINDOWS
        Platforms.Windows.WindowsHapticHelper.PerformClick();
#else
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[HapticService] Click: {ex.Message}"); }
#endif
    }

    public void PerformSuccess()
    {
#if WINDOWS
        Platforms.Windows.WindowsHapticHelper.PerformSuccess();
#else
        try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[HapticService] Success: {ex.Message}"); }
#endif
    }

    public void PerformError()
    {
#if WINDOWS
        Platforms.Windows.WindowsHapticHelper.PerformError();
#else
        try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250)); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[HapticService] Error: {ex.Message}"); }
#endif
    }

    public Task VibrateAsync(TimeSpan duration)
    {
#if WINDOWS
        return Platforms.Windows.WindowsHapticHelper.VibrateAsync(duration);
#else
        try { Vibration.Default.Vibrate(duration); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[HapticService] Vibrate: {ex.Message}"); }
        return Task.CompletedTask;
#endif
    }
}
