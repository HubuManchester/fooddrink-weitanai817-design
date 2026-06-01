namespace FoodExplorer.Services;

public class ShakeDetectionService : IShakeDetectionService
{
    private const double ShakeThreshold = 2.8;
    private static readonly TimeSpan DebounceInterval = TimeSpan.FromSeconds(2);

    private DateTime _lastShakeUtc = DateTime.MinValue;
    private bool _isMonitoring;

    public bool IsSupported => Accelerometer.Default.IsSupported;

    public event EventHandler? ShakeDetected;

    public void StartMonitoring()
    {
        if (_isMonitoring || !IsSupported)
            return;

        Accelerometer.Default.ReadingChanged += OnAccelerometerReading;
        Accelerometer.Default.Start(SensorSpeed.Game);
        _isMonitoring = true;
    }

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
