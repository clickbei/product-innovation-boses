using Android.Media;
using System.Diagnostics;

namespace BosesApp.Platforms.Android.Services;

/// <summary>
/// Android-specific audio recording implementation using MediaRecorder
/// </summary>
public class AndroidAudioRecordingService
{
    private MediaRecorder? _mediaRecorder;
    private string? _audioFilePath;
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
            var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Microphone>();
            }

            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Android Audio] Permission request failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> StartRecordingAsync()
    {
        if (_isRecording)
            return false;

        try
        {
            Debug.WriteLine("[Android Audio] Starting recording...");

            // Check permissions
            var hasPermission = await RequestPermissionsAsync();
            if (!hasPermission)
            {
                Debug.WriteLine("[Android Audio] Microphone permission not granted");
                return false;
            }

            // Create temporary file for recording
            var cacheDir = FileSystem.CacheDirectory;
            _audioFilePath = Path.Combine(cacheDir, $"recording_{DateTime.Now.Ticks}.3gp");

            // Initialize MediaRecorder
            _mediaRecorder = new MediaRecorder();
            _mediaRecorder.SetAudioSource(AudioSource.Mic);
            _mediaRecorder.SetOutputFormat(OutputFormat.ThreeGpp);
            _mediaRecorder.SetAudioEncoder(AudioEncoder.AmrNb);
            _mediaRecorder.SetOutputFile(_audioFilePath);

            // Prepare and start recording
            _mediaRecorder.Prepare();
            _mediaRecorder.Start();

            _isRecording = true;
            _recordingStartTime = DateTime.Now;

            Debug.WriteLine($"[Android Audio] Recording started to: {_audioFilePath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Android Audio] Failed to start recording: {ex.Message}");
            _isRecording = false;
            CleanupMediaRecorder();
            return false;
        }
    }

    public async Task<byte[]> StopRecordingAsync()
    {
        if (!_isRecording || _mediaRecorder == null || string.IsNullOrEmpty(_audioFilePath))
            return Array.Empty<byte>();

        try
        {
            Debug.WriteLine("[Android Audio] Stopping recording...");

            // Stop recording
            _mediaRecorder.Stop();
            _isRecording = false;

            // Read audio file
            byte[] audioData = Array.Empty<byte>();
            if (File.Exists(_audioFilePath))
            {
                audioData = await File.ReadAllBytesAsync(_audioFilePath);
                Debug.WriteLine($"[Android Audio] Recording stopped. Captured {audioData.Length} bytes");

                // Delete temporary file
                try
                {
                    File.Delete(_audioFilePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Android Audio] Failed to delete temp file: {ex.Message}");
                }
            }

            // Cleanup
            CleanupMediaRecorder();

            return audioData;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Android Audio] Failed to stop recording: {ex.Message}");
            CleanupMediaRecorder();
            return Array.Empty<byte>();
        }
    }

    private void CleanupMediaRecorder()
    {
        try
        {
            _mediaRecorder?.Release();
            _mediaRecorder?.Dispose();
            _mediaRecorder = null;
            _audioFilePath = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Android Audio] Cleanup error: {ex.Message}");
        }
    }
}
