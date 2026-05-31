# Bundle Vosk Models in Your App

## 📦 Overview

Instead of manually copying models to each device, you can **bundle them in your app** so they're automatically deployed with the app!

## 🎯 Benefits

✅ **No manual setup** - Models are included in the app
✅ **Works offline immediately** - No download needed
✅ **Cross-platform** - Works on Android, iOS, Windows
✅ **User-friendly** - Just install and run

⚠️ **Trade-off**: Increases app size by ~90 MB (40 MB English + 50 MB Filipino)

## 📂 Step 1: Download Models

### Option A: Download Script (Recommended)

Run the PowerShell script:
```powershell
.\download-vosk-models.ps1
```

This downloads models to: `C:\Users\Full Scale\AppData\Local\Temp\vosk-models\`

### Option B: Manual Download

1. **English Model** (Required):
   - Download: https://alphacephei.com/vosk/models/vosk-model-small-en-us-0.15.zip
   - Extract to get `vosk-model-small-en-us-0.15/` folder

2. **Filipino Model** (Optional):
   - Download: https://alphacephei.com/vosk/models/vosk-model-tl-ph-generic-0.6.zip
   - Extract to get `vosk-model-tl-ph-generic-0.6/` folder

## 📁 Step 2: Add Models to Project

### Create Resources Folder Structure

```
Boses/
├── Resources/
│   └── Raw/
│       └── VoskModels/
│           ├── vosk-model-small-en-us-0.15/
│           │   ├── am/
│           │   │   └── final.mdl
│           │   ├── conf/
│           │   │   ├── mfcc.conf
│           │   │   └── model.conf
│           │   ├── graph/
│           │   │   ├── disambig_tid.int
│           │   │   ├── Gr.fst
│           │   │   ├── HCLr.fst
│           │   │   ├── phones.txt
│           │   │   ├── phones/
│           │   │   │   └── word_boundary.int
│           │   │   └── words.txt
│           │   └── ivector/
│           │       ├── final.dubm
│           │       ├── final.ie
│           │       ├── final.mat
│           │       ├── global_cmvn.stats
│           │       ├── online_cmvn.conf
│           │       └── splice.conf
│           └── vosk-model-tl-ph-generic-0.6/
│               └── (same structure as above)
```

### Copy Models

1. **Navigate to your project**:
   ```
   C:\Users\Full Scale\Desktop\product-innovation\Boses\
   ```

2. **Create the directory**:
   ```powershell
   mkdir Resources\Raw\VoskModels
   ```

3. **Copy the model folders**:
   ```powershell
   # Copy English model
   xcopy /E /I "C:\Users\Full Scale\AppData\Local\Temp\vosk-models\vosk-model-small-en-us-0.15" "Resources\Raw\VoskModels\vosk-model-small-en-us-0.15"
   
   # Copy Filipino model
   xcopy /E /I "C:\Users\Full Scale\AppData\Local\Temp\vosk-models\vosk-model-tl-ph-generic-0.6" "Resources\Raw\VoskModels\vosk-model-tl-ph-generic-0.6"
   ```

## 🔧 Step 3: Update Project File

Edit `BosesApp.csproj` to include the models as **MauiAsset**:

```xml
<ItemGroup>
  <!-- Existing Raw Assets -->
  <MauiAsset Include="Resources\Raw\**" LogicalName="%(RecursiveDir)%(Filename)%(Extension)" />
  
  <!-- Vosk Models (explicitly included) -->
  <MauiAsset Include="Resources\Raw\VoskModels\**\*.*" />
