namespace BosesApp.Core.Interfaces;

/// <summary>
/// Analytics service for tracking app usage, feature adoption, and errors
/// Privacy-first: no PII stored, all data anonymised
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Track a named user action event
    /// </summary>
    Task TrackEventAsync(string eventName, Dictionary<string, string>? properties = null);

    /// <summary>
    /// Track a screen/page view
    /// </summary>
    Task TrackScreenViewAsync(string screenName);

    /// <summary>
    /// Track a feature usage
    /// </summary>
    Task TrackFeatureUsageAsync(AnalyticsFeature feature, Dictionary<string, string>? extra = null);

    /// <summary>
    /// Track a voice command (anonymised - no transcription stored)
    /// </summary>
    Task TrackVoiceCommandAsync(string intentCategory, bool wasSuccessful, int durationMs);

    /// <summary>
    /// Track a banking action (no account numbers or amounts)
    /// </summary>
    Task TrackBankingActionAsync(string actionType, bool wasSuccessful);

    /// <summary>
    /// Track a guardian alert event
    /// </summary>
    Task TrackGuardianEventAsync(string eventType, bool wasScamPrevented);

    /// <summary>
    /// Track an error
    /// </summary>
    Task TrackErrorAsync(string errorType, string errorMessage, string? context = null);

    /// <summary>
    /// Get analytics summary for display (admin/debug)
    /// </summary>
    Task<AnalyticsSummary> GetSummaryAsync();
}

/// <summary>
/// Enumeration of trackable features
/// </summary>
public enum AnalyticsFeature
{
    VoiceCommand,
    BankBalance,
    BankTransfer,
    BankTransactions,
    PwdDiscount,
    GuardianVerification,
    ScamDetection,
    VoiceBiometrics,
    AccessibilityHighContrast,
    AccessibilityLargeText,
    AccessibilityScreenReader,
    LanguageSwitch,
    OnboardingCompleted
}

/// <summary>
/// Summary of analytics data
/// </summary>
public class AnalyticsSummary
{
    public int TotalSessions { get; set; }
    public int TotalVoiceCommands { get; set; }
    public int SuccessfulCommands { get; set; }
    public int GuardianAlertsTriggered { get; set; }
    public int ScamsPreventedCount { get; set; }
    public double AverageCommandDurationMs { get; set; }
    public Dictionary<string, int> FeatureUsageCounts { get; set; } = new();
    public Dictionary<string, int> ErrorCounts { get; set; } = new();
    public DateTime FirstEventAt { get; set; }
    public DateTime LastEventAt { get; set; }
}
