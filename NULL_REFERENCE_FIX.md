# 🔧 NullReferenceException Fix

## ❌ Error

```
System.NullReferenceException
HResult=0x80004003
Message=Object reference not set to an instance of an object.
```

---

## 🔍 Root Causes Identified

### **1. Shell Navigation Issue**
**Problem:** `Shell.Current` is null because the app uses `NavigationPage`, not `Shell`

**Location:** `LanguageSelectionViewModel.cs` line 70
```csharp
await Shell.Current.GoToAsync(nameof(OnboardingPage)); // Shell.Current is null!
```

**Fix:** Use NavigationPage navigation instead
```csharp
if (Application.Current?.MainPage is NavigationPage navPage)
{
    var onboardingPage = Application.Current.Handler?.MauiContext?.Services
        .GetService<OnboardingPage>();
    if (onboardingPage != null)
    {
        await navPage.PushAsync(onboardingPage);
    }
}
```

### **2. Database Schema Mismatch**
**Problem:** Added `PreferredLanguage` field to `UserProfile` but database wasn't updated

**Solution:** Delete old database to recreate with new schema

### **3. Service Resolution Issues**
**Problem:** Services might not be properly registered or resolved

**Solution:** Added null checks and error handling

---

## ✅ Fixes Applied

### **Fix 1: Navigation**

Updated `LanguageSelectionViewModel.cs`:
- Replaced Shell navigation with NavigationPage navigation
- Added null checks for Application.Current
- Added null checks for MainPage
- Added null checks for service resolution
- Added try-catch for error handling

### **Fix 2: Error Handling**

Added comprehensive error handling:
```csharp
try
{
    // Navigation code
}
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
}
```

### **Fix 3: Initialization**

Added error handling to InitializeAsync:
```csharp
try
{
    await _voiceService.SpeakAsync("Welcome...");
}
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine($"Initialization error: {ex.Message}");
}
```

---

## 🚀 How to Fix

### **Step 1: Delete Old Database**

The database needs to be recreated with the new schema (PreferredLanguage field).

**Windows:**
```powershell
# Delete the old database
Remove-Item "$env:LOCALAPPDATA\Boses\boses.db" -ErrorAction SilentlyContinue

# Or manually delete:
# C:\Users\[YourName]\AppData\Local\Boses\boses.db
```

**Android:**
```bash
# Uninstall and reinstall the app
# Or clear app data in Settings
```

### **Step 2: Clean and Rebuild**

```bash
cd "C:\Users\Full Scale\Desktop\product-innovation\Boses"

# Clean everything
dotnet clean
Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue

# Restore packages
dotnet restore

# Rebuild
dotnet build -f net9.0-windows10.0.19041.0
```

### **Step 3: Run**

```bash
dotnet run --framework net9.0-windows10.0.19041.0
```

---

## 🧪 Testing

### **Test 1: Language Selection**

```bash
dotnet run --framework net9.0-windows10.0.19041.0
```

**Expected:**
1. ✅ Language selection page appears
2. ✅ Can select English or Tagalog
3. ✅ Hear voice confirmation
4. ✅ Click "Continue" navigates to onboarding
5. ✅ No NullReferenceException

### **Test 2: Database Creation**

After first run, check if database was created:

**Windows:**
```powershell
Test-Path "$env:LOCALAPPDATA\Boses\boses.db"
# Should return: True
```

### **Test 3: Navigation**

1. Select language
2. Click Continue
3. Should navigate to onboarding page
4. No errors in debug output

---

## 🔍 Debugging

### **Check Debug Output**

Look for these messages:
```
[TTS] Speaking: Welcome to Boses...
[Navigation] Navigating to OnboardingPage
[Database] Database created successfully
```

### **Check for Errors**

Look for these error messages:
```
Navigation error: [error message]
Initialization error: [error message]
```

### **Enable Verbose Logging**

Add to `MauiProgram.cs`:
```csharp
#if DEBUG
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif
```

---

## 📊 Common NullReferenceException Sources

### **1. Shell.Current**
```csharp
// ❌ Wrong (Shell.Current is null)
await Shell.Current.GoToAsync(nameof(Page));

// ✅ Correct (Use NavigationPage)
if (Application.Current?.MainPage is NavigationPage navPage)
{
    await navPage.PushAsync(page);
}
```

