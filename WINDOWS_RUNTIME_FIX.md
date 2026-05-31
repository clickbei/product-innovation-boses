# 🔧 Windows App Runtime Fix

## Issue: "Class not registered (0x80040154)"

This error occurs when the Windows App Runtime components are not properly installed on your system.

---

## ✅ Solution 1: Self-Contained Deployment (Recommended - Already Applied)

I've updated the project to use **self-contained deployment**, which bundles all required runtime components with your app. This means you don't need to install anything separately.

**Changes made to `BosesApp.csproj`:**
```xml
<WindowsPackageType>None</WindowsPackageType>
<WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
<PublishSelfContained>true</PublishSelfContained>
```

**Now rebuild:**
```bash
dotnet clean
dotnet restore
dotnet build -f net9.0-windows10.0.19041.0
```

---

## ✅ Solution 2: Install Windows App Runtime (Alternative)

If Solution 1 doesn't work, install the Windows App Runtime manually:

### Option A: Using winget (Recommended)
```powershell
winget install Microsoft.WindowsAppRuntime.1.6
```

### Option B: Direct Download
1. Download: https://aka.ms/windowsappsdk/1.6/latest/windowsappruntimeinstall-x64.exe
2. Run the installer
3. Restart your computer
4. Rebuild the project

---

## ✅ Solution 3: Use Unpackaged Mode (Simplest)

If you just want to run the app for development/testing:

### Step 1: Clean and Restore
```bash
cd "C:\Users\Full Scale\Desktop\product-innovation\Boses"
dotnet clean
dotnet restore --force
```

### Step 2: Build with Self-Contained
```bash
dotnet build -f net9.0-windows10.0.19041.0 /p:WindowsAppSDKSelfContained=true
```

### Step 3: Run
```bash
dotnet run --framework net9.0-windows10.0.19041.0
```

---

## 🔍 Verify Windows App SDK Installation

Check if Windows App SDK is installed:

```powershell
# Check installed packages
winget list Microsoft.WindowsAppRuntime

# Or check in PowerShell
Get-AppxPackage -Name "*WindowsAppRuntime*"
```

---

## 🎯 Quick Fix Commands

Run these commands in order:

```powershell
# Navigate to project
cd "C:\Users\Full Scale\Desktop\product-innovation\Boses"

# Clean everything
dotnet clean
Remove-Item -Recurse -Force bin, obj -ErrorAction SilentlyContinue

# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore --force

# Build with self-contained runtime
dotnet build -f net9.0-windows10.0.19041.0 /p:WindowsAppSDKSelfContained=true /p:PublishSelfContained=true

# Run the app
dotnet run --framework net9.0-windows10.0.19041.0
```

---

## 🐛 Still Having Issues?

### Check Your Windows Version

The app requires **Windows 10 version 1809 (Build 17763) or later**.

Check your version:
```powershell
winver
```

If you're on an older version, update Windows or increase the minimum version in the project file.

### Try Debug Mode

Run in debug mode to see more detailed error messages:

```bash
dotnet run --framework net9.0-windows10.0.19041.0 --configuration Debug
```

---

## 📋 Alternative: Target .NET MAUI Without WinUI3

If Windows App SDK continues to cause issues, we can switch to a simpler Windows target:

### Edit BosesApp.csproj

Change the Windows target framework from:
```xml
<TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">$(TargetFrameworks);net9.0-windows10.0.19041.0</TargetFrameworks>
```

To:
```xml
<!-- Temporarily disable Windows target for testing -->
<!-- <TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">$(TargetFrameworks);net9.0-windows10.0.19041.0</TargetFrameworks> -->
```

Then build for Android instead:
```bash
dotnet build -f net9.0-android
```

---

## ✅ Expected Result

After applying the fix, you should see:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:XX.XX
```

And the app should launch without the "Class not registered" error.

---

## 🎯 Recommended Approach

**For Development/Testing:**
1. Use the self-contained deployment (already applied)
2. Run: `dotnet run --framework net9.0-windows10.0.19041.0`

**For Distribution:**
1. Install Windows App Runtime on target machines
2. Or publish as self-contained: `dotnet publish -f net9.0-windows10.0.19041.0 -c Release`

---

## 📞 Additional Help

If the error persists:

1. **Check Event Viewer** for more details:
   - Open Event Viewer
   - Windows Logs → Application
   - Look for errors from your app

2. **Check Visual Studio Output** for detailed error messages

3. **Try Visual Studio** instead of command line:
   - Open `BosesApp.sln` in Visual Studio
   - Set Windows as startup project
   - Press F5

---

**The project has been updated with self-contained deployment. Try rebuilding now!** 🚀
