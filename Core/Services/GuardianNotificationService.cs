using BosesApp.Core.Interfaces;
using System.Text.Json;

namespace BosesApp.Core.Services;

/// <summary>
/// Guardian notification service implementation.
/// SMS delivery is delegated to <see cref="ISmsGateway"/> so the provider
/// can be swapped (SimulatedSmsGateway ? TextBelt ? Twilio) via DI in MauiProgram.cs.
/// </summary>
public class GuardianNotificationService : IGuardianNotificationService
{
    private readonly ISmsGateway _smsGateway;
    private readonly List<GuardianEvent> _eventLog = new();
    private int _nextEventId = 1;
    private readonly string _logFilePath;

    public GuardianNotificationService(ISmsGateway smsGateway)
    {
        _smsGateway  = smsGateway;
        var dataDir  = GetDataDirectory();
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
        var message =
            $"[BOSES ALERT] Hi {guardianName}! " +
            $"{userName} requests approval for: {transactionDetails}. " +
            $"Approval code: {verificationCode}. " +
            $"Reply YES to approve or NO to reject. " +
            $"If unexpected, call {userName} immediately.";

        var smsResult = await _smsGateway.SendAsync(guardianPhone, message);

        System.Diagnostics.Debug.WriteLine(
            $"[Guardian] SMS via {_smsGateway.ProviderName} ? " +
            $"success={smsResult.Success} id={smsResult.MessageId} quota={smsResult.QuotaRemaining}");

        await LogGuardianEventAsync(new GuardianEvent
        {
            UserId             = 0,
            EventType          = "VERIFICATION_REQUESTED",
            Description        = $"SMS sent to {guardianName} ({MaskPhone(guardianPhone)}) via {_smsGateway.ProviderName}" +
                                  (smsResult.Success ? "" : $" [FAILED: {smsResult.Error}]"),
            TransactionDetails = transactionDetails,
            Status             = smsResult.Success ? "PENDING" : "SMS_FAILED"
        });

        return new GuardianNotificationResult
        {
            Success        = smsResult.Success,
            Message        = smsResult.Success
                ? $"Verification SMS sent to {guardianName} at {MaskPhone(guardianPhone)} via {_smsGateway.ProviderName}."
                : $"SMS delivery failed ({smsResult.Error}). Event logged locally.",
            NotificationId = smsResult.MessageId ?? Guid.NewGuid().ToString("N")[..8]
        };
    }

    public async Task<GuardianNotificationResult> SendPushNotificationAsync(
        string guardianUserId,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        await Task.Delay(200); // Simulate FCM latency

        // In production: call Firebase Cloud Messaging API
        System.Diagnostics.Debug.WriteLine($"[PUSH?{guardianUserId}] {title}: {body}");

        return new GuardianNotificationResult
        {
            Success        = true,
            Message        = $"Push notification sent: {title}",
            NotificationId = Guid.NewGuid().ToString("N")[..8]
        };
    }

    public async Task<GuardianNotificationResult> SendScamAlertAsync(
        string guardianPhone,
        string userName,
        string scamDetails)
    {
        var urgentMessage =
            $"[BOSES SCAM ALERT] URGENT! " +
            $"Possible scam detected for {userName}. " +
            $"Details: {scamDetails}. " +
            $"Contact {userName} immediately!";

        var smsResult = await _smsGateway.SendAsync(guardianPhone, urgentMessage);

        await LogGuardianEventAsync(new GuardianEvent
        {
            EventType          = "SCAM_ALERT",
            Description        = $"Scam alert sent to guardian ({MaskPhone(guardianPhone)}) via {_smsGateway.ProviderName}" +
                                  (smsResult.Success ? "" : $" [FAILED: {smsResult.Error}]"),
            TransactionDetails = scamDetails,
            Status             = "ALERTED",
            WasScamAttempt     = true
        });

        return new GuardianNotificationResult
        {
            Success        = smsResult.Success,
            Message        = smsResult.Success
                ? $"Scam alert sent to guardian at {MaskPhone(guardianPhone)}."
                : $"SMS delivery failed ({smsResult.Error}). Event logged locally.",
            NotificationId = smsResult.MessageId ?? Guid.NewGuid().ToString("N")[..8]
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

    // ?? Helpers ???????????????????????????????????????????????????????????????

    private static string MaskPhone(string phone)
    {
        if (phone.Length <= 4) return "****";
        return new string('*', phone.Length - 4) + phone[^4..];
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
