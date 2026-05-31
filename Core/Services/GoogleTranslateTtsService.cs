using BosesApp.Core.Interfaces;
using System.Diagnostics;

namespace BosesApp.Core.Services;

/// <summary>
/// Free Filipino TTS using the Google Translate speech endpoint.
///
/// ? Zero setup   — no API key, no downloads, no account
/// ? Free         — Google Translate's public TTS endpoint
/// ? Filipino     — language code "tl" (Tagalog)
/// ? English      — language code "en"
/// ? Works immediately — just needs an internet connection
///
/// Fallback: if no internet or the request fails, the app falls through
/// to OsTtsService automatically (never crashes).
///
/// Note: This uses Google Translate's unofficial endpoint (same one the
/// website uses). For production, use Google Cloud TTS API with a key.
/// </summary>
public class GoogleTranslateTtsService : ITextToSpeechService
{
    private const string LogTag = "[GoogleTTS]";
    private const string BaseUrl = "https://translate.google.com/translate_tts";

    private readonly HttpClient _http;

    public string ProviderName => "Google Translate TTS (free, no setup)";
    public bool IsAvailable => true;   // always try; failure handled gracefully

    public GoogleTranslateTtsService(HttpClient http)
    {
        _http = http;
        // Google Translate TTS requires a browser-like User-Agent
        _http.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
            "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _http.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task SpeakAsync(string text, string language = "fil-PH",
                                  CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        // Map IETF tag ? Google Translate language code
        var lang = MapLanguage(language);

        // Google TTS has a ~200 character limit per request — chunk if needed
        var chunks = ChunkText(text, 200);

        foreach (var chunk in chunks)
        {
            if (cancellationToken.IsCancellationRequested) break;
            await SpeakChunkAsync(chunk, lang, cancellationToken);
        }
    }

    private async Task SpeakChunkAsync(string text, string lang, CancellationToken ct)
    {
        try
        {
            var encoded = Uri.EscapeDataString(text);
            var url = $"{BaseUrl}?ie=UTF-8&client=tw-ob&tl={lang}&q={encoded}";

            Debug.WriteLine($"{LogTag} [{lang}] {text[..Math.Min(50, text.Length)]}…");

            var mp3Bytes = await _http.GetByteArrayAsync(url, ct);

            if (mp3Bytes is { Length: > 0 })
                await PlayMp3Async(mp3Bytes, ct);   // awaits until playback finishes
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            Debug.WriteLine($"{LogTag} ? Request failed: {ex.Message}");
            throw;   // re-throw so HybridTtsService falls back to OS TTS
        }
    }

    // ?? Playback ???????????????????????????????????????????????????????????????

    private static async Task PlayMp3Async(byte[] mp3Bytes, CancellationToken ct)
    {
        try
        {
#if WINDOWS || ANDROID || IOS || MACCATALYST
            var tmp = Path.Combine(Path.GetTempPath(), $"boses_gtts_{Guid.NewGuid():N}.mp3");
            await File.WriteAllBytesAsync(tmp, mp3Bytes, ct);

            try
            {
                // Keep the stream open for the lifetime of the player
                var stream = File.OpenRead(tmp);
                var player = Plugin.Maui.Audio.AudioManager.Current.CreatePlayer(stream);

                // Wire up completion BEFORE Play() so we never miss the event
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                player.PlaybackEnded += (_, _) => tcs.TrySetResult(true);

                player.Play();

                // Safety ceiling: reported duration + 2 s, or byte-rate estimate + 2 s
                var ceilingMs = player.Duration > 0
                    ? (int)(player.Duration * 1000) + 2000
                    : EstimateDurationMs(mp3Bytes.Length) + 2000;

                Debug.WriteLine($"[GoogleTTS] Playing — ceiling={ceilingMs} ms, "
                    + $"duration={player.Duration:F2}s, bytes={mp3Bytes.Length}");

                // Await natural playback end, but never past the ceiling or cancellation
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(ceilingMs);
                try
                {
                    await tcs.Task.WaitAsync(timeoutCts.Token);
                    Debug.WriteLine("[GoogleTTS] Playback ended naturally via PlaybackEnded event");
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine("[GoogleTTS] Playback ceiling/cancellation hit — stopping player");
                }

                player.Stop();
                player.Dispose();
                stream.Dispose();
            }
            finally
            {
                try { File.Delete(tmp); } catch { /* ignore */ }
            }
#else
            await Task.Delay(EstimateDurationMs(mp3Bytes.Length), ct);
#endif
        }
        catch (OperationCanceledException) { /* expected on cancel */ }
        catch (Exception ex) { Debug.WriteLine($"[GoogleTTS] Playback error: {ex.Message}"); }
    }

    /// <summary>
    /// Fallback duration estimate when player.Duration is unavailable.
    /// Google Translate TTS outputs ~24 kbps MP3 (3 bytes per ms).
    /// </summary>
    private static int EstimateDurationMs(int byteLength) =>
        Math.Max((int)(byteLength / 3.0), 800);

    // ?? Helpers ????????????????????????????????????????????????????????????????

