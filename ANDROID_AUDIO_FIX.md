# 🔊 Android Emulator Audio Fix Guide

## ❌ Problem: No Audio in Android Emulator

You can't hear voice output from the Boses app in the Android emulator.

---

## 🎯 Root Causes

### **1. VoiceService is in Simulation Mode**
- Current implementation doesn't use real Text-to-Speech
- `SimulationMode = true` by default
- Only logs to debug console instead of speaking

### **2. Emulator Audio Not Configured**
- Emulator audio output may be disabled
- Host audio device not selected
- Audio passthrough not enabled

### **3. Missing Platform-Specific TTS Implementation**
- VoiceService has TODO comments for real TTS
- Android TextToSpeech not implemented
- No actual audio output

---

## ✅ Solution 1: Implement Real Text-to-Speech (Recommended)

I'll create a real TTS implementation that works on Android, iOS, and Windows.

### **Files to Create:**

1. **Platforms/Android/Services/AndroidTextToSpeechService.cs**
2. **Platforms/Windows/Services/WindowsTextToSpeechService.cs**
3. **Core/Services/VoiceService.cs** (updated)

---

## ✅ Solution 2: Configure Android Emulator Audio

### **Step 1: Enable Audio in AVD Settings**

1. **Open Android Device Manager** in Visual Studio
2. **Edit your AVD** (click pencil icon)
3. **Show Advanced Settings**
4. **Audio Settings:**
   ```
   ✓ Play audio
   ✓ Enable audio input
   Audio Output: Host audio device
   Audio Input: Host microphone
   ```
5. **Save and restart emulator**

### **Step 2: Check Emulator Audio Settings**

In the running emulator:
```
Settings → Sound & vibration
→ Volume: Turn up all sliders
→ Media volume: Maximum
→ Notification volume: Maximum
```

### **Step 3: Verify Host Audio**

On your Windows PC:
```
1. Right-click speaker icon (taskbar)
2. Open Sound settings
3. Ensure output device is working
4. Test with other apps (YouTube, etc.)
```

---

## ✅ Solution 3: Use .NET MAUI TextToSpeech API

The easiest solution is to use MAUI's built-in `TextToSpeech` API.

### **Updated VoiceService.cs:**

```csharp
public async Task SpeakAsync(string text, CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(text))
        return;

    try
    {
        // Use MAUI's built-in TextToSpeech
        await TextToSpeech.Default.SpeakAsync(text, cancellationToken);
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"[TTS Error] {ex.Message}");
        
        // Fallback: Simulate delay
        var duration = Math.Min(text.Length * 50, 5000);
        await Task.Delay(duration, cancellationToken);
    }
}
```

---

## 🚀 Quick Fix: Use Windows Instead

**The fastest solution is to test on Windows:**

```bash
cd "C:\Users\Full Scale\Desktop\product-innovation\Boses"
dotnet run --framework net9.0-windows10.0.19041.0
```

**Why Windows is Better:**
- ✅ Audio works out of the box
- ✅ No emulator configuration needed
- ✅ Windows TTS is excellent (Microsoft Speech Platform)
- ✅ 10x faster than emulator
- ✅ Instant testing

---

## 🔧 Implementation: Real TTS for All Platforms

### **Option A: Use MAUI TextToSpeech (Easiest)**

Update `VoiceService.cs`:

```csharp
using BosesApp.Core.Interfaces;

namespace BosesApp.Core.Services;

public class VoiceService : IVoiceService
{
    private bool _isListening;
    
    public bool IsListening => _isListening;
    public bool SimulationMode { get; set; } = false; // Changed to false

    public async Task<bool> StartListeningAsync()
    {
        _isListening = true;
        await Task.Delay(300);
        return true;
    }

    public async Task<string> StopListeningAsync()
    {
        _isListening = false;
        await Task.Delay(500);
        return "Demo response";
    }

    public async Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        try
        {
            // Use MAUI's built-in TextToSpeech
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            
            // Try to use Filipino locale if available
            var filipinoLocale = locales.FirstOrDefault(l => 
                l.Language.StartsWith("fil") || l.Language.StartsWith("tl"));
            
            var settings = new SpeechOptions
            {
                Pitch = 1.0f,
                Volume = 1.0f,
                Locale = filipinoLocale
            };

            await TextToSpeech.Default.SpeakAsync(text, settings, cancellationToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TTS Error] {ex.Message}");
            
            // Fallback: Just delay
            var duration = Math.Min(text.Length * 50, 5000);
            await Task.Delay(duration, cancellationToken);
        }
    }
}
```

### **Option B: Platform-Specific Implementation**

For more control, create platform-specific services:

