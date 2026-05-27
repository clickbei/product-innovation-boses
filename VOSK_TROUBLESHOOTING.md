# Vosk Model Loading Error - Troubleshooting Guide

## 🔴 Error: "libvosk" at Model..ctor

### Error Details
```
at Vosk.Model..ctor(String model_path)
at BosesApp.Core.Services.VoskSpeechRecognitionService.StartListeningAsync(String language)
```

This means Vosk can't load the model from the specified path.

## 🔍 Diagnosis Steps

### Step 1: Check if Models Exist on Device

Run the diagnostic script:
```powershell
.\check-android-models.ps1
```

This will show:
- ✅ Which models are present
- ❌ Which files are missing
- 📊 Model sizes

### Step 2: Check Debug Output

Look for these messages in Visual Studio Output window:

**Good**:
```
[SpeechRecognition] ✅ English model found
[SpeechRecognition] ✅ Initialized with FREE Vosk speech recognition
[SpeechRecognition] 🔍 Checking model path: /data/user/0/.../vosk-model-small-en-us-0.15
[SpeechRecognition] ✅ Model files verified, loading model...
```

**Bad**:
```
[SpeechRecognition] ❌ Model directory does not exist
[SpeechRecognition] ❌ Required file missing: am/final.mdl
```

## 🛠️ Common Causes & Fixes

### Cause 1: Models Not Deployed

**Symptoms**:
- First time running the app
- Models directory doesn't exist
- Debug shows "Models not found"

**Fix**:
```powershell
# Option A: Use ZIP method (recommended)
.\setup-vosk-zips.ps1
dotnet clean
dotnet build -f net9.0-android

# Option B: Manual ADB push
adb push vosk-model-small-en-us-0.15 /data/user/0/com.boses.accessibility/files/vosk-models/
```

### Cause 2: Incomplete Model Files

**Symptoms**:
- Model directory exists but is empty or incomplete
- Debug shows "Required file missing"

**Fix**:
```powershell
# Delete incomplete models
adb shell rm -rf /data/user/0/com.boses.accessibility/files/vosk-models

# Re-deploy
.\setup-vosk-zips.ps1
dotnet clean
dotnet build -f net9.0-android
```

### Cause 3: Wrong Model Path

**Symptoms**:
- Models exist but in wrong location
- Path in debug doesn't match actual location

**Fix**: Check the path in debug output matches where models actually are

### Cause 4: Corrupted ZIP Extraction

**Symptoms**:
- ZIP exists in Resources
- Extraction failed silently
- Model files are corrupted or incomplete

**Fix**:
```powershell
# Re-create ZIPs
cd "$env:TEMP\vosk-models"
Compress-Archive -Path "vosk-model-small-en-us-0.15" -DestinationPath "vosk-model-small-en-us-0.15.zip" -Force

# Copy to Resources
copy "vosk-model-small-en-us-0.15.zip" "C:\Users\Full Scale\Desktop\product-innovation\Boses\Resources\Raw\" -Force

# Clean rebuild
dotnet clean
dotnet build -f net9.0-android
```

### Cause 5: Insufficient Storage

**Symptoms**:
- Extraction starts but fails
- Partial model files

**Fix**: Free up space on device (need ~100 MB free)

### Cause 6: Android Permissions

**Symptoms**:
- Can't write to app data directory
- Permission denied errors

**Fix**: Uninstall and reinstall the app

## 🚀 Quick Fixes

### Fix 1: Clean Slate (Recommended)

```powershell
# 1. Delete old models on device
adb shell rm -rf /data/user/0/com.boses.accessibility/files/vosk-models

# 2. Clear app data
adb shell pm clear com.boses.accessibility

# 3. Re-setup ZIPs
.\setup-vosk-zips.ps1

# 4. Clean rebuild
dotnet clean
dotnet build -f net9.0-android

# 5. Deploy and run
```

### Fix 2: Manual Deployment (If ZIP fails)