    /// <summary>Maps IETF language tags to Google Translate language codes.</summary>
    private static string MapLanguage(string lang) => lang.ToLowerInvariant() switch
    {
        "fil-ph" or "fil" or "tl" or "tl-ph" => "tl",   // Tagalog/Filipino
        "en-us" or "en" => "en",
        _ => lang.Split('-')[0]                           // best-effort fallback
    };

    /// <summary>Splits text at sentence boundaries to stay under the 200-char limit.</summary>
    private static List<string> ChunkText(string text, int maxLen)
    {
        if (text.Length <= maxLen) return [text];

        var chunks = new List<string>();
        var remaining = text.AsSpan();

        while (remaining.Length > 0)
        {
            if (remaining.Length <= maxLen)
            {
                chunks.Add(remaining.ToString());
                break;
            }

            // Find last sentence-break within limit
            var slice = remaining[..maxLen];
            var cut = slice.LastIndexOfAny(['.', '!', '?', ',', ';']);

            if (cut <= 0) cut = maxLen - 1;

            chunks.Add(remaining[..(cut + 1)].ToString().Trim());
            remaining = remaining[(cut + 1)..].TrimStart();
        }

        return chunks;
    }
}

///
/// ? MIT license — truly free, no API key, no account
/// ? Offline — works without internet
/// ? Neural quality — sounds natural, not robotic
/// ? Filipino voice: fil_PH-ugnayan-medium (~57 MB model)
/// ? Windows x64 (primary demo platform)
///
/// ?? ONE-TIME SETUP (~5 minutes) ??????????????????????????????????????????????
///
/// Step 1 — Download Piper for Windows:
///   https://github.com/rhasspy/piper/releases/latest
///   ? piper_windows_amd64.zip  ?  extract piper.exe
///
/// Step 2 — Download the Filipino voice model (PowerShell):
///   $base = "https://huggingface.co/rhasspy/piper-voices/resolve/main/fil/fil_PH/ugnayan/medium"
///   New-Item -ItemType Directory -Force Resources\Raw\piper
///   Invoke-WebRequest "$base/fil_PH-ugnayan-medium.onnx"      -OutFile Resources\Raw\piper\fil_PH-ugnayan-medium.onnx
///   Invoke-WebRequest "$base/fil_PH-ugnayan-medium.onnx.json" -OutFile Resources\Raw\piper\fil_PH-ugnayan-medium.onnx.json
///
/// Step 3 — Place piper.exe in the same folder:
///   Resources\Raw\piper\piper.exe
///   Resources\Raw\piper\fil_PH-ugnayan-medium.onnx
///   Resources\Raw\piper\fil_PH-ugnayan-medium.onnx.json
///
/// The existing &lt;MauiAsset Include="Resources\Raw\**" /&gt; in BosesApp.csproj
/// bundles these automatically — no csproj changes needed.
///
/// ?? RUNTIME ??????????????????????????????????????????????????????????????????
/// On first use, assets are extracted to %LOCALAPPDATA%\Boses\piper\.
/// Each SpeakAsync call pipes text to piper.exe via stdin, reads WAV from stdout,
/// and plays it through Plugin.Maui.Audio.
///
/// Piper GitHub : https://github.com/rhasspy/piper
/// Voice models  : https://huggingface.co/rhasspy/piper-voices
/// </summary>
public class PiperTtsService : ITextToSpeechService
{
    private const string LogTag = "[PiperTTS]";
    private const string ExeName = "piper.exe";
    private const string ModelName = "fil_PH-ugnayan-medium.onnx";
    private const string ModelJson = "fil_PH-ugnayan-medium.onnx.json";
    private const string AssetDir = "piper";   // Resources/Raw/piper/

    private string? _piperExePath;
    private string? _modelPath;
    private bool _initialized;
    private bool _available;

    public string ProviderName => "Piper TTS (offline, MIT, fil_PH-ugnayan-medium)";
    public bool IsAvailable => _available;

    // ?? Lazy initialization ????????????????????????????????????????????????????

    /// <summary>
    /// Extracts piper.exe and the voice model from MAUI raw assets to the
    /// local app-data cache directory on the first call. Idempotent.
    /// </summary>
    public async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
#if WINDOWS
            var cacheDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Boses", "piper");
            Directory.CreateDirectory(cacheDir);

            _piperExePath = Path.Combine(cacheDir, ExeName);
            _modelPath = Path.Combine(cacheDir, ModelName);
            var jsonPath = Path.Combine(cacheDir, ModelJson);

            await ExtractAssetAsync($"{AssetDir}/{ExeName}", _piperExePath);
            await ExtractAssetAsync($"{AssetDir}/{ModelName}", _modelPath);
            await ExtractAssetAsync($"{AssetDir}/{ModelJson}", jsonPath);

            _available = File.Exists(_piperExePath) && File.Exists(_modelPath);

