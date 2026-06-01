namespace FoodExplorer.Services;

public record NarrationResult(bool Success, string? ErrorMessage)
{
    public static NarrationResult Ok() => new(true, null);
    public static NarrationResult Fail(string error) => new(false, error);
}

public interface ISpeechNarrationService
{
    bool IsSpeaking { get; }
    Task<NarrationResult> SpeakAsync(string text, CancellationToken cancellationToken = default);
    Task StopAsync();
}
