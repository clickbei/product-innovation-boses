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
    /// Date of birth for age calculation
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// User type: Senior, PWD, or Both
    /// </summary>
    public UserType UserType { get; set; } = UserType.None;

    /// <summary>
    /// Accessibility needs flags
    /// </summary>
    public AccessibilityNeeds AccessibilityNeeds { get; set; } = AccessibilityNeeds.None;

    /// <summary>
    /// PWD category if applicable
    /// </summary>
    public PwdCategory PwdCategory { get; set; } = PwdCategory.None;

    /// <summary>
    /// PWD ID number
    /// </summary>
    [MaxLength(50)]
    public string? PwdIdNumber { get; set; }

    /// <summary>
    /// Senior Citizen ID number
    /// </summary>
    [MaxLength(50)]
    public string? SeniorCitizenIdNumber { get; set; }

    /// <summary>
    /// Serialized voice biometric vector (JSON array of floats)
    /// </summary>
    public string? VoicePrintData { get; set; }

    public bool IsVoiceAuthEnabled { get; set; }

    /// <summary>
    /// Indicates if user completed onboarding
    /// </summary>
    public bool HasCompletedOnboarding { get; set; }

    /// <summary>
    /// Indicates if user prefers voice-only mode (no screen interaction)
    /// </summary>
    public bool VoiceOnlyMode { get; set; }

    /// <summary>
    /// Preferred application language (English or Tagalog)
    /// </summary>
    public AppLanguage PreferredLanguage { get; set; } = AppLanguage.English;

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

    /// <summary>
    /// Calculate age from date of birth
    /// </summary>
    public int? Age
    {
        get
        {
            if (!DateOfBirth.HasValue) return null;
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Value.Year;
            if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    /// <summary>
    /// Check if user is a senior citizen (60+)
    /// </summary>
    public bool IsSenior => Age.HasValue && Age.Value >= 60;

    /// <summary>
    /// Check if user has visual impairment
    /// </summary>
    public bool HasVisualImpairment => AccessibilityNeeds.HasFlag(AccessibilityNeeds.VisualImpairment);
}
