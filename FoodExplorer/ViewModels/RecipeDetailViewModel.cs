using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodExplorer.Models;
using FoodExplorer.Services;

namespace FoodExplorer.ViewModels;

public partial class InstructionStep : ObservableObject
{
    public int Number { get; init; }
    public string Text { get; init; } = string.Empty;

    [ObservableProperty]
    private bool _isCurrent;
}

[QueryProperty(nameof(RecipeId), "id")]
public partial class RecipeDetailViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;
    private readonly ICameraService _cameraService;
    private readonly ISpeechNarrationService _speechNarrationService;
    private readonly ISensorService _sensorService;
    private readonly IHapticService _hapticService;
    private readonly ISettingsService _settingsService;

    public RecipeDetailViewModel(
        IRecipeService recipeService,
        ICameraService cameraService,
        ISpeechNarrationService speechNarrationService,
        ISensorService sensorService,
        IHapticService hapticService,
        ISettingsService settingsService,
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

    partial void OnRecipeIdChanged(int value)
    {
        if (value > 0)
            _ = LoadRecipeAsync();
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
            SensorHint = "Tilt phone forward/back to change instruction steps.";
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

    public async void StopHardwareFeatures()
    {
        _sensorService.StopGyroscope();
        _sensorService.StopCompass();
        await _speechNarrationService.StopAsync();
        IsNarrating = false;
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
        }, "Unable to load recipe details.");
    }

    [RelayCommand]
    private async Task CapturePhotoAsync()
    {
        if (Recipe is null)
            return;

        HasPhotoStatus = false;
        PhotoStatusMessage = string.Empty;

        try
        {
            var result = await _cameraService.CapturePhotoAsync();

            if (!result.Success || result.Image is null)
            {
                if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                {
                    PhotoStatusMessage = result.ErrorMessage;
                    HasPhotoStatus = true;
                    _hapticService.PerformError();
                }
                return;
            }

            CapturedPhoto = result.Image;
            PhotoStatusMessage = "Dish photo captured successfully!";
            HasPhotoStatus = true;
            _hapticService.PerformSuccess();
            SemanticScreenReader.Announce("Dish photo captured.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeDetailViewModel] Camera: {ex}");
            PhotoStatusMessage = "Could not capture photo. Please try again.";
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

        var result = await _speechNarrationService.SpeakAsync(text);
        IsNarrating = false;

        if (!result.Success && !string.IsNullOrWhiteSpace(result.ErrorMessage))
            NarrationStatusMessage = result.ErrorMessage;
        else
            NarrationStatusMessage = "Narration finished.";
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
            CompassDisplay = $"🧭 {heading:F0}° — {GetCompassDirection(heading)}";
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
        builder.Append($"{recipe.TotalTimeDisplay}, {recipe.Servings} servings. ");

        if (recipe.Ingredients.Count > 0)
        {
            builder.Append("Ingredients: ");
            builder.Append(string.Join(", ", recipe.Ingredients.Select(i => i.DisplayText)));
            builder.Append(". ");
        }

        builder.Append("Instructions: ");
        for (var i = 0; i < recipe.Steps.Count; i++)
            builder.Append($"Step {i + 1}. {recipe.Steps[i]} ");

        return builder.ToString();
    }

    private static string GetCompassDirection(double heading)
    {
        var directions = new[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        var index = (int)Math.Round(heading / 45.0) % 8;
        return directions[index];
    }
}
