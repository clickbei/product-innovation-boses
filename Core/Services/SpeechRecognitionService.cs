using System.Diagnostics;
using BosesApp.Core.Interfaces;

namespace BosesApp.Core.Services;

/// <summary>
/// Speech recognition service with intelligent simulation
/// Ready for future integration with real speech recognition APIs
/// Currently uses smart simulation that validates phrase structure
/// </summary>
public class SpeechRecognitionService : ISpeechRecognitionService
{
    private readonly Random _random;
    private string? _currentLanguage;

    public event EventHandler<RecognitionResultEventArgs>? OnRecognitionResultUpdated;

    public bool IsRealRecognitionAvailable => false; // Simulation-only implementation
    public bool SimulationMode { get; set; } = true;
    public Byte[]  AudioData { get; set; }

    public SpeechRecognitionService()
    {
        _random = new Random();
        Debug.WriteLine("[SpeechRecognition] 🔄 Initialized with intelligent simulation");
        Debug.WriteLine("[SpeechRecognition] 💡 Ready for future real speech recognition integration");
    }

    public async Task<bool> StartListeningAsync(string language = "en-US")
    {
        _currentLanguage = language;
        Debug.WriteLine($"[SpeechRecognition] 🔄 Starting simulated recognition (language: {language})");
        await Task.CompletedTask;
        return true;
    }

    public async Task<string?> StopListeningAsync()
    {
        Debug.WriteLine("[SpeechRecognition] 🔄 Stopping simulated recognition");
        var result = await SimulateRecognitionAsync(_currentLanguage ?? "en-US");

        // Raise the event with recognition result
        if (result != null)
        {
            OnRecognitionResultUpdated?.Invoke(this, new RecognitionResultEventArgs 
            { 
                RecognizedText = result,
                Confidence = 0.95,
                IsFinal = true
            });
            Debug.WriteLine($"[SpeechRecognition] 📢 OnRecognitionResultUpdated raised with: '{result}'");
        }

        return result;
    }

    public async Task<string?> RecognizeAsync(byte[] audioData, string language = "en-US")
    {
        Debug.WriteLine($"[SpeechRecognition] 🔄 Simulating recognition for {audioData.Length} bytes");
        var result = await SimulateRecognitionAsync(language);

        // Raise the event with recognition result
        if (result != null)
        {
            OnRecognitionResultUpdated?.Invoke(this, new RecognitionResultEventArgs 
            { 
                RecognizedText = result,
                Confidence = 0.95,
                IsFinal = true
            });
            Debug.WriteLine($"[SpeechRecognition] 📢 OnRecognitionResultUpdated raised with: '{result}'");
        }

        return result;
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

    public Task<string?> StopListeningAsync(byte[]? audioData = null)
    {
        throw new NotImplementedException();
    }
}
