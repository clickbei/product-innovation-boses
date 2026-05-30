namespace BosesApp.Core.Configuration;

/// <summary>
/// Configuration helper for the Telegram Guardian notification gateway.
///
/// HOW TO CONFIGURE (5 minutes, completely free):
///   Option 1 — Environment variables (recommended, never commit secrets):
///     Windows PowerShell:
///       $env:TELEGRAM_BOT_TOKEN  = "7123456789:AAExxxxxxxx"
///       $env:TELEGRAM_CHAT_ID    = "123456789"
///
///   Option 2 — Hardcode below for a quick demo (never commit to git):
///     private const string _hardcodedBotToken = "7123456789:AAExxxxxxxx";
///     private const string _hardcodedChatId   = "123456789";
///
/// HOW TO GET A BOT TOKEN:
///   1. Open Telegram → search @BotFather → send /newbot
///   2. Follow prompts → copy the token BotFather gives you
///
/// HOW TO GET THE GUARDIAN CHAT ID:
///   1. Guardian opens Telegram, searches for your bot, sends any message
///   2. Visit https://api.telegram.org/bot{TOKEN}/getUpdates
///   3. Find "chat":{"id": 123456789} — that number is the chat ID
/// </summary>
public static class TelegramConfig
{
    // ── Option 2: paste hardcoded values here for a quick demo ──────────────
    // IMPORTANT: remove these before committing to source control.
    private const string _hardcodedBotToken = ""; // e.g. "7123456789:AAExxxxxxxx"
    private const string _hardcodedChatId   = "8771257662"; // e.g. "123456789"
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Telegram Bot token from @BotFather.
    /// Reads from TELEGRAM_BOT_TOKEN env var, then falls back to the hardcoded value.
    /// Returns <see langword="null"/> when not configured.
    /// </summary>
    public static string? BotToken =>
        Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")
        ?? (string.IsNullOrWhiteSpace(_hardcodedBotToken) ? null : _hardcodedBotToken);

    /// <summary>
    /// Guardian's Telegram chat ID.
    /// Reads from TELEGRAM_CHAT_ID env var, then falls back to the hardcoded value.
    /// Returns <see langword="null"/> when not configured.
    /// </summary>
    public static string? ChatId =>
        Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID")
        ?? (string.IsNullOrWhiteSpace(_hardcodedChatId) ? null : _hardcodedChatId);

    /// <summary>
    /// <see langword="true"/> when both a bot token and a chat ID have been configured.
    /// When <see langword="false"/>, <see cref="SimulatedSmsGateway"/> is used instead.
    /// </summary>
    public static bool IsConfigured =>
        !string.IsNullOrWhiteSpace(BotToken) && !string.IsNullOrWhiteSpace(ChatId);
}
