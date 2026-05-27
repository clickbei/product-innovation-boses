using BosesApp.Core.Interfaces;
using System.Text.Json;

namespace BosesApp.Core.Services;

/// <summary>
/// Guardian notification service implementation
/// Simulates SMS/push notifications with full audit logging
/// In production: replace SMS simulation with Twilio/Vonage and FCM push
/// </summary>
public class GuardianNotificationService : IGuardianNotificationService
{
    private readonly List<GuardianEvent> _eventLog = new();
    private int _nextEventId = 1;
    private readonly string _logFilePath;

    public GuardianNotificationService()
    {
        var dataDir = GetDataDirectory();
        _logFilePath = Path.Combine(dataDir, "guardian_events.json");
        LoadEventsFromDisk();
    }

    public async Task<GuardianNotificationResult> SendVerificationSmsAsync(
        string guardianPhone,
        string guardianName,
        string userName,
        string transactionDetails,
        string verificationCode)
    {
        await Task.Delay(400); // Simulate SMS gateway latency

        // In production this would call Twilio / Vonage:
        // var client = new TwilioRestClient(accountSid, authToken);
        // await client.Messages.CreateAsync(to: guardianPhone, ...);

        var message =
            $"[BOSES ALERT] Hi {guardianName}! " +
            $"{userName} is requesting your approval for: {transactionDetails}. " +
            $"Your approval code is: {verificationCode}. " +
            $"Reply YES to approve or NO to reject. " +
            $"If you did not expect this, please call {userName} immediately.";

        System.Diagnostics.Debug.WriteLine($"[SMS?{guardianPhone}] {message}");

        await LogGuardianEventAsync(new GuardianEvent
        {
            UserId = 0, // resolved by caller
            EventType = "VERIFICATION_REQUESTED",
            Description = $"SMS verification sent to guardian {guardianName} ({guardianPhone})",
            TransactionDetails = transactionDetails,
            Status = "PENDING"
        });

        return new GuardianNotificationResult
        {
            Success = true,
            Message = $"Verification SMS sent to {guardianName} at {MaskPhone(guardianPhone)}.",
            NotificationId = Guid.NewGuid().ToString("N")[..8]
        };
    }

    public async Task<GuardianNotificationResult> SendPushNotificationAsync(
        string guardianUserId,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        await Task.Delay(200); // Simulate FCM push latency

        // In production: call Firebase Cloud Messaging API
        // var fcm = new FirebaseClient(serverKey);
        // await fcm.SendAsync(new Message { To = guardianUserId, Notification = ... });

        System.Diagnostics.Debug.WriteLine($"[PUSH?{guardianUserId}] {title}: {body}");

        return new GuardianNotificationResult
        {
            Success = true,
            Message = $"Push notification sent: {title}",
            NotificationId = Guid.NewGuid().ToString("N")[..8]
        };
    }

    public async Task<GuardianNotificationResult> SendScamAlertAsync(
        string guardianPhone,
        string userName,
        string scamDetails)
    {
        await Task.Delay(300);

        var urgentMessage =
            $"?? [BOSES SCAM ALERT] URGENT! " +
            $"A possible scam has been detected for {userName}. " +
            $"Details: {scamDetails}. " +
            $"Please contact {userName} immediately to verify!";

        System.Diagnostics.Debug.WriteLine($"[SCAM ALERT SMS?{guardianPhone}] {urgentMessage}");

        await LogGuardianEventAsync(new GuardianEvent
        {
            EventType = "SCAM_ALERT",
            Description = $"Scam alert sent to guardian ({MaskPhone(guardianPhone)})",
            TransactionDetails = scamDetails,
            Status = "ALERTED",
            WasScamAttempt = true
        });

        return new GuardianNotificationResult
        {
            Success = true,
            Message = $"Scam alert sent to guardian at {MaskPhone(guardianPhone)}.",
            NotificationId = Guid.NewGuid().ToString("N")[..8]
        };
    }

    public async Task LogGuardianEventAsync(GuardianEvent guardianEvent)
    {
        guardianEvent.Id = _nextEventId++;
        guardianEvent.CreatedAt = DateTime.Now;
        _eventLog.Add(guardianEvent);
        await PersistEventsToDiskAsync();
    }

    public Task<IEnumerable<GuardianEvent>> GetGuardianEventsAsync(int userId, int limit = 50)
    {
        var events = _eventLog
            .Where(e => userId == 0 || e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(limit)
            .ToList();

        return Task.FromResult<IEnumerable<GuardianEvent>>(events);
    }

    // ?? Helpers ????????????????????????????????????????????????????????????????

    private static string MaskPhone(string phone)
    {
        if (phone.Length <= 4) return "****";
        return phone[..^4].Select(_ => '*').Aggregate("", (a, c) => a + c) + phone[^4..];
    }

    private void LoadEventsFromDisk()
    {
        try
        {
            if (File.Exists(_logFilePath))
            {
                var json = File.ReadAllText(_logFilePath);
                var events = JsonSerializer.Deserialize<List<GuardianEvent>>(json);
                if (events != null)
                {
                    _eventLog.AddRange(events);
                    _nextEventId = _eventLog.Count > 0 ? _eventLog.Max(e => e.Id) + 1 : 1;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GuardianNotification] Failed to load events: {ex.Message}");
        }
    }

    private async Task PersistEventsToDiskAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_eventLog, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_logFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GuardianNotification] Failed to persist events: {ex.Message}");
        }
    }

    private static string GetDataDirectory()
    {
#if ANDROID || IOS || MACCATALYST
        return FileSystem.AppDataDirectory;
#elif WINDOWS
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Boses");
        Directory.CreateDirectory(path);
        return path;
#else
        return FileSystem.AppDataDirectory;
#endif
    }
}
