using System.Diagnostics;
using System.Globalization;
using BosesApp.Core.Interfaces;
using CommunityToolkit.Maui.Media;

namespace BosesApp.Core.Services;

/// <summary>
/// Phase 1 — Speech-to-Text via MAUI Community Toolkit SpeechToText.
///
/// ?? SIMULATION MODE (SimulationMode = true — DEFAULT for demo) ??????????????
/// ?  • StartListeningAsync ? returns true immediately, fires a fake partial   ?
/// ?    result event after 800 ms with a canned Filipino phrase.               ?
/// ?  • StopListeningAsync  ? returns a random banking phrase; fires final     ?
/// ?    OnRecognitionResultUpdated with IsFinal = true.                        ?
/// ?  • RecognizeAsync      ? returns a canned phrase for any audio buffer.    ?
/// ?  No microphone, no network, no Android API-level restriction required.    ?
/// ????????????????????????????????????????????????????????????????????????????
///
/// ?? PRODUCTION PATH (SimulationMode = false) ???????????????????????????????
/// ?  Uses CommunityToolkit.Maui.Media.ISpeechToText, which routes to:        ?
/// ?    Android   ? SpeechRecognizer  (requires API 33+ for offline)          ?
/// ?    iOS/macOS ? SFSpeechRecognizer                                         ?
/// ?    Windows   ? Windows.Media.SpeechRecognition                           ?
/// ?                                                                           ?
/// ?  Deepgram upgrade path:                                                   ?
/// ?    Replace this class with DeepgramSpeechRecognitionService that opens    ?
/// ?    a WebSocket to wss://api.deepgram.com/v1/listen during recording.     ?
/// ????????????????????????????????????????????????????????????????????????????
/// </summary>
public class MauiSpeechRecognitionService : ISpeechRecognitionService
{
    // ?? Simulation flag ????????????????????????????????????????????????????????
    /// <summary>
    /// TRUE  = mock transcriptions — demo-safe, no microphone required.
    /// FALSE = CommunityToolkit on-device STT (StartListenAsync / StopListenAsync).
    /// Set to false and provide microphone permission to use real recognition.
    /// </summary>
    public bool SimulationMode { get; set; } = true; // TODO: flip to false on a real device

    private readonly ISpeechToText _speechToText;
    private string? _latestPartialResult;

    // ?? ISpeechRecognitionService contract ?????????????????????????????????????
    public event EventHandler<RecognitionResultEventArgs>? OnRecognitionResultUpdated;
    public bool IsRealRecognitionAvailable { get; private set; }

    // ?? Canned demo phrases (simulation) ??????????????????????????????????????
    private static readonly string[] _filipinoPhrases =
    [
        "Magkano ang balance ko?",
        "Ipadala ang 500 pesos kay Juan",
        "Ano ang mga recent transactions ko?",
        "Gusto kong mag-transfer ng pera",
        "Kalkulahin ang PWD discount para sa gamot",
        "Bayaran ang kuryente bill",
        "Tingnan ang aking loan status"
    ];

    private static readonly string[] _englishPhrases =
    [
        "my voice is my password",
        "i authorize this transaction",
        "this is my secure voice",
        "check my account balance",
        "show recent transactions"
    ];

    // ?? Constructor ????????????????????????????????????????????????????????????
    public MauiSpeechRecognitionService(ISpeechToText speechToText)
    {
        _speechToText = speechToText;

        // Wire up toolkit events (used in production path)
        _speechToText.RecognitionResultUpdated   += OnToolkitResultUpdated;
        _speechToText.RecognitionResultCompleted += OnToolkitResultCompleted;

        IsRealRecognitionAvailable = DetectPlatformAvailability();

        Debug.WriteLine(IsRealRecognitionAvailable
            ? "[STT] ? Real recognition available on this platform"
            : "[STT] ??  Real recognition not available — simulation will be used");
    }

