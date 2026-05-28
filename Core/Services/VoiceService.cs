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
        // Balance
        "Magkano ang balance ko?",
        "Paki-check ang aking savings account balance",
        "Ano ang laman ng aking account?",
        // Transfer
        "Ipadala ang 500 pesos kay Juan",
        "Mag-transfer ng 1000 pesos kay Maria",
        "Magpadala ng pera kay Pedro",
        // Withdraw / ATM
        "Mag-withdraw ng 2000 pesos",
        "Gusto kong mag-withdraw mula sa ATM",
        // Transactions
        "Ano ang mga recent transactions ko?",
        "Ipakita ang aking kasaysayan ng transaksyon",
        "Mga nakaraang bayad ko",
        // Bill payment
        "Bayaran ang Meralco bill",
        "Mag-bayad ng kuryente bill ng 850 pesos",
        "Bayaran ang PLDT internet bill",
        "Bayad ng Maynilad tubig",
        // GCash / Maya
        "Mag-send ng 300 pesos sa GCash",
        "I-send ang 500 pesos sa Maya",
        // PWD discount
        "Kalkulahin ang PWD discount para sa 1000 pesos na gamot",
        "PWD discount para sa 500 pesos",
        // Senior discount
        "Senior citizen discount para sa 400 pesos na pagkain",
        "Magkano ang senior discount sa 800 pesos na gamot?",
        // Loan
        "Magkano ang pwede ko pang i-loan?",
        "Gusto kong mag-apply ng personal loan",
        // Help
        "Ano ang kaya mong gawin?",
        "Tulong naman",
        "Help",
        // Greeting
        "Kumusta Boses!",
        "Magandang umaga"
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
