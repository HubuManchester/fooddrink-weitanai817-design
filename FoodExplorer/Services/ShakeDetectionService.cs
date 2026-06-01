namespace FoodExplorer.Services;

/// <summary>
/// Hardware #3 — Accelerometer shake detection.
/// Listens to the device accelerometer and fires <see cref="ShakeDetected"/>
/// when the user shakes the phone, triggering random recipe discovery.
/// Uses a 2-second debounce to prevent repeated triggers.
/// </summary>
public class ShakeDetectionService : IShakeDetectionService
{
    /// <summary>Minimum G-force magnitude above which a shake is registered.</summary>
    private const double ShakeThreshold = 2.8;

    /// <summary>Prevents multiple shake events within this interval.</summary>
    private static readonly TimeSpan DebounceInterval = TimeSpan.FromSeconds(2);

    private DateTime _lastShakeUtc = DateTime.MinValue;
    private bool _isMonitoring;

    public bool IsSupported => Accelerometer.Default.IsSupported;

    public event EventHandler? ShakeDetected;

    /// <summary>Starts listening for shake gestures via the accelerometer.</summary>
    public void StartMonitoring()
    {
        if (_isMonitoring || !IsSupported)
            return;

        Accelerometer.Default.ReadingChanged += OnAccelerometerReading;
        Accelerometer.Default.Start(SensorSpeed.Game);
        _isMonitoring = true;
    }

    /// <summary>Stops accelerometer monitoring and cleans up event handlers.</summary>
    public void StopMonitoring()
    {
        if (!_isMonitoring)
            return;

        Accelerometer.Default.ReadingChanged -= OnAccelerometerReading;
        Accelerometer.Default.Stop();
        _isMonitoring = false;
    }

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
}
