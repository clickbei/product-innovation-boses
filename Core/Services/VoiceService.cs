using BosesApp.Core.Interfaces;

namespace BosesApp.Core.Services;

/// <summary>
/// Voice interaction service with real Text-to-Speech
/// Handles speech-to-text and text-to-speech using MAUI APIs
/// </summary>
public class VoiceService : IVoiceService
{
    private bool _isListening;
    private string _simulatedInput = string.Empty;
    private readonly List<string> _demoResponses = new()
    {
        "Magkano ang balance ko?",
        "Ipadala ang 500 pesos kay Juan",
        "Ano ang mga recent transactions ko?",
        "Gusto kong mag-transfer ng pera"
    };

    public bool IsListening => _isListening;
    public bool SimulationMode { get; set; } = false; // Changed to false - use real TTS

    public async Task<bool> StartListeningAsync()
    {
        if (_isListening)
            return false;

        _isListening = true;

        if (SimulationMode)
        {
            // Simulate microphone initialization delay
            await Task.Delay(300);
        }
        else
        {
            // TODO: Initialize Deepgram streaming connection
            // In production, this would establish WebSocket connection to Deepgram
            await Task.Delay(500);
        }

        return true;
    }

    public async Task<string> StopListeningAsync()
    {
        if (!_isListening)
            return string.Empty;

        _isListening = false;

        if (SimulationMode)
        {
            // Return simulated input or random demo response
            await Task.Delay(500); // Simulate processing time

            if (!string.IsNullOrEmpty(_simulatedInput))
            {
                var result = _simulatedInput;
                _simulatedInput = string.Empty;
                return result;
            }

            // Return random demo response
            var random = new Random();
            return _demoResponses[random.Next(_demoResponses.Count)];
        }
        else
        {
            // TODO: Stop Deepgram streaming and get final transcription
            // In production, this would close WebSocket and return transcribed text
            await Task.Delay(800);
            return "Real transcription would appear here";
        }
    }

    public async Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        try
        {
            System.Diagnostics.Debug.WriteLine($"[TTS] Speaking: {text}");

            // Use MAUI's built-in TextToSpeech API
            var locales = await TextToSpeech.Default.GetLocalesAsync();

            // Try to find Filipino/Tagalog locale
            var filipinoLocale = locales.FirstOrDefault(l =>
                l.Language.StartsWith("fil", StringComparison.OrdinalIgnoreCase) ||
                l.Language.StartsWith("tl", StringComparison.OrdinalIgnoreCase));

            // If no Filipino locale, try English (Philippines)
            if (filipinoLocale == null)
            {
                filipinoLocale = locales.FirstOrDefault(l =>
                    l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase) &&
                    l.Country?.Equals("PH", StringComparison.OrdinalIgnoreCase) == true);
            }

            // Fallback to any English locale
            if (filipinoLocale == null)
            {
                filipinoLocale = locales.FirstOrDefault(l =>
                    l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase));
            }

            var settings = new SpeechOptions
            {
                Pitch = 1.0f,      // Normal pitch
                Volume = 1.0f,     // Maximum volume
                Locale = filipinoLocale
            };

            System.Diagnostics.Debug.WriteLine($"[TTS] Using locale: {filipinoLocale?.Name ?? "Default"}");

            // Speak the text using platform TTS
            await TextToSpeech.Default.SpeakAsync(text, settings, cancellationToken);

            System.Diagnostics.Debug.WriteLine($"[TTS] Finished speaking");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TTS Error] {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[TTS Error] Stack: {ex.StackTrace}");

            // Fallback: Simulate speech delay so UI doesn't break
            var duration = Math.Min(text.Length * 50, 5000); // Max 5 seconds
            await Task.Delay(duration, cancellationToken);
        }
    }

    public void SetSimulatedInput(string input)
    {
        _simulatedInput = input;
    }
}