    // ?? ISpeechRecognitionService — Start ?????????????????????????????????????
    public async Task<bool> StartListeningAsync(string language = "en-US")
    {
        Debug.WriteLine($"[STT] StartListeningAsync — lang={language} sim={SimulationMode}");
        _latestPartialResult = null;

        if (SimulationMode || !IsRealRecognitionAvailable)
        {
            // [SIMULATION] Fire a partial result after a short delay
            _ = Task.Run(async () =>
            {
                await Task.Delay(800);
                var partial = PickPhrase(language);
                _latestPartialResult = partial;
                RaiseResult(partial[..Math.Min(14, partial.Length)] + "…", 0.72, isFinal: false);
            });
            return true;
        }

        // ?? PRODUCTION PATH ????????????????????????????????????????????????????
        try
        {
            var micGranted = await Permissions.RequestAsync<Permissions.Microphone>();
            if (micGranted != PermissionStatus.Granted)
            {
                Debug.WriteLine("[STT] ? Microphone permission denied — falling back to simulation");
                IsRealRecognitionAvailable = false;
                return true; // allow simulation to take over
            }

            await _speechToText.RequestPermissions(CancellationToken.None);

            var lang = NormaliseLanguage(language);
            var opts = new SpeechToTextOptions
            {
                Culture                    = CultureInfo.GetCultureInfo(lang),
                ShouldReportPartialResults = true
            };

            await _speechToText.StartListenAsync(opts, CancellationToken.None);
            Debug.WriteLine("[STT] ? Real recognition started");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[STT] ? StartListenAsync failed: {ex.Message}");

            // Language fallback — retry with en-US
            if (!language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine("[STT] ?? Retrying with en-US fallback");
                try
                {
                    var opts = new SpeechToTextOptions
                    {
                        Culture                    = CultureInfo.GetCultureInfo("en-US"),
                        ShouldReportPartialResults = true
                    };
                    await _speechToText.StartListenAsync(opts, CancellationToken.None);
                    return true;
                }
                catch (Exception fallbackEx)
                {
                    Debug.WriteLine($"[STT] ? en-US fallback also failed: {fallbackEx.Message}");
                }
            }

            IsRealRecognitionAvailable = false;
            return true; // simulation takes over
        }
    }

    // ?? ISpeechRecognitionService — Stop ??????????????????????????????????????
    public async Task<string?> StopListeningAsync()
    {
        Debug.WriteLine($"[STT] StopListeningAsync — sim={SimulationMode}");

        if (SimulationMode || !IsRealRecognitionAvailable)
        {
            // [SIMULATION] Return the partial phrase already generated (or pick a new one)
            await Task.Delay(400);
            var result = string.IsNullOrEmpty(_latestPartialResult)
                ? PickPhrase("fil-PH")
                : _latestPartialResult;
            _latestPartialResult = null;

            RaiseResult(result, 0.91, isFinal: true);
            Debug.WriteLine($"[STT][SIM] ? Final: {result}");
            return result;
        }

        // ?? PRODUCTION PATH ????????????????????????????????????????????????????
        try
        {
            await _speechToText.StopListenAsync(CancellationToken.None);
            Debug.WriteLine("[STT] ?? Real recognition stopped");

            // Best result comes from partial events already stored in _latestPartialResult
            var result = _latestPartialResult;
            _latestPartialResult = null;

            if (!string.IsNullOrWhiteSpace(result))
                Debug.WriteLine($"[STT] ? Returning: '{result}'");
            else
                Debug.WriteLine("[STT] ?? No speech detected");

            return string.IsNullOrWhiteSpace(result) ? null : result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[STT] ? StopListenAsync failed: {ex.Message}");
            IsRealRecognitionAvailable = false;
            return null;
        }
    }

    // ?? ISpeechRecognitionService — Batch recognise from byte array ???????????
    public Task<string?> RecognizeAsync(byte[] audioData, string language = "en-US")
    {
        // CommunityToolkit does not support batch recognition from a byte array —
        // it only processes live microphone input. This method supports the interface
        // contract and returns a simulation result.
        //
        // PRODUCTION TODO: pipe audioData to a batch STT endpoint, e.g.:
        //   POST https://api.deepgram.com/v1/listen  (Deepgram)
        //   POST https://speech.googleapis.com/v1/speech:recognize  (Google)
        Debug.WriteLine("[STT] RecognizeAsync: batch mode not supported by toolkit — using simulation");
        var phrase = audioData.Length > 100 ? PickPhrase(language) : null;
        return Task.FromResult<string?>(phrase);
    }

    // ?? ISpeechRecognitionService — Phrase validation ?????????????????????????
    public bool ValidatePhrase(string recognizedText, string expectedPhrase, double threshold = 0.7)
    {
        if (string.IsNullOrWhiteSpace(recognizedText) || string.IsNullOrWhiteSpace(expectedPhrase))
            return false;

        var sim = CalculateSimilarity(recognizedText, expectedPhrase);
        var ok  = sim >= threshold;
        Debug.WriteLine($"[STT] ValidatePhrase: sim={sim:P1} (threshold {threshold:P0}) ? {(ok ? "PASS ?" : "FAIL ?")}");
        return ok;
    }

