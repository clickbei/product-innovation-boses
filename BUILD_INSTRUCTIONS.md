# 🔧 Boses Build Instructions (Updated for .NET 9)

## ✅ Issues Fixed

All compilation errors have been resolved:
- ✅ Updated to .NET 9 MAUI (from .NET 8 - out of support)
- ✅ Fixed Android namespace issues
- ✅ Fixed iOS/MacCatalyst namespace issues
- ✅ Fixed file-scoped namespace conflicts
- ✅ Updated all package versions to .NET 9
- ✅ Fixed platform version compatibility

---

## 📋 Prerequisites

### Required Software

1. **.NET 9 SDK** (Required)
   - Download: https://dotnet.microsoft.com/download/dotnet/9.0
   - Verify: `dotnet --version` (should show 9.0.x)

2. **Visual Studio 2022** (17.11 or later)
   - Download: https://visualstudio.microsoft.com/downloads/
   - Required Workload: ".NET Multi-platform App UI development"

### Installing .NET MAUI Workload

If you haven't installed the MAUI workload yet:

```bash
dotnet workload install maui
```

---

## 🚀 Building the Project

### Method 1: Visual Studio (Recommended)

1. **Open the Solution**
   ```
   Double-click: BosesApp.sln
   ```

2. **Restore NuGet Packages**
   - Visual Studio will automatically restore packages
   - Or manually: Right-click solution → Restore NuGet Packages

3. **Select Target Platform**
   - Windows: `net9.0-windows10.0.19041.0`
   - Android: `net9.0-android`
   - iOS: `net9.0-ios` (macOS only)
   - MacCatalyst: `net9.0-maccatalyst` (macOS only)

4. **Build**
   - Press `Ctrl+Shift+B` or
   - Build → Build Solution

5. **Run**
   - Press `F5` (Debug) or `Ctrl+F5` (Without Debug)

---

### Method 2: Command Line

#### Windows (Recommended for First Build)

```bash
# Navigate to project directory
cd "C:\Users\Full Scale\Desktop\product-innovation\Boses"

# Restore packages
dotnet restore

# Build for Windows
dotnet build -f net9.0-windows10.0.19041.0

# Run on Windows
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

#### Android

```bash
# Build for Android
dotnet build -f net9.0-android

# Run on Android emulator (must be started first)
dotnet build -t:Run -f net9.0-android
```

#### iOS (macOS only)

```bash
# Build for iOS
dotnet build -f net9.0-ios

# Run on iOS simulator
dotnet build -t:Run -f net9.0-ios
```

#### MacCatalyst (macOS only)

```bash
# Build for MacCatalyst
dotnet build -f net9.0-maccatalyst

# Run on Mac
dotnet build -t:Run -f net9.0-maccatalyst
```

---

## 🔍 Verification Script

Run the PowerShell verification script:

```powershell
.\build-verify.ps1
```

This will:
- ✅ Check .NET SDK version
- ✅ Verify project files
- ✅ Restore NuGet packages
- ✅ Build the project
- ✅ Report any errors

---

## 🐛 Troubleshooting

### Issue: "Workload 'maui' not found"

**Solution:**
```bash
dotnet workload install maui
```

### Issue: ".NET 9 SDK not found"

**Solution:**
1. Download .NET 9 SDK: https://dotnet.microsoft.com/download/dotnet/9.0
2. Install the SDK
3. Restart your terminal/IDE
4. Verify: `dotnet --version`

### Issue: "Android SDK not found"

**Solution:**
1. Open Visual Studio Installer
2. Modify → Individual Components
3. Install "Android SDK setup"
4. Restart Visual Studio

### Issue: NuGet Package Restore Fails

**Solution:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore with force
dotnet restore --force

# Rebuild
dotnet build
```

### Issue: SQLite Database Errors

**Solution:**
1. Open `MauiProgram.cs`
2. Find line 64: `var useJsonFallback = false;`
3. Change to: `var useJsonFallback = true;`
4. Rebuild

---

## 📦 Package Versions (Updated)

All packages have been updated to .NET 9 compatible versions:

```xml
<PackageReference Include="Microsoft.Maui.Controls" Version="9.0.0" />
<PackageReference Include="Microsoft.Maui.Controls.Compatibility" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="9.0.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />
<PackageReference Include="Microsoft.SemanticKernel" Version="1.20.0" />
<PackageReference Include="System.Text.Json" Version="9.0.0" />
```

---

## 🎯 Platform-Specific Notes

### Windows
- **Minimum Version**: Windows 10 version 1809 (Build 17763)
- **Recommended**: Windows 11
- **Best for**: Development and testing

### Android
- **Minimum API**: 21 (Android 5.0)
- **Target API**: 34 (Android 14)
- **Emulator**: Pixel 5 API 34 recommended

### iOS
- **Minimum Version**: iOS 14.2
- **Requires**: macOS with Xcode
- **Simulator**: iPhone 14 or later

### MacCatalyst
- **Minimum Version**: macOS 14.0
- **Requires**: macOS with Xcode
- **Best for**: Mac desktop apps

---

## ✅ Build Success Indicators

You'll know the build succeeded when you see:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## 🚀 Running the App

### First Launch

1. **Windows**: App opens in a new window
2. **Android**: App deploys to emulator/device
3. **iOS**: App opens in simulator
4. **Mac**: App opens as native Mac app

### Testing Features

Once running:
1. Click **Balance** button → See mock balance
2. Click **Transactions** button → View history
3. Click **PWD Discount** button → Calculate discount
4. Tap 🎤 microphone → Test voice simulation
5. Click ⚙️ settings → Toggle simulation mode

---

## 📊 Build Performance

Expected build times (first build):
- **Windows**: 2-3 minutes
- **Android**: 3-5 minutes
- **iOS**: 4-6 minutes
- **MacCatalyst**: 3-5 minutes

Subsequent builds: 30-60 seconds

---

## 🔄 Clean Build

If you encounter persistent issues:

```bash
# Clean the solution
dotnet clean

# Remove bin and obj folders
Remove-Item -Recurse -Force bin, obj

# Clear NuGet cache
dotnet nuget locals all --clear

# Restore and rebuild
dotnet restore
dotnet build
```

---

## 📞 Support

If you continue to have issues:

1. Check [QUICKSTART.md](QUICKSTART.md) - Troubleshooting section
2. Verify .NET 9 SDK is installed: `dotnet --version`
3. Ensure MAUI workload is installed: `dotnet workload list`
4. Check Visual Studio has MAUI workload installed

---

## ✅ Quick Checklist

Before building, ensure:
- [ ] .NET 9 SDK installed (`dotnet --version` shows 9.0.x)
- [ ] MAUI workload installed (`dotnet workload list` shows maui)
- [ ] Visual Studio 2022 (17.11+) with MAUI workload
- [ ] For Android: Android SDK installed
- [ ] For iOS/Mac: Xcode installed (macOS only)

---

**All issues have been fixed! The project should now build successfully.** 🎉

Run `.\build-verify.ps1` to verify everything is working correctly.
