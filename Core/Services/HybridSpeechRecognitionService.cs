using System.Diagnostics;
using System.Globalization;
using BosesApp.Core.Interfaces;
using CommunityToolkit.Maui.Media;

namespace BosesApp.Core.Services;

/// <summary>
/// Hybrid Speech-to-Text service with a 3-tier fallback chain:
///
///   Tier 1 — Native platform STT via CommunityToolkit.Maui.Media
///             • Windows  ? Windows.Media.SpeechRecognition  (en-US only)
///             • Android  ? Android SpeechRecognizer          (en-US only, API 33+)
///             • iOS      ? SFSpeechRecognizer                (en-US only)
///             Used automatically for English.
///
///   Tier 2 — Deepgram REST API  (cloud, requires internet)
///             • Tagalog / Filipino ? language=tl  ?
///             • English            ? language=en-US
///             Used automatically for Tagalog, or as fallback when Tier 1 fails.
///
///   Tier 3 — Simulation (canned phrases)
///             Used when both Tier 1 and Tier 2 are unavailable.
/// </summary>
public class HybridSpeechRecognitionService : ISpeechRecognitionService
{
    // ?? Dependencies ???????????????????????????????????????????????????????????
    private readonly ISpeechToText _nativeStt;       // Tier 1
    private readonly DeepgramSpeechRecognitionService _deepgram;  // Tier 2

    // ?? State ??????????????????????????????????????????????????????????????????
    private string _currentLanguage = "en-US";
    private bool _nativeAvailable;
    private bool _usingNative;          // true = current session uses Tier 1
    private string? _latestPartialResult;

    // ?? ISpeechRecognitionService contract ?????????????????????????????????????
    public event EventHandler<RecognitionResultEventArgs>? OnRecognitionResultUpdated;
    public bool IsRealRecognitionAvailable => _nativeAvailable || _deepgram.IsRealRecognitionAvailable;
    public bool SimulationMode { get; set; } = false;

    // ?? Canned demo phrases (Tier 3 simulation) ????????????????????????????????
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
    public HybridSpeechRecognitionService(
        ISpeechToText nativeStt,
        DeepgramSpeechRecognitionService deepgram)
    {
        _nativeStt = nativeStt;
        _deepgram = deepgram;

        // Wire up CommunityToolkit events for partial/final results (Tier 1)
        _nativeStt.RecognitionResultUpdated += OnNativeResultUpdated;
        _nativeStt.RecognitionResultCompleted += OnNativeResultCompleted;

        // Wire up Deepgram events (Tier 2)
        _deepgram.OnRecognitionResultUpdated += (s, e) =>
            OnRecognitionResultUpdated?.Invoke(this, e);

        _nativeAvailable = DetectNativePlatformAvailability();

        Debug.WriteLine("[Hybrid] ??????????????????????????????????????????");
        Debug.WriteLine($"[Hybrid] Tier 1 (Native CommunityToolkit): {(_nativeAvailable ? "? available" : "? unavailable")}");
        Debug.WriteLine($"[Hybrid] Tier 2 (Deepgram cloud):          {(_deepgram.IsRealRecognitionAvailable ? "? available" : "? no API key")}");
        Debug.WriteLine("[Hybrid] ??????????????????????????????????????????");
    }