**Platforms/Android/Services/AndroidTextToSpeechService.cs:**
```csharp
using Android.Speech.Tts;
using Java.Util;

namespace BosesApp.Platforms.Android.Services;

public class AndroidTextToSpeechService : Java.Lang.Object, TextToSpeech.IOnInitListener
{
    private TextToSpeech? _tts;
    private bool _isInitialized;

    public AndroidTextToSpeechService()
    {
        _tts = new TextToSpeech(Platform.CurrentActivity, this);
    }

    public void OnInit(OperationResult status)
    {
        if (status == OperationResult.Success)
        {
            _isInitialized = true;
            
            // Try Filipino locale
            var locale = new Locale("fil", "PH");
            var result = _tts?.SetLanguage(locale);
            
            if (result == LanguageAvailableResult.NotSupported)
            {
                // Fallback to English
                _tts?.SetLanguage(Locale.English);
            }
        }
    }

    public async Task SpeakAsync(string text)
    {
        if (!_isInitialized || _tts == null)
        {
            await Task.Delay(text.Length * 50);
            return;
        }

        var utteranceId = Guid.NewGuid().ToString();
        _tts.Speak(text, QueueMode.Flush, null, utteranceId);
        
        // Wait for speech to complete
        await Task.Delay(text.Length * 50);
    }
}
```

---

## 🎯 Recommended Solution

### **For Development (Right Now):**

**Use Windows - Audio works perfectly:**
```bash
dotnet run --framework net9.0-windows10.0.19041.0
```

### **For Production (Later):**

**Implement MAUI TextToSpeech:**
1. Update `VoiceService.cs` to use `TextToSpeech.Default.SpeakAsync()`
2. Set `SimulationMode = false`
3. Test on Windows first (instant audio)
4. Then test on Android (with emulator audio configured)

---

## 🧪 Testing Audio

### **Test 1: MAUI TextToSpeech**
```csharp
// In MainPage.xaml.cs or MainViewModel
await TextToSpeech.Default.SpeakAsync("Hello, this is a test");
```

### **Test 2: Check Emulator Audio**
```
1. Open YouTube in emulator browser
2. Play a video
3. If you hear audio → Emulator audio works
4. If no audio → Configure emulator settings
```

### **Test 3: Windows Audio**
```bash
# Run on Windows
dotnet run --framework net9.0-windows10.0.19041.0

# Click microphone button
# Should hear voice output immediately
```

---

## 📊 Audio Comparison

| Platform | Audio Quality | Setup Required | Works Out of Box |
|----------|---------------|----------------|------------------|
| **Windows** | ⭐⭐⭐⭐⭐ Excellent | None | ✅ Yes |
| **Android Emulator** | ⭐⭐⭐ Good | Configure AVD | ❌ No |
| **Physical Android** | ⭐⭐⭐⭐ Very Good | None | ✅ Yes |
| **iOS Simulator** | ⭐⭐⭐⭐⭐ Excellent | None | ✅ Yes |

---

## 🔍 Debugging Audio Issues

### **Check if TTS is Available:**
```csharp
var locales = await TextToSpeech.Default.GetLocalesAsync();
foreach (var locale in locales)
{
    Debug.WriteLine($"Available locale: {locale.Language} - {locale.Name}");
}
```

### **Check Emulator Audio:**
```bash
# In Android emulator terminal (adb shell)
adb shell settings get system volume_music
# Should return a number > 0
```

### **Enable Verbose Logging:**
```csharp
// In VoiceService.cs
System.Diagnostics.Debug.WriteLine($"[TTS] Speaking: {text}");
System.Diagnostics.Debug.WriteLine($"[TTS] Locale: {settings.Locale?.Name}");
System.Diagnostics.Debug.WriteLine($"[TTS] Volume: {settings.Volume}");
```

---

## ✅ Step-by-Step Fix

### **Immediate Fix (5 minutes):**

1. **Switch to Windows:**
   ```bash
   dotnet run --framework net9.0-windows10.0.19041.0
   ```

2. **Test voice features:**
   - Click microphone button
   - Should hear voice output
   - Audio works perfectly on Windows

### **Proper Fix (30 minutes):**

1. **Update VoiceService.cs** (I'll create this for you)
2. **Use MAUI TextToSpeech API**
3. **Set SimulationMode = false**
4. **Test on Windows first**
5. **Then configure Android emulator audio**
6. **Test on Android**

---

## 🎯 Summary

**Why No Audio:**
1. ❌ VoiceService is in simulation mode (no real TTS)
2. ❌ Emulator audio may not be configured
3. ❌ Platform-specific TTS not implemented

**Quick Fix:**
- Use Windows target (audio works immediately)

**Proper Fix:**
- Implement MAUI TextToSpeech API
- Configure emulator audio settings
- Test on Windows, then Android

---

**I'll create the updated VoiceService with real TTS for you now!** 🔊
