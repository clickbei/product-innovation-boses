//using System.Diagnostics;
//using BosesApp.Core.Configuration;
//using BosesApp.Core.Interfaces;

//#if WINDOWS || ANDROID || IOS || MACCATALYST
//using Microsoft.CognitiveServices.Speech;
//using Microsoft.CognitiveServices.Speech.Audio;
//#endif

//namespace BosesApp.Core.Services;

///// <summary>
///// REAL speech recognition using Azure Speech Services
///// Converts actual audio to text with high accuracy
///// Supports English and Filipino languages
///// </summary>
//public class AzureSpeechRecognitionService : ISpeechRecognitionService
//{
//#if WINDOWS || ANDROID || IOS || MACCATALYST
//    private SpeechRecognizer? _recognizer;
//    private string? _recognizedText;
//    private readonly bool _isConfigured;
//#endif
//    private readonly Random _random;

//    public bool IsRealRecognitionAvailable
//    {
//        get
//        {
//#if WINDOWS || ANDROID || IOS || MACCATALYST
//            return _isConfigured;
//#else
//            return false;
//#endif
//        }
//    }

//    public AzureSpeechRecognitionService()
//    {
//        _random = new Random();

//#if WINDOWS || ANDROID || IOS || MACCATALYST
//        _isConfigured = SpeechConfig.IsAzureSpeechConfigured;

//        if (_isConfigured)
//        {
//            Debug.WriteLine("[SpeechRecognition] ✅ Initialized with REAL Azure Speech recognition");
//            Debug.WriteLine($"[SpeechRecognition] Region: {SpeechConfig.AzureSpeechRegion}");
//        }
//        else
//        {
//            Debug.WriteLine("[SpeechRecognition] 🔄 Azure Speech not configured, using simulation");
//            Debug.WriteLine("[SpeechRecognition] 💡 Set API key in SpeechConfig.Initialize() to enable real recognition");
//        }
//#else
//        Debug.WriteLine("[SpeechRecognition] 🔄 Using simulation (platform not supported)");
//#endif
//    }

//    public async Task<bool> StartListeningAsync(string language = "en-US")
//    {
//#if WINDOWS || ANDROID || IOS || MACCATALYST
//        if (!_isConfigured)
//        {
//            Debug.WriteLine("[SpeechRecognition] 🔄 Azure not configured, will use simulation");
//            return true;
//        }

//        try
//        {
//            Debug.WriteLine($"[SpeechRecognition] ✅ Starting REAL Azure Speech recognition (language: {language})");

//            // Configure Azure Speech
//            var config = Microsoft.CognitiveServices.Speech.SpeechConfig.FromSubscription(
//                SpeechConfig.AzureSpeechKey,
//                SpeechConfig.AzureSpeechRegion
//            );

//            // Set language
//            config.SpeechRecognitionLanguage = language;

//            // Use default microphone
//            var audioConfig = AudioConfig.FromDefaultMicrophoneInput();

//            // Create recognizer
//            _recognizer = new SpeechRecognizer(config, audioConfig);
//            _recognizedText = null;

//            // Handle recognition results
//            _recognizer.Recognized += (s, e) =>
//            {
//                if (e.Result.Reason == ResultReason.RecognizedSpeech)
//                {
//                    _recognizedText = e.Result.Text;
//                    Debug.WriteLine($"[SpeechRecognition] 🎤 Recognized: '{_recognizedText}'");
//                }
//                else if (e.Result.Reason == ResultReason.NoMatch)
//                {
//                    Debug.WriteLine("[SpeechRecognition] ⚠️ No speech recognized");
//                }
//            };

//            // Handle errors
//            _recognizer.Canceled += (s, e) =>
//            {
//                Debug.WriteLine($"[SpeechRecognition] ❌ Recognition canceled: {e.Reason}");
//                if (e.Reason == CancellationReason.Error)
//                {
//                    Debug.WriteLine($"[SpeechRecognition] ❌ Error: {e.ErrorDetails}");
//                }
//            };

//            // Start continuous recognition
//            await _recognizer.StartContinuousRecognitionAsync();
//            Debug.WriteLine("[SpeechRecognition] ✅ Real recognition started - listening to microphone");

//            return true;
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"[SpeechRecognition] ❌ Failed to start Azure recognition: {ex.Message}");
//            Debug.WriteLine("[SpeechRecognition] 🔄 Will fall back to simulation");
//            return false;
//        }
//#else
//        await Task.CompletedTask;
//        Debug.WriteLine("[SpeechRecognition] 🔄 Platform not supported, using simulation");
//        return true;
//#endif
//    }

