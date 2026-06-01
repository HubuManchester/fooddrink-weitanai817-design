namespace FoodExplorer.Services;

public interface ISensorService
{
    bool IsGyroscopeSupported { get; }
    bool IsCompassSupported { get; }

    event Action? TiltForward;
    event Action? TiltBackward;
    event Action<double>? HeadingChanged;

    void StartGyroscope();
    void StopGyroscope();
    void StartCompass();
    void StopCompass();
}
