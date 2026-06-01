using CommunityToolkit.Mvvm.ComponentModel;

namespace FoodExplorer.Models;

public partial class InstructionStep : ObservableObject
{
    public int Number { get; init; }
    public string Text { get; init; } = string.Empty;

    [ObservableProperty]
    private bool _isCurrent;
}
