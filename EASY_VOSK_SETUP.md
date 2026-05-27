# Easy Vosk Models Setup (ZIP Method)

## 🎯 The Problem

You're getting "model not found in Resources/VoskModels" because MAUI has trouble with deeply nested folder structures in Resources.

## ✅ The Solution: Use ZIP Files

Instead of copying thousands of individual files, we'll use **ZIP files** which MAUI handles much better!

## 📦 Step-by-Step Setup

### Step 1: Download Models

Run the download script:
```powershell
.\download-vosk-models.ps1
```

This downloads models to: `C:\Users\Full Scale\AppData\Local\Temp\vosk-models\`

### Step 2: Create ZIP Files

```powershell
# Navigate to the models directory
cd "$env:TEMP\vosk-models"

# Create ZIP for English model
Compress-Archive -Path "vosk-model-small-en-us-0.15" -DestinationPath "vosk-model-small-en-us-0.15.zip" -Force

# Create ZIP for Filipino model
Compress-Archive -Path "vosk-model-tl-ph-generic-0.6" -DestinationPath "vosk-model-tl-ph-generic-0.6.zip" -Force
```

### Step 3: Copy ZIP Files to Resources

```powershell
# Copy ZIPs to Resources/Raw
copy "vosk-model-small-en-us-0.15.zip" "C:\Users\Full Scale\Desktop\product-innovation\Boses\Resources\Raw\"
copy "vosk-model-tl-ph-generic-0.6.zip" "C:\Users\Full Scale\Desktop\product-innovation\Boses\Resources\Raw\"
```

**Final structure**:
```
Boses/
└── Resources/
    └── Raw/
        ├── vosk-model-small-en-us-0.15.zip  (40 MB)
        └── vosk-model-tl-ph-generic-0.6.zip  (50 MB)
```

### Step 4: Rebuild & Deploy

```powershell
dotnet clean
dotnet build -f net9.0-android
```

Deploy to your device - **Done!** ✅

## 🔍 How It Works

```
App Starts
    ↓
Check if models exist in app data?
    ├─ YES → Use them ✅
    └─ NO  → Look for ZIP files in Resources/Raw
              ↓
         Found vosk-model-small-en-us-0.15.zip?
              ├─ YES → Extract to app data ✅
              └─ NO  → Use simulation 🔄
```

## 📊 Why ZIP is Better

| Approach | Files to Bundle | MAUI Compatibility | Reliability |
|----------|----------------|-------------------|-------------|
| **Folder** | ~1000+ files | ❌ Poor | ⚠️ Unreliable |
| **ZIP** | 2 files | ✅ Excellent | ✅ Reliable |

## ✅ Verification

### Check Debug Output

**First run (extracting ZIPs)**:
```
[ModelDeployer] Starting model deployment...
[ModelDeployer] 📦 Models not found, attempting deployment...
[ModelDeployer] 📂 Deploying vosk-model-small-en-us-0.15...
[ModelDeployer] 📦 Found vosk-model-small-en-us-0.15.zip in Resources
[ModelDeployer] 📂 Extracting vosk-model-small-en-us-0.15.zip...
[ModelDeployer] ✅ vosk-model-small-en-us-0.15 deployed successfully
[SpeechRecognition] ✅ Models deployed from Resources
[SpeechRecognition] ✅ Initialized with FREE Vosk speech recognition (offline)
```

**Subsequent runs**:
```
[ModelDeployer] ✅ Models already deployed, skipping
[SpeechRecognition] ✅ Initialized with FREE Vosk speech recognition (offline)
```

## 🚀 Complete Setup Script

Save as `setup-vosk-zips.ps1`:

```powershell
# Easy Vosk Setup - ZIP Method
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "   📦 Easy Vosk Setup (ZIP Method)" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$tempModels = "$env:TEMP\vosk-models"
$projectRoot = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
$resourcesRaw = "$projectRoot\Resources\Raw"