    // ?? Start ??????????????????????????????????????????????????????????????????
    public async Task<bool> StartListeningAsync(string language = "en-US")
    {
        _currentLanguage = language;
        _latestPartialResult = null;
        _usingNative = false;

        Debug.WriteLine($"[Hybrid] StartListeningAsync — lang={language} sim={SimulationMode}");

        // ?? Tier 3: simulation ?????????????????????????????????????????????????
        if (SimulationMode || !IsRealRecognitionAvailable)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(800);
                var partial = PickPhrase(language);
                _latestPartialResult = partial;
                RaiseResult(partial[..Math.Min(14, partial.Length)] + "…", 0.72, isFinal: false);
            });
            return true;
        }

        bool isFilipino = IsFilipino(language);

        // ?? Tier 1: native platform (English only) ?????????????????????????????
        //if (_nativeAvailable && !isFilipino)
        //{
        //    try
        //    {
        //        var micGranted = await Permissions.RequestAsync<Permissions.Microphone>();
        //        if (micGranted != PermissionStatus.Granted)
        //            throw new PermissionException("Microphone permission denied");

        //        await _nativeStt.RequestPermissions(CancellationToken.None);

        //        var opts = new SpeechToTextOptions
        //        {
        //            Culture = CultureInfo.GetCultureInfo("en-US"),
        //            ShouldReportPartialResults = true
        //        };

        //        await _nativeStt.StartListenAsync(opts, CancellationToken.None);
        //        _usingNative = true;
        //        Debug.WriteLine("[Hybrid] ? Tier 1 (native) started");
        //        return true;
        //    }
        //    catch (PermissionException permEx)
        //    {
        //        Debug.WriteLine($"[Hybrid] ?? Tier 1 permission error: {permEx.Message}");
        //        Debug.WriteLine("[Hybrid] ?? On Windows: Settings ? Privacy & Security ? Speech ? enable Online Speech Recognition");
        //        _nativeAvailable = false;
        //        // fall through to Tier 2
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"[Hybrid] ?? Tier 1 failed: {ex.Message} — falling to Tier 2");
        //        _nativeAvailable = false;
        //        // fall through to Tier 2
        //    }
        //}

        // ?? Tier 2: Deepgram (Tagalog + English fallback) ??????????????????????
        if (_deepgram.IsRealRecognitionAvailable)
        {
            Debug.WriteLine($"[Hybrid] ?? Using Tier 2 (Deepgram) — lang={language}");
            return await _deepgram.StartListeningAsync(language);
        }

        // ?? Tier 3: simulation ?????????????????????????????????????????????????
        Debug.WriteLine("[Hybrid] ?? Both tiers unavailable — using simulation");
        SimulationMode = true;
        _ = Task.Run(async () =>
        {
            await Task.Delay(800);
            var partial = PickPhrase(language);
            _latestPartialResult = partial;
            RaiseResult(partial[..Math.Min(14, partial.Length)] + "…", 0.72, isFinal: false);
        });
        return true;
    }

    // ?? Stop ???????????????????????????????????????????????????????????????????
    public async Task<string?> StopListeningAsync(Byte[] audioData)
    {
        Debug.WriteLine($"[Hybrid] StopListeningAsync — native={_usingNative} sim={SimulationMode}");

        // ?? Tier 3: simulation ?????????????????????????????????????????????????
        if (SimulationMode || !IsRealRecognitionAvailable)
        {
            await Task.Delay(400);
            var sim = string.IsNullOrEmpty(_latestPartialResult)
                ? PickPhrase(_currentLanguage)
                : _latestPartialResult;
            _latestPartialResult = null;
            RaiseResult(sim, 0.91, isFinal: true);
            return sim;
        }

        // ?? Tier 1: native ?????????????????????????????????????????????????????
        if (_usingNative)
        {
            try
            {
                await _nativeStt.StopListenAsync(CancellationToken.None);
                Debug.WriteLine("[Hybrid] ?? Tier 1 stopped");

                var result = _latestPartialResult;
                _latestPartialResult = null;

                if (!string.IsNullOrWhiteSpace(result))
                {
                    Debug.WriteLine($"[Hybrid] ? Tier 1 result: '{result}'");
                    return result;
                }

                Debug.WriteLine("[Hybrid] ?? Tier 1 returned no speech — falling to Tier 2 batch");
                // fall through to Tier 2 batch below
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Hybrid] ?? Tier 1 stop failed: {ex.Message}");
            }
        }

        // ?? Tier 2: Deepgram ???????????????????????????????????????????????????
        if (_deepgram.IsRealRecognitionAvailable)
        {
            return await _deepgram.StopListeningAsync(audioData);
        }

        return null;
    }

    // ?? Batch recognition ??????????????????????????????????????????????????????
    public async Task<string?> RecognizeAsync(byte[] audioData, string language = "en-US")
    {
        // Native CommunityToolkit does not support batch — always delegate to Deepgram
        if (_deepgram.IsRealRecognitionAvailable)
            return await _deepgram.RecognizeAsync(audioData, language);

        Debug.WriteLine("[Hybrid] ?? Batch recognize: Deepgram unavailable — using simulation");
        return audioData.Length > 100 ? PickPhrase(language) : null;
    }

    // ?? Phrase validation (shared logic) ??????????????????????????????????????
    public bool ValidatePhrase(string recognizedText, string expectedPhrase, double threshold = 0.7)
    {
        if (string.IsNullOrWhiteSpace(recognizedText) || string.IsNullOrWhiteSpace(expectedPhrase))
            return false;

        var sim = CalculateSimilarity(recognizedText, expectedPhrase);
        var ok = sim >= threshold;
        Debug.WriteLine($"[Hybrid] ValidatePhrase: sim={sim:P1} (threshold {threshold:P0}) ? {(ok ? "PASS ?" : "FAIL ?")}");
        return ok;
    }

    public double CalculateSimilarity(string text1, string text2)
    {
        if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2)) return 0;
        var a = Normalise(text1);
        var b = Normalise(text2);
        if (a == b) return 1.0;
        var dist = LevenshteinDistance(a, b);
        var max = Math.Max(a.Length, b.Length);
        return max == 0 ? 1.0 : 1.0 - (double)dist / max;
    }

    // ?? Native CommunityToolkit event handlers (Tier 1) ???????????????????????
    private void OnNativeResultUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.RecognitionResult) || !_usingNative) return;
        _latestPartialResult = e.RecognitionResult;
        Debug.WriteLine($"[Hybrid][Native] Partial: {e.RecognitionResult}");
        RaiseResult(e.RecognitionResult, 0.85, isFinal: false);
    }

    private void OnNativeResultCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
    {
        if (!_usingNative) return;
        if (e.RecognitionResult?.IsSuccessful == true &&
            !string.IsNullOrWhiteSpace(e.RecognitionResult.Text))
        {
            _latestPartialResult = e.RecognitionResult.Text;
            Debug.WriteLine($"[Hybrid][Native] Final: {e.RecognitionResult.Text}");
            RaiseResult(e.RecognitionResult.Text, 0.95, isFinal: true);
        }
        else
        {
            Debug.WriteLine($"[Hybrid][Native] Completed — no result: {e.RecognitionResult?.Exception?.Message ?? "none"}");
        }
    }

    // ?? Helpers ????????????????????????????????????????????????????????????????
    private void RaiseResult(string text, double confidence, bool isFinal) =>
        OnRecognitionResultUpdated?.Invoke(this, new RecognitionResultEventArgs
        {
            RecognizedText = text,
            Confidence = confidence,
            IsFinal = isFinal
        });

    private static bool IsFilipino(string language) =>
        language.StartsWith("fil", StringComparison.OrdinalIgnoreCase) ||
        language.StartsWith("tl", StringComparison.OrdinalIgnoreCase);

    private static string PickPhrase(string language) =>
        IsFilipino(language)
            ? _filipinoPhrases[Random.Shared.Next(_filipinoPhrases.Length)]
            : _englishPhrases[Random.Shared.Next(_englishPhrases.Length)];

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

    /// <summary>
    /// Detects whether the native CommunityToolkit STT can be used on this platform.
    /// Windows requires "Online Speech Recognition" ON in Privacy settings.
    /// Android requires API 33+ (Tiramisu).
    /// </summary>
    private static bool DetectNativePlatformAvailability()
    {
        try
        {
#if ANDROID
            var api = Android.OS.Build.VERSION.SdkInt;
            var ok = api >= Android.OS.BuildVersionCodes.Tiramisu;
            Debug.WriteLine($"[Hybrid] Android API {(int)api} — native STT {(ok ? "available" : "requires API 33+")}");
            return ok;
#elif IOS || MACCATALYST
            Debug.WriteLine("[Hybrid] iOS/macOS — SFSpeechRecognizer available");
            return true;
#elif WINDOWS
            Debug.WriteLine("[Hybrid] Windows — Windows.Media.SpeechRecognition available (pending privacy setting)");
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
