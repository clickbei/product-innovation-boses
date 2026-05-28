namespace BosesApp.Core.Interfaces;

/// <summary>
/// AI orchestration service using Semantic Kernel
/// Routes voice commands to appropriate plugins and services
/// Integrates with Google Gemini for natural language understanding
/// </summary>
public interface IAiOrchestrator
{
    /// <summary>
    /// Process a natural language voice command
    /// </summary>
    /// <param name="userInput">Transcribed voice input</param>
    /// <param name="userId">Current user ID for context</param>
    /// <returns>AI response text</returns>
    Task<string> ProcessCommandAsync(string userInput, int userId);

    /// <summary>
    /// Initialize AI kernel with plugins
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Check if a command requires guardian verification (anti-scam)
    /// </summary>
    Task<bool> RequiresGuardianVerificationAsync(string command);

    /// <summary>
    /// Extract transaction intent from natural language
    /// </summary>
    Task<TransactionIntent?> ExtractTransactionIntentAsync(string command);

    /// <summary>
    /// Enable simulation mode for testing
    /// </summary>
    bool SimulationMode { get; set; }

    /// <summary>
    /// Analyse a message for scam signals and return a structured result.
    /// Used for the scam-detection demo flow.
    /// </summary>
    Task<ScamDetectionResult> SimulateScamDetectionAsync(string message);
}

/// <summary>
/// Result of the scam-detection analysis
/// </summary>
public class ScamDetectionResult
{
    /// <summary>True when the message is classified as a scam attempt.</summary>
    public bool IsScam { get; set; }

    /// <summary>Human-readable scam category, e.g. "OTP Harvesting".</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Confidence score 0–100.</summary>
    public int ConfidencePercent { get; set; }

    /// <summary>Short explanation shown to the user.</summary>
    public string Explanation { get; set; } = string.Empty;

    /// <summary>Recommended action for the user.</summary>
    public string RecommendedAction { get; set; } = string.Empty;

    /// <summary>Specific red-flag phrases found in the message.</summary>
    public List<string> RedFlags { get; set; } = new();
}

/// <summary>
/// Represents extracted transaction intent from voice command
/// </summary>
public class TransactionIntent
{
    public string Action { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string? Recipient { get; set; }
    public string? AccountNumber { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
}
