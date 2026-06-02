namespace FoodExplorer.Services;

public record VoiceSearchResult(bool Success, string? Text, string? ErrorMessage, bool OpenSpeechSettings = false)
{
    public static VoiceSearchResult Ok(string text) => new(true, text, null);

    public static VoiceSearchResult Fail(string error, bool openSpeechSettings = false) =>
        new(false, null, error, openSpeechSettings);
}

public interface IVoiceSearchService
{
    Task<VoiceSearchResult> ListenAsync(CancellationToken cancellationToken = default);
}
