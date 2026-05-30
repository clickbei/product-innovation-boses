using System.Diagnostics;
using BosesApp.Core.Interfaces;

namespace BosesApp.Core.Services;

/// <summary>
/// Voice interaction service.
/// STT ? HybridSpeechRecognitionService (CommunityToolkit ? Deepgram ? Simulation)
/// TTS ? HybridTtsService               (Google Translate TTS ? OS TTS fallback)
///
/// Language detection uses Google Translate's auto-detect API so the correct
/// TTS voice is always selected, with a fast keyword fallback when offline.
/// Demo phrases are auto-translated to match the active app language.
/// </summary>
public class VoiceService : IVoiceService
{
    private readonly ISpeechRecognitionService  _speechRecognition;
    private readonly ITextToSpeechService       _tts;
    private readonly GoogleTranslationService   _translation;

    private bool _isListening;
    private string _simulatedInput = string.Empty;

    // Active language set by ToggleLanguageCommand in MainViewModel ("fil-PH" or "en-US")
    private string _activeLanguage;

    private static readonly string[] _filipinoDemoResponses =
    [
        "Magkano ang balance ko?",
        "Paki-check ang aking savings account balance",
        "Ano ang laman ng aking account?",
        "Ipadala ang 500 pesos kay Juan",
        "Mag-transfer ng 1000 pesos kay Maria",
        "Magpadala ng pera kay Pedro",
        "Mag-withdraw ng 2000 pesos",
        "Gusto kong mag-withdraw mula sa ATM",
        "Ano ang mga recent transactions ko?",
        "Ipakita ang aking kasaysayan ng transaksyon",
        "Mga nakaraang bayad ko",
        "Bayaran ang Meralco bill",
        "Mag-bayad ng kuryente bill ng 850 pesos",
        "Bayaran ang PLDT internet bill",
        "Bayad ng Maynilad tubig",
        "Mag-send ng 300 pesos sa GCash",
        "I-send ang 500 pesos sa Maya",
        "Kalkulahin ang PWD discount para sa 1000 pesos na gamot",
        "PWD discount para sa 500 pesos",
        "Senior citizen discount para sa 400 pesos na pagkain",
        "Magkano ang senior discount sa 800 pesos na gamot?",
        "Magkano ang pwede ko pang i-loan?",
        "Gusto kong mag-apply ng personal loan",
        "Ano ang kaya mong gawin?",
        "Tulong naman",
        "Help",
        "Kumusta Boses!",
        "Magandang umaga"
    ];

    // English translations are cached after first Google Translate call
    private List<string>? _englishDemoResponses;

    public bool IsListening => _isListening;

    private bool _simulationMode;
    public bool SimulationMode
    {
        get => _simulationMode;
        set
        {
            _simulationMode = value;
            _speechRecognition.SimulationMode = value;
        }
    }

    public VoiceService(ISpeechRecognitionService speechRecognition,
                        ITextToSpeechService tts,
                        GoogleTranslationService translation)
    {
        _speechRecognition            = speechRecognition;
        _tts                          = tts;
        _translation                  = translation;
        _simulationMode               = false;
        _speechRecognition.SimulationMode = false;
    }

    /// <summary>
    /// Called by MainViewModel when the user toggles the language.
    /// Clears the English demo cache so phrases are re-translated on next use.
    /// </summary>
    public void SetActiveLanguage(string ietfLanguage)
    {
        _activeLanguage = ietfLanguage;
        if (!ietfLanguage.StartsWith("fil", StringComparison.OrdinalIgnoreCase) &&
            !ietfLanguage.StartsWith("tl",  StringComparison.OrdinalIgnoreCase))
        {
            // Non-Filipino selected — clear cache so English phrases are fetched fresh
            _englishDemoResponses = null;
        }
        Debug.WriteLine($"[VoiceService] Active language set to: {_activeLanguage}");
    }

    // ?? Start listening ????????????????????????????????????????????????????????

