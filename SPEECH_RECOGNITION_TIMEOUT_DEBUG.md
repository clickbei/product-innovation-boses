# 🔍 Speech Recognition Timeout - Debugging Guide

## The Problem

The `StopListeningAsync()` method is always timing out and falling into the `else` block:

```csharp
if (completedTask == _recognitionCompletionSource.Task)
{
    // ✅ This should execute when recognition completes
    var result = await _recognitionCompletionSource.Task;
    return result;
}
else
{
    // ❌ This is always executing (TIMEOUT)
    Debug.WriteLine("[SpeechRecognition] ⚠️ Recognition timeout");
}
```

## Why This Happens

The timeout occurs because the `OnRecognitionResultCompleted` event is **not firing** or **not setting the result** properly. This can happen for several reasons:

### Possible Causes:

1. **Event Not Subscribed Properly**
   - The event handler might not be registered correctly
   - Check if `_speechToText.RecognitionResultCompleted += OnRecognitionResultCompleted;` is called

2. **Event Fires Before StopListenAsync**
   - The event might fire immediately after `StartListenAsync` (before you call `StopListenAsync`)
   - The `TaskCompletionSource` might already be completed

3. **Event Never Fires**
   - The speech recognition service might not be calling the event
   - Platform-specific issue (Android API level, permissions, etc.)

4. **Result is Null or Unsuccessful**
   - `e.RecognitionResult` might be null
   - `e.RecognitionResult.IsSuccessful` might be false
   - The event fires but doesn't set the result

5. **Wrong Event Type**
   - The event args might have a different structure than expected
   - `e.RecognitionResult` might be a string, not an object

---

## Enhanced Debugging

I've added comprehensive logging to help identify the issue. Here's what to look for:

### In StartListeningAsync:
```
[SpeechRecognition] 🎤 Starting REAL speech recognition (language: en-US)
[SpeechRecognition] 🎤 Microphone permission granted, starting listener...
[SpeechRecognition] 🎤 Listening started, waiting for speech...
```

### In OnRecognitionResultUpdated (Partial Results):
```
[SpeechRecognition] 🎤 Partial (REAL): my
[SpeechRecognition] 🎤 Partial (REAL): my voice
[SpeechRecognition] 🎤 Partial (REAL): my voice is my
```

### In OnRecognitionResultCompleted (Final Result):
```
[SpeechRecognition] 🔔 OnRecognitionResultCompleted event fired!
[SpeechRecognition] 🔍 e.RecognitionResult is null: False
[SpeechRecognition] 🔍 IsSuccessful: True
[SpeechRecognition] 🔍 Text: 'my voice is my password'
[SpeechRecognition] 🔍 Exception: null
[SpeechRecognition] ✅ REAL Recognition Success: 'my voice is my password'
[SpeechRecognition] 🔍 TrySetResult returned: True
```

### In StopListeningAsync:
```
[SpeechRecognition] 🔍 About to stop listening...
[SpeechRecognition] 🔍 _recognitionCompletionSource is null: False
[SpeechRecognition] 🎤 Stopped listening
[SpeechRecognition] 🔍 Waiting for completion event (2 second timeout)...
[SpeechRecognition] 🔍 Completed task is timeout: False
[SpeechRecognition] 🔍 Completed task is recognition: True
[SpeechRecognition] 🔍 Recognition task completed, getting result...
[SpeechRecognition] 🔍 Result from task: 'my voice is my password'
[SpeechRecognition] ✅ Returning REAL result: 'my voice is my password'
```

---

## Diagnostic Scenarios

### Scenario 1: Event Never Fires
**Symptoms:**
```
[SpeechRecognition] 🎤 Stopped listening
[SpeechRecognition] 🔍 Waiting for completion event (2 second timeout)...
[SpeechRecognition] ⚠️ Recognition timeout - event did not fire within 2 seconds
[SpeechRecognition] 💡 This usually means OnRecognitionResultCompleted was not called
```

**Possible Causes:**
- Platform doesn't support the event
- Speech recognition service is broken
- No speech was detected
- Event subscription failed

**Solution:**
- Check if you're on Android 33+ (API level 33)
- Verify microphone permission is granted
- Try speaking louder/clearer
- Check if simulation mode is being used instead

### Scenario 2: Event Fires But Result is Null
**Symptoms:**
```
[SpeechRecognition] 🔔 OnRecognitionResultCompleted event fired!
[SpeechRecognition] 🔍 e.RecognitionResult is null: True
[SpeechRecognition] ⚠️ Recognition completed but no result
```

**Possible Causes:**
- No speech detected
- Recognition failed silently
- Platform returned empty result

**Solution:**
- Speak clearly during recording
- Check microphone is working
- Try increasing recording duration

### Scenario 3: Event Fires But IsSuccessful is False
**Symptoms:**
```
[SpeechRecognition] 🔔 OnRecognitionResultCompleted event fired!
[SpeechRecognition] 🔍 e.RecognitionResult is null: False
[SpeechRecognition] 🔍 IsSuccessful: False
[SpeechRecognition] 🔍 Exception: NetworkFailure
[SpeechRecognition] ❌ Recognition failed: NetworkFailure
```

**Possible Causes:**
- Network error (if using online recognition)
- Platform error
- Service unavailable

