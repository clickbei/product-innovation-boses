# Vosk Models Setup - Complete Guide

## 🎯 What Was Implemented

The `VoskSpeechRecognitionService` now **automatically copies Vosk models** from your app's Resources folder to the device on first run!

## ✅ Benefits

- ✅ **Bundle models in your app** - No manual device setup
- ✅ **Automatic deployment** - Models copied on first run
- ✅ **Cross-platform** - Works on Android, iOS, Windows
- ✅ **Offline from day 1** - No internet required
- ✅ **User-friendly** - Just install and run

## 📋 Quick Setup (3 Steps)

### Step 1: Download Models
```powershell
.\download-vosk-models.ps1
```

This downloads:
- `vosk-model-small-en-us-0.15` (40 MB) - English
- `vosk-model-tl-ph-generic-0.6` (50 MB) - Filipino

### Step 2: Copy to Resources
```powershell
.\setup-bundled-models.ps1
```

This copies models to:
```
Resources/Raw/VoskModels/
├── vosk-model-small-en-us-0.15/
└── vosk-model-tl-ph-generic-0.6/
```

### Step 3: Rebuild & Deploy
```powershell
dotnet clean
dotnet build -f net9.0-android
```

Deploy to your device - **Done!** ✅

## 🔍 How It Works

### Code Flow

```
App Starts
    ↓
VoskSpeechRecognitionService Constructor
    ↓
Check if models exist in app data?
    ├─ YES → Use existing models ✅
    └─ NO  → Copy from Resources
              ↓
         Check Resources/Raw/VoskModels/
              ├─ Found → Copy to app data ✅
              └─ Not Found → Use simulation mode 🔄
```

### Implementation Details

**File**: `Core/Services/VoskSpeechRecognitionService.cs`

```csharp
public VoskSpeechRecognitionService()
{
    // Set models path in app data
    _modelsPath = Path.Combine(FileSystem.AppDataDirectory, "vosk-models");

    // Check if models exist, if not copy from Resources
    if (!CheckModelsExist())
    {
        Debug.WriteLine("Models not found, checking Resources...");
        CopyModelsFromResources(); // NEW METHOD
    }

    _isInitialized = CheckModelsExist();
}
```

**New Methods Added**:
1. `CopyModelsFromResources()` - Copies both models from Resources
2. `CopyModelFromResources(modelName)` - Copies a specific model
3. `CopyDirectoryFromResources(resourcePath, targetPath)` - Copies files

## 📊 App Size Impact

| Configuration | App Size | Notes |
|---------------|----------|-------|
| No models | ~10 MB | Uses simulation only |
| English only | ~50 MB | Real recognition for English |
| Both models | ~100 MB | Full bilingual support |

## 🎯 Deployment Paths

### Android
```
Resources/Raw/VoskModels/
    ↓ (copied on first run)
/data/user/0/com.boses.accessibility/files/vosk-models/
```

### Windows
```
Resources/Raw/VoskModels/
    ↓ (copied on first run)
C:\Users\Full Scale\AppData\Local\Boses\vosk-models\
```

### iOS
```
Resources/Raw/VoskModels/
    ↓ (copied on first run)
~/Library/Application Support/vosk-models/
```

## 🔧 Debug Output

### First Run (Models Copied)
```
[SpeechRecognition] 🔄 Models not found in app data, checking Resources...
[SpeechRecognition] 📦 Attempting to copy models from Resources...
[SpeechRecognition] 📂 Copying vosk-model-small-en-us-0.15 from Resources...
[SpeechRecognition] ✅ vosk-model-small-en-us-0.15 copied successfully
[SpeechRecognition] 📂 Copying vosk-model-tl-ph-generic-0.6 from Resources...
[SpeechRecognition] ✅ vosk-model-tl-ph-generic-0.6 copied successfully
[SpeechRecognition] ✅ Initialized with FREE Vosk speech recognition (offline)
[SpeechRecognition] ✅ English model found
[SpeechRecognition] ✅ Filipino model found
```

### Subsequent Runs (Models Already Exist)
```
[SpeechRecognition] ✅ vosk-model-small-en-us-0.15 already exists, skipping copy
[SpeechRecognition] ✅ vosk-model-tl-ph-generic-0.6 already exists, skipping copy
[SpeechRecognition] ✅ Initialized with FREE Vosk speech recognition (offline)
[SpeechRecognition] ✅ English model found
[SpeechRecognition] ✅ Filipino model found
```

