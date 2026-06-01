namespace FoodExplorer.Services;

public class AccessibilityService : IAccessibilityService
{
    private static readonly HashSet<Type> ScalableTypes =
    [
        typeof(Label), typeof(Button), typeof(Entry), typeof(Editor),
        typeof(Picker), typeof(SearchBar)
    ];

    public void ApplyFontScale(Page page, double scale)
    {
        foreach (var element in GetVisualElements(page))
        {
            if (!ScalableTypes.Contains(element.GetType()))
                continue;

            if (element.GetValue(BaseFontSizeProperty) is not double baseSize)
            {
                baseSize = element switch
                {
                    Label label => label.FontSize,
                    Button button => button.FontSize,
                    Entry entry => entry.FontSize,
                    Editor editor => editor.FontSize,
                    Picker picker => picker.FontSize,
                    SearchBar searchBar => searchBar.FontSize,
                    _ => 0
                };

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
        page.BackgroundColor = enabled
            ? Colors.Black
            : (Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#1A1A2E")
                : Color.FromArgb("#FFF8F5"));

        foreach (var label in GetVisualElements(page).OfType<Label>())
        {
            if (enabled)
                label.TextColor = Colors.White;
        }
    }

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

    public static readonly BindableProperty BaseFontSizeProperty =
        BindableProperty.CreateAttached(
            "BaseFontSize",
            typeof(double),
            typeof(AccessibilityService),
            0.0);
}
