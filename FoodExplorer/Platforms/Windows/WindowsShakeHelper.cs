using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using WinUiWindow = Microsoft.UI.Xaml.Window;
using WinAccelerometer = Windows.Devices.Sensors.Accelerometer;
using WinPoint = Windows.Foundation.Point;

namespace FoodExplorer.Platforms.Windows;

/// <summary>
/// Shake via WinRT accelerometer, or armed vertical mouse movement on desktops.
/// </summary>
internal sealed class WindowsShakeHelper
{
    private const double ShakeThreshold = 2.8;
    private static readonly TimeSpan DebounceInterval = TimeSpan.FromSeconds(2);
    private const double MinVerticalSegmentPixels = 35;
    private const int RequiredDirectionChanges = 3;

    private WinAccelerometer? _accelerometer;
    private WinUiWindow? _window;
    private DateTime _lastShakeUtc = DateTime.MinValue;
    private WinPoint _lastPointer;
    private bool _hasPointerBaseline;
    private bool _monitoring;
    private bool _armed;
    private bool _usesManualArm;
    private int _verticalDirectionChanges;
    private int _lastVerticalDirection;
    private double _segmentStartY;

    public bool IsSupported => true;
    public bool UsesManualArm => _usesManualArm;

    public event EventHandler? ShakeDetected;

    public void StartMonitoring()
    {
        if (_monitoring)
            return;

        _monitoring = true;
        _accelerometer = WinAccelerometer.GetDefault();

        if (_accelerometer is not null)
        {
            _usesManualArm = false;
            _accelerometer.ReadingChanged += OnAccelerometerReading;
            _accelerometer.ReportInterval = Math.Max(_accelerometer.MinimumReportInterval, 16);
            return;
        }

        _usesManualArm = true;
        _window = GetActiveWindow();
    }

    public void StopMonitoring()
    {
        if (!_monitoring)
            return;

        _monitoring = false;
        CancelManualShake();

        if (_accelerometer is not null)
        {
            _accelerometer.ReadingChanged -= OnAccelerometerReading;
            _accelerometer = null;
        }

        _window = null;
        _usesManualArm = false;
    }

    public void ArmManualShake()
    {
        if (!_monitoring || !_usesManualArm || _window?.Content is not UIElement root)
            return;

        CancelManualShake();
        _armed = true;
        _verticalDirectionChanges = 0;
        _lastVerticalDirection = 0;
        _hasPointerBaseline = false;
        _lastPointer = default;
        root.PointerMoved += OnPointerMoved;
    }

    public void CancelManualShake()
    {
        _armed = false;
        _verticalDirectionChanges = 0;
        _lastVerticalDirection = 0;
        _hasPointerBaseline = false;

        if (_window?.Content is UIElement root)
            root.PointerMoved -= OnPointerMoved;
    }

    private void OnAccelerometerReading(WinAccelerometer sender, global::Windows.Devices.Sensors.AccelerometerReadingChangedEventArgs args)
    {
        var data = args.Reading;
        var delta = Math.Abs(data.AccelerationX) + Math.Abs(data.AccelerationY) + Math.Abs(data.AccelerationZ);
        var magnitude = Math.Abs(delta - 1.0);

        if (magnitude > ShakeThreshold)
            RaiseShakeDetected();
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_armed)
            return;

        var point = e.GetCurrentPoint((UIElement)sender).Position;
        var dy = point.Y - _lastPointer.Y;

        if (!_hasPointerBaseline)
        {
            _lastPointer = point;
            _segmentStartY = point.Y;
            _hasPointerBaseline = true;
            return;
        }

        if (Math.Abs(dy) < 4)
            return;

        var direction = dy < 0 ? -1 : 1;

        if (_lastVerticalDirection != 0 && direction != _lastVerticalDirection)
        {
            if (Math.Abs(point.Y - _segmentStartY) >= MinVerticalSegmentPixels)
            {
                _verticalDirectionChanges++;
                _segmentStartY = point.Y;

                if (_verticalDirectionChanges >= RequiredDirectionChanges)
                {
                    CancelManualShake();
                    RaiseShakeDetected();
                }
            }
        }

        if (_lastVerticalDirection == 0)
            _segmentStartY = point.Y;

        _lastVerticalDirection = direction;
        _lastPointer = point;
    }

    private void RaiseShakeDetected()
    {
        var now = DateTime.UtcNow;
        if (now - _lastShakeUtc < DebounceInterval)
            return;

        _lastShakeUtc = now;
        ShakeDetected?.Invoke(this, EventArgs.Empty);
    }

    private static WinUiWindow? GetActiveWindow()
    {
        var mauiWindow = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        return mauiWindow?.Handler?.PlatformView as WinUiWindow;
    }
}
