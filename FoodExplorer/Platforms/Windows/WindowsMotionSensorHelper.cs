using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.Devices.Sensors;
using WinUiWindow = Microsoft.UI.Xaml.Window;
using WinCompass = Windows.Devices.Sensors.Compass;
using WinMagnetometer = Windows.Devices.Sensors.Magnetometer;

namespace FoodExplorer.Platforms.Windows;

/// <summary>
/// Tilt and compass via WinRT sensors, with mouse-wheel fallback for step navigation on desktops.
/// </summary>
internal sealed class WindowsMotionSensorHelper
{
    private const double InclinometerDeltaThreshold = 8.0;
    private static readonly TimeSpan GyroDebounce = TimeSpan.FromMilliseconds(900);

    private Inclinometer? _inclinometer;
    private WinCompass? _compass;
    private WinMagnetometer? _magnetometer;
    private WinUiWindow? _window;
    private WinUiWindow? _compassWindow;
    private bool _usingPointerCompass;
    private double _lastPitch;
    private bool _hasInclinometerBaseline;
    private DateTime _lastGyroEventUtc = DateTime.MinValue;
    private bool _gyroActive;
    private bool _compassActive;

    public bool IsGyroscopeSupported => true;
    public bool IsCompassSupported => true;

    public event Action? TiltForward;
    public event Action? TiltBackward;
    public event Action<double>? HeadingChanged;

    public void StartGyroscope()
    {
        if (_gyroActive)
            return;

        _gyroActive = true;
        _inclinometer = Inclinometer.GetDefault();

        if (_inclinometer is not null)
        {
            _inclinometer.ReadingChanged += OnInclinometerReading;
            _inclinometer.ReportInterval = Math.Max(_inclinometer.MinimumReportInterval, 16);
            _hasInclinometerBaseline = false;
            return;
        }

        _window = GetActiveWindow();
        if (_window?.Content is UIElement root)
            root.PointerWheelChanged += OnPointerWheelChanged;
    }

    public void StopGyroscope()
    {
        if (!_gyroActive)
            return;

        _gyroActive = false;

        if (_inclinometer is not null)
        {
            _inclinometer.ReadingChanged -= OnInclinometerReading;
            _inclinometer = null;
        }

        if (_window?.Content is UIElement root)
            root.PointerWheelChanged -= OnPointerWheelChanged;

        _window = null;
    }

    public void StartCompass()
    {
        if (_compassActive)
            return;

        _compassActive = true;
        _compass = WinCompass.GetDefault();
        _magnetometer = WinMagnetometer.GetDefault();

        if (_compass is not null)
        {
            _compass.ReadingChanged += OnCompassReading;
            _compass.ReportInterval = Math.Max(_compass.MinimumReportInterval, 16);
            return;
        }

        if (_magnetometer is not null)
        {
            _magnetometer.ReadingChanged += OnMagnetometerReading;
            _magnetometer.ReportInterval = Math.Max(_magnetometer.MinimumReportInterval, 16);
            return;
        }

        _compassWindow = GetActiveWindow();
        if (_compassWindow?.Content is UIElement compassRoot)
        {
            compassRoot.PointerMoved += OnPointerMovedForHeading;
            _usingPointerCompass = true;
        }
    }

    public void StopCompass()
    {
        if (!_compassActive)
            return;

        _compassActive = false;

        if (_compass is not null)
        {
            _compass.ReadingChanged -= OnCompassReading;
            _compass = null;
        }

        if (_magnetometer is not null)
        {
            _magnetometer.ReadingChanged -= OnMagnetometerReading;
            _magnetometer = null;
        }

        if (_usingPointerCompass && _compassWindow?.Content is UIElement compassRoot)
            compassRoot.PointerMoved -= OnPointerMovedForHeading;

        _compassWindow = null;
        _usingPointerCompass = false;
    }

    private void OnInclinometerReading(Inclinometer sender, InclinometerReadingChangedEventArgs args)
    {
        var pitch = args.Reading.PitchDegrees;

        if (!_hasInclinometerBaseline)
        {
            _lastPitch = pitch;
            _hasInclinometerBaseline = true;
            return;
        }

        var pitchDelta = pitch - _lastPitch;
        _lastPitch = pitch;

        if (Math.Abs(pitchDelta) < InclinometerDeltaThreshold)
            return;

        var now = DateTime.UtcNow;
        if (now - _lastGyroEventUtc < GyroDebounce)
            return;

        _lastGyroEventUtc = now;
        if (pitchDelta > 0)
            TiltForward?.Invoke();
        else
            TiltBackward?.Invoke();
    }

    private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var delta = e.GetCurrentPoint((UIElement)sender).Properties.MouseWheelDelta;
        if (delta == 0)
            return;

        var now = DateTime.UtcNow;
        if (now - _lastGyroEventUtc < GyroDebounce)
            return;

        _lastGyroEventUtc = now;
        if (delta > 0)
            TiltBackward?.Invoke();
        else
            TiltForward?.Invoke();
    }

    private void OnCompassReading(WinCompass sender, CompassReadingChangedEventArgs args)
    {
        HeadingChanged?.Invoke(NormalizeHeading(args.Reading.HeadingMagneticNorth));
    }

    private void OnMagnetometerReading(WinMagnetometer sender, MagnetometerReadingChangedEventArgs args)
    {
        var x = args.Reading.MagneticFieldX;
        var y = args.Reading.MagneticFieldY;
        var heading = Math.Atan2(y, x) * (180.0 / Math.PI);
        HeadingChanged?.Invoke(NormalizeHeading(heading));
    }

    private void OnPointerMovedForHeading(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not UIElement element)
            return;

        var point = e.GetCurrentPoint(element).Position;
        var size = element.ActualSize;
        if (size.X <= 0 || size.Y <= 0)
            return;

        var dx = point.X - (size.X / 2);
        var dy = (size.Y / 2) - point.Y;
        var heading = Math.Atan2(dx, dy) * (180.0 / Math.PI);
        HeadingChanged?.Invoke(NormalizeHeading(heading));
    }

    private static double NormalizeHeading(double heading)
    {
        heading %= 360;
        if (heading < 0)
            heading += 360;
        return heading;
    }

    private static WinUiWindow? GetActiveWindow()
    {
        var mauiWindow = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        return mauiWindow?.Handler?.PlatformView as WinUiWindow;
    }
}
