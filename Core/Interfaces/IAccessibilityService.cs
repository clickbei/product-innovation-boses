namespace BosesApp.Core.Interfaces;

/// <summary>
/// Accessibility service for enhanced UI support
/// Handles high-contrast, large text, screen reader, and motor accessibility
/// </summary>
public interface IAccessibilityService
{
    /// <summary>
    /// Current accessibility profile for the user
    /// </summary>
    AccessibilityProfile CurrentProfile { get; }

    /// <summary>
    /// Load accessibility settings for a user
    /// </summary>
    Task LoadProfileAsync(int userId);

    /// <summary>
    /// Save accessibility settings
    /// </summary>
    Task SaveProfileAsync(int userId, AccessibilityProfile profile);

    /// <summary>
    /// Apply the current profile to the app's UI resources
    /// </summary>
    void ApplyProfile();

    /// <summary>
    /// Announce text via screen reader / TTS (for screen-reader mode)
    /// </summary>
    Task AnnounceAsync(string text);

    /// <summary>
    /// Get the effective font size multiplier based on profile
    /// </summary>
    double GetFontSizeMultiplier();

    /// <summary>
    /// True if high-contrast mode is active
    /// </summary>
    bool IsHighContrastEnabled { get; }

    /// <summary>
    /// True if large-button / motor-assistance mode is active
    /// </summary>
    bool IsMotorAssistEnabled { get; }
}

/// <summary>
/// User's accessibility preferences
/// </summary>
public class AccessibilityProfile
{
    // Visual
    public bool HighContrast { get; set; } = false;
    public bool InvertColors { get; set; } = false;
    public FontSizeLevel FontSize { get; set; } = FontSizeLevel.Normal;

    // Audio
    public bool ScreenReaderMode { get; set; } = false;
    public float TtsSpeechRate { get; set; } = 1.0f;
    public float TtsVolume { get; set; } = 1.0f;

    // Motor
    public bool LargeButtons { get; set; } = false;
    public bool SingleSwitchMode { get; set; } = false;
    public int TouchHoldDelayMs { get; set; } = 500;

    // Cognitive
    public bool SimplifiedUi { get; set; } = false;
    public bool ShowTextAlongsideIcons { get; set; } = true;
}

/// <summary>
/// Font size levels for accessibility
/// </summary>
public enum FontSizeLevel
{
    Small = 0,
    Normal = 1,
    Large = 2,
    ExtraLarge = 3,
    Huge = 4
}