### **2. Service Resolution**
```csharp
// ❌ Wrong (might be null)
var service = services.GetService<IService>();
service.DoSomething(); // NullReferenceException!

// ✅ Correct (null check)
var service = services.GetService<IService>();
if (service != null)
{
    service.DoSomething();
}
```

### **3. Application.Current**
```csharp
// ❌ Wrong (might be null)
var mainPage = Application.Current.MainPage;

// ✅ Correct (null-conditional)
var mainPage = Application.Current?.MainPage;
```

### **4. Database Context**
```csharp
// ❌ Wrong (DbContext might not be initialized)
var users = _dbContext.UserProfiles.ToList();

// ✅ Correct (null check)
if (_dbContext != null)
{
    var users = _dbContext.UserProfiles.ToList();
}
```

---

## 🛠️ Additional Fixes

### **Fix: Remove AppShell (Not Used)**

Since we're using NavigationPage, we don't need AppShell:

**Option 1: Delete AppShell files**
```bash
Remove-Item "AppShell.xaml"
Remove-Item "AppShell.xaml.cs"
```

**Option 2: Keep but don't use**
- Leave files in place
- App.xaml.cs already uses NavigationPage
- No changes needed

### **Fix: Database Migration Script**

Create a script to handle database updates:

```powershell
# reset-database.ps1
$dbPath = "$env:LOCALAPPDATA\Boses\boses.db"

if (Test-Path $dbPath) {
    Write-Host "Deleting old database..." -ForegroundColor Yellow
    Remove-Item $dbPath -Force
    Write-Host "✅ Database deleted" -ForegroundColor Green
} else {
    Write-Host "No database found (first run)" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Database will be recreated on next app launch" -ForegroundColor Green
```

---

## ✅ Verification Checklist

After applying fixes:

- [ ] App launches without errors
- [ ] Language selection page appears
- [ ] Can select English/Tagalog
- [ ] Voice confirmation works
- [ ] Continue button navigates to onboarding
- [ ] Database is created
- [ ] No NullReferenceException in debug output

---

## 🎯 Quick Fix Script

```powershell
# quick-fix.ps1
Write-Host "🔧 Fixing NullReferenceException..." -ForegroundColor Cyan
Write-Host ""

$projectPath = "C:\Users\Full Scale\Desktop\product-innovation\Boses"
Set-Location $projectPath

# Step 1: Delete old database
Write-Host "1️⃣ Deleting old database..." -ForegroundColor Yellow
$dbPath = "$env:LOCALAPPDATA\Boses\boses.db"
if (Test-Path $dbPath) {
    Remove-Item $dbPath -Force
    Write-Host "✅ Database deleted" -ForegroundColor Green
} else {
    Write-Host "✅ No old database found" -ForegroundColor Green
}
Write-Host ""

# Step 2: Clean solution
Write-Host "2️⃣ Cleaning solution..." -ForegroundColor Yellow
dotnet clean
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "✅ Solution cleaned" -ForegroundColor Green
Write-Host ""

# Step 3: Restore packages
Write-Host "3️⃣ Restoring packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Packages restored" -ForegroundColor Green
} else {
    Write-Host "❌ Restore failed" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 4: Build
Write-Host "4️⃣ Building..." -ForegroundColor Yellow
dotnet build -f net9.0-windows10.0.19041.0
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 5: Run
Write-Host "5️⃣ Starting app..." -ForegroundColor Yellow
Write-Host ""
dotnet run --framework net9.0-windows10.0.19041.0
```

---

## 📝 Summary

**What Caused the Error:**
1. ❌ Shell.Current was null (app uses NavigationPage)
2. ❌ Database schema mismatch (new PreferredLanguage field)
3. ❌ Missing null checks

**What Was Fixed:**
1. ✅ Replaced Shell navigation with NavigationPage navigation
2. ✅ Added comprehensive null checks
3. ✅ Added error handling
4. ✅ Database will be recreated with new schema

**How to Fix:**
1. Delete old database
2. Clean and rebuild
3. Run app

---

**Run the quick-fix script or follow the manual steps above!** 🔧✨
