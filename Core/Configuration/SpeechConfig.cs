namespace BosesApp.Core.Configuration;

/// <summary>
/// Configuration for speech recognition services
/// </summary>
public static class SpeechConfig
{
    /// <summary>
    /// Azure Speech Services subscription key
    /// Get your free key at: https://azure.microsoft.com/free/cognitive-services/
    /// Free tier: 5 hours of audio per month
    /// </summary>
    public static string AzureSpeechKey { get; set; } = string.Empty;

    /// <summary>
    /// Azure Speech Services region (e.g., "eastus", "westus", "southeastasia")
    /// </summary>
    public static string AzureSpeechRegion { get; set; } = string.Empty;

    /// <summary>
    /// Check if Azure Speech is configured
    /// </summary>
    public static bool IsAzureSpeechConfigured =>
        !string.IsNullOrWhiteSpace(AzureSpeechKey) &&
        !string.IsNullOrWhiteSpace(AzureSpeechRegion);

    /// <summary>
    /// Initialize configuration
    /// Call this at app startup with your API keys
    /// </summary>
    public static void Initialize(string azureKey, string azureRegion)
    {
        AzureSpeechKey = azureKey;
        AzureSpeechRegion = azureRegion;
    }
}
