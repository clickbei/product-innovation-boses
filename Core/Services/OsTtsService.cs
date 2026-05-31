using BosesApp.Core.Interfaces;
using System.Diagnostics;

namespace BosesApp.Core.Services;

/// <summary>
/// TTS provider backed by MAUI's built-in <see cref="TextToSpeech"/> API.
/// Always available — uses whatever voices the OS has installed.
///
/// On Windows without the Filipino language pack the voice will be English.
/// Pair with <see cref="PiperTtsService"/> via <see cref="HybridTtsService"/>
/// to get a real Filipino voice regardless of OS language packs.
/// </summary>
public class OsTtsService : ITextToSpeechService
{
    public string ProviderName => "OS TTS (MAUI TextToSpeech)";
    public bool   IsAvailable  => true;   // OS TTS is always present

    public async Task SpeakAsync(string text, string language = "fil-PH",
                                  CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();

            // Priority: requested language ? English (PH) ? any English ? first available
            var locale =
                locales.FirstOrDefault(l => l.Language.StartsWith(
                    language.Split('-')[0], StringComparison.OrdinalIgnoreCase))
             ?? locales.FirstOrDefault(l =>
                    l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase) &&
                    l.Country?.Equals("PH", StringComparison.OrdinalIgnoreCase) == true)
             ?? locales.FirstOrDefault(l =>
                    l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
             ?? locales.FirstOrDefault();

            Debug.WriteLine($"[OsTTS] Speaking ({locale?.Name ?? "default"}): " +
                            $"{text[..Math.Min(60, text.Length)]}…");

            await TextToSpeech.Default.SpeakAsync(text,
                new SpeechOptions { Pitch = 1.0f, Volume = 1.0f, Locale = locale },
                cancellationToken);
        }
        catch (OperationCanceledException) { /* expected */ }
        catch (Exception ex)
        {
            Debug.WriteLine($"[OsTTS] Error: {ex.Message}");
            await Task.Delay(Math.Min(text.Length * 50, 4000), cancellationToken);
        }
    }
}
