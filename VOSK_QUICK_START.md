# Vosk Models - Quick Start Guide

## 🚀 3-Step Setup (Easiest Method)

### Step 1: Download
```powershell
.\download-vosk-models.ps1
```

### Step 2: Create ZIPs & Copy to Resources
```powershell
.\setup-vosk-zips.ps1
```

### Step 3: Rebuild & Deploy
```powershell
dotnet clean
dotnet build -f net9.0-android
```

**Done!** Models will be automatically extracted on first run ✅

---

## 📁 What Gets Created

```
Resources/Raw/
├── vosk-model-small-en-us-0.15.zip  (40 MB) ← English
└── vosk-model-tl-ph-generic-0.6.zip  (50 MB) ← Filipino
```

On first app run, these ZIPs are extracted to:
```
Android: /data/user/0/com.boses.accessibility/files/vosk-models/
Windows: C:\Users\Full Scale\AppData\Local\Boses\vosk-models\
```

---

## ✅ Verification

### Check Debug Output

**Success**:
```
[ModelDeployer] 📦 Found vosk-model-small-en-us-0.15.zip in Resources
[ModelDeployer] 📂 Extracting vosk-model-small-en-us-0.15.zip...
[ModelDeployer] ✅ vosk-model-small-en-us-0.15 deployed successfully
[SpeechRecognition] ✅ Initialized with FREE Vosk speech recognition (offline)
[SpeechRecognition] ✅ English model found
```

**Failure**:
```
[SpeechRecognition] 🔄 Vosk models not found, using simulation
[SpeechRecognition] 💡 Place models in Resources/VoskModels folder
```

---

## 🐛 Troubleshooting

### Problem: "model not found in Resources"

**Solution**:
1. Check ZIPs exist:
   ```powershell
   ls "C:\Users\Full Scale\Desktop\product-innovation\Boses\Resources\Raw\vosk-*.zip"
   ```

2. Re-run setup:
   ```powershell
   .\setup-vosk-zips.ps1
   ```

3. Clean rebuild:
   ```powershell
   dotnet clean
   dotnet build -f net9.0-android
   ```

### Problem: App size too large

**Solution 1** - English only (saves 50 MB):
- Delete `vosk-model-tl-ph-generic-0.6.zip` from Resources/Raw
- Rebuild

**Solution 2** - Manual deployment (saves 90 MB):
- Don't bundle ZIPs
- Use ADB to push models:
  ```bash
  adb push vosk-model-small-en-us-0.15 /data/user/0/com.boses.accessibility/files/vosk-models/
  ```

---

## 📊 Comparison

| Method | App Size | Setup | Reliability |
|--------|----------|-------|-------------|
| **ZIP (Recommended)** | ~100 MB | 3 commands | ✅ Excellent |
| Folder | ~100 MB | Complex | ⚠️ Unreliable |
| Manual ADB | ~10 MB | Manual | ✅ Good |
| Download on demand | ~10 MB | Auto | ⚠️ Requires internet |

---

## 🎯 Why ZIP Method?

✅ **Simple** - Just 3 PowerShell commands
✅ **Reliable** - MAUI handles ZIPs well
✅ **Fast** - Only 2 files to bundle (not 1000+)
✅ **Cross-platform** - Works on Android, iOS, Windows
✅ **Offline** - No internet required after deployment

---

## 📝 Files Reference

| File | Purpose |
|------|---------|
| `download-vosk-models.ps1` | Downloads models from Vosk website |
| `setup-vosk-zips.ps1` | Creates ZIPs and copies to Resources |
| `EASY_VOSK_SETUP.md` | Detailed setup guide |
| `VOSK_QUICK_START.md` | This file (quick reference) |

---

## 🔗 Model Sources

- **English**: https://alphacephei.com/vosk/models/vosk-model-small-en-us-0.15.zip
- **Filipino**: https://alphacephei.com/vosk/models/vosk-model-tl-ph-generic-0.6.zip
- **All models**: https://alphacephei.com/vosk/models

---

## 💡 Pro Tips

1. **First time setup**: Run all 3 steps in order
2. **Updating models**: Delete old ZIPs, re-run setup
3. **Testing**: Check Debug output for deployment messages
4. **Production**: Consider English-only to reduce app size
5. **Offline**: Models work completely offline once deployed

---

## ✨ Result

Real offline speech recognition that:
- ✅ Works without internet
- ✅ Validates spoken phrases accurately
- ✅ Supports English and Filipino
- ✅ Deploys automatically with your app
- ✅ No manual device setup needed

**Just run the 3 commands and you're done!** 🚀
