using BosesApp.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BosesApp.Presentation.Views;

/// <summary>
/// Base page that supports localization
/// All pages should inherit from this to get automatic language updates
/// </summary>
public abstract class LocalizedContentPage : ContentPage
{
    protected ILocalizationService? LocalizationService { get; private set; }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        // Get localization service from DI
        if (Handler?.MauiContext?.Services != null)
        {
            LocalizationService = Handler.MauiContext.Services.GetService<ILocalizationService>();
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateLocalizedStrings();
    }

    /// <summary>
    /// Override this method to update UI strings when language changes
    /// </summary>
    protected virtual void UpdateLocalizedStrings()
    {
        // Override in derived classes
    }

    /// <summary>
    /// Helper method to get localized string
    /// </summary>
    protected string GetString(string key)
    {
        return LocalizationService?.GetString(key) ?? key;
    }

    /// <summary>
    /// Helper method to get localized string with parameters
    /// </summary>
    protected string GetString(string key, params object[] args)
    {
        return LocalizationService?.GetString(key, args) ?? key;
    }
}

/// <summary>
/// Base ViewModel that supports localization
/// </summary>
public abstract partial class LocalizedViewModel : ObservableObject
{
    protected readonly ILocalizationService _localizationService;

    protected LocalizedViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    /// <summary>
    /// Helper method to get localized string
    /// </summary>
    protected string GetString(string key)
    {
        return _localizationService.GetString(key);
    }

    /// <summary>
    /// Helper method to get localized string with parameters
    /// </summary>
    protected string GetString(string key, params object[] args)
    {
        return _localizationService.GetString(key, args);
    }

    /// <summary>
    /// Call this method when language changes to update all observable properties
    /// </summary>
    public virtual void UpdateLocalizedStrings()
    {
        // Override in derived classes to update observable string properties
    }
}
