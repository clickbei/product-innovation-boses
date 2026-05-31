using AudioToolbox;
using AVFoundation;
using Foundation;
using System.Diagnostics;

namespace BosesApp.Platforms.iOS.Services;

/// <summary>
/// iOS-specific audio recording implementation using AVAudioRecorder
/// </summary>
public class iOSAudioRecordingService
{
    private AVAudioRecorder? _audioRecorder;
    private NSUrl? _audioFileUrl;
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
            Debug.WriteLine($"[iOS Audio] Permission request failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> StartRecordingAsync()
    {
        if (_isRecording)
            return false;

        try
        {
            Debug.WriteLine("[iOS Audio] Starting recording...");

            // Check permissions
            var hasPermission = await RequestPermissionsAsync();
            if (!hasPermission)
            {
                Debug.WriteLine("[iOS Audio] Microphone permission not granted");
                return false;
            }

            // Configure audio session
            var audioSession = AVAudioSession.SharedInstance();
            var error = audioSession.SetCategory(AVAudioSessionCategory.Record);
            if (error != null)
            {
                Debug.WriteLine($"[iOS Audio] Failed to set audio session category: {error}");
                return false;
            }

            error = audioSession.SetActive(true);
            if (error != null)
            {
                Debug.WriteLine($"[iOS Audio] Failed to activate audio session: {error}");
                return false;
            }

            // Create temporary file for recording
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var tempPath = Path.Combine(documentsPath, "..", "tmp");
            Directory.CreateDirectory(tempPath);
            var audioFilePath = Path.Combine(tempPath, $"recording_{DateTime.Now.Ticks}.wav");
            _audioFileUrl = NSUrl.FromFilename(audioFilePath);

            // Configure audio settings
            var audioSettings = new AudioSettings
            {
                Format = AudioFormatType.LinearPCM,
                SampleRate = 16000,
                NumberChannels = 1,
                LinearPcmBitDepth = 16,
                LinearPcmBigEndian = false,
                LinearPcmFloat = false
            };

            // Create and start recorder
            _audioRecorder = AVAudioRecorder.Create(_audioFileUrl, audioSettings, out error);
            if (error != null || _audioRecorder == null)
            {
                Debug.WriteLine($"[iOS Audio] Failed to create recorder: {error}");
                return false;
            }

            _audioRecorder.PrepareToRecord();
            var started = _audioRecorder.Record();

            if (!started)
            {
                Debug.WriteLine("[iOS Audio] Failed to start recording");
                return false;
            }

            _isRecording = true;
            _recordingStartTime = DateTime.Now;

            Debug.WriteLine($"[iOS Audio] Recording started to: {audioFilePath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[iOS Audio] Failed to start recording: {ex.Message}");
            _isRecording = false;
            CleanupAudioRecorder();
            return false;
        }
    }

    public async Task<byte[]> StopRecordingAsync()
    {
        if (!_isRecording || _audioRecorder == null || _audioFileUrl == null)
            return Array.Empty<byte>();

        try
        {
            Debug.WriteLine("[iOS Audio] Stopping recording...");

            // Stop recording
            _audioRecorder.Stop();
            _isRecording = false;

            // Deactivate audio session
            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetActive(false);

            // Read audio file
            byte[] audioData = Array.Empty<byte>();
            var audioFilePath = _audioFileUrl.Path;
            if (File.Exists(audioFilePath))
            {
                audioData = await File.ReadAllBytesAsync(audioFilePath);
                Debug.WriteLine($"[iOS Audio] Recording stopped. Captured {audioData.Length} bytes");

                // Delete temporary file
                try
                {
                    File.Delete(audioFilePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[iOS Audio] Failed to delete temp file: {ex.Message}");
                }
            }

            // Cleanup
            CleanupAudioRecorder();

            return audioData;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[iOS Audio] Failed to stop recording: {ex.Message}");
            CleanupAudioRecorder();
            return Array.Empty<byte>();
        }
    }

    private void CleanupAudioRecorder()
    {
        try
        {
            _audioRecorder?.Dispose();
            _audioRecorder = null;
            _audioFileUrl = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[iOS Audio] Cleanup error: {ex.Message}");
        }
    }
}