# Step 1: Check if models are downloaded
if (!(Test-Path "$tempModels\vosk-model-small-en-us-0.15")) {
    Write-Host "❌ Models not found. Please run download-vosk-models.ps1 first" -ForegroundColor Red
    pause
    exit
}

Write-Host "✅ Models found in temp directory" -ForegroundColor Green
Write-Host ""

# Step 2: Create ZIP files
Write-Host "Creating ZIP files..." -ForegroundColor Yellow
cd $tempModels

Write-Host "  Compressing English model (40 MB)..." -ForegroundColor Gray
Compress-Archive -Path "vosk-model-small-en-us-0.15" -DestinationPath "vosk-model-small-en-us-0.15.zip" -Force
Write-Host "  ✅ English ZIP created" -ForegroundColor Green

if (Test-Path "vosk-model-tl-ph-generic-0.6") {
    Write-Host "  Compressing Filipino model (50 MB)..." -ForegroundColor Gray
    Compress-Archive -Path "vosk-model-tl-ph-generic-0.6" -DestinationPath "vosk-model-tl-ph-generic-0.6.zip" -Force
    Write-Host "  ✅ Filipino ZIP created" -ForegroundColor Green
}

Write-Host ""

# Step 3: Copy to Resources/Raw
Write-Host "Copying ZIPs to Resources/Raw..." -ForegroundColor Yellow

Copy-Item "vosk-model-small-en-us-0.15.zip" -Destination $resourcesRaw -Force
Write-Host "  ✅ English ZIP copied" -ForegroundColor Green

if (Test-Path "vosk-model-tl-ph-generic-0.6.zip") {
    Copy-Item "vosk-model-tl-ph-generic-0.6.zip" -Destination $resourcesRaw -Force
    Write-Host "  ✅ Filipino ZIP copied" -ForegroundColor Green
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "   ✅ Setup Complete!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "ZIP files location:" -ForegroundColor Cyan
Write-Host "  $resourcesRaw" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Rebuild: dotnet clean && dotnet build -f net9.0-android" -ForegroundColor White
Write-Host "  2. Deploy to device" -ForegroundColor White
Write-Host "  3. Models will be automatically extracted on first run" -ForegroundColor White
Write-Host ""
Write-Host "App size will increase by ~90 MB" -ForegroundColor Gray
Write-Host ""

pause
```

## 🐛 Troubleshooting

### Still getting "model not found"?

**Check**:
1. ZIP files exist in `Resources/Raw/`:
   ```powershell
   ls "C:\Users\Full Scale\Desktop\product-innovation\Boses\Resources\Raw\*.zip"
   ```

2. ZIP files are included in build:
   - Open `BosesApp.csproj`
   - Verify: `<MauiAsset Include="Resources\Raw\**" .../>`

3. Clean and rebuild:
   ```powershell
   dotnet clean
   dotnet build -f net9.0-android
   ```

### ZIP extraction fails?

**Check Debug output** for:
```
[ModelDeployer] ❌ Error deploying vosk-model-small-en-us-0.15: ...
```

**Common causes**:
- Insufficient storage space on device
- Corrupted ZIP file
- Permissions issue

**Fix**: Re-create the ZIP files and try again

### Models work but app is too large?

**Option 1**: English only (saves 50 MB)
- Only include `vosk-model-small-en-us-0.15.zip`
- Remove Filipino ZIP

**Option 2**: Manual deployment (saves 90 MB)
- Don't bundle ZIPs in app
- Use ADB to push models manually:
  ```bash
  adb push vosk-model-small-en-us-0.15 /data/user/0/com.boses.accessibility/files/vosk-models/
  ```

## 📝 Summary

1. **Download models** → `download-vosk-models.ps1`
2. **Create ZIPs** → `setup-vosk-zips.ps1`
3. **Rebuild** → `dotnet clean && dotnet build`
4. **Deploy** → Models automatically extracted on first run ✅

**Result**: Real offline speech recognition with minimal hassle! 🚀
