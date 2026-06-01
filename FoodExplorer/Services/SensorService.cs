namespace FoodExplorer.Services;

public class SensorService : ISensorService
{
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

    public void StartGyroscope()
    {
        if (_gyroActive || !IsGyroscopeSupported)
            return;

        Gyroscope.Default.ReadingChanged += OnGyroscopeReading;
        Gyroscope.Default.Start(SensorSpeed.UI);
        _gyroActive = true;
    }

    public void StopGyroscope()
    {
        if (!_gyroActive)
            return;

        Gyroscope.Default.ReadingChanged -= OnGyroscopeReading;
        Gyroscope.Default.Stop();
        _gyroActive = false;
    }

    public void StartCompass()
    {
        if (_compassActive || !IsCompassSupported)
            return;

        Compass.Default.ReadingChanged += OnCompassReading;
        Compass.Default.Start(SensorSpeed.UI);
        _compassActive = true;
    }

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
