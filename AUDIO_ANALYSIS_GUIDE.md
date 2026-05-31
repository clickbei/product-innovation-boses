# Audio Analysis Guide

## Problem: Audio Recording Returns Bytes Even Without Speech

The microphone **always captures audio data**, even when you're not speaking. This includes:
- Silence (very low amplitude)
- Background noise (fans, AC, room tone)
- Ambient sounds

So checking `audioData.Length > 0` is **not enough** to verify someone actually spoke.

## Solution: Audio Level Analysis

We've added `AudioAnalysisService` that analyzes the **energy level** of the audio to detect if speech is present.

### How It Works

1. **RMS Energy Calculation**: Calculates the Root Mean Square (RMS) energy of the audio signal
   - RMS measures the "loudness" or "power" of the audio
   - Higher RMS = louder audio = likely speech
   - Lower RMS = quieter audio = likely silence

2. **Speech Detection**: Compares RMS energy against a threshold
   - Default threshold: `0.02` (2% of maximum volume)
   - If RMS >= threshold → Speech detected ✅
   - If RMS < threshold → Silence/too quiet ❌

### Implementation

```csharp
// In VoiceRegistrationViewModel.cs (line ~218)
var hasSpeech = _audioAnalysisService.HasSpeech(audioData, threshold: 0.02);
if (!hasSpeech)
{
    StatusMessage = "❌ No speech detected. Please speak louder.";
    // ... reject the recording
}
```

### Testing the Audio Analysis

#### Test 1: Silent Recording
1. Start voice registration
2. Tap record button
3. **Don't speak** - stay completely silent
4. Wait 5 seconds
5. **Expected**: "No speech detected. Please speak louder."

#### Test 2: Very Quiet Speech
1. Start voice registration
2. Tap record button
3. Whisper very quietly
4. Wait 5 seconds
5. **Expected**: May be rejected if too quiet (depends on microphone sensitivity)

#### Test 3: Normal Speech
1. Start voice registration
2. Tap record button
3. Speak the phrase clearly at normal volume
4. Wait 5 seconds
5. **Expected**: Speech detected, proceeds to validation ✅

### Debug Output

Check the Debug console for audio analysis logs:

```
[AudioAnalysis] RMS Energy: 0.0012 (threshold: 0.0200)
[AudioAnalysis] Speech detected: ❌ NO

[AudioAnalysis] RMS Energy: 0.0456 (threshold: 0.0200)
[AudioAnalysis] Speech detected: ✅ YES
```

### Adjusting the Threshold

If the detection is too sensitive or not sensitive enough, adjust the threshold:

```csharp
// More sensitive (detects quieter speech)
var hasSpeech = _audioAnalysisService.HasSpeech(audioData, threshold: 0.01);

// Less sensitive (requires louder speech)
var hasSpeech = _audioAnalysisService.HasSpeech(audioData, threshold: 0.05);
```

**Current setting**: `0.02` (good balance for most environments)

### Technical Details

**Audio Format**: 16-bit PCM, 16kHz, Mono
- Each sample is 2 bytes (16-bit signed integer)
- Range: -32768 to +32767
- Normalized RMS range: 0.0 to 1.0

**RMS Calculation**:
```
RMS = sqrt(sum(sample²) / sampleCount) / 32768
```

**Typical RMS Values**:
- Silence: 0.001 - 0.005
- Background noise: 0.005 - 0.015
- Quiet speech: 0.015 - 0.030
- Normal speech: 0.030 - 0.100
- Loud speech: 0.100 - 0.300

### Files Modified

1. **Created**: `Core/Services/AudioAnalysisService.cs`
   - New service for audio analysis
   - `HasSpeech()` - Detects if audio contains speech
   - `CalculateRmsEnergy()` - Calculates audio energy level
   - `IsSilence()` - Detects if audio is mostly silent

2. **Updated**: `MauiProgram.cs` (line 48)
   - Registered `IAudioAnalysisService` in DI container

3. **Updated**: `Presentation/ViewModels/VoiceRegistrationViewModel.cs`
   - Added `_audioAnalysisService` dependency (line 15)
   - Added speech detection check (line ~218)
   - Rejects recordings with no speech

### Why This Matters

Without audio analysis:
- ❌ User could register with silence
- ❌ Voice authentication would fail later
- ❌ Poor user experience

With audio analysis:
- ✅ Ensures user actually spoke
- ✅ Improves voice registration quality
- ✅ Better error messages
- ✅ Prevents invalid voice samples

### Alternative: Voice Activity Detection (VAD)

For even more advanced detection, you could implement:
- **Zero-Crossing Rate (ZCR)**: Detects speech vs noise
- **Spectral Entropy**: Measures randomness of frequency content
- **Vosk VAD**: Vosk library includes built-in Voice Activity Detection

But RMS energy analysis is simple, fast, and works well for most cases!
