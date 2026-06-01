namespace FoodExplorer.Services;

public interface IShakeDetectionService
{
    bool IsSupported { get; }
    event EventHandler? ShakeDetected;
    void StartMonitoring();
    void StopMonitoring();
}
