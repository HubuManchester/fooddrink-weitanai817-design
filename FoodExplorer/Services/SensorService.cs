namespace FoodExplorer.Services;

/// <summary>
/// Hardware #5 and #6 — Gyroscope and Compass integration.
/// Gyroscope tilt is mapped to recipe step navigation (forward/backward).
/// Compass provides real-time magnetic heading display on the recipe detail page.
/// </summary>
public class SensorService : ISensorService
{
    /// <summary>Angular velocity threshold (rad/s) before a tilt event is raised.</summary>
    private const double GyroThreshold = 1.2;

    private static readonly TimeSpan GyroDebounce = TimeSpan.FromMilliseconds(900);

    private DateTime _lastGyroEventUtc = DateTime.MinValue;
    private bool _gyroActive;
    private bool _compassActive;

    public bool IsGyroscopeSupported => Gyroscope.Default.IsSupported;
    public bool IsCompassSupported => Compass.Default.IsSupported;

    public event Action? TiltForward;
    public event Action? TiltBackward;
    public event Action<double>? HeadingChanged;

    /// <summary>Starts gyroscope monitoring for tilt-based step navigation.</summary>
    public void StartGyroscope()
    {
        if (_gyroActive || !IsGyroscopeSupported)
            return;

        Gyroscope.Default.ReadingChanged += OnGyroscopeReading;
        Gyroscope.Default.Start(SensorSpeed.UI);
        _gyroActive = true;
    }

    /// <summary>Stops gyroscope monitoring.</summary>
    public void StopGyroscope()
    {
        if (!_gyroActive)
            return;

        Gyroscope.Default.ReadingChanged -= OnGyroscopeReading;
        Gyroscope.Default.Stop();
        _gyroActive = false;
    }

    /// <summary>Starts compass monitoring for magnetic heading display.</summary>
    public void StartCompass()
    {
        if (_compassActive || !IsCompassSupported)
            return;

        Compass.Default.ReadingChanged += OnCompassReading;
        Compass.Default.Start(SensorSpeed.UI);
        _compassActive = true;
    }

    /// <summary>Stops compass monitoring.</summary>
    public void StopCompass()
    {
        if (!_compassActive)
            return;

        Compass.Default.ReadingChanged -= OnCompassReading;
        Compass.Default.Stop();
        _compassActive = false;
    }

    private void OnGyroscopeReading(object? sender, GyroscopeChangedEventArgs e)
    {
        var angularX = e.Reading.AngularVelocity.X;
        if (Math.Abs(angularX) < GyroThreshold)
            return;

        var now = DateTime.UtcNow;
        if (now - _lastGyroEventUtc < GyroDebounce)
            return;

        _lastGyroEventUtc = now;

        if (angularX > 0)
            TiltForward?.Invoke();
        else
            TiltBackward?.Invoke();
    }

    private void OnCompassReading(object? sender, CompassChangedEventArgs e)
    {
        HeadingChanged?.Invoke(e.Reading.HeadingMagneticNorth);
    }
}
