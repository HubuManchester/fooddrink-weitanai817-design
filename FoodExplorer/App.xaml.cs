using FoodExplorer.Services;

namespace FoodExplorer;

public partial class App : Application
{
    private readonly ISettingsService _settingsService;

    public App(ISettingsService settingsService, AppShell shell)
    {
        InitializeComponent();

        _settingsService = settingsService;
        _settingsService.SettingsChanged += OnSettingsChanged;

        ApplyTheme(_settingsService.IsDarkMode);
        ApplyFontScale(_settingsService.FontScale);

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

    public void ApplyFontScale(double scale)
    {
        // Font scale applied per-page in Phase 2
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        ApplyTheme(_settingsService.IsDarkMode);
        ApplyFontScale(_settingsService.FontScale);
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
