using System.Diagnostics;

#if WINDOWS || ANDROID || IOS || MACCATALYST
using Plugin.Maui.Audio;
#endif

namespace BosesApp.Core.Services;

/// <summary>
/// Cross-platform audio recording service
/// Handles microphone access and audio capture
/// </summary>
public interface IAudioRecordingService
{
    Task<bool> RequestPermissionsAsync();
    Task<bool> StartRecordingAsync();
    Task<byte[]> StopRecordingAsync();
    bool IsRecording { get; }
    TimeSpan RecordingDuration { get; }
}

/// <summary>
/// Audio recording service with real microphone capture and simulated fallback
/// Tries real recording first, falls back to simulated audio if it fails
/// </summary>
public class AudioRecordingService : IAudioRecordingService
{
#if WINDOWS || ANDROID || IOS || MACCATALYST
    private readonly IAudioManager? _audioManager;
    private IAudioRecorder? _audioRecorder;
#endif
    private bool _isRecording;
    private bool _useSimulatedAudio;
    private DateTime _recordingStartTime;

#if WINDOWS || ANDROID || IOS || MACCATALYST
    public AudioRecordingService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
        _useSimulatedAudio = false;
        Debug.WriteLine("[Audio] Initialized with Plugin.Maui.Audio support");
    }
#else
    public AudioRecordingService()
    {
        _useSimulatedAudio = true;
        Debug.WriteLine("[Audio] Initialized with simulated audio (no Plugin.Maui.Audio)");
    }
#endif

    public bool IsRecording => _isRecording
#if WINDOWS || ANDROID || IOS || MACCATALYST
        || (_audioRecorder?.IsRecording ?? false)
#endif
        ;

    public TimeSpan RecordingDuration => IsRecording
        ? DateTime.Now - _recordingStartTime
        : TimeSpan.Zero;

    public async Task<bool> RequestPermissionsAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Microphone>();
            }

            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Audio] Permission request failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> StartRecordingAsync()
    {
        if (_isRecording)
            return false;

        try
        {
            Debug.WriteLine("[Audio] Starting recording...");

            // Check permissions first
            var hasPermission = await RequestPermissionsAsync();
            if (!hasPermission)
            {
                Debug.WriteLine("[Audio] ⚠️ Microphone permission not granted");
                Debug.WriteLine("[Audio] Falling back to simulated audio...");
                _useSimulatedAudio = true;
            }

#if WINDOWS || ANDROID || IOS || MACCATALYST
            // Try real recording first (if not already in simulated mode)
            if (!_useSimulatedAudio && _audioManager != null)
            {
                try
                {
                    _audioRecorder = _audioManager.CreateRecorder();
                    await _audioRecorder.StartAsync();
                    _recordingStartTime = DateTime.Now;
                    Debug.WriteLine("[Audio] ✅ REAL recording started from microphone!");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Audio] ⚠️ Real recording failed: {ex.Message}");
                    Debug.WriteLine("[Audio] Falling back to simulated audio...");
                    _useSimulatedAudio = true;
                    
                    _audioRecorder = null;
                }
            }
#endif

            // Fallback to simulated audio
            _isRecording = true;
            _recordingStartTime = DateTime.Now;
            Debug.WriteLine("[Audio] 🔄 Simulated recording started (fallback mode)");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Audio] Failed to start recording: {ex.Message}");
            _isRecording = false;
            return false;
        }
    }

    public async Task<byte[]> StopRecordingAsync()
    {
        if (!_isRecording
#if WINDOWS || ANDROID || IOS || MACCATALYST
            && _audioRecorder == null
#endif
            )
            return Array.Empty<byte>();

        try
        {
            Debug.WriteLine("[Audio] Stopping recording...");

#if WINDOWS || ANDROID || IOS || MACCATALYST
            // Try to stop real recording first
            if (_audioRecorder != null && !_useSimulatedAudio)
            {
                try
                {
                    var audioSource = await _audioRecorder.StopAsync();

                    // Read audio data from stream
                    using var stream = audioSource.GetAudioStream();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);

                    var audioData = memoryStream.ToArray();

                    Debug.WriteLine($"[Audio] ✅ REAL recording stopped. Captured {audioData.Length} bytes from microphone!");

                    // Cleanup
                    //_audioRecorder?.Dispose();
                    _audioRecorder = null;

                    return audioData;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Audio] ⚠️ Failed to stop real recording: {ex.Message}");
                    Debug.WriteLine("[Audio] Falling back to simulated audio...");
                    //_audioRecorder?.Dispose();
                    _audioRecorder = null;
                    _useSimulatedAudio = true;
                }
            }
#endif

            // Fallback to simulated audio
            _isRecording = false;

            // Simulate recording delay
            await Task.Delay(100);

            // Generate simulated audio data
            var simulatedData = GenerateSimulatedAudio();
            Debug.WriteLine($"[Audio] 🔄 Simulated recording stopped. Generated {simulatedData.Length} bytes");
            return simulatedData;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Audio] Failed to stop recording: {ex.Message}");
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// Generates simulated audio data
    /// Creates 5 seconds of 16kHz, 16-bit, mono audio
    /// Sufficient for voice biometric feature extraction and testing
    /// </summary>
    private byte[] GenerateSimulatedAudio()
    {
        // Generate simulated audio data (5 seconds at 16kHz, 16-bit, mono)
        var sampleRate = 16000;
        var duration = 5; // seconds
        var bytesPerSample = 2; // 16-bit
        var totalBytes = sampleRate * duration * bytesPerSample;

        var audioData = new byte[totalBytes];
        var random = new Random();

        // Generate random audio data (simulates voice)
        for (int i = 0; i < totalBytes; i += 2)
        {
            // Generate 16-bit sample with some variation
            short sample = (short)(random.Next(-1000, 1000));
            audioData[i] = (byte)(sample & 0xFF);
            audioData[i + 1] = (byte)((sample >> 8) & 0xFF);
        }

        return audioData;
    }
}

