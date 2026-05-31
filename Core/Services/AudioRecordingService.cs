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
    private const int RecordSampleRate = 16000;

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
        // Guard: already recording
        if (IsRecording)
        {
            Debug.WriteLine("[Audio] Already recording — ignoring StartRecordingAsync");
            return true;
        }

        // Reset simulation flag each attempt so transient failures don't lock us out permanently
        _useSimulatedAudio = false;

        try
        {
            Debug.WriteLine("[Audio] Starting recording...");

            var hasPermission = await RequestPermissionsAsync();
            if (!hasPermission)
            {
                Debug.WriteLine("[Audio] ⚠️ Microphone permission not granted — falling back to simulation");
                _useSimulatedAudio = true;
            }

#if WINDOWS || ANDROID || IOS || MACCATALYST
            // Try real recording first (if not already in simulated mode)
            if (!_useSimulatedAudio && _audioManager != null)
            {
                try
                {

                    var options = new AudioRecorderOptions
                    {
                        SampleRate          = RecordSampleRate,
                        BitDepth            = BitDepth.Pcm16bit,
                        Channels            = ChannelType.Mono,
                        ThrowIfNotSupported = false   // gracefully fall back if platform can't honour request
                    };

                    _audioRecorder = _audioManager.CreateRecorder();
                    await _audioRecorder.StartAsync(options);
                    _recordingStartTime = DateTime.Now;
                    Debug.WriteLine($"[Audio] ✅ REAL recording started — {RecordSampleRate} Hz / 16-bit / mono");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Audio] ⚠️ Real recording failed: {ex.Message} — falling back to simulation");
                    _audioRecorder     = null;
                    _useSimulatedAudio = true;
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
        if (!IsRecording)
        {
            Debug.WriteLine("[Audio] StopRecordingAsync called but not recording");
            return Array.Empty<byte>();
        }

        try
        {
            Debug.WriteLine("[Audio] Stopping recording...");

#if WINDOWS || ANDROID || IOS || MACCATALYST
            if (_audioRecorder != null && !_useSimulatedAudio)
            {
                try
                {
                    var audioSource = await _audioRecorder.StopAsync();

                    using var stream       = audioSource.GetAudioStream();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);

                    var audioData  = memoryStream.ToArray();
                    _audioRecorder = null;
                    _isRecording   = false;

                    Debug.WriteLine($"[Audio] ✅ REAL recording stopped — {audioData.Length} bytes captured from microphone");
                    return audioData;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Audio] ⚠️ Failed to stop real recording: {ex.Message} — falling back to simulation");
                    _audioRecorder     = null;
                    _isRecording       = false;
                    _useSimulatedAudio = true;
                }
            }
#endif

            // Simulation fallback
            _isRecording = false;
            await Task.Delay(100);

            var simulatedData = GenerateSimulatedAudio();
            Debug.WriteLine($"[Audio] 🔄 Simulated recording stopped — {simulatedData.Length} bytes generated");
            return simulatedData;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Audio] Failed to stop recording: {ex.Message}");
            _isRecording   = false;
            _audioRecorder = null;
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// Generates 3 seconds of near-silent PCM at 16 kHz / 16-bit / mono.
    /// Matches the WAV header parameters so Deepgram can decode it cleanly,
    /// though it will return an empty transcript (no real speech).
    /// </summary>
    private static byte[] GenerateSimulatedAudio()
    {
        const int duration = 3;
        var totalBytes     = RecordSampleRate * duration * 2; // 16-bit = 2 bytes per sample
        var audioData      = new byte[totalBytes];

        for (int i = 0; i < totalBytes; i += 2)
        {
            // Very low-amplitude noise — keeps the format valid without fooling Deepgram
            short sample     = (short)Random.Shared.Next(-80, 80);
            audioData[i]     = (byte)(sample & 0xFF);
            audioData[i + 1] = (byte)((sample >> 8) & 0xFF);
        }

        return audioData;
    }
}

