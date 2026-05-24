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
