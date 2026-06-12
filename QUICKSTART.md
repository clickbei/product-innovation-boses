# 🚀 Boses Quick Start Guide

Get Boses running in **5 minutes**!

---

## ⚡ Fast Track Setup

### Step 1: Prerequisites Check

Ensure you have:
- ✅ **.NET 9 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- ✅ **Visual Studio 2022** (17.8+) with MAUI workload

**Verify installation**:
```bash
dotnet --version
# Should show 9.0.x or higher
```

### Step 2: Open the Project

**Option A: Visual Studio**
1. Double-click `BosesApp.sln`
2. Wait for NuGet restore (automatic)
3. Select target platform (Windows/Android/iOS)
4. Press **F5** to run

**Option B: Command Line**
```bash
cd "C:\Users\Full Scale\Desktop\product-innovation\Boses"
dotnet restore
dotnet build
```

### Step 3: Run the App

**Windows (Recommended for first run)**:
```bash
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

---

## 🎮 First Use Tutorial

### 1. Launch the App
You'll see the Boses welcome screen with a large green microphone button.

### 2. Try Quick Actions (No Voice Required!)
Click any of these buttons:
- **Balance** - See mock account balance
- **Transactions** - View recent transactions
- **PWD Discount** - Calculate discount

### 3. Test Voice Commands (Simulation Mode)
Tap the **🎤 microphone button**:
- App will simulate listening
- After 2 seconds, it returns a random demo command
- Watch the AI response appear

### 4. Use Custom Commands
Click the **⚙️ settings button** to toggle simulation mode, then use the quick action buttons to test specific scenarios.

---

## 🎯 Demo Scenarios for Presentations

### Scenario 1: Balance Check (30 seconds)
1. Click **Balance** button
2. Show the response: "Ang iyong kasalukuyang balanse ay 15,750 pesos..."
3. Highlight: Natural Filipino language response

### Scenario 2: Money Transfer (1 minute)
1. Click microphone
2. Simulated input: "Ipadala ang 500 pesos kay Juan"
3. Show: Voice authentication prompt
4. Highlight: Security layer

### Scenario 3: Guardian Protection (2 minutes)
1. Click microphone
2. Simulated input: "Transfer 10000 pesos"
3. Show: Risk assessment triggers
4. Show: Guardian verification request
5. Highlight: Anti-scam protection

### Scenario 4: PWD Discount (30 seconds)
1. Click **PWD Discount** button
2. Show: 20% discount calculation
3. Highlight: Accessibility feature

---

## 🔧 Troubleshooting

### Issue: Build Errors

**Solution 1**: Clean and rebuild
```bash
dotnet clean
dotnet restore --force
dotnet build
```

**Solution 2**: Clear NuGet cache
```bash
dotnet nuget locals all --clear
dotnet restore
```

### Issue: "Database is locked"

**Solution**: Enable JSON fallback
1. Open `MauiProgram.cs`
2. Line 52: Change `var useJsonFallback = false;` to `true`
3. Rebuild

### Issue: Android Emulator Not Starting

**Solution**:
1. Open Android Device Manager in Visual Studio
2. Create a new emulator (API 21+)
3. Start emulator before running app

### Issue: Windows App Won't Run

**Solution**: Check Windows SDK
1. Visual Studio Installer
2. Modify → Individual Components
3. Install "Windows 10 SDK (10.0.19041.0)"

---

## 📱 Platform-Specific Notes

### Windows
- **Best for**: Initial development and testing
- **Microphone**: May require permission prompt
- **Database**: SQLite works perfectly

### Android
- **Best for**: Mobile demo
- **Permissions**: Auto-requested on first run
- **Emulator**: Use Pixel 5 API 33 for best results

### iOS (macOS only)
- **Best for**: Production-like testing
- **Permissions**: Microphone access required
- **Simulator**: Voice input not available (use simulation mode)

---

## 🎨 Customization Quick Tips

### Change Mock Account Balance
**File**: `Core/Network/Services/MockBrankasApiClient.cs`  
**Line**: 35-42

```csharp
_mockAccounts["ACC001"] = new BankAccount
{
    AccountId = "ACC001",
    AccountNumber = "1234567890",
    BankName = "UnionBank",
    AccountType = "SAVINGS",
    Balance = 15750.50m  // ← Change this
};
```

### Add New Voice Responses
**File**: `Core/Services/AiOrchestratorService.cs`  
**Method**: `ProcessCommandSimulatedAsync`

```csharp
// Add your custom command
if (input.Contains("your keyword"))
{
    return "Your custom response in Filipino";
}
```

### Modify Guardian Risk Threshold
**File**: `Modules/Plugins/GuardianPlugin.cs`  
**Line**: 45

```csharp
// Require guardian for amounts over 5000 PHP
return amount.HasValue && amount.Value > 5000;  // ← Change threshold
```

---

## 📊 Performance Tips

### Faster Startup
1. Use Release configuration for demos
2. Pre-warm emulator before presentation
3. Keep app running in background

### Smoother Animations
1. Enable hardware acceleration (Android)
2. Use physical device instead of emulator
3. Close other apps during demo

---

## 🎤 Presentation Checklist

**Before Your Demo**:
- [ ] App builds without errors
- [ ] Tested all quick action buttons
- [ ] Verified simulation mode works
- [ ] Prepared backup slides (if app fails)
- [ ] Charged device/laptop fully
- [ ] Closed unnecessary applications

**During Demo**:
- [ ] Start with quick actions (most reliable)
- [ ] Explain simulation mode upfront
- [ ] Show conversation history
- [ ] Highlight guardian protection
- [ ] Mention production roadmap

**Talking Points**:
1. "Voice-first design for elderly and PWD users"
2. "Bilingual support: Tagalog and English"
3. "Guardian anti-scam protection for vulnerable users"
4. "Production-ready architecture with fallback mechanisms"
5. "Open Banking integration ready (simulated for demo)"

---

## 🆘 Emergency Fallback Plan

If the app crashes during demo:

### Plan A: Use Quick Actions Only
- Avoid microphone button
- Click Balance, Transactions, PWD Discount buttons
- These are most stable

### Plan B: Show Code Architecture
- Open `ARCHITECTURE.md`
- Walk through the clean architecture diagram
- Explain MVVM pattern and plugins

### Plan C: Explain Simulation Mode
- "This demonstrates production architecture"
- "Real integrations: Deepgram, Google Gemini, Brankas"
- Show `MockBrankasApiClient.cs` code

---

## 📞 Quick Reference Commands

### Build Commands
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run on Windows
dotnet build -t:Run -f net9.0-windows10.0.19041.0

# Run on Android
dotnet build -t:Run -f net9.0-android

# Clean build
dotnet clean
```

### Visual Studio Shortcuts
- **F5**: Start debugging
- **Ctrl+Shift+B**: Build solution
- **Ctrl+F5**: Run without debugging
- **Shift+F5**: Stop debugging

---

## 🎓 Next Steps

After getting the app running:

1. **Read**: `README.md` for full documentation
2. **Study**: `ARCHITECTURE.md` for design patterns
3. **Explore**: Code comments for implementation details
4. **Customize**: Mock data and responses
5. **Extend**: Add new plugins and features

---

## ✅ Success Indicators

You're ready to demo when:
- ✅ App launches without errors
- ✅ Quick action buttons work
- ✅ Conversation history displays
- ✅ Simulation mode toggles
- ✅ UI is responsive and smooth

---

**You're all set! Time to showcase Boses! 🎉**

For detailed documentation, see `README.md` and `ARCHITECTURE.md`.
