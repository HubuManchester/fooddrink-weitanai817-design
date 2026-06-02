namespace FoodExplorer.Services;

/// <summary>
/// Applies font scaling and high-contrast overrides while preserving theme bindings when disabled.
/// </summary>
public class AccessibilityService : IAccessibilityService
{
    private static readonly HashSet<Type> ScalableTypes =
    [
        typeof(Label), typeof(Button), typeof(Entry), typeof(Editor),
        typeof(Picker), typeof(SearchBar)
    ];

    public static readonly BindableProperty BaseFontSizeProperty =
        BindableProperty.CreateAttached(
            "BaseFontSize",
            typeof(double),
            typeof(AccessibilityService),
            0.0);

    public static readonly BindableProperty SkipFontScaleProperty =
        BindableProperty.CreateAttached(
            "SkipFontScale",
            typeof(bool),
            typeof(AccessibilityService),
            false);

    public static void SetSkipFontScale(BindableObject view, bool value) =>
        view.SetValue(SkipFontScaleProperty, value);

    public static bool GetSkipFontScale(BindableObject view) =>
        (bool)view.GetValue(SkipFontScaleProperty);

    private static readonly BindableProperty HighContrastAppliedProperty =
        BindableProperty.CreateAttached(
            "HighContrastApplied",
            typeof(bool),
            typeof(AccessibilityService),
            false);

    public void ApplyFontScale(Page page, double scale)
    {
        foreach (var element in GetVisualElements(page))
        {
            if (!ScalableTypes.Contains(element.GetType()))
                continue;

            if (element.GetValue(SkipFontScaleProperty) is true)
                continue;

            double baseSize;
            var storedBase = element.GetValue(BaseFontSizeProperty);
            if (storedBase is double stored && stored > 0)
            {
                baseSize = stored;
            }
            else
            {
                baseSize = GetElementFontSize(element);
                if (baseSize <= 0)
                    continue;

                element.SetValue(BaseFontSizeProperty, baseSize);
            }

            var scaled = Math.Max(10, baseSize * scale);
            switch (element)
            {
                case Label label:
                    label.FontSize = scaled;
                    break;
                case Button button:
                    button.FontSize = scaled;
                    break;
                case Entry entry:
                    entry.FontSize = scaled;
                    break;
                case Editor editor:
                    editor.FontSize = scaled;
                    break;
                case Picker picker:
                    picker.FontSize = scaled;
                    break;
                case SearchBar searchBar:
                    searchBar.FontSize = scaled;
                    break;
            }
        }
    }

    public void ApplyHighContrast(Page page, bool enabled)
    {
        if (enabled)
            EnableHighContrast(page);
        else
            DisableHighContrast(page);
    }

    private static void EnableHighContrast(Page page)
    {
        page.BackgroundColor = Colors.Black;

        foreach (var border in GetVisualElements(page).OfType<Border>())
        {
            border.SetValue(HighContrastAppliedProperty, true);
            border.BackgroundColor = Color.FromArgb("#1A1A1A");
            border.Stroke = Colors.White;
            border.StrokeThickness = 1;
        }

        foreach (var label in GetVisualElements(page).OfType<Label>())
        {
            label.SetValue(HighContrastAppliedProperty, true);
            label.TextColor = Colors.White;
        }

        foreach (var picker in GetVisualElements(page).OfType<Picker>())
        {
            picker.SetValue(HighContrastAppliedProperty, true);
            picker.TextColor = Colors.White;
            picker.TitleColor = Colors.LightGray;
        }

        foreach (var button in GetVisualElements(page).OfType<Button>())
        {
            if (button.BackgroundColor == Colors.Transparent || button.BackgroundColor == null)
                continue;

            button.SetValue(HighContrastAppliedProperty, true);
            button.BackgroundColor = Colors.Black;
            button.TextColor = Colors.White;
            button.BorderColor = Colors.White;
        }
    }

    private static void DisableHighContrast(Page page)
    {
        page.ClearValue(VisualElement.BackgroundColorProperty);

        foreach (var label in GetVisualElements(page).OfType<Label>())
        {
            label.ClearValue(HighContrastAppliedProperty);
            label.ClearValue(Label.TextColorProperty);
        }

        foreach (var border in GetVisualElements(page).OfType<Border>())
        {
            border.ClearValue(HighContrastAppliedProperty);
            border.ClearValue(Border.BackgroundColorProperty);
            border.ClearValue(Border.StrokeProperty);
            border.ClearValue(Border.StrokeThicknessProperty);
        }

        foreach (var picker in GetVisualElements(page).OfType<Picker>())
        {
            picker.ClearValue(HighContrastAppliedProperty);
            picker.ClearValue(Picker.TextColorProperty);
            picker.ClearValue(Picker.TitleColorProperty);
        }

        foreach (var button in GetVisualElements(page).OfType<Button>())
        {
            if (button.GetValue(HighContrastAppliedProperty) is not true)
                continue;

            button.ClearValue(HighContrastAppliedProperty);
            button.ClearValue(Button.BackgroundColorProperty);
            button.ClearValue(Button.TextColorProperty);
            button.ClearValue(Button.BorderColorProperty);
        }
    }

    private static double GetElementFontSize(VisualElement element) => element switch
    {
        Label label => label.FontSize,
        Button button => button.FontSize,
        Entry entry => entry.FontSize,
        Editor editor => editor.FontSize,
        Picker picker => picker.FontSize,
        SearchBar searchBar => searchBar.FontSize,
        _ => 0
    };

    private static IEnumerable<VisualElement> GetVisualElements(IView root)
    {
        if (root is not VisualElement visual)
            yield break;

        yield return visual;

        if (visual is IContentView contentView && contentView.Content is VisualElement child)
        {
            foreach (var nested in GetVisualElements(child))
                yield return nested;
        }
        else if (visual is Layout layout)
        {
            foreach (var childElement in layout.Children.OfType<VisualElement>())
            {
                foreach (var nested in GetVisualElements(childElement))
                    yield return nested;
            }
        }
        else if (visual is ScrollView scroll && scroll.Content is VisualElement scrollContent)
        {
            foreach (var nested in GetVisualElements(scrollContent))
                yield return nested;
        }
        else if (visual is Border border && border.Content is VisualElement borderContent)
        {
            foreach (var nested in GetVisualElements(borderContent))
                yield return nested;
        }
        else if (visual is ContentView cv && cv.Content is VisualElement cvContent)
        {
            foreach (var nested in GetVisualElements(cvContent))
                yield return nested;
        }
    }
}
