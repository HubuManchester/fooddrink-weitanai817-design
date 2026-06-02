using Android.OS;
using Android.Speech.Tts;
using FoodExplorer.Services;
using Java.Util;
using AndroidTextToSpeech = Android.Speech.Tts.TextToSpeech;
using Locale = Java.Util.Locale;

namespace FoodExplorer.Platforms.Android;

/// <summary>
/// Forces Android TTS to use en-US voice; MAUI SpeechOptions alone is ignored on some MIUI devices.
/// </summary>
internal sealed class AndroidTtsSpeaker : Java.Lang.Object, AndroidTextToSpeech.IOnInitListener,
#pragma warning disable CS0618
    AndroidTextToSpeech.IOnUtteranceCompletedListener
#pragma warning restore CS0618
{
    private readonly TaskCompletionSource<bool> _initTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly AndroidTextToSpeech _tts;
    private TaskCompletionSource<bool>? _utteranceTcs;
    private int _expectedUtterances;
    private int _completedUtterances;

    public AndroidTtsSpeaker()
    {
        _tts = new AndroidTextToSpeech(Platform.AppContext, this);
#pragma warning disable CS0618
        _tts.SetOnUtteranceCompletedListener(this);
#pragma warning restore CS0618
    }

    public Task WaitForInitAsync() => _initTcs.Task;

    public void OnInit(OperationResult status)
    {
        if (status == OperationResult.Success)
            _initTcs.TrySetResult(true);
        else
            _initTcs.TrySetException(new InvalidOperationException("Failed to initialize text-to-speech."));
    }

    public async Task<NarrationResult> SpeakAsync(string text, CancellationToken cancellationToken)
    {
        await WaitForInitAsync();

        var englishLocale = Locale.ForLanguageTag("en-US");
        var availability = _tts.IsLanguageAvailable(englishLocale);
        if (availability is not LanguageAvailableResult.Available
            and not LanguageAvailableResult.CountryAvailable
            and not LanguageAvailableResult.CountryVarAvailable)
        {
            englishLocale = Locale.English;
            availability = _tts.IsLanguageAvailable(englishLocale);
            if (availability is not LanguageAvailableResult.Available
                and not LanguageAvailableResult.CountryAvailable
                and not LanguageAvailableResult.CountryVarAvailable)
            {
                return NarrationResult.Fail(
                    "English voice is not installed. Download English (United States) in Settings → " +
                    "Language & input → Text-to-speech output.");
            }
        }

        _tts.SetLanguage(englishLocale);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop && _tts.Voices is not null)
        {
            var englishVoice = _tts.Voices?
                .FirstOrDefault(v =>
                    v.Locale?.Language?.Equals("en", StringComparison.OrdinalIgnoreCase) == true
                    && (v.Locale?.Country?.Equals("US", StringComparison.OrdinalIgnoreCase) == true
                        || v.Locale?.Country?.Equals("GB", StringComparison.OrdinalIgnoreCase) == true));

            englishVoice ??= _tts.Voices?
                .FirstOrDefault(v => v.Locale?.Language?.Equals("en", StringComparison.OrdinalIgnoreCase) == true);

            if (englishVoice is not null)
                _tts.SetVoice(englishVoice);
        }

        _tts.SetPitch(1.0f);
        _tts.SetSpeechRate(1.0f);

        var chunks = SplitText(text, AndroidTextToSpeech.MaxSpeechInputLength);
        _utteranceTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _expectedUtterances = chunks.Count;
        _completedUtterances = 0;

        using var registration = cancellationToken.Register(() =>
        {
            try
            {
                _tts.Stop();
                _utteranceTcs?.TrySetCanceled(cancellationToken);
            }
            catch
            {
                // ignored
            }
        });

        var utteranceRoot = Guid.NewGuid().ToString();
        for (var i = 0; i < chunks.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var map = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { AndroidTextToSpeech.Engine.KeyParamUtteranceId, $"{utteranceRoot}.{i}" }
            };

#pragma warning disable CS0618
            _tts.Speak(chunks[i], i == 0 ? QueueMode.Flush : QueueMode.Add, map);
#pragma warning restore CS0618
        }

        try
        {
            await _utteranceTcs.Task;
            return NarrationResult.Ok();
        }
        catch (System.OperationCanceledException)
        {
            return NarrationResult.Fail("Narration stopped.");
        }
    }

    public void OnUtteranceCompleted(string? utteranceId)
    {
        _completedUtterances++;
        if (_completedUtterances >= _expectedUtterances)
            _utteranceTcs?.TrySetResult(true);
    }

    public void Stop()
    {
        try
        {
            _tts.Stop();
        }
        catch
        {
            // ignored
        }
    }

    public void Shutdown()
    {
        try
        {
            _tts.Stop();
            _tts.Shutdown();
        }
        catch
        {
            // ignored
        }
    }

    private static List<string> SplitText(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return [text];

        var parts = new List<string>();
        var start = 0;
        while (start < text.Length)
        {
            var length = Math.Min(maxLength, text.Length - start);
            if (start + length < text.Length)
            {
                var breakAt = text.LastIndexOf(". ", start + length - 1, length, StringComparison.Ordinal);
                if (breakAt > start)
                    length = breakAt - start + 1;
            }

            parts.Add(text.Substring(start, length).Trim());
            start += length;
        }

        return parts;
    }
}

internal static class AndroidTextToSpeechHelper
{
    private static AndroidTtsSpeaker? _speaker;
    private static readonly object Sync = new();

    public static Task<NarrationResult> SpeakAsync(string text, CancellationToken cancellationToken)
    {
        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var speaker = GetOrCreateSpeaker();
            return await speaker.SpeakAsync(text, cancellationToken);
        });
    }

    public static void Stop()
    {
        lock (Sync)
        {
            _speaker?.Stop();
        }
    }

    private static AndroidTtsSpeaker GetOrCreateSpeaker()
    {
        lock (Sync)
        {
            if (_speaker is null)
            {
                _speaker = new AndroidTtsSpeaker();
            }

            return _speaker;
        }
    }
}