            Debug.WriteLine(_available
                ? $"{LogTag} ? Ready — {_piperExePath}"
                : $"{LogTag} ??  piper.exe or .onnx model missing — see setup instructions in PiperTtsService.cs");
#else
            // Piper ships Windows-only binaries; other platforms fall back to OS TTS
            Debug.WriteLine($"{LogTag} ??  Windows only in this build — falling back to OS TTS");
            _available = false;
#endif
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{LogTag} ? Initialization failed: {ex.Message}");
            _available = false;
        }
    }

    // ?? ITextToSpeechService ???????????????????????????????????????????????????

    public async Task SpeakAsync(string text, string language = "fil-PH",
                                  CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        await EnsureInitializedAsync();

        if (!_available)
        {
            Debug.WriteLine($"{LogTag} Not available — skipping");
            return;
        }

        Debug.WriteLine($"{LogTag} Speaking: {text[..Math.Min(70, text.Length)]}…");

        try
        {
            var wavBytes = await SynthesizeAsync(text, cancellationToken);
            if (wavBytes is { Length: > 44 })
                await PlayWavAsync(wavBytes, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine($"{LogTag} Cancelled");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{LogTag} ? SpeakAsync error: {ex.Message}");
        }
    }

    // ?? Synthesis ??????????????????????????????????????????????????????????????

    /// <summary>
    /// Pipes <paramref name="text"/> to piper.exe stdin and returns WAV bytes
    /// from stdout.  Uses <c>--output_file -</c> to get a standard WAV stream.
    /// </summary>
    private async Task<byte[]?> SynthesizeAsync(string text, CancellationToken ct)
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = _piperExePath,
            Arguments = $"--model \"{_modelPath}\" --output_file -",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var proc = System.Diagnostics.Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start piper.exe");

        // Feed text to stdin, then close to signal EOF
        await proc.StandardInput.WriteAsync(text.AsMemory(), ct);
        proc.StandardInput.Close();

        // Read WAV from stdout
        using var ms = new MemoryStream();
        var copyTask = proc.StandardOutput.BaseStream.CopyToAsync(ms, ct);
        var exitTask = proc.WaitForExitAsync(ct);

        await Task.WhenAll(copyTask, exitTask);

        if (proc.ExitCode != 0)
        {
            var err = await proc.StandardError.ReadToEndAsync(ct);
            Debug.WriteLine($"{LogTag} piper exited {proc.ExitCode}: {err}");
            return null;
        }

        var wav = ms.ToArray();
        Debug.WriteLine($"{LogTag} ? Synthesized {wav.Length:N0} WAV bytes");
        return wav;
    }

    // ?? Playback ???????????????????????????????????????????????????????????????

    private static async Task PlayWavAsync(byte[] wavBytes, CancellationToken ct)
    {
        try
        {
#if WINDOWS || ANDROID || IOS || MACCATALYST
            var tmp = Path.Combine(Path.GetTempPath(), $"boses_piper_{Guid.NewGuid():N}.wav");
            await File.WriteAllBytesAsync(tmp, wavBytes, ct);

            try
            {
                using var stream = File.OpenRead(tmp);
                var player = Plugin.Maui.Audio.AudioManager.Current.CreatePlayer(stream);
                player.Play();

                // Estimate duration from WAV header fields
                // Byte 24: sample rate (int32), 22: channels (int16), 34: bits/sample (int16)
                var sampleRate = wavBytes.Length > 27 ? BitConverter.ToInt32(wavBytes, 24) : 22050;
                var channels = wavBytes.Length > 23 ? BitConverter.ToInt16(wavBytes, 22) : 1;
                var bitsPerSample = wavBytes.Length > 35 ? BitConverter.ToInt16(wavBytes, 34) : 16;
                var dataSize = Math.Max(wavBytes.Length - 44, 0);
                var bytesPerSec = sampleRate * channels * (bitsPerSample / 8);
                var durationMs = bytesPerSec > 0
                    ? (int)((double)dataSize / bytesPerSec * 1000) + 400
                    : 3000;

                await Task.Delay(durationMs, ct);
                player.Dispose();
            }
            finally
            {
                try { File.Delete(tmp); } catch { /* ignore */ }
            }
#else
            await Task.Delay(2500, ct);
#endif
        }
        catch (OperationCanceledException) { /* expected */ }
        catch (Exception ex) { Debug.WriteLine($"{LogTag} Playback error: {ex.Message}"); }
    }

    // ?? Asset extraction ???????????????????????????????????????????????????????

    private static async Task ExtractAssetAsync(string assetPath, string destPath)
    {
        if (File.Exists(destPath))
        {
            Debug.WriteLine($"{LogTag} ? Already extracted: {Path.GetFileName(destPath)}");
            return;
        }

        try
        {
            using var src = await FileSystem.OpenAppPackageFileAsync(assetPath);
            using var dest = File.Create(destPath);
            await src.CopyToAsync(dest);
            Debug.WriteLine($"{LogTag} ?? Extracted {Path.GetFileName(destPath)} ({dest.Length:N0} bytes)");
        }
        catch (Exception ex)
        {
            // Asset not present — user hasn't placed the files yet, which is fine
            Debug.WriteLine($"{LogTag} ??  Could not extract '{assetPath}': {ex.Message}");
        }
    }
}