</ItemGroup>
```

**Note**: The existing line `<MauiAsset Include="Resources\Raw\**" .../>` should already include the VoskModels, but adding it explicitly ensures it's included.

## 🚀 Step 4: How It Works

### Automatic Deployment

When you build and deploy the app:

1. **Build time**: Models are packaged into the app bundle
2. **First run**: `VoskSpeechRecognitionService` checks if models exist in app data
3. **If not found**: Automatically copies from `Resources/Raw/VoskModels/` to app data directory
4. **Subsequent runs**: Uses the copied models (no re-copy needed)

### Code Flow

```csharp
// In VoskSpeechRecognitionService constructor:
if (!CheckModelsExist())
{
    Debug.WriteLine("Models not found in app data, checking Resources...");
    CopyModelsFromResources(); // Copies from Resources to app data
}
```

### Target Locations

**Android**:
```
Resources/Raw/VoskModels/ → /data/user/0/com.boses.accessibility/files/vosk-models/
```

**Windows**:
```
Resources/Raw/VoskModels/ → C:\Users\Full Scale\AppData\Local\Boses\vosk-models\
```

**iOS**:
```
Resources/Raw/VoskModels/ → ~/Library/Application Support/vosk-models/
```

## ✅ Step 5: Verify

### Check Debug Output

When you run the app, you should see:

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

### Test Speech Recognition

1. Go to voice registration
2. Record a sample
3. Check if it recognizes your speech (not simulation)

## 📊 App Size Impact

| Component | Size |
|-----------|------|
| Base app | ~10 MB |
| English model | +40 MB |
| Filipino model | +50 MB |
| **Total** | **~100 MB** |

### Optimization Options

If 100 MB is too large:

1. **English only** (50 MB total):
   - Only include `vosk-model-small-en-us-0.15`
   - Remove Filipino model

2. **Download on demand** (10 MB app):
   - Don't bundle models
   - Download on first run (requires internet)
   - Use the existing manual setup approach

3. **Compressed models** (70 MB total):
   - Compress models with 7zip/gzip
   - Decompress on first run
   - Saves ~20-30% space

## 🔄 Alternative: Hybrid Approach

Bundle only English, download Filipino on demand:

```csharp
// In VoskSpeechRecognitionService
if (!HasFilipinoModel() && userWantsFilipinoLanguage)
{
    await DownloadFilipinoModelAsync();
}
```

## 🐛 Troubleshooting

### Models not copying

**Check**:
1. Models exist in `Resources/Raw/VoskModels/`
2. Build action is set to `MauiAsset`
3. Clean and rebuild the project

**Fix**:
```powershell
# Clean
dotnet clean

# Rebuild
dotnet build -f net9.0-android
```

### App size too large

**Solutions**:
1. Use only English model (remove Filipino)
2. Use smaller models (if available)
3. Download models on first run instead of bundling

### Models not found at runtime

**Check Debug output**:
```
[SpeechRecognition] ⚠️ vosk-model-small-en-us-0.15 not found in Resources/VoskModels
```

**Fix**: Ensure models are in correct folder structure

## 📝 Quick Setup Script

Save as `setup-bundled-models.ps1`:

```powershell
# Setup Bundled Vosk Models
Write-Host "Setting up bundled Vosk models..." -ForegroundColor Cyan

$projectRoot = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
$resourcesPath = "$projectRoot\Resources\Raw\VoskModels"
$tempModels = "$env:TEMP\vosk-models"

# Create Resources directory
New-Item -ItemType Directory -Force -Path $resourcesPath | Out-Null

# Copy models
if (Test-Path "$tempModels\vosk-model-small-en-us-0.15") {
    Write-Host "Copying English model..." -ForegroundColor Yellow
    xcopy /E /I /Y "$tempModels\vosk-model-small-en-us-0.15" "$resourcesPath\vosk-model-small-en-us-0.15"
    Write-Host "✅ English model copied" -ForegroundColor Green
} else {
    Write-Host "❌ English model not found in $tempModels" -ForegroundColor Red
    Write-Host "Run download-vosk-models.ps1 first" -ForegroundColor Yellow
}

if (Test-Path "$tempModels\vosk-model-tl-ph-generic-0.6") {
    Write-Host "Copying Filipino model..." -ForegroundColor Yellow
    xcopy /E /I /Y "$tempModels\vosk-model-tl-ph-generic-0.6" "$resourcesPath\vosk-model-tl-ph-generic-0.6"
    Write-Host "✅ Filipino model copied" -ForegroundColor Green
} else {
    Write-Host "⚠️ Filipino model not found (optional)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "✅ Models are now in Resources/Raw/VoskModels/" -ForegroundColor Green
Write-Host "Rebuild your project to include them in the app" -ForegroundColor Cyan

pause
```

## 🎯 Summary

1. **Download models** → `download-vosk-models.ps1`
2. **Copy to Resources** → `setup-bundled-models.ps1`
3. **Rebuild app** → Models are bundled
4. **Deploy** → Models automatically copied to device on first run
5. **Works offline** → No manual setup needed! ✅
