namespace BosesApp.Core.Interfaces;

/// <summary>
/// Abstraction over any Text-to-Speech provider.
/// Allows swapping between Piper (offline Filipino), OS TTS, and cloud
/// providers without touching call sites.
/// </summary>
public interface ITextToSpeechService
{
    /// <summary>Human-readable provider name for logging.</summary>
    string ProviderName { get; }

    /// <summary>True when this provider can deliver audio.</summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Synthesise and play <paramref name="text"/>.
    /// </summary>
    /// <param name="text">Plain text to speak.</param>
    /// <param name="language">IETF language tag, e.g. "fil-PH" or "en-US".</param>
    /// <param name="cancellationToken">Optional cancellation.</param>
    Task SpeakAsync(string text, string language = "fil-PH",
                    CancellationToken cancellationToken = default);
}
