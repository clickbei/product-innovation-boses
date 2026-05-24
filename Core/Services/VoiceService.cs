using BosesApp.Core.Interfaces;

namespace BosesApp.Core.Services;

/// <summary>
/// Voice interaction service with Deepgram simulation
/// Handles speech-to-text and text-to-speech with fallback modes
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
    public bool SimulationMode { get; set; } = true;

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

        if (SimulationMode)
        {
            // Simulate text-to-speech processing time
            var duration = Math.Min(text.Length * 50, 5000); // Max 5 seconds
            await Task.Delay(duration, cancellationToken);

            // In demo mode, just log the speech
            System.Diagnostics.Debug.WriteLine($"[TTS] Speaking: {text}");
        }
        else
        {
            // TODO: Use platform-specific TTS
            // iOS: AVSpeechSynthesizer
            // Android: TextToSpeech
            // Windows: SpeechSynthesizer
            await Task.Delay(1000, cancellationToken);
        }
    }

    public void SetSimulatedInput(string input)
    {
        _simulatedInput = input;
    }
}
