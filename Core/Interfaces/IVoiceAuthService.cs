namespace BosesApp.Core.Interfaces;

/// <summary>
/// Voice biometric authentication service
/// Handles voice fingerprint registration and verification
/// </summary>
public interface IVoiceAuthService
{
    /// <summary>
    /// Register a new voice print for a user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="audioSamples">Voice audio samples for enrollment</param>
    /// <returns>Voice print data (serialized vector)</returns>
    Task<string> RegisterVoicePrintAsync(int userId, byte[] audioSamples);

    /// <summary>
    /// Verify voice authentication
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="audioSample">Voice sample to verify</param>
    /// <param name="storedVoicePrint">Stored voice print data</param>
    /// <returns>True if voice matches, false otherwise</returns>
    Task<bool> VerifyVoiceAsync(int userId, byte[] audioSample, string storedVoicePrint);

    /// <summary>
    /// Calculate similarity score between voice samples (0.0 to 1.0)
    /// </summary>
    Task<double> CalculateSimilarityAsync(byte[] sample1, byte[] sample2);

    /// <summary>
    /// Enable simulation mode for testing
    /// </summary>
    bool SimulationMode { get; set; }

    /// <summary>
    /// Force authentication result in simulation mode
    /// </summary>
    void SetSimulatedAuthResult(bool shouldPass);
}
