namespace BosesApp.Core.Configuration;

/// <summary>
/// Configuration helper for Google Gemini AI API.
/// Set the GEMINI_API_KEY environment variable (or replace the placeholder below)
/// to enable live Gemini NLU instead of the rule-based simulation fallback.
/// </summary>
public static class GeminiConfig
{
    // ── How to set your key ──────────────────────────────────────────────────
    // Option 1 (recommended): set an environment variable before launching.
    //   Windows PowerShell:  $env:GEMINI_API_KEY = "AIza..."
    //   Windows CMD:         set GEMINI_API_KEY=AIza...
    //
    // Option 2: replace the empty string below with your key (never commit).
    //   private const string _hardcodedKey = "AIza...";
    //
    // Get a free key at: https://aistudio.google.com/app/apikey
    // Free tier: 15 RPM, 1 M tokens/day — sufficient for demos.
    // ────────────────────────────────────────────────────────────────────────

    private const string _hardcodedKey = ""; // ← paste key here for quick demo (never commit)

    /// <summary>
    /// Returns the Gemini API key from environment variable or the hardcoded fallback.
    /// Returns <see langword="null"/> when no key is configured.
    /// </summary>
    public static string? ApiKey =>
        Environment.GetEnvironmentVariable("GEMINI_API_KEY")
        ?? (string.IsNullOrWhiteSpace(_hardcodedKey) ? null : _hardcodedKey);

    /// <summary>
    /// Model to use. "gemini-1.5-flash" is free-tier friendly and fast.
    /// Alternatives: "gemini-1.5-pro", "gemini-2.0-flash"
    /// </summary>
    public const string ModelId = "gemini-1.5-flash";

    /// <summary>True when a Gemini API key has been configured.</summary>
    public static bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
