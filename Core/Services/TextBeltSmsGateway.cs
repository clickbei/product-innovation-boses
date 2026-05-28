using BosesApp.Core.Interfaces;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BosesApp.Core.Services;

/// <summary>
/// SMS gateway backed by TextBelt (https://textbelt.com).
///
/// FREE TIER — demo use:
///   Key      : "textbelt"  (literal string, no sign-up needed)
///   Limit    : 1 free SMS per day, globally
///   PH nums  : Supported (+63...)
///
/// PAID TIER — production:
///   Buy credits at https://textbelt.com (~$0.01 / SMS)
///   Replace apiKey with your purchased key in MauiProgram.cs.
///
/// Quota-exhausted behaviour:
///   Returns Success=false with a clear error message.
///   GuardianNotificationService logs the event locally so nothing is lost.
/// </summary>
public class TextBeltSmsGateway : ISmsGateway
{
    private const string BaseUrl          = "https://textbelt.com/text";
    private const string FreeKey          = "textbelt";
    private const int    MaxMessageLength = 160;

    private readonly HttpClient _http;
    private readonly string     _apiKey;

    public string ProviderName => _apiKey == FreeKey ? "TextBelt (free)" : "TextBelt (paid)";

    public TextBeltSmsGateway(HttpClient http, string? apiKey = null)
    {
        _http   = http;
        _apiKey = string.IsNullOrWhiteSpace(apiKey) ? FreeKey : apiKey;
    }

    public async Task<SmsResult> SendAsync(string toPhone, string message)
    {
        if (message.Length > MaxMessageLength)
            message = message[..MaxMessageLength];

        Debug.WriteLine($"[TextBelt] Sending to {MaskPhone(toPhone)} — key={MaskKey(_apiKey)}");

        try
        {
            var payload = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["phone"]   = toPhone,
                ["message"] = message,
                ["key"]     = _apiKey
            });

            using var response = await _http.PostAsync(BaseUrl, payload);
            var body = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"[TextBelt] Response ({(int)response.StatusCode}): {body}");

            var result = JsonSerializer.Deserialize<TextBeltResponse>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result is null)
                return Fail("Empty response from TextBelt");

            if (!result.Success)
            {
                var error = result.Error ?? "Unknown error from TextBelt";

                // Free-tier quota exhausted — degrade gracefully
                if (error.Contains("quota", StringComparison.OrdinalIgnoreCase)
                    || error.Contains("Out of quota", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine("[TextBelt] ?? Daily quota exhausted — SMS logged locally only");
                    return new SmsResult
                    {
                        Success        = false,
                        Error          = "Daily free quota exhausted. SMS was logged locally only.",
                        QuotaRemaining = 0
                    };
                }

                return Fail(error);
            }

            return new SmsResult
            {
                Success        = true,
                MessageId      = result.TextId,
                QuotaRemaining = result.QuotaRemaining
            };
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"[TextBelt] ? Network error: {ex.Message}");
            return Fail($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TextBelt] ? Unexpected error: {ex.Message}");
            return Fail(ex.Message);
        }
    }

    private static SmsResult Fail(string error) => new() { Success = false, Error = error };

    private static string MaskPhone(string phone)
        => phone.Length <= 4 ? "****" : new string('*', phone.Length - 4) + phone[^4..];

    private static string MaskKey(string key)
        => key == FreeKey ? key : (key.Length > 6 ? key[..3] + "***" + key[^3..] : "***");

    private sealed class TextBeltResponse
    {
        [JsonPropertyName("success")]
        public bool    Success        { get; set; }

        [JsonPropertyName("textId")]
        public string? TextId         { get; set; }

        [JsonPropertyName("quotaRemaining")]
        public int?    QuotaRemaining { get; set; }

        [JsonPropertyName("error")]
        public string? Error          { get; set; }
    }
}
