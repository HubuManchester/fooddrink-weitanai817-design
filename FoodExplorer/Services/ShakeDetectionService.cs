namespace FoodExplorer.Services;

/// <summary>
/// Hardware #3 — Accelerometer shake detection.
/// Android/mobile uses MAUI Accelerometer; Windows uses WinRT accelerometer or armed mouse shake.
/// </summary>
public class ShakeDetectionService : IShakeDetectionService
{
#if WINDOWS
    private readonly Platforms.Windows.WindowsShakeHelper _windowsShake = new();
#endif

    private const double ShakeThreshold = 2.8;
    private static readonly TimeSpan DebounceInterval = TimeSpan.FromSeconds(2);

    private DateTime _lastShakeUtc = DateTime.MinValue;
#if !WINDOWS
    private bool _isMonitoring;
#endif

    public bool IsSupported
    {
        get
        {
#if WINDOWS
            return _windowsShake.IsSupported;
#else
            return Accelerometer.Default.IsSupported;
#endif
        }
    }

    public bool SupportsManualShakeArm
    {
        get
        {
#if WINDOWS
            return _windowsShake.UsesManualArm;
#else
            return false;
#endif
        }
    }

    public event EventHandler? ShakeDetected;

#if WINDOWS
    public ShakeDetectionService()
    {
        _windowsShake.ShakeDetected += (_, _) => ShakeDetected?.Invoke(this, EventArgs.Empty);
    }
#endif

    public void StartMonitoring()
    {
#if WINDOWS
        _windowsShake.StartMonitoring();
#else
        if (_isMonitoring || !IsSupported)
            return;

        Accelerometer.Default.ReadingChanged += OnAccelerometerReading;
        Accelerometer.Default.Start(SensorSpeed.Game);
        _isMonitoring = true;
#endif
    }

    public void StopMonitoring()
    {
#if WINDOWS
        _windowsShake.StopMonitoring();
#else
        if (!_isMonitoring)
            return;

        Accelerometer.Default.ReadingChanged -= OnAccelerometerReading;
        Accelerometer.Default.Stop();
        _isMonitoring = false;
#endif
    }

    public void ArmManualShake()
    {
#if WINDOWS
        _windowsShake.ArmManualShake();
#endif
    }

    public void CancelManualShake()
    {
#if WINDOWS
        _windowsShake.CancelManualShake();
#endif
    }

#if !WINDOWS
    private void OnAccelerometerReading(object? sender, AccelerometerChangedEventArgs e)
    {
        var data = e.Reading;
        var delta = Math.Abs(data.Acceleration.X) + Math.Abs(data.Acceleration.Y) + Math.Abs(data.Acceleration.Z);
        var magnitude = Math.Abs(delta - 1.0);

        if (magnitude <= ShakeThreshold)
            return;

        var now = DateTime.UtcNow;
        if (now - _lastShakeUtc < DebounceInterval)
            return;

        _lastShakeUtc = now;
        ShakeDetected?.Invoke(this, EventArgs.Empty);
    }
#endif
}