**Solution:**
- Check internet connection (if required)
- Verify platform supports offline recognition
- Check for platform-specific errors

### Scenario 4: TrySetResult Returns False
**Symptoms:**
```
[SpeechRecognition] ✅ REAL Recognition Success: 'my voice is my password'
[SpeechRecognition] 🔍 TrySetResult returned: False
```

**Possible Causes:**
- TaskCompletionSource was already completed
- TaskCompletionSource was cancelled
- Race condition

**Solution:**
- Event might be firing multiple times
- Check if event fires before StopListenAsync is called
- Increase timeout duration

---

## Common Issues and Fixes

### Issue 1: Type Mismatch in OnRecognitionResultUpdated

**Problem:**
```csharp
// Line 48 - WRONG!
if (e.RecognitionResult != null && !string.IsNullOrEmpty(e.RecognitionResult))
```

This treats `e.RecognitionResult` as both an object (null check) and a string (IsNullOrEmpty check).

**Fix:**
```csharp
// CORRECT - e.RecognitionResult is a string
if (!string.IsNullOrEmpty(e.RecognitionResult))
{
    var partialText = e.RecognitionResult;
    Debug.WriteLine($"[SpeechRecognition] 🎤 Partial (REAL): {partialText}");
}
```

### Issue 2: Event Fires Before StopListenAsync

**Problem:**
The event might fire immediately after speech ends, before you call `StopListenAsync()`.

**Fix:**
Increase the timeout or handle the case where the task is already completed:

```csharp
// Check if already completed
if (_recognitionCompletionSource.Task.IsCompleted)
{
    var result = await _recognitionCompletionSource.Task;
    Debug.WriteLine($"[SpeechRecognition] ✅ Result already available: '{result}'");
    return result;
}

// Otherwise wait with timeout
var timeoutTask = Task.Delay(5000); // Increase to 5 seconds
var completedTask = await Task.WhenAny(_recognitionCompletionSource.Task, timeoutTask);
```

### Issue 3: Platform Doesn't Support Events

**Problem:**
Some platforms might not fire the `RecognitionResultCompleted` event reliably.

**Fix:**
Use a hybrid approach - check both the event result and the stored `_recognizedText`:

```csharp
// Wait for event
var timeoutTask = Task.Delay(2000);
var completedTask = await Task.WhenAny(_recognitionCompletionSource.Task, timeoutTask);

if (completedTask == _recognitionCompletionSource.Task)
{
    // Event fired - use event result
    var result = await _recognitionCompletionSource.Task;
    return result;
}
else
{
    // Timeout - check if we have a stored result anyway
    if (!string.IsNullOrWhiteSpace(_recognizedText))
    {
        Debug.WriteLine($"[SpeechRecognition] ⚠️ Timeout but found stored result: '{_recognizedText}'");
        return _recognizedText;
    }
}
```

---

## Testing Steps

### Step 1: Build and Deploy
```powershell
cd "C:\Users\Full Scale\Desktop\product-innovation\Boses"
dotnet clean
dotnet restore
dotnet build -f net9.0-android
# Deploy to device
```

### Step 2: Run Voice Registration
1. Open the app
2. Navigate to voice registration
3. Tap the microphone button
4. Speak clearly: "My voice is my password"
5. Wait for recording to stop

### Step 3: Check Debug Output
Look for these key messages in the Output window:

**Expected (Success):**
```
[SpeechRecognition] 🔔 OnRecognitionResultCompleted event fired!
[SpeechRecognition] 🔍 TrySetResult returned: True
[SpeechRecognition] 🔍 Completed task is recognition: True
[SpeechRecognition] ✅ Returning REAL result: 'my voice is my password'
```

**Actual (Timeout):**
```
[SpeechRecognition] 🔍 Waiting for completion event (2 second timeout)...
[SpeechRecognition] ⚠️ Recognition timeout - event did not fire within 2 seconds
```

### Step 4: Analyze the Logs
Based on what you see, identify which scenario matches your situation and apply the corresponding fix.

---

## Quick Fixes to Try

### Fix 1: Increase Timeout
Change the timeout from 2 seconds to 5 seconds:

```csharp
var timeoutTask = Task.Delay(5000); // Increased from 2000
```

### Fix 2: Check Task Before Waiting
Add this before the timeout check:

```csharp
// Check if task is already completed
if (_recognitionCompletionSource.Task.IsCompleted)
{
    Debug.WriteLine("[SpeechRecognition] 🔍 Task already completed!");
    var result = await _recognitionCompletionSource.Task;
    return result;
}
```

### Fix 3: Use Stored Result as Fallback
After the timeout, check the stored `_recognizedText`:

```csharp
else
{
    Debug.WriteLine("[SpeechRecognition] ⚠️ Recognition timeout");
    
    // Check if we have a stored result anyway
    if (!string.IsNullOrWhiteSpace(_recognizedText))
    {
        Debug.WriteLine($"[SpeechRecognition] 💡 Using stored result: '{_recognizedText}'");
        return _recognizedText;
    }
}
```

---

## Next Steps

1. **Build and run** the app with the enhanced debugging
2. **Check the Debug output** to see which scenario you're hitting
3. **Apply the appropriate fix** based on the diagnostic scenario
4. **Report back** with the debug output so we can identify the exact issue

The enhanced logging will tell us exactly what's happening! 🔍
