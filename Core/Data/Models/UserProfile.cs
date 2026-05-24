using System.ComponentModel.DataAnnotations;

namespace BosesApp.Core.Data.Models;

/// <summary>
/// User profile entity for SQLite persistence
/// </summary>
public class UserProfile
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// Serialized voice biometric vector (JSON array of floats)
    /// </summary>
    public string? VoicePrintData { get; set; }

    public bool IsVoiceAuthEnabled { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Guardian contact for anti-scam verification
    /// </summary>
    [MaxLength(20)]
    public string? GuardianPhoneNumber { get; set; }

    [MaxLength(100)]
    public string? GuardianName { get; set; }

    /// <summary>
    /// Accessibility preferences (serialized JSON)
    /// </summary>
    public string? AccessibilitySettings { get; set; }

    /// <summary>
    /// Linked bank accounts (encrypted, serialized JSON)
    /// </summary>
    public string? LinkedBankAccounts { get; set; }
}
