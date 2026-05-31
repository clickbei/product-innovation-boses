namespace BosesApp.Core.Interfaces;

/// <summary>
/// Event args for speech recognition result updates
/// </summary>
public class RecognitionResultEventArgs : EventArgs
{
    public string? RecognizedText { get; set; }
    public double Confidence { get; set; }
    public bool IsFinal { get; set; }
}

/// <summary>
/// Speech recognition service for converting audio to text
/// Used for validating spoken phrases during voice registration
/// </summary>
public interface ISpeechRecognitionService
{
    /// <summary>
    /// Event raised when speech recognition result is updated
    /// </summary>
    event EventHandler<RecognitionResultEventArgs>? OnRecognitionResultUpdated;

    /// <summary>
    /// Starts live speech recognition
    /// </summary>
    /// <param name="language">Language code (e.g., "en-US", "fil-PH")</param>
    /// <returns>True if recognition started successfully</returns>
    Task<bool> StartListeningAsync(string language = "en-US");

    /// <summary>
    /// Stops live speech recognition and returns recognized text
    /// </summary>
    /// <returns>Recognized text or null if recognition failed</returns>
    Task<string?> StopListeningAsync(Byte[]? audioData = null);

    /// <summary>
    /// Converts audio data to text (fallback method)
    /// </summary>
    /// <param name="audioData">PCM audio data (16-bit, 16kHz, mono)</param>
    /// <param name="language">Language code (e.g., "en-US", "fil-PH")</param>
    /// <returns>Recognized text or null if recognition failed</returns>
    Task<string?> RecognizeAsync(byte[] audioData, string language = "en-US");

    /// <summary>
    /// Checks if the recognized text matches the expected phrase
    /// Uses fuzzy matching to account for pronunciation variations
    /// </summary>
    /// <param name="recognizedText">Text from speech recognition</param>
    /// <param name="expectedPhrase">Expected phrase</param>
    /// <param name="threshold">Similarity threshold (0.0 to 1.0, default 0.7)</param>
    /// <returns>True if the phrases match within the threshold</returns>
    bool ValidatePhrase(string recognizedText, string expectedPhrase, double threshold = 0.7);

    /// <summary>
    /// Calculates similarity score between two phrases
    /// </summary>
    /// <param name="text1">First text</param>
    /// <param name="text2">Second text</param>
    /// <returns>Similarity score (0.0 to 1.0)</returns>
    double CalculateSimilarity(string text1, string text2);

    /// <summary>
    /// Checks if real speech recognition is available
    /// </summary>
    bool IsRealRecognitionAvailable { get; }

    /// <summary>
    /// Enable simulation mode — returns canned phrases instead of using the microphone.
    /// </summary>
    bool SimulationMode { get; set; }

    
}
