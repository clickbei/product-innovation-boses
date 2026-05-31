using System.Diagnostics;

namespace BosesApp.Core.Services;

/// <summary>
/// Audio analysis service for detecting speech and audio quality
/// Analyzes audio data to determine if actual speech is present
/// Uses multiple Voice Activity Detection (VAD) techniques
/// </summary>
public interface IAudioAnalysisService
{
    /// <summary>
    /// Analyzes audio data to detect if speech is present
    /// Returns true if audio contains speech-like characteristics
    /// Uses RMS energy, Zero-Crossing Rate, and Spectral Entropy
    /// </summary>
    bool HasSpeech(byte[] audioData, double threshold = 0.02);

    /// <summary>
    /// Calculates the RMS (Root Mean Square) energy level of audio
    /// Higher values indicate louder audio
    /// </summary>
    double CalculateRmsEnergy(byte[] audioData);

    /// <summary>
    /// Detects if audio is mostly silence
    /// </summary>
    bool IsSilence(byte[] audioData, double silenceThreshold = 0.01);

    /// <summary>
    /// Calculates Zero-Crossing Rate (ZCR)
    /// Measures how often the signal crosses zero amplitude
    /// Speech has moderate ZCR, noise has high ZCR, silence has low ZCR
    /// </summary>
    double CalculateZeroCrossingRate(byte[] audioData);

    /// <summary>
    /// Calculates Spectral Entropy
    /// Measures randomness of frequency content
    /// Speech has lower entropy (structured), noise has higher entropy (random)
    /// </summary>
    double CalculateSpectralEntropy(byte[] audioData);

    /// <summary>
    /// Advanced speech detection using multiple features
    /// Combines RMS, ZCR, and Spectral Entropy for robust detection
    /// </summary>
    bool HasSpeechAdvanced(byte[] audioData);
}

public class AudioAnalysisService : IAudioAnalysisService
{
    public bool HasSpeech(byte[] audioData, double threshold = 0.02)
    {
        if (audioData == null || audioData.Length < 2)
        {
            Debug.WriteLine("[AudioAnalysis] No audio data to analyze");
            return false;
        }

        var rmsEnergy = CalculateRmsEnergy(audioData);
        var hasSpeech = rmsEnergy >= threshold;

        Debug.WriteLine($"[AudioAnalysis] RMS Energy: {rmsEnergy:F4} (threshold: {threshold:F4})");
        Debug.WriteLine($"[AudioAnalysis] Speech detected: {(hasSpeech ? "✅ YES" : "❌ NO")}");

        return hasSpeech;
    }

    public bool HasSpeechAdvanced(byte[] audioData)
    {
        if (audioData == null || audioData.Length < 2)
        {
            Debug.WriteLine("[AudioAnalysis] No audio data to analyze");
            return false;
        }

        // Calculate multiple features
        var rmsEnergy = CalculateRmsEnergy(audioData);
        var zcr = CalculateZeroCrossingRate(audioData);
        var spectralEntropy = CalculateSpectralEntropy(audioData);

        Debug.WriteLine($"[AudioAnalysis] === Advanced VAD Analysis ===");
        Debug.WriteLine($"[AudioAnalysis] RMS Energy: {rmsEnergy:F4}");
        Debug.WriteLine($"[AudioAnalysis] Zero-Crossing Rate: {zcr:F4}");
        Debug.WriteLine($"[AudioAnalysis] Spectral Entropy: {spectralEntropy:F4}");

        // Decision rules based on multiple features
        bool energyCheck = rmsEnergy >= 0.02;  // Sufficient energy
        bool zcrCheck = zcr >= 0.05 && zcr <= 0.30;  // Moderate ZCR (speech range)
        bool entropyCheck = spectralEntropy >= 0.3 && spectralEntropy <= 0.8;  // Structured (not random noise)

        Debug.WriteLine($"[AudioAnalysis] Energy Check: {(energyCheck ? "✅" : "❌")} (>= 0.02)");
        Debug.WriteLine($"[AudioAnalysis] ZCR Check: {(zcrCheck ? "✅" : "❌")} (0.05-0.30)");
        Debug.WriteLine($"[AudioAnalysis] Entropy Check: {(entropyCheck ? "✅" : "❌")} (0.3-0.8)");

        // Speech detected if at least 2 out of 3 checks pass
        int passedChecks = (energyCheck ? 1 : 0) + (zcrCheck ? 1 : 0) + (entropyCheck ? 1 : 0);
        bool hasSpeech = passedChecks >= 2;

        Debug.WriteLine($"[AudioAnalysis] Passed Checks: {passedChecks}/3");
        Debug.WriteLine($"[AudioAnalysis] Speech detected: {(hasSpeech ? "✅ YES" : "❌ NO")}");

        return hasSpeech;
    }

