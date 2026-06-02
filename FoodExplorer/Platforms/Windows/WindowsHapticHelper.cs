namespace FoodExplorer.Platforms.Windows;

internal static class WindowsHapticHelper
{
    public static void PerformClick()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[WindowsHapticHelper] Click: {ex.Message}"); }
    }

    public static void PerformSuccess()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[WindowsHapticHelper] Success: {ex.Message}"); }
    }

    public static void PerformError()
    {
        try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250)); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[WindowsHapticHelper] Error: {ex.Message}"); }
    }

    public static Task VibrateAsync(TimeSpan duration)
    {
        try { Vibration.Default.Vibrate(duration); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[WindowsHapticHelper] Vibrate: {ex.Message}"); }
        return Task.CompletedTask;
    }
}