    public async Task<bool> StartListeningAsync()
    {
        if (_isListening) return false;

        if (SimulationMode)
        {
            _isListening = true;
            await Task.Delay(300);
            Debug.WriteLine("[VoiceService] ?? Simulation mode — mic start skipped");
            return true;
        }

        Debug.WriteLine("[VoiceService] ?? Starting real microphone...");
        var started = await _speechRecognition.StartListeningAsync(_activeLanguage);

        if (started)
        {
            _isListening = true;
            Debug.WriteLine("[VoiceService] ? Real microphone started");
        }
        else
        {
            Debug.WriteLine("[VoiceService] ? Mic failed — falling back to simulation");
            _simulationMode = true;
            _isListening    = true;
        }

        return true;
    }

    // ?? Stop listening ?????????????????????????????????????????????????????????

    public async Task<string> StopListeningAsync()
    {
        if (!_isListening) return string.Empty;

        _isListening = false;

        if (SimulationMode)
        {
            await Task.Delay(500);

            if (!string.IsNullOrEmpty(_simulatedInput))
            {
                var result = _simulatedInput;
                _simulatedInput = string.Empty;
                Debug.WriteLine($"[VoiceService] ?? Simulation preset: '{result}'");
                return result;
            }

            var phrase = await PickDemoResponseAsync();
            Debug.WriteLine($"[VoiceService] ?? Simulation demo phrase: '{phrase}'");
            return phrase;
        }

        Debug.WriteLine("[VoiceService] ?? Stopping real microphone...");
        var transcription = await _speechRecognition.StopListeningAsync();

        if (!string.IsNullOrWhiteSpace(transcription))
        {
            Debug.WriteLine($"[VoiceService] ? Transcribed: '{transcription}'");
            return transcription;
        }

        Debug.WriteLine("[VoiceService] ?? No speech detected");
        return string.Empty;
    }

    // ?? Speak ??????????????????????????????????????????????????????????????????

    /// <summary>
    /// Speaks <paramref name="text"/> through the hybrid TTS chain.
    /// Language is detected via Google Translate's auto-detect API (falls back
    /// to keyword heuristic when offline).
    /// </summary>
    public async Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        // Use Google's API to detect language; keyword heuristic is the offline fallback
        var language = await _translation.DetectLanguageAsync(text, cancellationToken);

        if (language != _activeLanguage)
          text = await _translation.TranslateToAsync(text, _activeLanguage);

        Debug.WriteLine($"[VoiceService] ?? TTS [{language}] via {_tts.ProviderName}: "
            + $"{text[..Math.Min(60, text.Length)]}…");

        await _tts.SpeakAsync(text, language, cancellationToken);
    }

    public void SetSimulatedInput(string input) => _simulatedInput = input;

    // ?? Demo response picker ???????????????????????????????????????????????????

    /// <summary>
    /// Returns a random demo phrase in the active language.
    /// Filipino phrases are served from a static list.
    /// English phrases are translated on first use and then cached.
    /// </summary>
    private async Task<string> PickDemoResponseAsync()
    {
        var isFilipino = _activeLanguage.StartsWith("fil", StringComparison.OrdinalIgnoreCase)
                      || _activeLanguage.StartsWith("tl",  StringComparison.OrdinalIgnoreCase);

        if (isFilipino)
            return _filipinoDemoResponses[Random.Shared.Next(_filipinoDemoResponses.Length)];

        // English — translate lazily and cache the whole list
        if (_englishDemoResponses is null)
        {
            Debug.WriteLine("[VoiceService] Translating demo phrases to English via Google Translate…");
            var tasks   = _filipinoDemoResponses.Select(p => _translation.TranslateToAsync(p, "en-US"));
            var results = await Task.WhenAll(tasks);
            _englishDemoResponses = [.. results];
            Debug.WriteLine($"[VoiceService] ? Cached {_englishDemoResponses.Count} English demo phrases");
        }

        return _englishDemoResponses[Random.Shared.Next(_englishDemoResponses.Count)];
    }
}
