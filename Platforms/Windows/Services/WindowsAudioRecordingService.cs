using System.Diagnostics;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;

namespace BosesApp.Platforms.Windows.Services;

/// <summary>
/// Windows-specific audio recording implementation using Windows.Media.Capture
/// </summary>
public class WindowsAudioRecordingService
{
    private MediaCapture? _mediaCapture;
    private InMemoryRandomAccessStream? _audioStream;
    private bool _isRecording;
    private DateTime _recordingStartTime;

    public bool IsRecording => _isRecording;

    public TimeSpan RecordingDuration => _isRecording
        ? DateTime.Now - _recordingStartTime
        : TimeSpan.Zero;

    public async Task<bool> RequestPermissionsAsync()
    {
        try
        {
            // Windows automatically prompts for microphone permission
            var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Microphone>();
            }

            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Windows Audio] Permission request failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> StartRecordingAsync()
    {
        if (_isRecording)
            return false;

        try
        {
            Debug.WriteLine("[Windows Audio] Starting recording...");

            // Check permissions
            var hasPermission = await RequestPermissionsAsync();
            if (!hasPermission)
            {
                Debug.WriteLine("[Windows Audio] Microphone permission not granted");
                return false;
            }

            // Initialize MediaCapture
            _mediaCapture = new MediaCapture();
            var settings = new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Audio
            };

            await _mediaCapture.InitializeAsync(settings);

            // Create audio stream
            _audioStream = new InMemoryRandomAccessStream();

            // Configure audio encoding
            var encodingProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Medium);

            // Start recording
            await _mediaCapture.StartRecordToStreamAsync(encodingProfile, _audioStream);

            _isRecording = true;
            _recordingStartTime = DateTime.Now;

            Debug.WriteLine("[Windows Audio] Recording started successfully");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Windows Audio] Failed to start recording: {ex.Message}");
            _isRecording = false;
            CleanupMediaCapture();
            return false;
        }
    }

    public async Task<byte[]> StopRecordingAsync()
    {
        if (!_isRecording || _mediaCapture == null || _audioStream == null)
            return Array.Empty<byte>();

        try
        {
            Debug.WriteLine("[Windows Audio] Stopping recording...");

            // Stop recording
            await _mediaCapture.StopRecordAsync();
            _isRecording = false;

            // Read audio data from stream
            _audioStream.Seek(0);
            var reader = new DataReader(_audioStream.GetInputStreamAt(0));
            var bytes = new byte[_audioStream.Size];
            await reader.LoadAsync((uint)_audioStream.Size);
            reader.ReadBytes(bytes);

            Debug.WriteLine($"[Windows Audio] Recording stopped. Captured {bytes.Length} bytes");

            // Cleanup
            CleanupMediaCapture();

            return bytes;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Windows Audio] Failed to stop recording: {ex.Message}");
            CleanupMediaCapture();
            return Array.Empty<byte>();
        }
    }

    private void CleanupMediaCapture()
    {
        try
        {
            _mediaCapture?.Dispose();
            _mediaCapture = null;

            _audioStream?.Dispose();
            _audioStream = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Windows Audio] Cleanup error: {ex.Message}");
        }
    }
}
