using System.Diagnostics;
using System.Text.Json;
using BosesApp.Core.Interfaces;

namespace BosesApp.Core.Services;

/// <summary>
/// Speech-to-Text via Deepgram REST API.
/// Free tier: 12,000 minutes/year — https://console.deepgram.com
///
/// Supports:
///   • Tagalog / Filipino  ? language=tl
///   • English             ? language=en-US
///
/// Flow:
///   StartListeningAsync ? starts Plugin.Maui.Audio microphone recording
///   StopListeningAsync  ? stops recording, POSTs WAV bytes to Deepgram, fires OnRecognitionResultUpdated
/// </summary>
public class DeepgramSpeechRecognitionService : ISpeechRecognitionService
{
    // Configuration
    /// <summary>
    /// Replace this with your Deepgram API key from https://console.deepgram.com
    /// ?? Do NOT commit real keys to source control — move to config/secrets in production.
    /// </summary>
    public const string DefaultApiKey = "d7f32931c45ef98a2c1a85cd7575c7e654b486c6";

    private readonly IAudioRecordingService _audioRecorder;
    private readonly HttpClient             _http;
    private readonly string                 _apiKey;
    private string                          _currentLanguage = "en-US";

    // ?? ISpeechRecognitionService contract 
    public event EventHandler<RecognitionResultEventArgs>? OnRecognitionResultUpdated;
    public bool IsRealRecognitionAvailable => !string.IsNullOrWhiteSpace(_apiKey);
    public bool SimulationMode             { get; set; } = false;

    // ?? Canned demo phrases (simulation fallback) ??????????????????????????????
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
    public DeepgramSpeechRecognitionService(IAudioRecordingService audioRecorder, string apiKey)
    {
        _audioRecorder = audioRecorder;
        _apiKey        = apiKey;
        _http          = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        _http.DefaultRequestHeaders.Add("Authorization", $"Token {_apiKey}");

        Debug.WriteLine(IsRealRecognitionAvailable
            ? "[Deepgram] ? Initialized — real Tagalog/English recognition ready"
            : "[Deepgram] ??  No API key — falling back to simulation");
    }

    // ?? Start: begin microphone capture ???????????????????????????????????????
    public async Task<bool> StartListeningAsync(string language = "en-US")
    {
        _currentLanguage = language;
        Debug.WriteLine($"[Deepgram] StartListeningAsync — lang={language} sim={SimulationMode}");

        if (SimulationMode || !IsRealRecognitionAvailable)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(800);
                var partial = PickPhrase(language);
                RaiseResult(partial[..Math.Min(14, partial.Length)] + "…", 0.72, isFinal: false);
            });
            return true;
        }

        var started = await _audioRecorder.StartRecordingAsync();
        if (!started)
            Debug.WriteLine("[Deepgram] ?? Audio recorder failed to start — check microphone permission");

        return started;
    }

    // ?? Stop: send audio to Deepgram, fire result event ???????????????????????
    public async Task<string?> StopListeningAsync(byte[]? audioData = null)
    {
        Debug.WriteLine($"[Deepgram] StopListeningAsync — sim={SimulationMode}");

        if (SimulationMode || !IsRealRecognitionAvailable)
        {
            await Task.Delay(400);
            var sim = PickPhrase(_currentLanguage);
            RaiseResult(sim, 0.91, isFinal: true);
            return sim;
        }

        if (audioData == null)
            audioData = await _audioRecorder.StopRecordingAsync();

        if (audioData.Length == 0)
        {
            Debug.WriteLine("[Deepgram] ?? No audio data captured");
            return null;
        }

        var result = await RecognizeAsync(audioData, _currentLanguage);

        if (!string.IsNullOrWhiteSpace(result))
        {
            Debug.WriteLine($"[Deepgram] ? Final: {result}");
            RaiseResult(result, 0.95, isFinal: true);
        }
        else
        {
            Debug.WriteLine("[Deepgram] ?? No speech detected in audio");
        }

        return result;
    }

    // ?? Batch: POST audio bytes to Deepgram REST API ??????????????????????????
    public async Task<string?> RecognizeAsync(byte[] audioData, string language = "en-US")
    {
        if (!IsRealRecognitionAvailable || audioData.Length == 0)
            return PickPhrase(language);

        try
        {
            var lang = NormaliseLanguage(language);
            var url  = $"https://api.deepgram.com/v1/listen?language={lang}&model=nova-3-general&punctuate=true&encoding=linear16&sample_rate=16000&channels=1";

            var content = new ByteArrayContent(audioData);
            content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");

            Debug.WriteLine($"[Deepgram] POSTing {audioData.Length} bytes — lang={lang}");

            var response = await _http.PostAsync(url, content);
            var json     = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[Deepgram] ? API error {(int)response.StatusCode}: {json}");
                return null;
            }

            // Parse: results.channels[0].alternatives[0].transcript
            using var doc  = JsonDocument.Parse(json);
            var channel    = doc.RootElement
                               .GetProperty("results")
                               .GetProperty("channels")[0];
            var alternative = channel.GetProperty("alternatives")[0];
            var transcript  = alternative.GetProperty("transcript").GetString();
            var confidence  = alternative.GetProperty("confidence").GetDouble();

            Debug.WriteLine($"[Deepgram] ? Transcript: '{transcript}' (conf={confidence:P0})");
            return string.IsNullOrWhiteSpace(transcript) ? null : transcript;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Deepgram] ? RecognizeAsync failed: {ex.Message}");
            return null;
        }
    }

    // ?? Phrase validation ??????????????????????????????????????????????????????
    public bool ValidatePhrase(string recognizedText, string expectedPhrase, double threshold = 0.7)
    {
        if (string.IsNullOrWhiteSpace(recognizedText) || string.IsNullOrWhiteSpace(expectedPhrase))
            return false;

        var sim = CalculateSimilarity(recognizedText, expectedPhrase);
        var ok  = sim >= threshold;
        Debug.WriteLine($"[Deepgram] ValidatePhrase: sim={sim:P1} (threshold {threshold:P0}) ? {(ok ? "PASS ?" : "FAIL ?")}");
        return ok;
    }

    public double CalculateSimilarity(string text1, string text2)
    {
        if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2)) return 0;
        var a    = Normalise(text1);
        var b    = Normalise(text2);
        if (a == b) return 1.0;
        var dist = LevenshteinDistance(a, b);
        var max  = Math.Max(a.Length, b.Length);
        return max == 0 ? 1.0 : 1.0 - (double)dist / max;
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
        return isFilipino
            ? _filipinoPhrases[Random.Shared.Next(_filipinoPhrases.Length)]
            : _englishPhrases [Random.Shared.Next(_englishPhrases.Length)];
    }

    /// <summary>
    /// Maps IETF language tags to Deepgram language codes.
    /// Deepgram uses "tl" for Tagalog/Filipino and "en-US" for English.
    /// </summary>
    private static string NormaliseLanguage(string lang) => lang.ToLowerInvariant() switch
    {
        "fil" or "fil-ph" or "tl" or "tl-ph" => "tl",
        "en"                                  => "en-US",
        _                                     => lang
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

   
}
