namespace BosesApp.Core.Interfaces;

/// <summary>
/// Voice interaction service interface
/// Handles speech-to-text and text-to-speech operations
/// Simulates Deepgram integration with fallback modes
/// </summary>
public interface IVoiceService
{
    /// <summary>
    /// Start listening for voice input
    /// </summary>
    Task<bool> StartListeningAsync();

    /// <summary>
    /// Stop listening and return transcribed text
    /// </summary>
    Task<string> StopListeningAsync();

    /// <summary>
    /// Convert text to speech and play audio
    /// </summary>
    Task SpeakAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if voice service is currently listening
    /// </summary>
    bool IsListening { get; }

    /// <summary>
    /// Enable simulation mode for demo/testing
    /// </summary>
    bool SimulationMode { get; set; }

    /// <summary>
    /// Set simulated voice input for testing
    /// </summary>
    void SetSimulatedInput(string input);
}
