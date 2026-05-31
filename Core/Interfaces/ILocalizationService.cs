using BosesApp.Core.Data.Models;

namespace BosesApp.Core.Interfaces;

/// <summary>
/// Service for managing application localization
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Get current application language
    /// </summary>
    AppLanguage CurrentLanguage { get; }

    /// <summary>
    /// Set application language
    /// </summary>
    void SetLanguage(AppLanguage language);

    /// <summary>
    /// Get localized string by key
    /// </summary>
    string GetString(string key);

    /// <summary>
    /// Get localized string with parameters
    /// </summary>
    string GetString(string key, params object[] args);
}
