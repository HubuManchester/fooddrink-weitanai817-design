namespace FoodExplorer.Services;

/// <summary>
/// Hardware #2 — Microphone / voice search.
/// Requests microphone permission and delegates to the Android <c>SpeechRecognizer</c>
/// to convert spoken queries into recipe search text.
/// </summary>
public class VoiceSearchService : IVoiceSearchService
{
    /// <summary>Listens for a spoken search query and returns the recognised text.</summary>
    public async Task<VoiceSearchResult> ListenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.Microphone>();

            if (status != PermissionStatus.Granted)
                return VoiceSearchResult.Fail(
                    "Microphone permission denied. Enable microphone access in device settings to use voice search.");

#if ANDROID
            return await Platforms.Android.AndroidSpeechHelper.ListenAsync(cancellationToken);
#else
            await Task.CompletedTask;
            return VoiceSearchResult.Fail("Voice search is only available on Android.");
#endif
        }
        catch (PermissionException)
        {
            return VoiceSearchResult.Fail(
                "Microphone permission denied. Enable microphone access in device settings.");
        }
        catch (Exception ex)
        {
            return VoiceSearchResult.Fail($"Voice search error: {ex.Message}");
        }
    }
}
