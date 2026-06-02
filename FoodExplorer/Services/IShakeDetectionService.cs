namespace FoodExplorer.Services;

public interface IShakeDetectionService
{
    bool IsSupported { get; }

    /// <summary>True when shake must be armed manually (Windows mouse fallback).</summary>
    bool SupportsManualShakeArm { get; }

    event EventHandler? ShakeDetected;

    void StartMonitoring();
    void StopMonitoring();

    /// <summary>Arms mouse up/down shake listening (Windows only).</summary>
    void ArmManualShake();

    /// <summary>Cancels armed mouse shake listening (Windows only).</summary>
    void CancelManualShake();
}
