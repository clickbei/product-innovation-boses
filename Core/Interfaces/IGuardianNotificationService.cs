namespace BosesApp.Core.Interfaces;

/// <summary>
/// Guardian notification service for anti-scam protection
/// Sends SMS/push alerts to guardian when high-risk events occur
/// </summary>
public interface IGuardianNotificationService
{
    /// <summary>
    /// Send SMS alert to guardian for high-risk transaction approval
    /// </summary>
    Task<GuardianNotificationResult> SendVerificationSmsAsync(
        string guardianPhone,
        string guardianName,
        string userName,
        string transactionDetails,
        string verificationCode);

    /// <summary>
    /// Send push notification to guardian (when app is installed)
    /// </summary>
    Task<GuardianNotificationResult> SendPushNotificationAsync(
        string guardianUserId,
        string title,
        string body,
        Dictionary<string, string>? data = null);

    /// <summary>
    /// Send scam alert to guardian immediately
    /// </summary>
    Task<GuardianNotificationResult> SendScamAlertAsync(
        string guardianPhone,
        string userName,
        string scamDetails);

    /// <summary>
    /// Log a guardian event for dashboard viewing
    /// </summary>
    Task LogGuardianEventAsync(GuardianEvent guardianEvent);

    /// <summary>
    /// Get all guardian events for a user (for dashboard)
    /// </summary>
    Task<IEnumerable<GuardianEvent>> GetGuardianEventsAsync(int userId, int limit = 50);
}

/// <summary>
/// Result of a guardian notification attempt
/// </summary>
public class GuardianNotificationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? NotificationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

/// <summary>
/// Guardian event record for dashboard tracking
/// </summary>
public class GuardianEvent
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string EventType { get; set; } = string.Empty; // VERIFICATION_REQUESTED, SCAM_ALERT, APPROVED, REJECTED
    public string Description { get; set; } = string.Empty;
    public string? TransactionDetails { get; set; }
    public decimal? Amount { get; set; }
    public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED, EXPIRED
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ResolvedAt { get; set; }
    public string? GuardianResponse { get; set; }
    public bool WasScamAttempt { get; set; }
}