```powershell
# 1. Download models
.\download-vosk-models.ps1

# 2. Push directly via ADB
cd "$env:TEMP\vosk-models"
adb push vosk-model-small-en-us-0.15 /data/user/0/com.boses.accessibility/files/vosk-models/

# 3. Verify
adb shell ls -la /data/user/0/com.boses.accessibility/files/vosk-models/vosk-model-small-en-us-0.15/
```

### Fix 3: Use Simulation Mode (Temporary)

If you just want to test other features:
- The app will automatically fall back to simulation mode
- Voice registration will work (simulated)
- You can fix models later

## 📋 Verification Checklist

After applying fixes, verify:

- [ ] Models directory exists on device
- [ ] English model folder exists: `vosk-model-small-en-us-0.15/`
- [ ] Essential files exist:
  - [ ] `am/final.mdl`
  - [ ] `conf/mfcc.conf`
  - [ ] `graph/HCLr.fst`
  - [ ] `graph/words.txt`
- [ ] Debug output shows "✅ Model files verified"
- [ ] Debug output shows "✅ Loaded model from: ..."
- [ ] No "libvosk" error when starting recognition

## 🔍 Debug Commands

### Check if models exist:
```bash
adb shell ls -la /data/user/0/com.boses.accessibility/files/vosk-models/
```

### Check model size:
```bash
adb shell du -sh /data/user/0/com.boses.accessibility/files/vosk-models/*
```

### Check specific file:
```bash
adb shell test -f /data/user/0/com.boses.accessibility/files/vosk-models/vosk-model-small-en-us-0.15/am/final.mdl && echo "EXISTS" || echo "MISSING"
```

### Delete models:
```bash
adb shell rm -rf /data/user/0/com.boses.accessibility/files/vosk-models
```

### Check app data directory:
```bash
adb shell ls -la /data/user/0/com.boses.accessibility/files/
```

## 📊 Expected Debug Output

### Successful Initialization:
```
[SpeechRecognition] 🔄 Models not found in app data, attempting deployment...
[ModelDeployer] Starting model deployment...
[ModelDeployer] 📦 Found vosk-model-small-en-us-0.15.zip in Resources
[ModelDeployer] 📂 Extracting vosk-model-small-en-us-0.15.zip...
[ModelDeployer] ✅ vosk-model-small-en-us-0.15 deployed successfully
[SpeechRecognition] ✅ Models deployed from Resources
[SpeechRecognition] ✅ English model found
[SpeechRecognition] ✅ Initialized with FREE Vosk speech recognition (offline)
```

### Successful Recognition Start:
```
[SpeechRecognition] ✅ Starting FREE Vosk recognition (language: en-US)
[SpeechRecognition] 🔍 Checking model path: /data/user/0/.../vosk-model-small-en-us-0.15
[SpeechRecognition] ✅ Model files verified, loading model...
[SpeechRecognition] ✅ Loaded model from: /data/user/0/.../vosk-model-small-en-us-0.15
[SpeechRecognition] ✅ Vosk recognizer ready - will process audio
```

## 🎯 Most Common Solution

**90% of the time, this fixes it**:

```powershell
# Complete clean setup
adb shell rm -rf /data/user/0/com.boses.accessibility/files/vosk-models
.\setup-vosk-zips.ps1
dotnet clean
dotnet build -f net9.0-android
```

Then deploy and check the Debug output!

## 💡 Prevention

To avoid this issue in the future:

1. **Always use ZIP method** - More reliable than folders
2. **Check Debug output** - Verify deployment succeeded
3. **Run diagnostic script** - Before reporting issues
4. **Keep ZIPs in Resources** - Don't delete them after first deployment

## 🆘 Still Not Working?

If none of the above fixes work:

1. **Run diagnostic script**:
   ```powershell
   .\check-android-models.ps1
   ```

2. **Share the output** showing:
   - What's in the vosk-models directory
   - Which files exist/missing
   - Model sizes

3. **Share Debug output** from Visual Studio Output window

4. **Try manual ADB push** as a workaround:
   ```powershell
   cd "$env:TEMP\vosk-models"
   adb push vosk-model-small-en-us-0.15 /data/user/0/com.boses.accessibility/files/vosk-models/
   ```
