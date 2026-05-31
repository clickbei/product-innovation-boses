namespace BosesApp.Core.Data.Models;

/// <summary>
/// User type enumeration
/// </summary>
[Flags]
public enum UserType
{
    None = 0,
    Senior = 1,      // 60+ years old
    PWD = 2,         // Person with Disability
    Both = 3         // Senior + PWD
}

/// <summary>
/// Accessibility needs enumeration
/// </summary>
[Flags]
public enum AccessibilityNeeds
{
    None = 0,
    VisualImpairment = 1,      // Cannot see screen
    HearingImpairment = 2,      // Cannot hear audio
    MotorImpairment = 4,        // Difficulty with touch
    CognitiveImpairment = 8     // Need simplified interface
}

/// <summary>
/// PWD discount category
/// </summary>
public enum PwdCategory
{
    None,
    Mobility,           // Wheelchair, crutches
    Visual,             // Blind, low vision
    Hearing,            // Deaf, hard of hearing
    Cognitive,          // Intellectual disability
    Psychosocial,       // Mental health
    Multiple            // Multiple disabilities
}
