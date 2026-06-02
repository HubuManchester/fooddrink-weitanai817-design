namespace FoodExplorer.Services;

/// <summary>
/// Hardware #5 and #6 — Gyroscope and Compass integration.
/// Mobile uses MAUI sensors; Windows uses WinRT sensors with mouse-wheel fallback for tilt.
/// </summary>
public class SensorService : ISensorService
{
#if WINDOWS
    private readonly Platforms.Windows.WindowsMotionSensorHelper _windowsMotion = new();
#endif

    private const double GyroThreshold = 1.2;
    private static readonly TimeSpan GyroDebounce = TimeSpan.FromMilliseconds(900);

    private DateTime _lastGyroEventUtc = DateTime.MinValue;
#if !WINDOWS
    private bool _gyroActive;
    private bool _compassActive;
#endif

    public bool IsGyroscopeSupported
    {
        get
        {
#if WINDOWS
            return _windowsMotion.IsGyroscopeSupported;
#else
            return Gyroscope.Default.IsSupported;
#endif
        }
    }

    public bool IsCompassSupported
    {
        get
        {
#if WINDOWS
            return _windowsMotion.IsCompassSupported;
#else
            return Compass.Default.IsSupported;
#endif
        }
    }

    public event Action? TiltForward;
    public event Action? TiltBackward;
    public event Action<double>? HeadingChanged;

#if WINDOWS
    public SensorService()
    {
        _windowsMotion.TiltForward += () => TiltForward?.Invoke();
        _windowsMotion.TiltBackward += () => TiltBackward?.Invoke();
        _windowsMotion.HeadingChanged += heading => HeadingChanged?.Invoke(heading);
    }
#endif

    public void StartGyroscope()
    {
#if WINDOWS
        _windowsMotion.StartGyroscope();
#else
        if (_gyroActive || !IsGyroscopeSupported)
            return;

        Gyroscope.Default.ReadingChanged += OnGyroscopeReading;
        Gyroscope.Default.Start(SensorSpeed.UI);
        _gyroActive = true;
#endif
    }

    public void StopGyroscope()
    {
#if WINDOWS
        _windowsMotion.StopGyroscope();
#else
        if (!_gyroActive)
            return;

        Gyroscope.Default.ReadingChanged -= OnGyroscopeReading;
        Gyroscope.Default.Stop();
        _gyroActive = false;
#endif
    }

    public void StartCompass()
    {
#if WINDOWS
        _windowsMotion.StartCompass();
#else
        if (_compassActive || !IsCompassSupported)
            return;

        Compass.Default.ReadingChanged += OnCompassReading;
        Compass.Default.Start(SensorSpeed.UI);
        _compassActive = true;
#endif
    }

    public void StopCompass()
    {
#if WINDOWS
        _windowsMotion.StopCompass();
#else
        if (!_compassActive)
            return;

        Compass.Default.ReadingChanged -= OnCompassReading;
        Compass.Default.Stop();
        _compassActive = false;
#endif
    }

#if !WINDOWS
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
#endif
}
