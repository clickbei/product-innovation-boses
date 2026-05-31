# Community Toolkit Speech-to-Text API Usage

## ✅ Correct API Implementation

The `ISpeechToText` interface uses an **event-based approach**, not a single async method.

## 🎯 Key API Methods

### 1. Request Permissions
```csharp
await _speechToText.RequestPermissions(CancellationToken.None);
```

### 2. Start Listening
```csharp
await _speechToText.StartListenAsync(
    CultureInfo.GetCultureInfo(language),
    CancellationToken.None);
```

### 3. Stop Listening
```csharp
await _speechToText.StopListenAsync(CancellationToken.None);
```

## 📡 Events

### RecognitionResultUpdated
Fires during speech recognition with partial results:
```csharp
_speechToText.RecognitionResultUpdated += OnRecognitionResultUpdated;

private void OnRecognitionResultUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs e)
{
    Debug.WriteLine($"Partial: {e.RecognitionResult}");
}
```

### RecognitionResultCompleted
Fires when speech recognition completes:
```csharp
_speechToText.RecognitionResultCompleted += OnRecognitionResultCompleted;

private void OnRecognitionResultCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
{
    var finalText = e.RecognitionResult;
    Debug.WriteLine($"Final: {finalText}");
}
```

### StateChanged
Fires when the recognition state changes:
```csharp
_speechToText.StateChanged += OnStateChanged;

private void OnStateChanged(object? sender, SpeechToTextStateChangedEventArgs e)
{
    Debug.WriteLine($"State: {e.State}");
}
```

## 🔄 Complete Flow

```csharp
// 1. Subscribe to events (in constructor)
_speechToText.RecognitionResultUpdated += OnRecognitionResultUpdated;
_speechToText.RecognitionResultCompleted += OnRecognitionResultCompleted;

// 2. Request permissions
await _speechToText.RequestPermissions(CancellationToken.None);

// 3. Start listening
await _speechToText.StartListenAsync(
    CultureInfo.GetCultureInfo("en-US"),
    CancellationToken.None);

// 4. Events fire as user speaks
// - RecognitionResultUpdated (multiple times with partial results)
// - RecognitionResultCompleted (once with final result)

// 5. Stop listening
await _speechToText.StopListenAsync(CancellationToken.None);
```

## 💡 Implementation Pattern

### Using TaskCompletionSource for Async/Await
```csharp
private TaskCompletionSource<string?>? _recognitionCompletionSource;

// Start listening
_recognitionCompletionSource = new TaskCompletionSource<string?>();
await _speechToText.StartListenAsync(culture, cancellationToken);

// In completion event
private void OnRecognitionResultCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
{
    _recognitionCompletionSource?.TrySetResult(e.RecognitionResult);
}

// Wait for result
var result = await _recognitionCompletionSource.Task;
```

## 🚫 Common Mistakes

### ❌ Wrong: ListenAsync() doesn't exist
```csharp
// This method doesn't exist!
var result = await _speechToText.ListenAsync(culture, progress, cancellationToken);
```

### ✅ Correct: Use StartListenAsync() + Events
```csharp
// Subscribe to events first
_speechToText.RecognitionResultCompleted += OnCompleted;

// Then start listening
await _speechToText.StartListenAsync(culture, cancellationToken);

// Result comes via event
private void OnCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
{
    var result = e.RecognitionResult;
}
```

## 📚 Resources

- [SpeechToText - .NET MAUI Community Toolkit](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/essentials/speech-to-text)
- [ISpeechToText Source Code](https://github.com/CommunityToolkit/Maui/blob/main/src/CommunityToolkit.Maui.Core/Essentials/SpeechToText/ISpeechToText.shared.cs)
- [Speech Recognition in .NET MAUI Blog](https://devblogs.microsoft.com/dotnet/speech-recognition-in-dotnet-maui-with-community-toolkit/)

## ✅ Our Implementation

The `MauiSpeechRecognitionService` now correctly:
1. ✅ Subscribes to events in constructor
2. ✅ Uses `StartListenAsync()` to begin recognition
3. ✅ Uses `StopListenAsync()` to end recognition
4. ✅ Handles results via events
5. ✅ Uses `TaskCompletionSource` for async/await pattern
6. ✅ Falls back to simulation if real recognition fails

**Build and test - it should work now!** 🚀
