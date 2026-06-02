using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FoodExplorer.Services;

/// <summary>
/// Hardware #4 — Text-to-Speech narration in English only.
/// Android uses native TTS with en-US; other platforms use MAUI TextToSpeech.
/// </summary>
public class SpeechNarrationService : ISpeechNarrationService
{
    private const int MaxChunkLength = 3500;

    private static readonly Regex CjkCharacters = new(
        @"[\u3040-\u30ff\u3400-\u4dbf\u4e00-\u9fff\uf900-\ufaff]",
        RegexOptions.Compiled);

    private readonly object _sync = new();
    private CancellationTokenSource? _speakCts;

    public bool IsSpeaking
    {
        get
        {
            lock (_sync)
                return _speakCts is not null && !_speakCts.IsCancellationRequested;
        }
    }

    public async Task<NarrationResult> SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return NarrationResult.Fail("Nothing to read aloud.");

        await StopAsync();

        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        lock (_sync)
            _speakCts = cts;

        try
        {
            var speakableText = PrepareEnglishSpeakableText(text);
            if (string.IsNullOrWhiteSpace(speakableText))
                return NarrationResult.Fail("Nothing to read aloud in English.");

#if ANDROID
            return await Platforms.Android.AndroidTextToSpeechHelper.SpeakAsync(speakableText, cts.Token);
#else
            return await MainThread.InvokeOnMainThreadAsync(async () =>
                await SpeakWithMauiAsync(speakableText, cts.Token));
#endif
        }
        finally
        {
            lock (_sync)
            {
                if (ReferenceEquals(_speakCts, cts))
                {
                    _speakCts = null;
                    SafeDispose(cts);
                }
            }
        }
    }

    public Task StopAsync()
    {
        CancellationTokenSource? cts;
        lock (_sync)
        {
            cts = _speakCts;
            _speakCts = null;
        }

#if ANDROID
        Platforms.Android.AndroidTextToSpeechHelper.Stop();
#endif

        if (cts is null)
            return Task.CompletedTask;

        SafeCancel(cts);
        SafeDispose(cts);
        return Task.CompletedTask;
    }

    private static async Task<NarrationResult> SpeakWithMauiAsync(string text, CancellationToken token)
    {
        try
        {
            var locales = (await TextToSpeech.Default.GetLocalesAsync()).ToList();
            if (locales.Count == 0)
            {
                return NarrationResult.Fail(
                    "Text-to-speech is not available on this device.");
            }

            var locale = SelectEnglishLocale(locales);
            if (locale is null)
            {
                return NarrationResult.Fail(
                    "English text-to-speech is not installed on this device.");
            }

            var options = new SpeechOptions
            {
                Pitch = 1.0f,
                Volume = 1.0f,
                Locale = locale
            };

            foreach (var chunk in ChunkText(text))
            {
                token.ThrowIfCancellationRequested();
                await TextToSpeech.Default.SpeakAsync(chunk, options, token);
            }

            return NarrationResult.Ok();
        }
        catch (OperationCanceledException)
        {
            return NarrationResult.Fail("Narration stopped.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SpeechNarrationService] {ex}");
            return NarrationResult.Fail($"Text-to-speech error: {ex.Message}");
        }
    }

    private static Locale? SelectEnglishLocale(IReadOnlyList<Locale> locales)
    {
        static bool IsEnglish(Locale l) =>
            l.Language.Equals("en", StringComparison.OrdinalIgnoreCase)
            || l.Language.Equals("eng", StringComparison.OrdinalIgnoreCase);

        var english = locales.Where(IsEnglish).ToList();
        if (english.Count == 0)
            return null;

        var usVoice = english.FirstOrDefault(l =>
            l.Name.Contains("United States", StringComparison.OrdinalIgnoreCase)
            || l.Name.Contains("U.S.", StringComparison.OrdinalIgnoreCase)
            || string.Equals(l.Country, "US", StringComparison.OrdinalIgnoreCase));

        if (usVoice is not null && !string.IsNullOrEmpty(usVoice.Id))
            return usVoice;

        string[] preferredCountries = ["US", "GB", "AU", "CA"];
        foreach (var country in preferredCountries)
        {
            var match = english.FirstOrDefault(l =>
                string.Equals(l.Country, country, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(l.Id));
            if (match is not null)
                return match;
        }

        return english.FirstOrDefault(l => !string.IsNullOrEmpty(l.Id)) ?? english.FirstOrDefault();
    }

    private static string PrepareEnglishSpeakableText(string text)
    {
        var cleaned = CjkCharacters.Replace(text, " ");
        cleaned = cleaned.Replace('°', ' ');

        var sb = new StringBuilder(cleaned.Length);
        foreach (var ch in cleaned)
        {
            if (ch < 128 || char.IsWhiteSpace(ch))
                sb.Append(ch);
        }

        cleaned = Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
        return EnglishNumberSpeller.SpellDigitsInText(cleaned);
    }

    private static IEnumerable<string> ChunkText(string text)
    {
        if (text.Length <= MaxChunkLength)
        {
            yield return text;
            yield break;
        }

        var start = 0;
        while (start < text.Length)
        {
            var length = Math.Min(MaxChunkLength, text.Length - start);
            if (start + length < text.Length)
            {
                var breakAt = text.LastIndexOf(". ", start + length - 1, length, StringComparison.Ordinal);
                if (breakAt > start)
                    length = breakAt - start + 1;
            }

            yield return text.Substring(start, length).Trim();
            start += length;
        }
    }

    private static void SafeCancel(CancellationTokenSource cts)
    {
        try
        {
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private static void SafeDispose(CancellationTokenSource cts)
    {
        try
        {
            cts.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }
    }
}
