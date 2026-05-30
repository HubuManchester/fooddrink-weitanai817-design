using Android.Content;
using Android.OS;
using Android.Speech;
using FoodExplorer.Services;

namespace FoodExplorer.Platforms.Android;

internal class SpeechRecognitionListener : Java.Lang.Object, IRecognitionListener
{
    private readonly TaskCompletionSource<VoiceSearchResult> _tcs;
    private readonly SpeechRecognizer _recognizer;

    public SpeechRecognitionListener(
        TaskCompletionSource<VoiceSearchResult> tcs,
        SpeechRecognizer recognizer)
    {
        _tcs = tcs;
        _recognizer = recognizer;
    }

    public void OnResults(Bundle? results)
    {
        var matches = results?.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
        var text = matches?.FirstOrDefault();
        _recognizer.Destroy();

        if (string.IsNullOrWhiteSpace(text))
            _tcs.TrySetResult(VoiceSearchResult.Fail("No speech was recognised. Please try again."));
        else
            _tcs.TrySetResult(VoiceSearchResult.Ok(text));
    }

    public void OnError(SpeechRecognizerError error)
    {
        _recognizer.Destroy();
        var message = error switch
        {
            SpeechRecognizerError.NoMatch => "No speech match found. Try speaking clearly.",
            SpeechRecognizerError.Audio => "Audio recording error. Check your microphone.",
            SpeechRecognizerError.Client => "Voice search client error.",
            SpeechRecognizerError.InsufficientPermissions => "Microphone permission required.",
            SpeechRecognizerError.Network => "Network error during voice recognition.",
            SpeechRecognizerError.NetworkTimeout => "Voice recognition timed out.",
            _ => $"Voice search error: {error}"
        };
        _tcs.TrySetResult(VoiceSearchResult.Fail(message));
    }

    public void OnBeginningOfSpeech() { }
    public void OnBufferReceived(byte[]? buffer) { }
    public void OnEndOfSpeech() { }
    public void OnEvent(int eventType, Bundle? @params) { }
    public void OnPartialResults(Bundle? partialResults) { }
    public void OnReadyForSpeech(Bundle? @params) { }
    public void OnRmsChanged(float rmsdB) { }
}

public static class AndroidSpeechHelper
{
    public static Task<VoiceSearchResult> ListenAsync(CancellationToken cancellationToken)
    {
        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        if (activity is null)
            return Task.FromResult(VoiceSearchResult.Fail("Unable to start voice search on this device."));

        var tcs = new TaskCompletionSource<VoiceSearchResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        cancellationToken.Register(() =>
        {
            tcs.TrySetResult(VoiceSearchResult.Fail("Voice search cancelled."));
        });

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!SpeechRecognizer.IsRecognitionAvailable(activity))
            {
                tcs.TrySetResult(VoiceSearchResult.Fail("Speech recognition is not available on this device."));
                return;
            }

            var recognizer = SpeechRecognizer.CreateSpeechRecognizer(activity);
            if (recognizer is null)
            {
                tcs.TrySetResult(VoiceSearchResult.Fail("Could not start speech recognition on this device."));
                return;
            }

            recognizer.SetRecognitionListener(new SpeechRecognitionListener(tcs, recognizer));

            var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            intent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.English);
            intent.PutExtra(RecognizerIntent.ExtraPrompt, "Say a recipe name, cuisine, or category…");
            intent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

            recognizer.StartListening(intent);
        });

        return tcs.Task;
    }
}