### No Models in Resources (Fallback)
```
[SpeechRecognition] 🔄 Models not found in app data, checking Resources...
[SpeechRecognition] ⚠️ vosk-model-small-en-us-0.15 not found in Resources/VoskModels
[SpeechRecognition] ⚠️ vosk-model-tl-ph-generic-0.6 not found in Resources/VoskModels
[SpeechRecognition] 🔄 Vosk models not found, using simulation
[SpeechRecognition] 💡 Place models in Resources/VoskModels folder
```

## 📁 Project Structure

```
Boses/
├── Resources/
│   └── Raw/
│       └── VoskModels/                    ← Add models here
│           ├── vosk-model-small-en-us-0.15/
│           │   ├── am/
│           │   │   └── final.mdl
│           │   ├── conf/
│           │   │   ├── mfcc.conf
│           │   │   └── model.conf
│           │   ├── graph/
│           │   │   ├── Gr.fst
│           │   │   ├── HCLr.fst
│           │   │   ├── phones.txt
│           │   │   └── words.txt
│           │   └── ivector/
│           │       ├── final.dubm
│           │       ├── final.ie
│           │       └── final.mat
│           └── vosk-model-tl-ph-generic-0.6/
│               └── (same structure)
├── Core/
│   └── Services/
│       └── VoskSpeechRecognitionService.cs  ← Updated
├── download-vosk-models.ps1                 ← NEW
├── setup-bundled-models.ps1                 ← NEW
└── BUNDLE_VOSK_MODELS_GUIDE.md             ← NEW
```

## 🚀 Complete Workflow

### For Development
```bash
# 1. Download models
.\download-vosk-models.ps1

# 2. Copy to Resources
.\setup-bundled-models.ps1

# 3. Rebuild
dotnet clean
dotnet build -f net9.0-android

# 4. Deploy
# Models are automatically copied on first run!
```

### For Production
```bash
# Same as development, but:
# - Models are bundled in the APK/IPA
# - Users get offline speech recognition immediately
# - No manual setup required
```

## 🔄 Alternative Approaches

### Option 1: Bundle in App (Current Implementation)
✅ No manual setup
✅ Works offline immediately
⚠️ Larger app size (~100 MB)

### Option 2: Download on First Run
✅ Smaller app size (~10 MB)
⚠️ Requires internet on first run
⚠️ Longer first-run time

### Option 3: Manual Setup (Original)
✅ Smallest app size
⚠️ Requires manual ADB commands
⚠️ Not user-friendly

**Recommendation**: Use Option 1 (Bundle in App) for best user experience!

## 🐛 Troubleshooting

### Models not copying?

**Check**:
1. Models exist in `Resources/Raw/VoskModels/`
2. Rebuild the project (Clean + Build)
3. Check Debug output for error messages

**Fix**:
```powershell
# Re-run setup
.\setup-bundled-models.ps1

# Clean and rebuild
dotnet clean
dotnet build -f net9.0-android
```

### App size too large?

**Solutions**:
1. **English only**: Remove Filipino model (saves 50 MB)
2. **Download on demand**: Don't bundle, download on first run
3. **Compression**: Compress models, decompress on first run

### Still using simulation?

**Check Debug output**:
```
[SpeechRecognition] 🔄 Vosk models not found, using simulation
```

**Possible causes**:
1. Models not in Resources folder
2. Models not copied to device
3. Wrong folder structure

**Fix**: Follow the setup steps again

## 📝 Files Created

1. **`download-vosk-models.ps1`** - Downloads models from Vosk website
2. **`setup-bundled-models.ps1`** - Copies models to Resources folder
3. **`BUNDLE_VOSK_MODELS_GUIDE.md`** - Detailed setup guide
4. **`VOSK_MODELS_SETUP_SUMMARY.md`** - This file

## 🎉 Summary

You can now **bundle Vosk models in your app** for automatic deployment!

**Steps**:
1. Run `download-vosk-models.ps1`
2. Run `setup-bundled-models.ps1`
3. Rebuild and deploy

**Result**: Real offline speech recognition works immediately! 🚀
