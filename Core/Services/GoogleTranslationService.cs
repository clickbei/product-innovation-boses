using System.Diagnostics;
using System.Text.Json;

namespace BosesApp.Core.Services;

/// <summary>
/// Language detection and translation via the Google Translate unofficial JSON endpoint.
/// Zero setup — no API key, no account required.
///
/// ? Detects language (Filipino vs English) from any input text
/// ? Translates English ? Filipino on demand
/// ? Falls back to fast keyword heuristic if network is unavailable
/// </summary>
public class GoogleTranslationService
{
    private const string LogTag  = "[Translation]";
    private const string BaseUrl = "https://translate.googleapis.com/translate_a/single";

    private readonly HttpClient _http;

    public GoogleTranslationService()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        _http.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
            + "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    // ?? Translation ????????????????????????????????????????????????????????????

    /// <summary>
    /// Translates <paramref name="text"/> to the given target language.
    /// Source language is auto-detected by Google.
    /// Returns the original text unchanged on any failure.
    /// </summary>
    /// <param name="targetLanguage">IETF tag — "en-US"/"en" for English, "fil-PH"/"tl" for Filipino.</param>
    public async Task<string> TranslateToAsync(string text,
                                                string targetLanguage = "en-US",
                                                CancellationToken ct  = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        var tl  = ToGoogleCode(targetLanguage);
        var url = $"{BaseUrl}?client=gtx&sl=auto&tl={tl}&dt=t&q={Uri.EscapeDataString(text)}";

        try
        {
            var json       = await _http.GetStringAsync(url, ct);
            var translated = ParseTranslation(json);

            if (!string.IsNullOrWhiteSpace(translated))
            {
                Debug.WriteLine($"{LogTag} ? {tl}: "
                    + $"{text[..Math.Min(40, text.Length)]} ? "
                    + $"{translated[..Math.Min(40, translated.Length)]}");
                return translated;
            }
        }
        catch (OperationCanceledException) { /* caller cancelled */ }
        catch (Exception ex)
        {
            Debug.WriteLine($"{LogTag} Translate failed: {ex.Message} — returning original");
        }

        return text;
    }

    // ?? Language detection ?????????????????????????????????????????????????????

    /// <summary>
    /// Detects the language of <paramref name="text"/> using Google Translate's
    /// auto-detect engine. Returns "fil-PH", "en-US", or the raw Google language
    /// code for other languages.
    /// Falls back to <see cref="DetectLanguageFallback"/> when offline.
    /// </summary>
    public async Task<string> DetectLanguageAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return DetectLanguageFallback(text);

        // dt=t  ? get translation (required by the endpoint)
        // dt=ld ? get language detection metadata
        var url = $"{BaseUrl}?client=gtx&sl=auto&tl=en&dt=t&dt=ld&q={Uri.EscapeDataString(text)}";

        try
        {
            var json = await _http.GetStringAsync(url, ct);
            var detected = ParseDetectedLanguage(json);

            if (!string.IsNullOrWhiteSpace(detected))
            {
                var ietf = GoogleCodeToIetf(detected);
                Debug.WriteLine($"{LogTag} Detected '{detected}' ? {ietf}: "
                    + $"{text[..Math.Min(40, text.Length)]}");
                return ietf;
            }
        }
        catch (OperationCanceledException) { /* caller cancelled */ }
        catch (Exception ex)
        {
            Debug.WriteLine($"{LogTag} Detect failed: {ex.Message} — using keyword fallback");
        }

        return DetectLanguageFallback(text);
    }

    // ?? Helpers ????????????????????????????????????????????????????????????????

    /// <summary>
    /// Parses translated text from the Google Translate JSON response.
    /// Format: [[["translated", "original", ...], ...], ...]
    /// </summary>
    private static string? ParseTranslation(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var sb        = new System.Text.StringBuilder();

            foreach (var sentence in doc.RootElement[0].EnumerateArray())
            {
                if (sentence.ValueKind == JsonValueKind.Array
                    && sentence.GetArrayLength() > 0
                    && sentence[0].ValueKind == JsonValueKind.String)
                {
                    sb.Append(sentence[0].GetString());
                }
            }

            return sb.Length > 0 ? sb.ToString() : null;
        }
        catch { return null; }
    }

    /// <summary>
    /// Extracts the detected source language code from root[2] of the response.
    /// </summary>
    private static string? ParseDetectedLanguage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root      = doc.RootElement;

            // root[2] is the detected language code string (e.g. "tl", "en")
            if (root.GetArrayLength() > 2 && root[2].ValueKind == JsonValueKind.String)
                return root[2].GetString();
        }
        catch { /* ignore */ }

        return null;
    }

    /// <summary>
    /// Fast keyword-based fallback detector used when the network call fails.
    /// Returns "fil-PH" or "en-US".
    /// </summary>
    public static string DetectLanguageFallback(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "en-US";

        var lower = text.ToLowerInvariant();
        ReadOnlySpan<string> markers =
        [
            " ang ", " ng ", " sa ", " ko ", " mo ", " ka ",
            " ay ", " po ", " ho ", " naman", " kita",
            "magkano", "ipadala", "bayaran", "mag-", "i-send",
            "gusto", "paki", "tulong", "kumusta", "magtanong",
            "pesos", "nakikinig", "magsalita", "simulan"
        ];

        foreach (var m in markers)
            if (lower.Contains(m)) return "fil-PH";

        return "en-US";
    }

    /// <summary>Maps Google language codes to IETF tags used throughout the app.</summary>
    private static string GoogleCodeToIetf(string code) => code.ToLowerInvariant() switch
    {
        "tl" or "fil" => "fil-PH",
        "en"          => "en-US",
        _             => code
    };

    /// <summary>Maps IETF tags to Google Translate language codes.</summary>
    public static string ToGoogleCode(string lang) => lang.ToLowerInvariant() switch
    {
        "fil-ph" or "fil" or "tl" or "tl-ph" => "tl",
        "en-us"  or "en"                      => "en",
        _                                     => lang.Split('-')[0]
    };
}
