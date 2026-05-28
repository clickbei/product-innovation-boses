namespace BosesApp.Core.Interfaces;

/// <summary>
/// Abstraction over any SMS provider.
/// Swap implementations in MauiProgram.cs without touching business logic.
/// </summary>
public interface ISmsGateway
{
    /// <summary>
    /// Send an SMS to the given phone number.
    /// </summary>
    /// <param name="toPhone">E.164 format, e.g. +639171234567</param>
    /// <param name="message">Plain-text message body (max 160 chars recommended)</param>
    Task<SmsResult> SendAsync(string toPhone, string message);

    /// <summary>Human-readable name of the provider (used in logs).</summary>
    string ProviderName { get; }
}

/// <summary>Result returned by any SMS gateway implementation.</summary>
public class SmsResult
{
    public bool    Success          { get; set; }
    public string? MessageId        { get; set; }
    public string? Error            { get; set; }
    /// <summary>Remaining quota reported by the provider (if available).</summary>
    public int?    QuotaRemaining   { get; set; }
}
