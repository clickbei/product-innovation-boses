using BosesApp.Core.Interfaces;
using System.Text.Json;

namespace BosesApp.Core.Services;

/// <summary>
/// Accessibility service - manages user-specific accessibility profiles
/// and applies them to the MAUI application resources at runtime.
/// </summary>
public class AccessibilityService : IAccessibilityService
{
    private AccessibilityProfile _profile = new();
    private readonly IVoiceService _voiceService;
    private readonly string _profilesDir;

    // Font size multipliers per level
    private static readonly double[] FontMultipliers = { 0.8, 1.0, 1.25, 1.5, 2.0 };

    public AccessibilityProfile CurrentProfile => _profile;
    public bool IsHighContrastEnabled => _profile.HighContrast;
    public bool IsMotorAssistEnabled => _profile.LargeButtons;

    public AccessibilityService(IVoiceService voiceService)
    {
        _voiceService = voiceService;
        _profilesDir = GetDataDirectory();
    }

    public async Task LoadProfileAsync(int userId)
    {
        try
        {
            var path = GetProfilePath(userId);
            if (File.Exists(path))
            {
                var json = await File.ReadAllTextAsync(path);
                var loaded = JsonSerializer.Deserialize<AccessibilityProfile>(json);
                if (loaded != null)
                    _profile = loaded;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Accessibility] Load failed: {ex.Message}");
        }
    }

    public async Task SaveProfileAsync(int userId, AccessibilityProfile profile)
    {
        _profile = profile;
        try
        {
            var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(GetProfilePath(userId), json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Accessibility] Save failed: {ex.Message}");
        }
        ApplyProfile();
    }

    public void ApplyProfile()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var app = Application.Current;
                if (app?.Resources == null) return;

                // ── Font scaling ─────────────────────────────────────────────
                var multiplier = GetFontSizeMultiplier();
                app.Resources["AccessibilityFontMultiplier"] = multiplier;
                app.Resources["BodyFontSize"] = 14.0 * multiplier;
                app.Resources["TitleFontSize"] = 24.0 * multiplier;
                app.Resources["SubtitleFontSize"] = 18.0 * multiplier;
                app.Resources["CaptionFontSize"] = 12.0 * multiplier;
                app.Resources["ButtonFontSize"] = 16.0 * multiplier;

                // ── High contrast colours ────────────────────────────────────
                if (_profile.HighContrast)
                {
                    app.Resources["PageBackground"] = Color.FromArgb("#000000");
                    app.Resources["PrimaryText"] = Color.FromArgb("#FFFFFF");
                    app.Resources["SecondaryText"] = Color.FromArgb("#FFFF00");
                    app.Resources["AccentColor"] = Color.FromArgb("#00FF00");
                    app.Resources["CardBackground"] = Color.FromArgb("#1A1A1A");
                    app.Resources["ButtonBackground"] = Color.FromArgb("#FFFFFF");
                    app.Resources["ButtonText"] = Color.FromArgb("#000000");
                    app.Resources["BorderColor"] = Color.FromArgb("#FFFFFF");
                }
                else
                {
                    app.Resources["PageBackground"] = Color.FromArgb("#F5F5F5");
                    app.Resources["PrimaryText"] = Color.FromArgb("#212121");
                    app.Resources["SecondaryText"] = Color.FromArgb("#757575");
                    app.Resources["AccentColor"] = Color.FromArgb("#2ECC71");
                    app.Resources["CardBackground"] = Color.FromArgb("#FFFFFF");
                    app.Resources["ButtonBackground"] = Color.FromArgb("#2ECC71");
                    app.Resources["ButtonText"] = Color.FromArgb("#FFFFFF");
                    app.Resources["BorderColor"] = Color.FromArgb("#E0E0E0");
                }

                // ── Large buttons / motor assist ─────────────────────────────
                app.Resources["MicButtonSize"] = _profile.LargeButtons ? 140.0 : 100.0;
                app.Resources["QuickActionHeight"] = _profile.LargeButtons ? 60.0 : 44.0;
                app.Resources["MinimumTouchTarget"] = _profile.LargeButtons ? 60.0 : 44.0;

                System.Diagnostics.Debug.WriteLine(
                    $"[Accessibility] Profile applied — HighContrast={_profile.HighContrast}, " +
                    $"FontSize={_profile.FontSize}, LargeButtons={_profile.LargeButtons}");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Accessibility] ApplyProfile error: {ex.Message}");
        }
    }

    public async Task AnnounceAsync(string text)
    {
        if (!_profile.ScreenReaderMode) return;

        try
        {
            // Use the voice service to speak the announcement
            await _voiceService.SpeakAsync(text);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Accessibility] Announce error: {ex.Message}");
        }
    }

    public double GetFontSizeMultiplier()
    {
        var idx = Math.Clamp((int)_profile.FontSize, 0, FontMultipliers.Length - 1);
        return FontMultipliers[idx];
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private string GetProfilePath(int userId) =>
        Path.Combine(_profilesDir, $"accessibility_{userId}.json");

    private static string GetDataDirectory()
    {
#if ANDROID || IOS || MACCATALYST
        return FileSystem.AppDataDirectory;
#elif WINDOWS
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Boses");
        Directory.CreateDirectory(path);
        return path;
#else
        return FileSystem.AppDataDirectory;
#endif
    }
}
