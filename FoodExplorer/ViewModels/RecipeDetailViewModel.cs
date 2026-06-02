using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodExplorer.Models;
using FoodExplorer.Services;

namespace FoodExplorer.ViewModels;

[QueryProperty(nameof(RecipeId), "id")]
public partial class RecipeDetailViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;
    private readonly ICameraService _cameraService;
    private readonly ISpeechNarrationService _speechNarrationService;
    private readonly ISensorService _sensorService;
    private readonly IHapticService _hapticService;
    private readonly ISettingsService _settingsService;
    private readonly IDeviceLayoutService _deviceLayoutService;
    private readonly IMapLauncherService _mapLauncherService;

    public RecipeDetailViewModel(
        IRecipeService recipeService,
        ICameraService cameraService,
        ISpeechNarrationService speechNarrationService,
        ISensorService sensorService,
        IHapticService hapticService,
        ISettingsService settingsService,
        IDeviceLayoutService deviceLayoutService,
        IMapLauncherService mapLauncherService,
        INavigationService navigationService,
        IDialogService dialogService)
        : base(navigationService, dialogService)
    {
        _recipeService = recipeService;
        _cameraService = cameraService;
        _speechNarrationService = speechNarrationService;
        _sensorService = sensorService;
        _hapticService = hapticService;
        _settingsService = settingsService;
        _deviceLayoutService = deviceLayoutService;
        _mapLauncherService = mapLauncherService;
        Title = "Recipe Detail";

        _sensorService.TiltForward += OnTiltForward;
        _sensorService.TiltBackward += OnTiltBackward;
        _sensorService.HeadingChanged += OnHeadingChanged;
    }

    public ObservableCollection<InstructionStep> InstructionSteps { get; } = new();

    [ObservableProperty]
    private Recipe? _recipe;

    [ObservableProperty]
    private int _recipeId;

    [ObservableProperty]
    private ImageSource? _capturedPhoto;

    [ObservableProperty]
    private string _photoStatusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasPhotoStatus;

    [ObservableProperty]
    private int _currentStepIndex;

    [ObservableProperty]
    private string _compassDisplay = "Compass unavailable";

    [ObservableProperty]
    private string _sensorHint = string.Empty;

    [ObservableProperty]
    private bool _isNarrating;

    [ObservableProperty]
    private string _narrationStatusMessage = string.Empty;

    [ObservableProperty]
    private bool _isWideLayout;

    [ObservableProperty]
    private double _contentMaxWidth = double.PositiveInfinity;

    [ObservableProperty]
    private string _locationStatusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasLocationStatus;

    public string DetailTotalTime => Recipe?.TotalTimeDisplay ?? "—";

    public string DetailServings =>
        Recipe is null || Recipe.Servings <= 0 ? "—" : Recipe.Servings.ToString();

    public string DetailCalories =>
        Recipe is null || Recipe.CaloriesPerServing <= 0 ? "—" : Recipe.CaloriesPerServing.ToString();

    public string DetailRating => Recipe?.RatingDisplay ?? "—";

    public bool HasCapturedPhoto => CapturedPhoto is not null;

    public void UpdateLayout(double pageWidth)
    {
        IsWideLayout = _deviceLayoutService.IsTablet(pageWidth);
        ContentMaxWidth = _deviceLayoutService.GetContentMaxWidth(pageWidth);
    }

    partial void OnRecipeIdChanged(int value)
    {
        if (value > 0)
            _ = LoadRecipeAsync();
    }

    partial void OnRecipeChanged(Recipe? value)
    {
        NotifyDetailFieldsChanged();
    }

    partial void OnCapturedPhotoChanged(ImageSource? value)
    {
        OnPropertyChanged(nameof(HasCapturedPhoto));
    }

    private void NotifyDetailFieldsChanged()
    {
        OnPropertyChanged(nameof(DetailTotalTime));
        OnPropertyChanged(nameof(DetailServings));
        OnPropertyChanged(nameof(DetailCalories));
        OnPropertyChanged(nameof(DetailRating));
    }

    public void StartHardwareFeatures()
    {
        if (_settingsService.ReduceMotion)
        {
            SensorHint = "Motion sensors disabled (Reduce Motion is on).";
            return;
        }

        if (_sensorService.IsGyroscopeSupported)
        {
            _sensorService.StartGyroscope();
#if WINDOWS
            SensorHint = "Tilt your device, or scroll the mouse wheel over the page, to change steps.";
#else
            SensorHint = "Tilt phone forward/back to change instruction steps.";
#endif
        }
        else
        {
            SensorHint = "Gyroscope not available — use step buttons below.";
        }

        if (_sensorService.IsCompassSupported)
            _sensorService.StartCompass();
        else
            CompassDisplay = "Compass not available on this device.";
    }

    public async Task StopHardwareFeaturesAsync()
    {
        try
        {
            _sensorService.StopGyroscope();
            _sensorService.StopCompass();
            await _speechNarrationService.StopAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeDetailViewModel] StopHardwareFeatures: {ex}");
        }
        finally
        {
            IsNarrating = false;
        }
    }

    [RelayCommand]
    private async Task LoadRecipeAsync()
    {
        await ExecuteAsync(async () =>
        {
            Recipe = await _recipeService.GetRecipeByIdAsync(RecipeId);

            if (Recipe is null)
            {
                HasError = true;
                ErrorMessage = "Recipe not found.";
                await DialogService.DisplayAlertAsync("Not Found", ErrorMessage);
                await NavigationService.GoBackAsync();
                return;
            }

            Title = Recipe.Name;
            CapturedPhoto = null;
            HasPhotoStatus = false;
            PhotoStatusMessage = string.Empty;
            NarrationStatusMessage = string.Empty;
            HasLocationStatus = false;
            LocationStatusMessage = string.Empty;

            InstructionSteps.Clear();
            for (var i = 0; i < Recipe.Steps.Count; i++)
            {
                InstructionSteps.Add(new InstructionStep
                {
                    Number = i + 1,
                    Text = Recipe.Steps[i]
                });
            }

            CurrentStepIndex = InstructionSteps.Count > 0 ? 1 : 0;
            UpdateStepHighlights();
            NotifyDetailFieldsChanged();
        }, "Unable to load recipe details.");
    }

    [RelayCommand]
    private Task CapturePhotoAsync() =>
        SetDishPhotoAsync(_cameraService.CapturePhotoAsync(), "Dish photo captured!", "capture");

    [RelayCommand]
    private Task PickPhotoFromGalleryAsync() =>
        SetDishPhotoAsync(_cameraService.PickPhotoAsync(), "Dish photo added from gallery!", "gallery");

    private async Task SetDishPhotoAsync(
        Task<CameraCaptureResult> photoTask,
        string successMessage,
        string source)
    {
        if (Recipe is null)
            return;

        HasPhotoStatus = false;
        PhotoStatusMessage = string.Empty;

        try
        {
            var result = await photoTask;

            if (!result.Success || result.Image is null)
            {
                PhotoStatusMessage = string.IsNullOrWhiteSpace(result.ErrorMessage)
                    ? "Photo selection was cancelled."
                    : result.ErrorMessage;
                HasPhotoStatus = true;
                _hapticService.PerformError();
                return;
            }

            CapturedPhoto = result.Image;
            PhotoStatusMessage = successMessage;
            HasPhotoStatus = true;
            _hapticService.PerformSuccess();
            SemanticScreenReader.Announce(source == "gallery"
                ? "Dish photo selected from gallery."
                : "Dish photo captured.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeDetailViewModel] Photo ({source}): {ex}");
            PhotoStatusMessage = "Could not add photo. Please try again.";
            HasPhotoStatus = true;
            _hapticService.PerformError();
        }
    }

    [RelayCommand]
    private async Task NarrateRecipeAsync()
    {
        if (Recipe is null)
            return;

        var text = BuildNarrationText(Recipe);
        IsNarrating = true;
        NarrationStatusMessage = "Reading recipe aloud…";
        _hapticService.PerformClick();

        try
        {
            var result = await _speechNarrationService.SpeakAsync(text);

            if (!result.Success)
            {
                NarrationStatusMessage = result.ErrorMessage ?? "Text-to-speech failed.";
                _hapticService.PerformError();
                return;
            }

            NarrationStatusMessage = "Narration finished.";
            _hapticService.PerformSuccess();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeDetailViewModel] Narration: {ex}");
            NarrationStatusMessage = "Text-to-speech failed. Please try again.";
            _hapticService.PerformError();
        }
        finally
        {
            IsNarrating = false;
        }
    }

    [RelayCommand]
    private async Task StopNarrationAsync()
    {
        await _speechNarrationService.StopAsync();
        IsNarrating = false;
        NarrationStatusMessage = "Narration stopped.";
        _hapticService.PerformClick();
    }

    [RelayCommand]
    private void NextStep()
    {
        if (CurrentStepIndex >= InstructionSteps.Count)
            return;

        CurrentStepIndex++;
        UpdateStepHighlights();
        _hapticService.PerformClick();
        AnnounceCurrentStep();
    }

    [RelayCommand]
    private void PreviousStep()
    {
        if (CurrentStepIndex <= 1)
            return;

        CurrentStepIndex--;
        UpdateStepHighlights();
        _hapticService.PerformClick();
        AnnounceCurrentStep();
    }

    [RelayCommand]
    private async Task ToggleFavouriteAsync()
    {
        if (Recipe is null)
            return;

        await ExecuteAsync(async () =>
        {
            var isFavourite = await _recipeService.ToggleFavouriteAsync(Recipe.Id);
            Recipe.IsFavourite = isFavourite;
            OnPropertyChanged(nameof(Recipe));
            _hapticService.PerformClick();
            SemanticScreenReader.Announce(isFavourite ? "Added to favourites." : "Removed from favourites.");
        }, "Could not update favourite.");
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await NavigationService.GoBackAsync();
    }

    /// <summary>
    /// Hardware #9 — Geolocation. Finds the user's location and suggests nearby restaurants
    /// serving this recipe's cuisine via MAUI Essentials Geolocation API.
    /// </summary>
    [RelayCommand]
    private async Task FindNearbyRestaurantsAsync()
    {
        if (Recipe is null)
            return;

        HasLocationStatus = false;
        LocationStatusMessage = string.Empty;

        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                LocationStatusMessage = "Location permission denied. Enable in Settings → App Permissions.";
                HasLocationStatus = true;
                _hapticService.PerformError();
                return;
            }

            LocationStatusMessage = "📍 Getting your location…";
            HasLocationStatus = true;

            var location = await Geolocation.Default.GetLastKnownLocationAsync();
            if (location is null)
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                location = await Geolocation.Default.GetLocationAsync(request);
            }

            if (location is null)
            {
                LocationStatusMessage = "Could not determine your location. Please try again.";
                _hapticService.PerformError();
                return;
            }

            var cuisine = string.IsNullOrWhiteSpace(Recipe.Cuisine) ? "restaurant" : Recipe.Cuisine;
            var searchQuery = $"{cuisine} restaurants";

            LocationStatusMessage = "📍 Opening maps…";
            var mapResult = await _mapLauncherService.OpenNearbySearchAsync(
                location.Latitude,
                location.Longitude,
                searchQuery);

            if (!mapResult.Success)
            {
                LocationStatusMessage = mapResult.ErrorMessage
                    ?? "Could not open maps. Install Google Maps and try again.";
                HasLocationStatus = true;
                _hapticService.PerformError();
                return;
            }

            LocationStatusMessage =
                $"📍 Opened maps for nearby {cuisine} restaurants.\n" +
                $"Your location: {location.Latitude:F4}°, {location.Longitude:F4}°";
            _hapticService.PerformSuccess();
            SemanticScreenReader.Announce($"Opened maps to find {cuisine} restaurants near you.");
        }
        catch (FeatureNotSupportedException)
        {
            LocationStatusMessage = "Location services are not available on this device.";
            HasLocationStatus = true;
        }
        catch (PermissionException)
        {
            LocationStatusMessage = "Location permission was denied. Please enable it in device settings.";
            HasLocationStatus = true;
            _hapticService.PerformError();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeDetailViewModel] Location: {ex}");
            LocationStatusMessage = "Location lookup failed. Please check your connection and try again.";
            HasLocationStatus = true;
            _hapticService.PerformError();
        }
    }

    private void OnTiltForward()
    {
        MainThread.BeginInvokeOnMainThread(() => NextStepCommand.Execute(null));
    }

    private void OnTiltBackward()
    {
        MainThread.BeginInvokeOnMainThread(() => PreviousStepCommand.Execute(null));
    }

    private void OnHeadingChanged(double heading)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var dir = heading switch
            {
                < 22.5 or >= 337.5 => "N",
                < 67.5 => "NE",
                < 112.5 => "E",
                < 157.5 => "SE",
                < 202.5 => "S",
                < 247.5 => "SW",
                < 292.5 => "W",
                _ => "NW"
            };
            CompassDisplay = $"🧭 Heading: {dir} {heading:F0}°";
        });
    }

    private void UpdateStepHighlights()
    {
        foreach (var step in InstructionSteps)
            step.IsCurrent = step.Number == CurrentStepIndex;
    }

    private void AnnounceCurrentStep()
    {
        var step = InstructionSteps.FirstOrDefault(s => s.Number == CurrentStepIndex);
        if (step is not null)
            SemanticScreenReader.Announce($"Step {step.Number}: {step.Text}");
    }

    private static string BuildNarrationText(Recipe recipe)
    {
        var builder = new StringBuilder();
        builder.Append($"{recipe.Name}. ");
        builder.Append($"{FormatTimeForSpeech(recipe.TotalTimeMinutes)}, ");
        builder.Append(CultureInfo.InvariantCulture, $"{recipe.Servings} servings. ");
        builder.Append(CultureInfo.InvariantCulture, $"{recipe.CaloriesPerServing} calories per serving. ");

        if (recipe.Ingredients.Count > 0)
        {
            builder.Append("Ingredients: ");
            builder.Append(string.Join(", ", recipe.Ingredients.Select(i => i.DisplayText)));
            builder.Append(". ");
        }

        builder.Append("Instructions: ");
        for (var i = 0; i < recipe.Steps.Count; i++)
            builder.Append(CultureInfo.InvariantCulture, $"Step {i + 1}. {recipe.Steps[i]} ");

        return builder.ToString();
    }

    private static string FormatTimeForSpeech(int totalMinutes)
    {
        if (totalMinutes <= 0)
            return "no time listed";

        if (totalMinutes < 60)
            return $"{totalMinutes.ToString(CultureInfo.InvariantCulture)} minutes";

        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;
        if (minutes == 0)
            return $"{hours.ToString(CultureInfo.InvariantCulture)} hours";

        return $"{hours.ToString(CultureInfo.InvariantCulture)} hours {minutes.ToString(CultureInfo.InvariantCulture)} minutes";
    }
}
