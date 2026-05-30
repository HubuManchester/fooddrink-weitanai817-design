using System.Windows.Input;
using FoodExplorer.Models;

namespace FoodExplorer.Controls;

public partial class RecipeCardView : ContentView
{
    public static readonly BindableProperty RecipeProperty =
        BindableProperty.Create(nameof(Recipe), typeof(RecipeSummary), typeof(RecipeCardView));

    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(RecipeCardView));

    public static readonly BindableProperty TapCommandParameterProperty =
        BindableProperty.Create(nameof(TapCommandParameter), typeof(object), typeof(RecipeCardView));

    public static readonly BindableProperty FavouriteCommandProperty =
        BindableProperty.Create(nameof(FavouriteCommand), typeof(ICommand), typeof(RecipeCardView));

    public RecipeCardView()
    {
        InitializeComponent();
    }

    public RecipeSummary? Recipe
    {
        get => (RecipeSummary?)GetValue(RecipeProperty);
        set => SetValue(RecipeProperty, value);
    }

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public object? TapCommandParameter
    {
        get => GetValue(TapCommandParameterProperty);
        set => SetValue(TapCommandParameterProperty, value);
    }

    public ICommand? FavouriteCommand
    {
        get => (ICommand?)GetValue(FavouriteCommandProperty);
        set => SetValue(FavouriteCommandProperty, value);
    }
}
