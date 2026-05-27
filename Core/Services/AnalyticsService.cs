using BosesApp.Core.Interfaces;
using System.Text.Json;

namespace BosesApp.Core.Services;

/// <summary>
/// Analytics service - local event store with in-memory aggregation.
/// Privacy-first: no PII, no account numbers, no transcriptions stored.
/// In production: flush to Azure Application Insights / Firebase Analytics.
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly List<AnalyticsEvent> _events = new();
    private readonly string _dataPath;
    private int _sessionCount = 0;

    public AnalyticsService()
    {
        _dataPath = Path.Combine(GetDataDirectory(), "analytics.json");
        LoadFromDisk();
        _sessionCount++;
        _ = TrackEventAsync("app_session_start", new Dictionary<string, string>
        {
            ["session_number"] = _sessionCount.ToString(),
            ["platform"] = DeviceInfo.Platform.ToString()
        });
    }

    public async Task TrackEventAsync(string eventName, Dictionary<string, string>? properties = null)
    {
        var ev = new AnalyticsEvent
        {
            Name = eventName,
            Properties = properties ?? new(),
            Timestamp = DateTime.Now
        };
        _events.Add(ev);
        System.Diagnostics.Debug.WriteLine($"[Analytics] {eventName} {(properties != null ? JsonSerializer.Serialize(properties) : "")}");
        await PersistAsync();
    }

    public Task TrackScreenViewAsync(string screenName) =>
        TrackEventAsync("screen_view", new Dictionary<string, string> { ["screen"] = screenName });

    public Task TrackFeatureUsageAsync(AnalyticsFeature feature, Dictionary<string, string>? extra = null)
    {
        var props = extra ?? new Dictionary<string, string>();
        props["feature"] = feature.ToString();
        return TrackEventAsync("feature_used", props);
    }

    public Task TrackVoiceCommandAsync(string intentCategory, bool wasSuccessful, int durationMs) =>
        TrackEventAsync("voice_command", new Dictionary<string, string>
        {
            ["intent"] = intentCategory,
            ["success"] = wasSuccessful.ToString(),
            ["duration_ms"] = durationMs.ToString()
        });

    public Task TrackBankingActionAsync(string actionType, bool wasSuccessful) =>
        TrackEventAsync("banking_action", new Dictionary<string, string>
        {
            ["action"] = actionType,
            ["success"] = wasSuccessful.ToString()
        });

    public Task TrackGuardianEventAsync(string eventType, bool wasScamPrevented) =>
        TrackEventAsync("guardian_event", new Dictionary<string, string>
        {
            ["type"] = eventType,
            ["scam_prevented"] = wasScamPrevented.ToString()
        });

    public Task TrackErrorAsync(string errorType, string errorMessage, string? context = null) =>
        TrackEventAsync("app_error", new Dictionary<string, string>
        {
            ["error_type"] = errorType,
            ["message"] = errorMessage.Length > 200 ? errorMessage[..200] : errorMessage,
            ["context"] = context ?? "unknown"
        });

    public Task<AnalyticsSummary> GetSummaryAsync()
    {
        var summary = new AnalyticsSummary
        {
            TotalSessions = _events.Count(e => e.Name == "app_session_start"),
            TotalVoiceCommands = _events.Count(e => e.Name == "voice_command"),
            SuccessfulCommands = _events.Count(e =>
                e.Name == "voice_command" &&
                e.Properties.TryGetValue("success", out var s) &&
                s == "True"),
            GuardianAlertsTriggered = _events.Count(e => e.Name == "guardian_event"),
            ScamsPreventedCount = _events.Count(e =>
                e.Name == "guardian_event" &&
                e.Properties.TryGetValue("scam_prevented", out var sp) &&
                sp == "True"),
            FirstEventAt = _events.Count > 0 ? _events.Min(e => e.Timestamp) : DateTime.Now,
            LastEventAt = _events.Count > 0 ? _events.Max(e => e.Timestamp) : DateTime.Now
        };

        // Average command duration
        var durations = _events
            .Where(e => e.Name == "voice_command" &&
                        e.Properties.TryGetValue("duration_ms", out _))
            .Select(e => int.TryParse(e.Properties["duration_ms"], out var d) ? d : 0)
            .ToList();

        summary.AverageCommandDurationMs = durations.Count > 0 ? durations.Average() : 0;

        // Feature usage counts
        foreach (var fe in _events.Where(e => e.Name == "feature_used"))
        {
            if (fe.Properties.TryGetValue("feature", out var feat))
            {
                summary.FeatureUsageCounts.TryGetValue(feat, out var cnt);
                summary.FeatureUsageCounts[feat] = cnt + 1;
            }
        }

        // Error counts
        foreach (var err in _events.Where(e => e.Name == "app_error"))
        {
            if (err.Properties.TryGetValue("error_type", out var et))
            {
                summary.ErrorCounts.TryGetValue(et, out var cnt);
                summary.ErrorCounts[et] = cnt + 1;
            }
        }

        return Task.FromResult(summary);
    }

    // ── Persistence ─────────────────────────────────────────────────────────────

    private void LoadFromDisk()
    {
        try
        {
            if (File.Exists(_dataPath))
            {
                var json = File.ReadAllText(_dataPath);
                var loaded = JsonSerializer.Deserialize<List<AnalyticsEvent>>(json);
                if (loaded != null)
                    _events.AddRange(loaded);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Analytics] Load failed: {ex.Message}");
        }
    }

    private async Task PersistAsync()
    {
        try
        {
            // Keep last 10,000 events on disk to avoid unbounded growth
            var toSave = _events.TakeLast(10_000).ToList();
            var json = JsonSerializer.Serialize(toSave);
            await File.WriteAllTextAsync(_dataPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Analytics] Persist failed: {ex.Message}");
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

    private class AnalyticsEvent
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string> Properties { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }
}
