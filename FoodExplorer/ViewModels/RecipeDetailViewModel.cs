using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodExplorer.Models;
using FoodExplorer.Services;

namespace FoodExplorer.ViewModels;

public class InstructionStep
{
    public int Number { get; init; }
    public string Text { get; init; } = string.Empty;
}

[QueryProperty(nameof(RecipeId), "id")]
public partial class RecipeDetailViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;
    private readonly ICameraService _cameraService;

    public RecipeDetailViewModel(
        IRecipeService recipeService,
        ICameraService cameraService,
        INavigationService navigationService,
        IDialogService dialogService)
        : base(navigationService, dialogService)
    {
        _recipeService = recipeService;
        _cameraService = cameraService;
        Title = "Recipe Detail";
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

    partial void OnRecipeIdChanged(int value)
    {
        if (value > 0)
            _ = LoadRecipeAsync();
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

            InstructionSteps.Clear();
            for (var i = 0; i < Recipe.Steps.Count; i++)
            {
                InstructionSteps.Add(new InstructionStep
                {
                    Number = i + 1,
                    Text = Recipe.Steps[i]
                });
            }
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
                }
                return;
            }

            CapturedPhoto = result.Image;
            PhotoStatusMessage = "Dish photo captured successfully!";
            HasPhotoStatus = true;
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce("Dish photo captured.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecipeDetailViewModel] Camera: {ex}");
            PhotoStatusMessage = "Could not capture photo. Please try again.";
            HasPhotoStatus = true;
        }
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
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce(isFavourite ? "Added to favourites." : "Removed from favourites.");
        }, "Could not update favourite.");
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await NavigationService.GoBackAsync();
    }
}
