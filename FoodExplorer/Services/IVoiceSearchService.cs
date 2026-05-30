namespace FoodExplorer.Services;

public record VoiceSearchResult(bool Success, string? Text, string? ErrorMessage)
{
    public static VoiceSearchResult Ok(string text) => new(true, text, null);
    public static VoiceSearchResult Fail(string error) => new(false, null, error);
}

public interface IVoiceSearchService
{
    Task<VoiceSearchResult> ListenAsync(CancellationToken cancellationToken = default);
}
