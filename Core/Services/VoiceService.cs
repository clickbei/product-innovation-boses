using System.Diagnostics;
using BosesApp.Core.Interfaces;

namespace BosesApp.Core.Services;

/// <summary>
/// Voice interaction service with real Speech-to-Text (via MauiSpeechRecognitionService)
/// and real Text-to-Speech (via MAUI TextToSpeech API).
///
/// SimulationMode = false  →  uses the device microphone + CommunityToolkit STT.
/// SimulationMode = true   →  returns canned Filipino phrases (demo / CI safety net).
/// </summary>
public class VoiceService : IVoiceService
{
    private readonly ISpeechRecognitionService _speechRecognition;

    private bool _isListening;
    private string _simulatedInput = string.Empty;
    private readonly List<string> _demoResponses =
    [
        "Magkano ang balance ko?",
        "Ipadala ang 500 pesos kay Juan",
        "Ano ang mga recent transactions ko?",
        "Gusto kong mag-transfer ng pera"
    ];

    public bool IsListening => _isListening;

    private bool _simulationMode;
    public bool SimulationMode
    {
        get => _simulationMode;
        set
        {
            _simulationMode = value;
            // Keep the underlying STT service in sync so it doesn't use real
            // microphone when we are in simulation mode.
            _speechRecognition.SimulationMode = value;
        }
    }

    public VoiceService(ISpeechRecognitionService speechRecognition)
    {
        _speechRecognition = speechRecognition;
        // Default: use real microphone for demo.
        _simulationMode = false;
        _speechRecognition.SimulationMode = false;
    }

    // ── Start listening ────────────────────────────────────────────────────────

    public async Task<bool> StartListeningAsync()
    {
        if (_isListening)
            return false;

        if (SimulationMode)
        {
            _isListening = true;
            await Task.Delay(300); // simulate mic warm-up
            Debug.WriteLine("[VoiceService] 🔄 Simulation mode — mic start skipped");
            return true;
        }

        // Real microphone path — delegate to MauiSpeechRecognitionService
        Debug.WriteLine("[VoiceService] 🎤 Starting real microphone via STT service...");
        var started = await _speechRecognition.StartListeningAsync("fil-PH");
        if (started)
        {
            _isListening = true;
            Debug.WriteLine("[VoiceService] ✅ Real microphone started");
        }
        else
        {
            Debug.WriteLine("[VoiceService] ❌ Failed to start microphone — falling back to simulation");
            _simulationMode = true;
            _isListening = true;
        }

        return true;
    }

    // ── Stop listening ─────────────────────────────────────────────────────────

    public async Task<string> StopListeningAsync()
    {
        if (!_isListening)
            return string.Empty;

        _isListening = false;

        if (SimulationMode)
        {
            await Task.Delay(500); // simulate STT processing

            if (!string.IsNullOrEmpty(_simulatedInput))
            {
                var result = _simulatedInput;
                _simulatedInput = string.Empty;
                Debug.WriteLine($"[VoiceService] 🔄 Simulation: returning preset input '{result}'");
                return result;
            }

            var phrase = _demoResponses[Random.Shared.Next(_demoResponses.Count)];
            Debug.WriteLine($"[VoiceService] 🔄 Simulation: returning demo phrase '{phrase}'");
            return phrase;
        }

        // Real microphone path
        Debug.WriteLine("[VoiceService] 🛑 Stopping real microphone...");
        var transcription = await _speechRecognition.StopListeningAsync();

        if (!string.IsNullOrWhiteSpace(transcription))
        {
            Debug.WriteLine($"[VoiceService] ✅ Transcribed: '{transcription}'");
            return transcription;
        }

        // No speech detected — return empty so the caller can show a "try again" message
        Debug.WriteLine("[VoiceService] ⚠️ No speech detected");
        return string.Empty;
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