    /// <summary>
    /// Levenshtein-based similarity (0–1).  Normalised so identical strings = 1.0.
    /// </summary>
    public double CalculateSimilarity(string text1, string text2)
    {
        if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2)) return 0;

        var a = Normalise(text1);
        var b = Normalise(text2);
        if (a == b) return 1.0;

        var dist = LevenshteinDistance(a, b);
        var max  = Math.Max(a.Length, b.Length);
        return max == 0 ? 1.0 : 1.0 - (double)dist / max;
    }

    // ?? Toolkit event forwarders (production) ?????????????????????????????????

    private void OnToolkitResultUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.RecognitionResult)) return;
        _latestPartialResult = e.RecognitionResult;
        Debug.WriteLine($"[STT] Partial: {e.RecognitionResult}");
        RaiseResult(e.RecognitionResult, 0.85, isFinal: false);
    }

    private void OnToolkitResultCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
    {
        if (e.RecognitionResult?.IsSuccessful == true &&
            !string.IsNullOrWhiteSpace(e.RecognitionResult.Text))
        {
            _latestPartialResult = e.RecognitionResult.Text;
            Debug.WriteLine($"[STT] Final: {e.RecognitionResult.Text}");
            RaiseResult(e.RecognitionResult.Text, 0.95, isFinal: true);
        }
        else
        {
            Debug.WriteLine($"[STT] Completed but no result — exception: {e.RecognitionResult?.Exception?.Message ?? "none"}");
        }
    }

    // ?? Helpers ????????????????????????????????????????????????????????????????

    private void RaiseResult(string text, double confidence, bool isFinal) =>
        OnRecognitionResultUpdated?.Invoke(this, new RecognitionResultEventArgs
        {
            RecognizedText = text,
            Confidence     = confidence,
            IsFinal        = isFinal
        });

    private static string PickPhrase(string language)
    {
        bool isFilipino = language.StartsWith("fil", StringComparison.OrdinalIgnoreCase)
                       || language.StartsWith("tl",  StringComparison.OrdinalIgnoreCase);
        var pool = isFilipino ? _filipinoPhrases : _englishPhrases;
        return pool[Random.Shared.Next(pool.Length)];
    }

    /// <summary>
    /// Maps Filipino/Tagalog locales to en-US because most Android devices lack a
    /// fil-PH STT model. Change this when a Filipino STT model is available.
    /// </summary>
    private static string NormaliseLanguage(string lang) => lang.ToLowerInvariant() switch
    {
        "fil"    => "en-US",
        "fil-ph" => "en-US",
        "tl"     => "en-US",
        "tl-ph"  => "en-US",
        "en"     => "en-US",
        _        => lang
    };

    private static string Normalise(string s) =>
        string.Join(" ", s.ToLowerInvariant()
            .Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries));

    private static int LevenshteinDistance(string s1, string s2)
    {
        int[,] m = new int[s1.Length + 1, s2.Length + 1];
        for (int i = 0; i <= s1.Length; i++) m[i, 0] = i;
        for (int j = 0; j <= s2.Length; j++) m[0, j] = j;
        for (int i = 1; i <= s1.Length; i++)
            for (int j = 1; j <= s2.Length; j++)
                m[i, j] = Math.Min(
                    Math.Min(m[i - 1, j] + 1, m[i, j - 1] + 1),
                    m[i - 1, j - 1] + (s1[i - 1] == s2[j - 1] ? 0 : 1));
        return m[s1.Length, s2.Length];
    }

    private static bool DetectPlatformAvailability()
    {
        try
        {
#if ANDROID
            var api = Android.OS.Build.VERSION.SdkInt;
            var ok  = api >= Android.OS.BuildVersionCodes.Tiramisu; // API 33
            Debug.WriteLine($"[STT] Android API {(int)api} — real STT {(ok ? "available" : "requires 33+")}");
            return ok;
#elif IOS || MACCATALYST
            Debug.WriteLine("[STT] iOS/macOS — SFSpeechRecognizer available");
            return true;
#elif WINDOWS
            Debug.WriteLine("[STT] Windows — Windows.Media.SpeechRecognition available");
            return true;
#else
            return false;
#endif
        }
        catch
        {
            return false;
        }
    }
}
