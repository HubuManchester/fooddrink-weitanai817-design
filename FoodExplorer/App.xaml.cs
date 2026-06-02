using FoodExplorer.Services;

namespace FoodExplorer;

public partial class App : Application
{
    private readonly ISettingsService _settingsService;
    private readonly IAccessibilityService _accessibilityService;

    public App(
        ISettingsService settingsService,
        IAccessibilityService accessibilityService,
        AppShell shell)
    {
        InitializeComponent();

        _settingsService = settingsService;
        _accessibilityService = accessibilityService;
        _settingsService.SettingsChanged += OnSettingsChanged;

        ApplyTheme(_settingsService.IsDarkMode);
        ApplyAccessibilityToCurrentPage();

        try
        {
            MainPage = shell;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FoodExplorer] Startup failed: {ex}");
            MainPage = new ContentPage
            {
                BackgroundColor = Colors.White,
                Content = new Label
                {
                    Text = $"Startup error: {ex.Message}",
                    TextColor = Colors.Black,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(20)
                }
            };
        }
    }

    public void ApplyTheme(bool isDark)
    {
        UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
    }

    public void ApplyAccessibilityToPage(Page page)
    {
        _accessibilityService.ApplyHighContrast(page, _settingsService.HighContrast);
        _accessibilityService.ApplyFontScale(page, _settingsService.FontScale);
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        ApplyTheme(_settingsService.IsDarkMode);
        ApplyAccessibilityToCurrentPage();
    }

    private void ApplyAccessibilityToCurrentPage()
    {
        if (GetCurrentPage() is Page page)
            ApplyAccessibilityToPage(page);
    }

    private static Page? GetCurrentPage()
    {
        if (Current?.MainPage is Shell shell)
            return shell.CurrentPage;

        return Current?.MainPage as Page;
    }

    protected override void OnStart()
    {
        base.OnStart();

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            System.Diagnostics.Debug.WriteLine($"[FoodExplorer] Unhandled exception: {args.ExceptionObject}");
        };
    }
}