    public double CalculateRmsEnergy(byte[] audioData)
    {
        if (audioData == null || audioData.Length < 2)
            return 0.0;

        // Calculate RMS energy from 16-bit PCM audio
        long sumOfSquares = 0;
        int sampleCount = audioData.Length / 2;

        for (int i = 0; i < audioData.Length - 1; i += 2)
        {
            // Convert two bytes to 16-bit signed sample
            short sample = (short)(audioData[i] | (audioData[i + 1] << 8));
            sumOfSquares += (long)sample * sample;
        }

        // Calculate RMS
        double meanSquare = (double)sumOfSquares / sampleCount;
        double rms = Math.Sqrt(meanSquare);

        // Normalize to 0.0 - 1.0 range (16-bit audio max is 32768)
        double normalizedRms = rms / 32768.0;

        return normalizedRms;
    }

    public bool IsSilence(byte[] audioData, double silenceThreshold = 0.01)
    {
        var rmsEnergy = CalculateRmsEnergy(audioData);
        var isSilent = rmsEnergy < silenceThreshold;

        Debug.WriteLine($"[AudioAnalysis] RMS Energy: {rmsEnergy:F4} (silence threshold: {silenceThreshold:F4})");
        Debug.WriteLine($"[AudioAnalysis] Is silence: {(isSilent ? "✅ YES" : "❌ NO")}");

        return isSilent;
    }

    public double CalculateZeroCrossingRate(byte[] audioData)
    {
        if (audioData == null || audioData.Length < 4)
            return 0.0;

        int zeroCrossings = 0;
        int sampleCount = audioData.Length / 2;

        // Get first sample
        short prevSample = (short)(audioData[0] | (audioData[1] << 8));

        // Count zero crossings
        for (int i = 2; i < audioData.Length - 1; i += 2)
        {
            short currentSample = (short)(audioData[i] | (audioData[i + 1] << 8));

            // Check if sign changed (crossed zero)
            if ((prevSample >= 0 && currentSample < 0) || (prevSample < 0 && currentSample >= 0))
            {
                zeroCrossings++;
            }

            prevSample = currentSample;
        }

        // Normalize by number of samples
        double zcr = (double)zeroCrossings / sampleCount;

        return zcr;
    }

    public double CalculateSpectralEntropy(byte[] audioData)
    {
        if (audioData == null || audioData.Length < 2)
            return 0.0;

        // Calculate power spectrum using simplified approach
        // For full implementation, use FFT (Fast Fourier Transform)
        // Here we use a simplified frequency band energy approach

        int sampleCount = audioData.Length / 2;
        int numBands = 8; // Divide spectrum into 8 frequency bands
        int samplesPerBand = sampleCount / numBands;

        double[] bandEnergies = new double[numBands];
        double totalEnergy = 0.0;

        // Calculate energy in each frequency band
        for (int band = 0; band < numBands; band++)
        {
            double bandEnergy = 0.0;
            int startIdx = band * samplesPerBand * 2;
            int endIdx = Math.Min((band + 1) * samplesPerBand * 2, audioData.Length);

            for (int i = startIdx; i < endIdx - 1; i += 2)
            {
                short sample = (short)(audioData[i] | (audioData[i + 1] << 8));
                bandEnergy += (double)sample * sample;
            }

            bandEnergies[band] = bandEnergy;
            totalEnergy += bandEnergy;
        }

        // Avoid division by zero
        if (totalEnergy < 1e-10)
            return 0.0;

        // Calculate entropy
        double entropy = 0.0;
        for (int band = 0; band < numBands; band++)
        {
            double probability = bandEnergies[band] / totalEnergy;
            if (probability > 1e-10)
            {
                entropy -= probability * Math.Log(probability, 2);
            }
        }

        // Normalize entropy to 0-1 range (max entropy for 8 bands is log2(8) = 3)
        double normalizedEntropy = entropy / Math.Log(numBands, 2);

        return normalizedEntropy;
    }
}
