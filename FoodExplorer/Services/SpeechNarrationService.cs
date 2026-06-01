namespace FoodExplorer.Services;

/// <summary>
/// Hardware #4 — Text-to-Speech narration.
/// Uses the MAUI Essentials TextToSpeech API to read recipe instructions aloud,
/// supporting cancellation and English locale selection.
/// </summary>
public class SpeechNarrationService : ISpeechNarrationService
{
    private CancellationTokenSource? _speakCts;

    public bool IsSpeaking => _speakCts is not null;

    /// <summary>Reads the given text aloud using the device TTS engine.</summary>
    public async Task<NarrationResult> SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return NarrationResult.Fail("Nothing to read aloud.");

        try
        {
            await StopAsync();

            _speakCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = _speakCts.Token;

            var locales = await TextToSpeech.Default.GetLocalesAsync();
            var locale = locales.FirstOrDefault(l => l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                         ?? locales.FirstOrDefault();

            var options = new SpeechOptions
            {
                Pitch = 1.0f,
                Volume = 0.9f,
                Locale = locale
            };

            await TextToSpeech.Default.SpeakAsync(text, options, token);
            return NarrationResult.Ok();
        }
        catch (OperationCanceledException)
        {
            return NarrationResult.Fail("Narration stopped.");
        }
        catch (Exception ex)
        {
            return NarrationResult.Fail($"Text-to-speech error: {ex.Message}");
        }
        finally
        {
            _speakCts?.Dispose();
            _speakCts = null;
        }
    }

    /// <summary>Cancels any in-progress narration.</summary>
    public Task StopAsync()
    {
        if (_speakCts is null)
            return Task.CompletedTask;

        _speakCts.Cancel();
        _speakCts.Dispose();
        _speakCts = null;
        return Task.CompletedTask;
    }
}
