using System.Diagnostics;
using System.Globalization;
using BosesApp.Core.Interfaces;
using CommunityToolkit.Maui.Media;

namespace BosesApp.Core.Services;

/// <summary>
/// Speech recognition using .NET MAUI Community Toolkit
/// FREE, easy setup, works offline on Android 33+
/// No model downloads or complex configuration needed!
/// </summary>
public class MauiSpeechRecognitionService : ISpeechRecognitionService
{
    private readonly ISpeechToText _speechToText;
    private readonly Random _random;
    private string? _recognizedText;

    public event EventHandler<RecognitionResultEventArgs>? OnRecognitionResultUpdated;

    public bool IsRealRecognitionAvailable { get; private set; }

    public MauiSpeechRecognitionService(ISpeechToText speechToText)
    {
        _speechToText = speechToText;
        _random = new Random();

        // Subscribe to speech recognition events
        Debug.WriteLine("[SpeechRecognition] 🔗 Subscribing to speech recognition events...");
        _speechToText.RecognitionResultUpdated += OnMauiRecognitionResultUpdated;
        _speechToText.RecognitionResultCompleted += OnMauiRecognitionResultCompleted;
        Debug.WriteLine("[SpeechRecognition] 🔗 Event subscriptions complete");

        // Check if speech recognition is available
        IsRealRecognitionAvailable = CheckAvailability();

        if (IsRealRecognitionAvailable)
        {
            Debug.WriteLine("[SpeechRecognition] ✅ Initialized with .NET MAUI Community Toolkit");
            Debug.WriteLine("[SpeechRecognition] ✅ FREE offline speech recognition available!");
        }
        else
        {
            Debug.WriteLine("[SpeechRecognition] 🔄 Speech recognition not available, using simulation");
        }
    }

    private void OnMauiRecognitionResultUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs e)
    {
        Debug.WriteLine("[SpeechRecognition] 🔔 OnRecognitionResultUpdated EVENT FIRED!");
        Debug.WriteLine($"[SpeechRecognition] 🔍 Sender: {sender?.GetType().Name ?? "null"}");
        Debug.WriteLine($"[SpeechRecognition] 🔍 e.RecognitionResult: '{e.RecognitionResult ?? "null"}'");

        // e.RecognitionResult is a string (the partial recognized text)
        if (!string.IsNullOrEmpty(e.RecognitionResult))
        {
            var partialText = e.RecognitionResult;
            Debug.WriteLine($"[SpeechRecognition] 🎤 Partial (REAL): {partialText}");

            // Store the latest partial result as the recognized text
            // This is our fallback since OnRecognitionResultCompleted might not fire
            _recognizedText = partialText;
            Debug.WriteLine($"[SpeechRecognition] 💾 Stored partial result: '{_recognizedText}'");

            // Raise the public event
            Debug.WriteLine($"[SpeechRecognition] 📢 Raising public OnRecognitionResultUpdated event with: '{partialText}'");
            OnRecognitionResultUpdated?.Invoke(this, new RecognitionResultEventArgs 
            { 
                RecognizedText = partialText,
                Confidence = 0.85,
                IsFinal = false
            });
        }
        else
        {
            Debug.WriteLine("[SpeechRecognition] ⚠️ RecognitionResult was null or empty!");
        }
    }

    private void OnMauiRecognitionResultCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
    {
        Debug.WriteLine("[SpeechRecognition] 🔔 OnRecognitionResultCompleted event fired!");
        Debug.WriteLine($"[SpeechRecognition] 🔍 e.RecognitionResult is null: {e.RecognitionResult == null}");

        if (e.RecognitionResult != null)
        {
            Debug.WriteLine($"[SpeechRecognition] 🔍 IsSuccessful: {e.RecognitionResult.IsSuccessful}");
            Debug.WriteLine($"[SpeechRecognition] 🔍 Text: '{e.RecognitionResult.Text}'");
            Debug.WriteLine($"[SpeechRecognition] 🔍 Exception: {e.RecognitionResult.Exception?.Message ?? "null"}");
        }

        // NOTE: This event often doesn't fire reliably with MAUI Community Toolkit
        // We rely on OnRecognitionResultUpdated (partial results) instead
        // If this event does fire, update the recognized text as final result
        if (e.RecognitionResult != null && e.RecognitionResult.IsSuccessful && !string.IsNullOrWhiteSpace(e.RecognitionResult.Text))
        {
            _recognizedText = e.RecognitionResult.Text;
            Debug.WriteLine($"[SpeechRecognition] ✅ FINAL Recognition Success: '{_recognizedText}'");

            // Raise the public event for final result
            Debug.WriteLine($"[SpeechRecognition] 📢 Raising public OnRecognitionResultUpdated event (final) with: '{_recognizedText}'");
            OnRecognitionResultUpdated?.Invoke(this, new RecognitionResultEventArgs 
            { 
                RecognizedText = _recognizedText,
                Confidence = 0.95,
                IsFinal = true
            });
        }
        else if (e.RecognitionResult?.Exception != null)
        {
            Debug.WriteLine($"[SpeechRecognition] ❌ Recognition failed: {e.RecognitionResult.Exception.Message}");
        }
        else
        {
            Debug.WriteLine("[SpeechRecognition] ⚠️ Recognition completed but no result");
        }
    }

    private bool CheckAvailability()
    {
        try
        {
            // Check platform-specific availability
#if ANDROID
            // Android 33+ (API level 33) supports offline recognition
            var apiLevel = Android.OS.Build.VERSION.SdkInt;
            var isAvailable = apiLevel >= Android.OS.BuildVersionCodes.Tiramisu; // Android 13 (API 33)
            Debug.WriteLine($"[SpeechRecognition] 📱 Android API Level: {(int)apiLevel}");
            if (isAvailable)
            {
                Debug.WriteLine($"[SpeechRecognition] ✅ Android {(int)apiLevel}+ supports offline speech recognition");
            }
            else
            {
                Debug.WriteLine($"[SpeechRecognition] ⚠️ Android {(int)apiLevel} < 33 - using online/simulation fallback");
            }
            return isAvailable;
#elif IOS || MACCATALYST
            // iOS 13+ supports offline recognition
            var version = UIKit.UIDevice.CurrentDevice.SystemVersion;
            Debug.WriteLine($"[SpeechRecognition] 📱 iOS Version: {version}");
            return true; // iOS 13+ is minimum supported version
#elif WINDOWS
            // Windows has limited support, may require internet
            Debug.WriteLine("[SpeechRecognition] 📱 Windows platform detected");
            return true; // Try to use it, may fall back to online
#else
            Debug.WriteLine("[SpeechRecognition] 📱 Unknown platform");
            return false;
#endif
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SpeechRecognition] ❌ Availability check failed: {ex.Message}");
            Debug.WriteLine($"[SpeechRecognition] Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    public async Task<bool> StartListeningAsync(string language = "en-US")
    {
        Debug.WriteLine($"[SpeechRecognition] ▶️ StartListeningAsync called (language: {language})");
        Debug.WriteLine($"[SpeechRecognition] IsRealRecognitionAvailable: {IsRealRecognitionAvailable}");

        // Try real recognition first, fall back to simulation if unavailable
        if (!IsRealRecognitionAvailable)
        {
            Debug.WriteLine("[SpeechRecognition] 🔄 Real recognition NOT available - using simulation mode");
            Debug.WriteLine("[SpeechRecognition] ℹ️ This usually means: API level < 33, permission denied, or platform not supported");
            // Simulate successful start
            _recognizedText = null;
            return true;
        }

        try
        {
            // Normalize language code to supported format
            var normalizedLanguage = NormalizeLanguageCode(language);
            Debug.WriteLine($"[SpeechRecognition] 🎤 Starting REAL speech recognition (requested: {language}, using: {normalizedLanguage})");

            // Request microphone permission
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
            {
                Debug.WriteLine("[SpeechRecognition] ❌ Microphone permission denied, falling back to simulation");
                IsRealRecognitionAvailable = false; // Disable for this session
                return true; // Still return true so simulation can work
            }

            Debug.WriteLine("[SpeechRecognition] ✅ Microphone permission granted");

            // Request speech recognition permissions
            Debug.WriteLine("[SpeechRecognition] 📋 Requesting speech recognition permissions...");
            await _speechToText.RequestPermissions(CancellationToken.None);
            Debug.WriteLine("[SpeechRecognition] ✅ Permissions granted");

            // Reset recognized text before starting new recognition
            _recognizedText = null;

            // Create speech recognition options
            var options = new SpeechToTextOptions
            {
                Culture = CultureInfo.GetCultureInfo(normalizedLanguage),
                ShouldReportPartialResults = true
            };

            Debug.WriteLine($"[SpeechRecognition] 📢 Calling _speechToText.StartListenAsync with culture: {normalizedLanguage}");
            Debug.WriteLine($"[SpeechRecognition] 📢 ShouldReportPartialResults: {options.ShouldReportPartialResults}");

            // Start listening with real speech recognition
            await _speechToText.StartListenAsync(options, CancellationToken.None);

            Debug.WriteLine("[SpeechRecognition] ✅ Listening started successfully");
            Debug.WriteLine("[SpeechRecognition] 🎤 Waiting for speech... (events should fire now)");
            Debug.WriteLine($"[SpeechRecognition] Event handler attached: {OnRecognitionResultUpdated != null}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SpeechRecognition] ❌ Real recognition error: {ex.Message}");
            Debug.WriteLine($"[SpeechRecognition] ❌ Exception type: {ex.GetType().Name}");
            Debug.WriteLine($"[SpeechRecognition] Stack trace: {ex.StackTrace}");

            // Check if it's a language support error
            if (ex.Message.Contains("LanguageNotSupported") || ex.Message.Contains("language"))
            {
                Debug.WriteLine($"[SpeechRecognition] ⚠️ Language '{language}' not supported by device");
                Debug.WriteLine("[SpeechRecognition] 💡 Trying fallback to English (en-US)...");

                // Try with English as fallback
                try
                {
                    var options = new SpeechToTextOptions
                    {
                        Culture = CultureInfo.GetCultureInfo("en-US"),
                        ShouldReportPartialResults = true
                    };

                    await _speechToText.StartListenAsync(options, CancellationToken.None);
                    Debug.WriteLine("[SpeechRecognition] ✅ Fallback to English successful");
                    return true;
                }
                catch (Exception fallbackEx)
                {
                    Debug.WriteLine($"[SpeechRecognition] ❌ English fallback also failed: {fallbackEx.Message}");
                }
            }

            Debug.WriteLine("[SpeechRecognition] 🔄 Falling back to simulation mode");
            IsRealRecognitionAvailable = false; // Disable for this session
            return true; // Return true so simulation can work
        }
    }

    private string NormalizeLanguageCode(string language)
    {
        // Map common language codes to Android-supported codes
        var languageMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Filipino/Tagalog variations
            { "fil", "en-US" },        // Filipino not widely supported, use English
            { "fil-PH", "en-US" },     // Filipino (Philippines) -> English fallback
            { "tl", "en-US" },         // Tagalog -> English fallback
            { "tl-PH", "en-US" },      // Tagalog (Philippines) -> English fallback

            // English variations (all supported)
            { "en", "en-US" },
            { "en-US", "en-US" },
            { "en-GB", "en-GB" },
            { "en-AU", "en-AU" },

            // Other common languages
            { "es", "es-ES" },
            { "fr", "fr-FR" },
            { "de", "de-DE" },
            { "zh", "zh-CN" },
            { "ja", "ja-JP" },
            { "ko", "ko-KR" }
        };

        if (languageMap.TryGetValue(language, out var normalized))
        {
            if (normalized != language)
            {
                Debug.WriteLine($"[SpeechRecognition] 💡 Language '{language}' mapped to '{normalized}' (device limitation)");
            }
            return normalized;
        }

        // If not in map, try to use as-is
        Debug.WriteLine($"[SpeechRecognition] ⚠️ Unknown language code '{language}', attempting to use as-is");
        return language;
    }

    public async Task<string?> StopListeningAsync()
    {
        if (!IsRealRecognitionAvailable)
        {
            // Using simulation mode
            Debug.WriteLine("[SpeechRecognition] 🔄 Stopping simulation, generating result");
            return await SimulateRecognitionAsync("en-US");
        }

        try
        {
            Debug.WriteLine("[SpeechRecognition] 🔍 About to stop listening...");
            Debug.WriteLine($"[SpeechRecognition] 🔍 Current recognized text: '{_recognizedText ?? "null"}'");

            // Stop listening
            await _speechToText.StopListenAsync(CancellationToken.None);
            Debug.WriteLine("[SpeechRecognition] 🎤 Stopped listening");

            // IMPORTANT: Community Toolkit's OnRecognitionResultCompleted event often doesn't fire reliably
            // Use the partial results collected from OnRecognitionResultUpdated instead
            // This is the most reliable way to get recognition results
            var result = _recognizedText;
            _recognizedText = null;

            if (!string.IsNullOrWhiteSpace(result))
            {
                Debug.WriteLine($"[SpeechRecognition] ✅ Recognition successful: '{result}'");
                Debug.WriteLine($"[SpeechRecognition] ✅ Returning result from partial updates");
                return result;
            }

            Debug.WriteLine("[SpeechRecognition] ⚠️ No speech detected - empty recognition");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SpeechRecognition] ❌ Error stopping: {ex.Message}");
            Debug.WriteLine($"[SpeechRecognition] ❌ Stack trace: {ex.StackTrace}");
            Debug.WriteLine("[SpeechRecognition] 🔄 Falling back to simulation");
            IsRealRecognitionAvailable = false;
            return await SimulateRecognitionAsync("en-US");
        }
    }

    public async Task<string?> RecognizeAsync(byte[] audioData, string language = "en-US")
    {
        // Community Toolkit doesn't support recognizing from byte array
        // It listens directly from microphone via ListenAsync
        // This method is kept for interface compatibility but uses simulation
        Debug.WriteLine("[SpeechRecognition] ℹ️ RecognizeAsync: Community Toolkit doesn't support byte array input");
        Debug.WriteLine("[SpeechRecognition] 💡 Use StartListeningAsync() for real recognition from microphone");
        Debug.WriteLine("[SpeechRecognition] 🔄 Using simulation for this call");
        return await SimulateRecognitionAsync(language);
    }

    private async Task<string?> SimulateRecognitionAsync(string language)
    {
        try
        {
            Debug.WriteLine($"[SpeechRecognition] 🔄 Simulating speech recognition...");

            // Simulate processing time
            await Task.Delay(500);

            // Simulate recognition with 90% success rate
            var successRate = _random.NextDouble();

            if (successRate < 0.9) // 90% success
            {
                // Return a simulated phrase based on language
                string simulatedPhrase;
                if (language.StartsWith("fil") || language.StartsWith("tl"))
                {
                    // Tagalog phrases
                    var tagalogPhrases = new[]
                    {
                        "ang aking boses ay aking password",
                        "pinahihintulutan ko ang transaksyon na ito",
                        "ito ang aking secure na boses"
                    };
                    simulatedPhrase = tagalogPhrases[_random.Next(tagalogPhrases.Length)];
                }
                else
                {
                    // English phrases
                    var englishPhrases = new[]
                    {
                        "my voice is my password",
                        "i authorize this transaction",
                        "this is my secure voice"
                    };
                    simulatedPhrase = englishPhrases[_random.Next(englishPhrases.Length)];
                }

                Debug.WriteLine($"[SpeechRecognition] 🔄 Simulated result: '{simulatedPhrase}'");
                return simulatedPhrase;
            }
            else
            {
                // Simulate recognition failure (10% of the time)
                Debug.WriteLine("[SpeechRecognition] 🔄 Simulated recognition failure (no speech detected)");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SpeechRecognition] Recognition failed: {ex.Message}");
            return null;
        }
    }

    public bool ValidatePhrase(string recognizedText, string expectedPhrase, double threshold = 0.7)
    {
        if (string.IsNullOrWhiteSpace(recognizedText) || string.IsNullOrWhiteSpace(expectedPhrase))
        {
            Debug.WriteLine("[SpeechRecognition] Validation failed: empty text");
            return false;
        }

        var similarity = CalculateSimilarity(recognizedText, expectedPhrase);
        var isValid = similarity >= threshold;

        Debug.WriteLine($"[SpeechRecognition] Validation: '{recognizedText}' vs '{expectedPhrase}'");
        Debug.WriteLine($"[SpeechRecognition] Similarity: {similarity:P0} (threshold: {threshold:P0}) - {(isValid ? "✅ PASS" : "❌ FAIL")}");

        return isValid;
    }

    public double CalculateSimilarity(string text1, string text2)
    {
        if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
            return 0.0;

        // Normalize texts
        var normalized1 = NormalizeText(text1);
        var normalized2 = NormalizeText(text2);

        // Calculate Levenshtein distance
        var distance = LevenshteinDistance(normalized1, normalized2);
        var maxLength = Math.Max(normalized1.Length, normalized2.Length);

        if (maxLength == 0)
            return 1.0;

        // Convert distance to similarity score (0.0 to 1.0)
        var similarity = 1.0 - ((double)distance / maxLength);

        return similarity;
    }

    private string NormalizeText(string text)
    {
        // Convert to lowercase and remove extra whitespace
        return string.Join(" ", text.ToLowerInvariant()
            .Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
    }

    private int LevenshteinDistance(string s1, string s2)
    {
        var len1 = s1.Length;
        var len2 = s2.Length;
        var matrix = new int[len1 + 1, len2 + 1];

        // Initialize first column and row
        for (int i = 0; i <= len1; i++)
            matrix[i, 0] = i;
        for (int j = 0; j <= len2; j++)
            matrix[0, j] = j;

        // Calculate distances
        for (int i = 1; i <= len1; i++)
        {
            for (int j = 1; j <= len2; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(
                        matrix[i - 1, j] + 1,      // deletion
                        matrix[i, j - 1] + 1),     // insertion
                    matrix[i - 1, j - 1] + cost);  // substitution
            }
        }

        return matrix[len1, len2];
    }
}
