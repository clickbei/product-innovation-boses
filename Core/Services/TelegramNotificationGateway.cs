using BosesApp.Core.Interfaces;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BosesApp.Core.Services;

/// <summary>
/// SMS/notification gateway backed by the Telegram Bot API.
///
/// WHY TELEGRAM FOR DEMOS:
///   ? 100% free — no credit card, no sign-up fees, no daily limits
///   ? Works globally, including Philippine numbers
///   ? Rich message formatting supported
///   ? Guardian receives real push notifications instantly
///   ? Simple REST API — no SDK required
///
/// ONE-TIME SETUP (5 minutes):
///   1. Open Telegram ? search @BotFather ? send /newbot
///   2. Follow prompts, copy the TOKEN BotFather gives you
///   3. Guardian opens Telegram, searches for your bot, sends any message
///   4. Get guardian's chat_id:
///        GET https://api.telegram.org/bot{TOKEN}/getUpdates
///      Look for "chat":{"id": 123456789} in the response
///   5. Set TOKEN and CHAT_ID in MauiProgram.cs (OPTION C)
///
/// API REFERENCE: https://core.telegram.org/bots/api#sendmessage
/// </summary>
public class TelegramNotificationGateway : ISmsGateway
{
    private const string BaseUrl = "https://api.telegram.org/bot";

    private readonly HttpClient _http;
    private readonly string     _botToken;
    private readonly string     _chatId;

    public string ProviderName => "Telegram Bot (free)";

    /// <param name="http">Injected HttpClient</param>
    /// <param name="botToken">Token from @BotFather, e.g. "7123456789:AAExxxxxxxx"</param>
    /// <param name="chatId">Guardian's Telegram chat ID, e.g. "123456789"</param>
    public TelegramNotificationGateway(HttpClient http, string botToken, string chatId)
    {
        _http      = http;
        _botToken  = botToken;
        _chatId    = chatId;
    }

    public async Task<SmsResult> SendAsync(string toPhone, string message)
    {
        // Telegram doesn't use phone numbers — we send to the guardian's chat_id.
        // toPhone is accepted for interface compatibility but not used here.

        Debug.WriteLine($"[Telegram] Sending to chat_id={MaskId(_chatId)}");

        try
        {
            var url     = $"{BaseUrl}{_botToken}/sendMessage";
            var payload = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["chat_id"]    = _chatId,
                ["text"]       = message,
                ["parse_mode"] = "HTML"   // supports <b>bold</b> etc.
            });

            using var response = await _http.PostAsync(url, payload);
            var body = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"[Telegram] Response ({(int)response.StatusCode}): {body}");

            var result = JsonSerializer.Deserialize<TelegramResponse>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result is null || !result.Ok)
            {
                var desc = result?.Description ?? "Unknown Telegram error";

                // Provide actionable guidance for the most common mistakes
                if (desc.Contains("bot was blocked", StringComparison.OrdinalIgnoreCase))
                    desc = "Guardian has blocked the bot. Ask them to unblock it in Telegram.";
                else if (desc.Contains("chat not found", StringComparison.OrdinalIgnoreCase))
                    desc = "Chat ID not found. Ensure guardian has started the bot first (/start).";
                else if (desc.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase))
                    desc = "Invalid bot token. Verify TOKEN in MauiProgram.cs.";

                return new SmsResult { Success = false, Error = desc };
            }

            return new SmsResult
            {
                Success   = true,
                MessageId = result.Result?.MessageId.ToString()
            };
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"[Telegram] ? Network error: {ex.Message}");
            return new SmsResult { Success = false, Error = $"Network error: {ex.Message}" };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Telegram] ? Unexpected error: {ex.Message}");
            return new SmsResult { Success = false, Error = ex.Message };
        }
    }

    private static string MaskId(string id)
        => id.Length <= 3 ? "***" : id[..3] + new string('*', id.Length - 3);

    // ?? Telegram API response shapes ?????????????????????????????????????????

    private sealed class TelegramResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("result")]
        public TelegramMessage? Result { get; set; }
    }

    private sealed class TelegramMessage
    {
        [JsonPropertyName("message_id")]
        public int MessageId { get; set; }
    }
}
