using FoodExplorer.Services;
using Windows.Media.SpeechRecognition;

namespace FoodExplorer.Platforms.Windows;

internal static class WindowsSpeechHelper
{
    private const uint HResultPrivacyStatementDeclined = 0x80045509;

    private const string SpeechSettingsMessage =
        "Online speech recognition is off in Windows. Open Settings → Privacy & security → Speech, turn on \"Online speech recognition\", then try again.";

    public static Task<VoiceSearchResult> ListenAsync(CancellationToken cancellationToken)
    {
        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            SpeechRecognizer? recognizer = null;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                recognizer = new SpeechRecognizer();
                recognizer.UIOptions.AudiblePrompt = "Say a recipe name, cuisine, or category";
                recognizer.Constraints.Add(
                    new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation"));

                var compileResult = await recognizer.CompileConstraintsAsync();
                if (compileResult.Status != SpeechRecognitionResultStatus.Success)
                {
                    return VoiceSearchResult.Fail(
                        "Speech recognition failed to initialize. Check microphone access in Windows Settings.");
                }

                var result = await recognizer.RecognizeWithUIAsync();
                cancellationToken.ThrowIfCancellationRequested();

                return result.Status switch
                {
                    SpeechRecognitionResultStatus.Success when !string.IsNullOrWhiteSpace(result.Text)
                        => VoiceSearchResult.Ok(result.Text),
                    SpeechRecognitionResultStatus.UserCanceled
                        => VoiceSearchResult.Fail("Voice search cancelled."),
                    _
                        => VoiceSearchResult.Fail("No speech was recognised. Please try again.")
                };
            }
            catch (OperationCanceledException)
            {
                return VoiceSearchResult.Fail("Voice search cancelled.");
            }
            catch (Exception ex) when (IsSpeechPrivacyError(ex))
            {
                System.Diagnostics.Debug.WriteLine($"[WindowsSpeechHelper] Speech privacy: {ex}");
                return VoiceSearchResult.Fail(SpeechSettingsMessage, openSpeechSettings: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WindowsSpeechHelper] {ex}");
                var message = GetFriendlyMessage(ex);
                return VoiceSearchResult.Fail(message);
            }
            finally
            {
                recognizer?.Dispose();
            }
        });
    }

    public static async Task<bool> TryOpenSpeechSettingsAsync()
    {
        try
        {
            return await global::Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-speech"));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WindowsSpeechHelper] Open settings: {ex}");
            return false;
        }
    }

    private static bool IsSpeechPrivacyError(Exception ex) =>
        (uint)ex.HResult == HResultPrivacyStatementDeclined
        || ex.Message.Contains("privacy policy", StringComparison.OrdinalIgnoreCase)
        || ex.Message.Contains("speech privacy", StringComparison.OrdinalIgnoreCase)
        || (ex.InnerException is not null && IsSpeechPrivacyError(ex.InnerException));

    private static string GetFriendlyMessage(Exception ex)
    {
        if (IsSpeechPrivacyError(ex))
            return SpeechSettingsMessage;

        if (ex.InnerException is not null)
        {
            var inner = GetFriendlyMessage(ex.InnerException);
            if (inner != ex.InnerException.Message || !IsGenericHResultMessage(ex.Message))
                return inner;
        }

        if (!string.IsNullOrWhiteSpace(ex.Message) && !IsGenericHResultMessage(ex.Message))
            return ex.Message;

        return "Voice search failed. Allow microphone access in Windows Settings → Privacy → Microphone.";
    }

    private static bool IsGenericHResultMessage(string message) =>
        message.Contains("error code could not be found", StringComparison.OrdinalIgnoreCase);
}
