using FoodExplorer.Models;

namespace FoodExplorer.Services;

public class SettingsService : ISettingsService
{
    private const string DarkModeKey = "settings_dark_mode";
    private const string FontSizeKey = "settings_font_size";
    private const string ReduceMotionKey = "settings_reduce_motion";
    private const string HighContrastKey = "settings_high_contrast";

    private bool _isDarkMode;
    private FontSizeOption _fontSize = FontSizeOption.Medium;
    private bool _reduceMotion;
    private bool _highContrast;

    public SettingsService()
    {
        _isDarkMode = Preferences.Default.Get(DarkModeKey, false);
        _fontSize = Enum.TryParse<FontSizeOption>(
            Preferences.Default.Get(FontSizeKey, FontSizeOption.Medium.ToString()),
            out var parsed)
            ? parsed
            : FontSizeOption.Medium;
        _reduceMotion = Preferences.Default.Get(ReduceMotionKey, false);
        _highContrast = Preferences.Default.Get(HighContrastKey, false);
    }

    public event EventHandler? SettingsChanged;

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (_isDarkMode == value)
                return;

            _isDarkMode = value;
            Preferences.Default.Set(DarkModeKey, value);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public FontSizeOption FontSize
    {
        get => _fontSize;
        set
        {
            if (_fontSize == value)
                return;

            _fontSize = value;
            Preferences.Default.Set(FontSizeKey, value.ToString());
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public double FontScale => FontSize.GetScale();

    public bool ReduceMotion
    {
        get => _reduceMotion;
        set
        {
            if (_reduceMotion == value)
                return;

            _reduceMotion = value;
            Preferences.Default.Set(ReduceMotionKey, value);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool HighContrast
    {
        get => _highContrast;
        set
        {
            if (_highContrast == value)
                return;

            _highContrast = value;
            Preferences.Default.Set(HighContrastKey, value);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
