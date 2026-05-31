# Speech Recognition Not Triggering - Diagnostic Guide

## Quick Diagnosis

Check your **Visual Studio Debug Output** (View > Output) for these messages:

### 1. **Check If Real Recognition Is Available**
Look for one of these messages during app startup:

```
✅ Android 33+ supports offline speech recognition
```
OR
```
⚠️ Android 32 < 33 - using online/simulation fallback
```

**If you see the second message**: Your emulator/device is running Android API 32 or lower. You need **Android 33+** (Android 13 Tiramisu) for offline speech recognition.

---

### 2. **Check Initialization**
During app startup, you should see:

```
[SpeechRecognition] 🔗 Subscribing to speech recognition events...
[SpeechRecognition] 🔗 Event subscriptions complete
[SpeechRecognition] ✅ Initialized with .NET MAUI Community Toolkit
```

### 3. **Check Voice Registration ViewModel Subscribe**
When voice registration page opens, you should see:

```
[VoiceRegistration] 📢 Subscribed to OnRecognitionResultUpdated event
```

---

### 4. **Check StartListeningAsync**
When you press the record button, look for:

```
[SpeechRecognition] ▶️ StartListeningAsync called (language: en-US)
[SpeechRecognition] IsRealRecognitionAvailable: True
[SpeechRecognition] ✅ Microphone permission granted
[SpeechRecognition] ✅ Permissions granted
[SpeechRecognition] 📢 Calling _speechToText.StartListenAsync with culture: en-US
[SpeechRecognition] 📢 ShouldReportPartialResults: True
[SpeechRecognition] ✅ Listening started successfully
[SpeechRecognition] 🎤 Waiting for speech... (events should fire now)
```

**If you see:**
```
[SpeechRecognition] 🔄 Real recognition NOT available - using simulation mode
[SpeechRecognition] ℹ️ This usually means: API level < 33, permission denied, or platform not supported
```

→ **Android API is < 33 or permission denied**

---

### 5. **Check Event Firing**
While speaking, you should see:

```
[SpeechRecognition] 🔔 OnRecognitionResultUpdated EVENT FIRED!
[SpeechRecognition] 🎤 Partial (REAL): my voice
[SpeechRecognition] 📢 Raising public OnRecognitionResultUpdated event with: 'my voice'
```

**If you DON'T see these messages**:
- MAUI Community Toolkit is not firing events
- OR recognition never started properly

---

### 6. **Check ViewModel Handler**
You should see from the ViewModel:

```
[VoiceRegistration] 📢 OnRecognitionResultUpdated triggered!
[VoiceRegistration] 📝 Recognized text: 'my voice'
[VoiceRegistration] 📊 Confidence: 85%
[VoiceRegistration] ✅ IsFinal: False
```

---

## Troubleshooting Steps

### **If you see "Real recognition NOT available"**

**Solution 1: Update Android Emulator**
```
Device Manager > Actions > Update SDK
```
Make sure you have Android 33+ installed

**Solution 2: Create a new AVD with Android 33+**
```
Device Manager > Create Device
Select: Android 13 or higher (API 33+)
```

**Solution 3: Check Physical Device**
```
Settings > About Phone > Android version
Must be Android 13 (API 33) or higher
```

---

### **If real recognition is available but events not firing**

This could be a MAUI Community Toolkit issue. Try these:

**Step 1: Check Permissions**
- App manifest must include RECORD_AUDIO and INTERNET permissions
- Device must grant microphone permission

**Step 2: Check Event Subscription**
In the debug output, confirm:
```
[SpeechRecognition] 🔗 Subscribing to speech recognition events...
[SpeechRecognition] 🔗 Event subscriptions complete
```

**Step 3: Restart the App**
- Rebuild solution: `Build > Rebuild Solution`
- Uninstall app: `adb uninstall com.boses.accessibility`
- Redeploy fresh

---

### **If events fire but ViewModel doesn't receive them**

**Check DI Registration:**
In `MauiProgram.cs`, verify:
```csharp
builder.Services.AddSingleton<ISpeechRecognitionService, MauiSpeechRecognitionService>();
```

**Check ViewModel Subscription:**
In `VoiceRegistrationViewModel.InitializeAsync()`, confirm:
```csharp
_speechRecognitionService.OnRecognitionResultUpdated += OnSpeechRecognitionResultUpdated;
```

---

## What to Report

If speech recognition still isn't working, share these debug logs:

1. **Startup logs** (app launch to voice registration page opening)
2. **During recognition** (press record → speak → stop)
3. Any **error messages** or **exception stack traces**
4. **Android API level** (Settings > About Phone > Android version)

Example command to capture logs:
```powershell
adb logcat | Select-String "SpeechRecognition"
```

---

##Events Flow Diagram

```
VoiceRegistrationViewModel.InitializeAsync()
	↓
_speechRecognitionService.OnRecognitionResultUpdated += OnSpeechRecognitionResultUpdated
	↓
User clicks Record button
	↓
StartListeningAsync("en-US")
	↓
_speechToText.StartListenAsync()
	↓
User speaks: "my voice is my password"
	↓
MAUI Community Toolkit fires: OnMauiRecognitionResultUpdated
	↓
MauiSpeechRecognitionService raises: OnRecognitionResultUpdated
	↓
VoiceRegistrationViewModel.OnSpeechRecognitionResultUpdated() receives event
	↓
Updates UI with recognized text
```

---

## Key Messages to Look For

| Message | Status | Meaning |
|---------|--------|---------|
| `✅ Android 33+ supports` | ✅ Good | Device supports offline recognition |
| `⚠️ Android 32 < 33` | ❌ Issue | API level too old, need Android 13+ |
| `✅ Listening started successfully` | ✅ Good | Recognition started |
| `🔄 Real recognition NOT available` | ❌ Issue | Falling back to simulation |
| `🔔 OnRecognitionResultUpdated EVENT FIRED!` | ✅ Good | Recognition is working |
| `📢 OnRecognitionResultUpdated triggered!` | ✅ Good | ViewModel received event |

---

## Still Not Working?

Run this command to check emulator API level:
```powershell
adb shell getprop ro.build.version.release
```

Output should be **13 or higher**.

If it's 12 or lower, you need to create a new Android 13+ emulator device.
