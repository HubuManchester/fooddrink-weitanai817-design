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

    public RecipeDetailViewModel(
        IRecipeService recipeService,
        INavigationService navigationService,
        IDialogService dialogService)
        : base(navigationService, dialogService)
    {
        _recipeService = recipeService;
        Title = "Recipe Detail";
    }

    public ObservableCollection<InstructionStep> InstructionSteps { get; } = new();

    [ObservableProperty]
    private Recipe? _recipe;

    [ObservableProperty]
    private int _recipeId;

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
    private async Task ToggleFavouriteAsync()
    {
        if (Recipe is null)
            return;

        Recipe.IsFavourite = !Recipe.IsFavourite;
        OnPropertyChanged(nameof(Recipe));
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await NavigationService.GoBackAsync();
    }
}
