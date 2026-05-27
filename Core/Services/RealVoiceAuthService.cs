using BosesApp.Core.Interfaces;
using System.Security.Cryptography;
using System.Text.Json;

namespace BosesApp.Core.Services;

/// <summary>
/// Real voice biometric authentication service with actual audio processing
/// Uses MFCC-like feature extraction for voice fingerprinting
/// </summary>
public class RealVoiceAuthService : IVoiceAuthService
{
    private readonly IAudioRecordingService _audioRecordingService;
    private bool _simulatedAuthResult = true;

    public bool SimulationMode { get; set; } = false;

    public RealVoiceAuthService(IAudioRecordingService audioRecordingService)
    {
        _audioRecordingService = audioRecordingService;
    }

    public async Task<string> RegisterVoicePrintAsync(int userId, byte[] audioSamples)
    {
        if (SimulationMode)
        {
            // Simulate processing time
            await Task.Delay(2000);
            var mockVector = GenerateMockVoiceVector(audioSamples);
            return JsonSerializer.Serialize(mockVector);
        }

        // Real voice print extraction
        await Task.Delay(1500); // Simulate processing time

        if (audioSamples == null || audioSamples.Length == 0)
        {
            throw new InvalidOperationException("No audio data provided for voice registration");
        }

        // Extract voice features from audio
        var voiceFeatures = ExtractVoiceFeatures(audioSamples);

        // Serialize and return
        return JsonSerializer.Serialize(voiceFeatures);
    }

    public async Task<bool> VerifyVoiceAsync(int userId, byte[] audioSample, string storedVoicePrint)
    {
        if (SimulationMode)
        {
            await Task.Delay(1500);
            return _simulatedAuthResult;
        }

        // Real voice verification
        await Task.Delay(1000);

        if (audioSample == null || audioSample.Length == 0)
            return false;

        if (string.IsNullOrEmpty(storedVoicePrint))
            return false;

        try
        {
            // Extract features from new sample
            var newFeatures = ExtractVoiceFeatures(audioSample);

            // Deserialize stored features
            var storedFeatures = JsonSerializer.Deserialize<float[]>(storedVoicePrint);

            if (storedFeatures == null || storedFeatures.Length != newFeatures.Length)
                return false;

            // Calculate similarity
            var similarity = CalculateCosineSimilarity(newFeatures, storedFeatures);

            // Threshold: 85% similarity required
            return similarity >= 0.85;
        }
        catch
        {
            return false;
        }
    }

    public async Task<double> CalculateSimilarityAsync(byte[] sample1, byte[] sample2)
    {
        if (SimulationMode)
        {
            await Task.Delay(800);
            return _simulatedAuthResult ? 0.92 : 0.45;
        }

        if (sample1 == null || sample2 == null || sample1.Length == 0 || sample2.Length == 0)
            return 0.0;

        var features1 = ExtractVoiceFeatures(sample1);
        var features2 = ExtractVoiceFeatures(sample2);

        return CalculateCosineSimilarity(features1, features2);
    }

    public void SetSimulatedAuthResult(bool shouldPass)
    {
        _simulatedAuthResult = shouldPass;
    }

    /// <summary>
    /// Extract voice features from audio data
    /// Simplified MFCC-like feature extraction
    /// </summary>
    private float[] ExtractVoiceFeatures(byte[] audioData)
    {
        const int featureCount = 128;
        var features = new float[featureCount];

        if (audioData.Length == 0)
            return features;

        // Simple feature extraction based on audio characteristics
        // In production, use proper MFCC, i-vectors, or x-vectors

        // 1. Energy-based features (first 32 features)
        for (int i = 0; i < 32; i++)
        {
            int startIdx = (i * audioData.Length) / 32;
            int endIdx = ((i + 1) * audioData.Length) / 32;

            double energy = 0;
            for (int j = startIdx; j < endIdx && j < audioData.Length; j++)
            {
                energy += Math.Abs(audioData[j] - 128);
            }
            features[i] = (float)(energy / (endIdx - startIdx));
        }

        // 2. Spectral features (next 32 features)
        for (int i = 0; i < 32; i++)
        {
            int startIdx = (i * audioData.Length) / 32;
            int endIdx = ((i + 1) * audioData.Length) / 32;

            double spectral = 0;
            for (int j = startIdx; j < endIdx - 1 && j < audioData.Length - 1; j++)
            {
                spectral += Math.Abs(audioData[j + 1] - audioData[j]);
            }
            features[32 + i] = (float)(spectral / (endIdx - startIdx));
        }

        // 3. Zero-crossing rate features (next 32 features)
        for (int i = 0; i < 32; i++)
        {
            int startIdx = (i * audioData.Length) / 32;
            int endIdx = ((i + 1) * audioData.Length) / 32;

            int zeroCrossings = 0;
            for (int j = startIdx; j < endIdx - 1 && j < audioData.Length - 1; j++)
            {
                if ((audioData[j] >= 128 && audioData[j + 1] < 128) ||
                    (audioData[j] < 128 && audioData[j + 1] >= 128))
                {
                    zeroCrossings++;
                }
            }
            features[64 + i] = (float)zeroCrossings / (endIdx - startIdx);
        }

        // 4. Statistical features (last 32 features)
        for (int i = 0; i < 32; i++)
        {
            int startIdx = (i * audioData.Length) / 32;
            int endIdx = ((i + 1) * audioData.Length) / 32;

            double mean = 0;
            for (int j = startIdx; j < endIdx && j < audioData.Length; j++)
            {
                mean += audioData[j];
            }
            mean /= (endIdx - startIdx);

            double variance = 0;
            for (int j = startIdx; j < endIdx && j < audioData.Length; j++)
            {
                variance += Math.Pow(audioData[j] - mean, 2);
            }
            variance /= (endIdx - startIdx);

            features[96 + i] = (float)Math.Sqrt(variance);
        }

        // Normalize features
        NormalizeFeatures(features);

        return features;
    }

    /// <summary>
    /// Normalize feature vector to unit length
    /// </summary>
    private void NormalizeFeatures(float[] features)
    {
        double magnitude = 0;
        foreach (var feature in features)
        {
            magnitude += feature * feature;
        }
        magnitude = Math.Sqrt(magnitude);

        if (magnitude > 0)
        {
            for (int i = 0; i < features.Length; i++)
            {
                features[i] /= (float)magnitude;
            }
        }
    }

    /// <summary>
    /// Calculate cosine similarity between two feature vectors
    /// </summary>
    private double CalculateCosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length)
            return 0.0;

        double dotProduct = 0;
        double magnitude1 = 0;
        double magnitude2 = 0;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        magnitude1 = Math.Sqrt(magnitude1);
        magnitude2 = Math.Sqrt(magnitude2);

        if (magnitude1 == 0 || magnitude2 == 0)
            return 0.0;

        return dotProduct / (magnitude1 * magnitude2);
    }

    /// <summary>
    /// Generate mock voice vector for simulation mode
    /// </summary>
    private float[] GenerateMockVoiceVector(byte[] audioSamples)
    {
        var hash = SHA256.HashData(audioSamples);
        var vector = new float[128];

        for (int i = 0; i < 128; i++)
        {
            var seed = BitConverter.ToInt32(hash, (i * 4) % hash.Length);
            var rng = new Random(seed);
            vector[i] = (float)(rng.NextDouble() * 2.0 - 1.0);
        }

        return vector;
    }
}