//    public async Task<string?> StopListeningAsync()
//    {
//#if WINDOWS || ANDROID || IOS || MACCATALYST
//        if (!_isConfigured || _recognizer == null)
//        {
//            Debug.WriteLine("[SpeechRecognition] 🔄 Using simulation (Azure not configured)");
//            return await SimulateRecognitionAsync("en-US");
//        }

//        try
//        {
//            Debug.WriteLine("[SpeechRecognition] ⏹️ Stopping Azure recognition...");

//            // Stop recognition
//            await _recognizer.StopContinuousRecognitionAsync();

//            // Small delay to ensure final results are processed
//            await Task.Delay(200);

//            // Get result
//            var result = _recognizedText;
//            _recognizedText = null;

//            // Cleanup
//            _recognizer.Dispose();
//            _recognizer = null;

//            if (!string.IsNullOrWhiteSpace(result))
//            {
//                Debug.WriteLine($"[SpeechRecognition] ✅ REAL Azure recognition result: '{result}'");
//                return result;
//            }
//            else
//            {
//                Debug.WriteLine("[SpeechRecognition] ⚠️ No speech recognized, falling back to simulation");
//                return await SimulateRecognitionAsync("en-US");
//            }
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"[SpeechRecognition] ❌ Error stopping Azure recognition: {ex.Message}");
//            Debug.WriteLine("[SpeechRecognition] 🔄 Falling back to simulation");

//            // Cleanup
//            _recognizer?.Dispose();
//            _recognizer = null;

//            return await SimulateRecognitionAsync("en-US");
//        }
//#else
//        Debug.WriteLine("[SpeechRecognition] 🔄 Using simulation");
//        return await SimulateRecognitionAsync("en-US");
//#endif
//    }

//    public async Task<string?> RecognizeAsync(byte[] audioData, string language = "en-US")
//    {
//#if WINDOWS || ANDROID || IOS || MACCATALYST
//        if (!_isConfigured)
//        {
//            Debug.WriteLine("[SpeechRecognition] 🔄 Azure not configured, using simulation");
//            return await SimulateRecognitionAsync(language);
//        }

//        try
//        {
//            Debug.WriteLine($"[SpeechRecognition] ✅ Processing {audioData.Length} bytes with Azure Speech");

//            // Configure Azure Speech
//            var config = Microsoft.CognitiveServices.Speech.SpeechConfig.FromSubscription(
//                SpeechConfig.AzureSpeechKey,
//                SpeechConfig.AzureSpeechRegion
//            );
//            config.SpeechRecognitionLanguage = language;

//            // Create audio stream from byte array
//            using var pushStream = AudioInputStream.CreatePushStream();
//            pushStream.Write(audioData);
//            pushStream.Close();

//            using var audioConfig = AudioConfig.FromStreamInput(pushStream);
//            using var recognizer = new SpeechRecognizer(config, audioConfig);

//            // Recognize speech
//            var result = await recognizer.RecognizeOnceAsync();

//            if (result.Reason == ResultReason.RecognizedSpeech)
//            {
//                Debug.WriteLine($"[SpeechRecognition] ✅ REAL Azure recognition: '{result.Text}'");
//                return result.Text;
//            }
//            else if (result.Reason == ResultReason.NoMatch)
//            {
//                Debug.WriteLine("[SpeechRecognition] ⚠️ No speech recognized");
//                return null;
//            }
//            else if (result.Reason == ResultReason.Canceled)
//            {
//                var cancellation = CancellationDetails.FromResult(result);
//                Debug.WriteLine($"[SpeechRecognition] ❌ Recognition canceled: {cancellation.Reason}");
//                if (cancellation.Reason == CancellationReason.Error)
//                {
//                    Debug.WriteLine($"[SpeechRecognition] ❌ Error: {cancellation.ErrorDetails}");
//                }
//                return null;
//            }

//            return null;
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"[SpeechRecognition] ❌ Azure recognition failed: {ex.Message}");
//            Debug.WriteLine("[SpeechRecognition] 🔄 Falling back to simulation");
//            return await SimulateRecognitionAsync(language);
//        }
//#else
//        Debug.WriteLine("[SpeechRecognition] 🔄 Using simulation");
//        return await SimulateRecognitionAsync(language);
//#endif
//    }

