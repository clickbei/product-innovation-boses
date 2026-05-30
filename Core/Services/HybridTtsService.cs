using BosesApp.Core.Interfaces;
using System.Diagnostics;

namespace BosesApp.Core.Services;

/// <summary>
/// Routes TTS calls to the best available provider for the detected language.
///
/// Priority chain
/// ??????????????????????????????????????????????????????????????????????????
///  Filipino (fil-PH / tl)
///    Tier 1 ? PiperTtsService  — offline, MIT, fil_PH-ugnayan-medium neural
///    Tier 2 ? OsTtsService     — OS voices (needs Filipino language pack)
///
///  English (en / en-US)
///    Tier 1 ? OsTtsService     — OS built-in English voice
///
/// All tiers degrade silently; the app never crashes due to a missing voice.
/// </summary>
public class HybridTtsService : ITextToSpeechService
{
    private readonly GoogleTranslateTtsService _google;
    private readonly OsTtsService              _os;

    public string ProviderName => "HybridTTS (Google Translate ? OS)";
    public bool   IsAvailable  => true;

    public HybridTtsService(GoogleTranslateTtsService google, OsTtsService os)
    {
        _google = google;
        _os     = os;

        Debug.WriteLine("[HybridTTS] ????????????????????????????????????????????");
        Debug.WriteLine("[HybridTTS] Google Translate TTS : ? zero-setup Filipino + English");
        Debug.WriteLine("[HybridTTS] OS TTS               : ? always-available fallback");
        Debug.WriteLine("[HybridTTS] ????????????????????????????????????????????");
    }

    public async Task SpeakAsync(string text, string language = "fil-PH",
                                  CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        // Tier 1 — Google Translate TTS (free, no setup, supports Tagalog)
        try
        {
            Debug.WriteLine($"[HybridTTS] ? Google Translate TTS ({language})");
            await _google.SpeakAsync(text, language, cancellationToken);
            return;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HybridTTS] Google TTS failed ({ex.Message}) — falling back to OS TTS");
        }

        // Tier 2 — OS TTS (always available, English voice)
        Debug.WriteLine("[HybridTTS] ? OS TTS fallback");
        await _os.SpeakAsync(text, language, cancellationToken);
    }
}
