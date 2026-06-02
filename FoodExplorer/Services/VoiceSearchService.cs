namespace FoodExplorer.Services;

/// <summary>
/// Hardware #2 — Microphone / voice search.
/// Android uses SpeechRecognizer; Windows uses WinRT speech recognition UI.
/// </summary>
public class VoiceSearchService : IVoiceSearchService
{
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
#elif WINDOWS
            return await Platforms.Windows.WindowsSpeechHelper.ListenAsync(cancellationToken);
#else
            await Task.CompletedTask;
            return VoiceSearchResult.Fail("Voice search is not supported on this platform.");
#endif
        }
        catch (PermissionException)
        {
            return VoiceSearchResult.Fail(
                "Microphone permission denied. Enable microphone access in device settings.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VoiceSearchService] {ex}");
            return VoiceSearchResult.Fail(
                string.IsNullOrWhiteSpace(ex.Message)
                    ? "Voice search failed. Please try again."
                    : ex.Message);
        }
    }
}
