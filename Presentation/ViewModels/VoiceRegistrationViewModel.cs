using System.Diagnostics;
using BosesApp.Core.Interfaces;
using BosesApp.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BosesApp.Presentation.ViewModels;

/// <summary>
/// ViewModel for voice registration/enrollment process
/// Handles the 3-sample voice biometric registration with speech validation
/// </summary>
public partial class VoiceRegistrationViewModel : ObservableObject
{
    private readonly IAudioRecordingService _audioRecordingService;
    private readonly IAudioAnalysisService _audioAnalysisService;
    private readonly IVoiceAuthService _voiceAuthService;
    private readonly IUserRepository _userRepository;
    private readonly IVoiceService _voiceService;
    private readonly ISpeechRecognitionService _speechRecognitionService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private string _statusMessage = "Ready to register your voice";

    [ObservableProperty]
    private bool _isRecording;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private int _currentSample = 0;

    [ObservableProperty]
    private int _totalSamples = 3;

    [ObservableProperty]
    private double _progress = 0.0;

    [ObservableProperty]
    private string _instructionText = "Tap the button and speak for 5 seconds. We're capturing your unique voice pattern.";

    [ObservableProperty]
    private bool _isComplete;

    [ObservableProperty]
    private string _recordingDuration = "0:00";

    [ObservableProperty]
    private string _audioFeedback = string.Empty;

    [ObservableProperty]
    private string _expectedPhrase = string.Empty;

    [ObservableProperty]
    private bool _showValidationFeedback;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _isValidationSuccess;

    private readonly List<byte[]> _voiceSamples = new();
    private readonly List<string> _validationPhrases = new();
    private int _userId;
    private System.Timers.Timer? _durationTimer;

    public VoiceRegistrationViewModel(
        IAudioRecordingService audioRecordingService,
        IAudioAnalysisService audioAnalysisService,
        IVoiceAuthService voiceAuthService,
        IUserRepository userRepository,
        IVoiceService voiceService,
        ISpeechRecognitionService speechRecognitionService,
        ILocalizationService localizationService)
    {
        _audioRecordingService = audioRecordingService;
        _audioAnalysisService = audioAnalysisService;
        _voiceAuthService = voiceAuthService;
        _userRepository = userRepository;
        _voiceService = voiceService;
        _speechRecognitionService = speechRecognitionService;
        _localizationService = localizationService;
    }

    public async Task InitializeAsync(int userId)
    {
        _userId = userId;
        CurrentSample = 0;
        Progress = 0.0;
        IsComplete = false;
        _voiceSamples.Clear();
        _validationPhrases.Clear();
        ShowValidationFeedback = false;

        // Subscribe to speech recognition events
        _speechRecognitionService.OnRecognitionResultUpdated += OnSpeechRecognitionResultUpdated;
        Debug.WriteLine("[VoiceRegistration] 📢 Subscribed to OnRecognitionResultUpdated event");

        // Load validation phrases
        _validationPhrases.Add(_localizationService.GetString("voice_phrase_1"));
        _validationPhrases.Add(_localizationService.GetString("voice_phrase_2"));
        _validationPhrases.Add(_localizationService.GetString("voice_phrase_3"));

        // Set first expected phrase
        ExpectedPhrase = _validationPhrases[0];

        StatusMessage = "Ready to register your voice";
        InstructionText = _localizationService.GetString("voice_validation_instruction", ExpectedPhrase);
        AudioFeedback = $"💡 For security, you must say the exact phrase shown above.\n\n📝 Phrase {CurrentSample + 1}: \"{ExpectedPhrase}\"";

        // Request microphone permissions
        var hasPermission = await _audioRecordingService.RequestPermissionsAsync();
        if (!hasPermission)
        {
            StatusMessage = "⚠️ Microphone permission required";
            AudioFeedback = "Please allow microphone access in your device settings.";
            await _voiceService.SpeakAsync("Please grant microphone permission to register your voice.");
        }
        else
        {
            await _voiceService.SpeakAsync($"Ready to register your voice. Please say: {ExpectedPhrase}");
        }
    }

    [RelayCommand]
    private async Task ToggleRecordingAsync()
    {
        if (IsBusy || IsComplete) return;

        if (IsRecording)
        {
            await StopRecordingAsync();
        }
        else
        {
            await StartRecordingAsync();
        }
    }

    private async Task StartRecordingAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Starting recording...";