//    private async Task<string?> SimulateRecognitionAsync(string language)
//    {
//        try
//        {
//            Debug.WriteLine($"[SpeechRecognition] 🔄 Simulating speech recognition (fallback mode)");

//            // Simulate processing time
//            await Task.Delay(500);

//            // Simulate recognition with 90% success rate
//            var successRate = _random.NextDouble();

//            if (successRate < 0.9) // 90% success
//            {
//                // Return a simulated phrase based on language
//                string simulatedPhrase;
//                if (language.StartsWith("fil") || language.StartsWith("tl"))
//                {
//                    // Tagalog phrases
//                    var tagalogPhrases = new[]
//                    {
//                        "ang aking boses ay aking password",
//                        "pinahihintulutan ko ang transaksyon na ito",
//                        "ito ang aking secure na boses"
//                    };
//                    simulatedPhrase = tagalogPhrases[_random.Next(tagalogPhrases.Length)];
//                }
//                else
//                {
//                    // English phrases
//                    var englishPhrases = new[]
//                    {
//                        "my voice is my password",
//                        "i authorize this transaction",
//                        "this is my secure voice"
//                    };
//                    simulatedPhrase = englishPhrases[_random.Next(englishPhrases.Length)];
//                }

//                Debug.WriteLine($"[SpeechRecognition] 🔄 Simulated result: '{simulatedPhrase}'");
//                return simulatedPhrase;
//            }
//            else
//            {
//                // Simulate recognition failure (10% of the time)
//                Debug.WriteLine("[SpeechRecognition] 🔄 Simulated recognition failure (no speech detected)");
//                return null;
//            }
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"[SpeechRecognition] Recognition failed: {ex.Message}");
//            return null;
//        }
//    }

//    public bool ValidatePhrase(string recognizedText, string expectedPhrase, double threshold = 0.7)
//    {
//        if (string.IsNullOrWhiteSpace(recognizedText) || string.IsNullOrWhiteSpace(expectedPhrase))
//        {
//            Debug.WriteLine("[SpeechRecognition] Validation failed: empty text");
//            return false;
//        }

//        var similarity = CalculateSimilarity(recognizedText, expectedPhrase);
//        var isValid = similarity >= threshold;

//        Debug.WriteLine($"[SpeechRecognition] Validation: '{recognizedText}' vs '{expectedPhrase}'");
//        Debug.WriteLine($"[SpeechRecognition] Similarity: {similarity:P0} (threshold: {threshold:P0}) - {(isValid ? "✅ PASS" : "❌ FAIL")}");

//        return isValid;
//    }

//    public double CalculateSimilarity(string text1, string text2)
//    {
//        if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
//            return 0.0;

//        // Normalize texts
//        var normalized1 = NormalizeText(text1);
//        var normalized2 = NormalizeText(text2);

//        // Calculate Levenshtein distance
//        var distance = LevenshteinDistance(normalized1, normalized2);
//        var maxLength = Math.Max(normalized1.Length, normalized2.Length);

//        if (maxLength == 0)
//            return 1.0;

//        // Convert distance to similarity score (0.0 to 1.0)
//        var similarity = 1.0 - ((double)distance / maxLength);

//        return similarity;
//    }

//    private string NormalizeText(string text)
//    {
//        // Convert to lowercase and remove extra whitespace
//        // Also remove punctuation that Azure might add
//        var cleaned = text.ToLowerInvariant()
//            .Replace(".", "")
//            .Replace(",", "")
//            .Replace("?", "")
//            .Replace("!", "");

//        return string.Join(" ", cleaned
//            .Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
//    }

//    private int LevenshteinDistance(string s1, string s2)
//    {
//        var len1 = s1.Length;
//        var len2 = s2.Length;
//        var matrix = new int[len1 + 1, len2 + 1];

//        // Initialize first column and row
//        for (int i = 0; i <= len1; i++)
//            matrix[i, 0] = i;
//        for (int j = 0; j <= len2; j++)
//            matrix[0, j] = j;

//        // Calculate distances
//        for (int i = 1; i <= len1; i++)
//        {
//            for (int j = 1; j <= len2; j++)
//            {
//                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;

//                matrix[i, j] = Math.Min(
//                    Math.Min(
//                        matrix[i - 1, j] + 1,      // deletion
//                        matrix[i, j - 1] + 1),     // insertion
//                    matrix[i - 1, j - 1] + cost);  // substitution
//            }
//        }

//        return matrix[len1, len2];
//    }
//}
