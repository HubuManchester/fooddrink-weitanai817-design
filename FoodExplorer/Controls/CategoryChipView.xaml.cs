using System.Windows.Input;
using FoodExplorer.Models;

namespace FoodExplorer.Controls;

public partial class CategoryChipView : ContentView
{
    public static readonly BindableProperty CategoryProperty =
        BindableProperty.Create(nameof(Category), typeof(RecipeCategory), typeof(CategoryChipView));

    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(CategoryChipView));

    public static readonly BindableProperty TapCommandParameterProperty =
        BindableProperty.Create(nameof(TapCommandParameter), typeof(object), typeof(CategoryChipView));

    public CategoryChipView()
    {
        InitializeComponent();
    }

    public RecipeCategory? Category
    {
        get => (RecipeCategory?)GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
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
}