            // Start audio recording
            var started = await _audioRecordingService.StartRecordingAsync();
            await _speechRecognitionService.StartListeningAsync(language: _localizationService.CurrentLanguage == Core.Data.Models.AppLanguage.English ? "en-US" : "fil-PH");
            if (!started)
            {
                StatusMessage = "❌ Failed to start recording. Check microphone permissions.";
                await _voiceService.SpeakAsync("Failed to start recording. Please check microphone permissions.");
                IsBusy = false;
                return;
            }

            // Check if Vosk is available
            if (_speechRecognitionService.IsRealRecognitionAvailable)
            {
                Debug.WriteLine("[VoiceRegistration] ✅ Vosk speech recognition available");

            }
            else
            {
                Debug.WriteLine("[VoiceRegistration] 🔄 Vosk not available, will use simulation");
            }

            IsRecording = true;
            StatusMessage = $"🎤 Recording sample {CurrentSample + 1} of {TotalSamples}...";
            InstructionText = _localizationService.GetString("voice_validation_instruction", ExpectedPhrase);
            AudioFeedback = $"🔴 Recording in progress...\n\n📝 Say: \"{ExpectedPhrase}\"";
            ShowValidationFeedback = false;

            // Start duration timer
            StartDurationTimer();

            // Speak instruction
            await _voiceService.SpeakAsync($"Recording sample {CurrentSample + 1}. Please say: {ExpectedPhrase}");

