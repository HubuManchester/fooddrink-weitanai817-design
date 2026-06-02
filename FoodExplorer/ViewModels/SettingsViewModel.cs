using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodExplorer.Models;
using FoodExplorer.Services;

namespace FoodExplorer.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;

    public SettingsViewModel(
        ISettingsService settingsService,
        INavigationService navigationService,
        IDialogService dialogService)
        : base(navigationService, dialogService)
    {
        _settingsService = settingsService;
        Title = "Settings";
        SyncFromService();
        _settingsService.SettingsChanged += (_, _) => SyncFromService();
    }

    [ObservableProperty]
    private bool _isDarkMode;

    [ObservableProperty]
    private FontSizeOption _selectedFontSize;

    [ObservableProperty]
    private bool _reduceMotion;

    [ObservableProperty]
    private bool _highContrast;

    public IReadOnlyList<FontSizeOption> FontSizeOptions { get; } =
        Enum.GetValues<FontSizeOption>().ToList();

    public string FontSizePreview =>
        $"Preview text at {SelectedFontSize.GetDisplayName()} size";

    public double FontSizePreviewSize => 16 * SelectedFontSize.GetScale();

    partial void OnIsDarkModeChanged(bool value)
    {
        if (_settingsService.IsDarkMode != value)
            _settingsService.IsDarkMode = value;
    }

    partial void OnSelectedFontSizeChanged(FontSizeOption value)
    {
        if (_settingsService.FontSize != value)
            _settingsService.FontSize = value;

        OnPropertyChanged(nameof(FontSizePreview));
        OnPropertyChanged(nameof(FontSizePreviewSize));
    }

    partial void OnReduceMotionChanged(bool value)
    {
        if (_settingsService.ReduceMotion != value)
            _settingsService.ReduceMotion = value;
    }

    partial void OnHighContrastChanged(bool value)
    {
        if (_settingsService.HighContrast != value)
            _settingsService.HighContrast = value;
    }

    [RelayCommand]
    private async Task ResetSettingsAsync()
    {
        var confirm = await DialogService.DisplayConfirmAsync(
            "Reset Settings",
            "Restore all settings to their default values?");

        if (!confirm)
            return;

        _settingsService.IsDarkMode = false;
        _settingsService.FontSize = FontSizeOption.Medium;
        _settingsService.ReduceMotion = false;
        _settingsService.HighContrast = false;
        SyncFromService();
    }

    private void SyncFromService()
    {
        IsDarkMode = _settingsService.IsDarkMode;
        SelectedFontSize = _settingsService.FontSize;
        ReduceMotion = _settingsService.ReduceMotion;
        HighContrast = _settingsService.HighContrast;
        OnPropertyChanged(nameof(FontSizePreview));
        OnPropertyChanged(nameof(FontSizePreviewSize));
    }
}
