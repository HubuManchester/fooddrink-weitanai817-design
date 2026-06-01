namespace FoodExplorer.Services;

/// <summary>
/// Hardware #7 and #8 — Haptic feedback and Vibration.
/// Uses two distinct APIs:
/// <list type="bullet">
///   <item><description><c>HapticFeedback.Default.Perform</c> — tactile click/success feedback</description></item>
///   <item><description><c>Vibration.Default.Vibrate</c> — error states and shake confirmation</description></item>
/// </list>
/// </summary>
public class HapticService : IHapticService
{
    /// <summary>Light haptic click for button presses and toggles.</summary>
    public void PerformClick()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[HapticService] Click: {ex.Message}"); }
    }

    /// <summary>Long-press haptic for successful actions.</summary>
    public void PerformSuccess()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[HapticService] Success: {ex.Message}"); }
    }

    /// <summary>Vibration pattern for errors and permission denials.</summary>
    public void PerformError()
    {
        try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250)); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[HapticService] Error: {ex.Message}"); }
    }

    /// <summary>Custom-duration vibration (e.g. shake confirmation).</summary>
    public Task VibrateAsync(TimeSpan duration)
    {
        try { Vibration.Default.Vibrate(duration); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[HapticService] Vibrate: {ex.Message}"); }
        return Task.CompletedTask;
    }
}