            // Auto-stop after 5 seconds
            _ = Task.Run(async () =>
            {
                await Task.Delay(5000);
                if (IsRecording)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () => await StopRecordingAsync());
                }
            });

            IsBusy = false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            IsRecording = false;
            IsBusy = false;
        }
    }

    private async Task StopRecordingAsync()
    {
        try
        {
            IsBusy = true;
            StopDurationTimer();
            StatusMessage = "Processing audio...";

            var audioData = await _audioRecordingService.StopRecordingAsync();
            IsRecording = false;
            var recognizedText = await _speechRecognitionService.StopListeningAsync();
            if (audioData == null || audioData.Length == 0)
            {
                StatusMessage = "❌ No audio captured. Please try again.";
                AudioFeedback = "⚠️ No audio detected. Make sure your microphone is working and you're speaking clearly.";
                await _voiceService.SpeakAsync("No audio captured. Please try again.");
                ShowValidationFeedback = false;
                IsBusy = false;
                return;
            }

            // Check if audio contains actual speech using advanced VAD
            // Uses RMS Energy, Zero-Crossing Rate, and Spectral Entropy
            var hasSpeech = _audioAnalysisService.HasSpeechAdvanced(audioData);
            if (!hasSpeech)
            {
                StatusMessage = "❌ No speech detected. Please speak louder.";
                AudioFeedback = "⚠️ The audio is too quiet, silent, or contains only noise.\n\n" +
                               "Please:\n" +
                               "• Speak clearly and at normal volume\n" +
                               "• Reduce background noise\n" +
                               "• Hold the microphone closer";
                await _voiceService.SpeakAsync("No speech detected. Please speak clearly at normal volume and try again.");
                ShowValidationFeedback = false;
                IsBusy = false;
                return;
            }

            // Calculate audio duration (assuming 16-bit, 16kHz)
            var durationSeconds = audioData.Length / (2 * 16000.0);

            // Recognize speech from audio data
            StatusMessage = _localizationService.GetString("voice_validation_processing");
            AudioFeedback = $"📊 Audio Data: {audioData.Length:N0} bytes ({durationSeconds:F1} seconds)\n" +
                           $"⏳ Validating your speech...";

            var languageCode = _localizationService.CurrentLanguage == Core.Data.Models.AppLanguage.English ? "en-US" : "fil-PH";
            //var recognizedText = await _speechRecognitionService.RecognizeAsync(audioData, languageCode);

            if (_speechRecognitionService.IsRealRecognitionAvailable && !string.IsNullOrWhiteSpace(recognizedText))
            {
                Debug.WriteLine($"[VoiceRegistration] ✅ Using REAL Vosk recognition result: '{recognizedText}'");
            }
            else
            {
                Debug.WriteLine($"[VoiceRegistration] 🔄 Using simulated recognition result: '{recognizedText}'");
            }

            if (string.IsNullOrWhiteSpace(recognizedText))
            {
                // Recognition failed
                StatusMessage = "❌ Could not recognize speech. Please try again.";
                AudioFeedback = "⚠️ Speech recognition failed. Please speak clearly and try again.";
                ValidationMessage = _localizationService.GetString("voice_validation_retry");
                IsValidationSuccess = false;
                ShowValidationFeedback = true;
                await _voiceService.SpeakAsync("Could not recognize your speech. Please try again.");
                IsBusy = false;
                return;
            }

            // Validate phrase
            var isValid = _speechRecognitionService.ValidatePhrase(recognizedText, ExpectedPhrase, threshold: 0.7);

            if (!isValid)
            {
                // Phrase doesn't match
                StatusMessage = "❌ Phrase doesn't match";
                AudioFeedback = $"🔍 You said: \"{recognizedText}\"\n" +
                               $"📝 Expected: \"{ExpectedPhrase}\"\n\n" +
                               $"⚠️ Please say the exact phrase shown above.";
                ValidationMessage = _localizationService.GetString("voice_validation_failed", ExpectedPhrase);
                IsValidationSuccess = false;
                ShowValidationFeedback = true;
                await _voiceService.SpeakAsync($"Phrase doesn't match. Please say: {ExpectedPhrase}");
                IsBusy = false;
                return;
            }

            // Validation successful!
            StatusMessage = _localizationService.GetString("voice_validation_success");
            AudioFeedback = $"✅ You said: \"{recognizedText}\"\n" +
                           $"✅ Matches: \"{ExpectedPhrase}\"\n\n" +
                           $"🎉 Phrase validated successfully!";
            ValidationMessage = _localizationService.GetString("voice_validation_success");
            IsValidationSuccess = true;
            ShowValidationFeedback = true;

            // Add sample to collection
            _voiceSamples.Add(audioData);
            CurrentSample++;
            Progress = (double)CurrentSample / TotalSamples;

            if (CurrentSample < TotalSamples)
            {
                // More samples needed - update expected phrase
                ExpectedPhrase = _validationPhrases[CurrentSample];
                InstructionText = _localizationService.GetString("voice_validation_instruction", ExpectedPhrase);
                AudioFeedback += $"\n\n📝 Next phrase: \"{ExpectedPhrase}\"";
                await _voiceService.SpeakAsync($"Sample {CurrentSample} validated. Please say: {ExpectedPhrase}");

                // Hide validation feedback after 3 seconds
                _ = Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    await MainThread.InvokeOnMainThreadAsync(() => ShowValidationFeedback = false);
                });
            }
            else
            {
                // All samples collected and validated, process registration
                AudioFeedback = "✅ All samples validated! Processing your voice fingerprint...";
                await _voiceService.SpeakAsync("All samples validated successfully. Processing your voice registration.");
                await CompleteRegistrationAsync();
            }

            IsBusy = false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            AudioFeedback = $"❌ Error: {ex.Message}";
            IsRecording = false;
            ShowValidationFeedback = false;
            IsBusy = false;
        }
    }

    private async Task CompleteRegistrationAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Processing voice samples...";
            AudioFeedback = "⏳ Step 1/4: Combining audio samples...";
            await _voiceService.SpeakAsync("Processing your voice samples.");

            // Combine all samples for voice print generation
            var combinedAudio = CombineAudioSamples(_voiceSamples);
            AudioFeedback = $"✅ Step 1/4: Combined {_voiceSamples.Count} samples ({combinedAudio.Length:N0} bytes total)";
            await Task.Delay(500);

            // Generate voice print
            StatusMessage = "Extracting voice features...";
            AudioFeedback = "⏳ Step 2/4: Extracting 128 unique voice features...\n" +
                           "   • Energy patterns\n" +
                           "   • Spectral characteristics\n" +
                           "   • Voice texture\n" +
                           "   • Statistical properties";
            var voicePrint = await _voiceAuthService.RegisterVoicePrintAsync(_userId, combinedAudio);
            AudioFeedback = "✅ Step 2/4: Voice features extracted (128-dimensional fingerprint)";
            await Task.Delay(500);

            // Save to database
            StatusMessage = "Saving voice profile...";
            AudioFeedback = "⏳ Step 3/4: Saving voice fingerprint to secure database...";
            var user = await _userRepository.GetUserByIdAsync(_userId);
            if (user != null)
            {
                user.VoicePrintData = voicePrint;
                user.IsVoiceAuthEnabled = true;
                user.HasCompletedOnboarding = true;

                await _userRepository.UpdateUserAsync(user);
            }
            AudioFeedback = "✅ Step 3/4: Voice profile saved securely";
            await Task.Delay(500);

            // Complete
            StatusMessage = "Finalizing registration...";
            AudioFeedback = "⏳ Step 4/4: Finalizing registration...";
            await Task.Delay(500);

            IsComplete = true;
            Progress = 1.0;
            StatusMessage = "✅ Voice registration complete!";
            InstructionText = "Your voice has been successfully registered for secure authentication";
            AudioFeedback = "🎉 Success! Your voice is now registered!\n\n" +
                           "✅ Voice fingerprint created (128 features)\n" +
                           "✅ Securely saved to your profile\n" +
                           "✅ Ready for voice authentication\n\n" +
                           "💡 Your voice is now like a password - unique to you!";

            await _voiceService.SpeakAsync("Voice registration complete! Your voice is now registered for secure authentication.");

            IsBusy = false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Registration failed: {ex.Message}";
            AudioFeedback = $"❌ Error: {ex.Message}\n\nPlease try again or contact support if the problem persists.";
            await _voiceService.SpeakAsync("Registration failed. Please try again.");
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResetRegistrationAsync()
    {
        CurrentSample = 0;
        Progress = 0.0;
        IsComplete = false;
        _voiceSamples.Clear();
        ShowValidationFeedback = false;

        // Reset to first phrase
        ExpectedPhrase = _validationPhrases[0];

        StatusMessage = "Ready to register your voice";
        InstructionText = _localizationService.GetString("voice_validation_instruction", ExpectedPhrase);
        AudioFeedback = $"💡 For security, you must say the exact phrase shown above.\n\n📝 Phrase {CurrentSample + 1}: \"{ExpectedPhrase}\"";

        await _voiceService.SpeakAsync($"Registration reset. Ready to start again. Please say: {ExpectedPhrase}");
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        // Clean up resources before leaving
        Cleanup();

        if (Application.Current?.Windows.FirstOrDefault()?.Page is NavigationPage navPage)
        {
            await navPage.PopAsync();
        }
    }

    private byte[] CombineAudioSamples(List<byte[]> samples)
    {
        var totalLength = samples.Sum(s => s.Length);
        var combined = new byte[totalLength];
        var offset = 0;

        foreach (var sample in samples)
        {
            Buffer.BlockCopy(sample, 0, combined, offset, sample.Length);
            offset += sample.Length;
        }

        return combined;
    }

    private void StartDurationTimer()
    {
        RecordingDuration = "0:00";
        _durationTimer = new System.Timers.Timer(100);
        _durationTimer.Elapsed += (s, e) =>
        {
            if (_audioRecordingService.IsRecording)
            {
                var duration = _audioRecordingService.RecordingDuration;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RecordingDuration = $"{duration.Seconds}:{duration.Milliseconds:D2}";
                });
            }
        };
        _durationTimer.Start();
    }

    private void StopDurationTimer()
    {
        _durationTimer?.Stop();
        _durationTimer?.Dispose();
        _durationTimer = null;
    }

    private void OnSpeechRecognitionResultUpdated(object? sender, RecognitionResultEventArgs e)
    {
        Debug.WriteLine($"[VoiceRegistration] 📢 OnRecognitionResultUpdated triggered!");
        Debug.WriteLine($"[VoiceRegistration] 📝 Recognized text: '{e.RecognizedText}'");
        Debug.WriteLine($"[VoiceRegistration] 📊 Confidence: {e.Confidence:P0}");
        Debug.WriteLine($"[VoiceRegistration] ✅ IsFinal: {e.IsFinal}");

        if (!string.IsNullOrWhiteSpace(e.RecognizedText))
        {
            // Update UI with recognized text
            AudioFeedback = $"🎤 Recognized: \"{e.RecognizedText}\"\n" +
                           $"📊 Confidence: {e.Confidence:P0}\n" +
                           (e.IsFinal ? "✅ Recognition complete" : "⏳ Listening...");

            Debug.WriteLine($"[VoiceRegistration] ✅ AudioFeedback updated");
        }
    }

    public void Cleanup()
    {
        // Unsubscribe from events when the view model is disposed
        if (_speechRecognitionService != null)
        {
            _speechRecognitionService.OnRecognitionResultUpdated -= OnSpeechRecognitionResultUpdated;
            Debug.WriteLine("[VoiceRegistration] 📢 Unsubscribed from OnRecognitionResultUpdated event");
        }

        StopDurationTimer();
    }
}

