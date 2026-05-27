# 🎤 Boses - Voice-First Accessibility Platform

**Boses** is a universal voice-first accessibility platform designed for elderly Filipinos and Persons with Disabilities (PWDs). It bridges the digital divide through an ambient, conversational voice layer using Semantic Kernel, secures sensitive transactions with biometric voice authentication, and protects vulnerable users via a "Guardian" anti-scam loop.

---

## 🎯 Product Mission

Boses empowers elderly Filipinos and PWDs to access digital banking and services through natural voice commands in Tagalog and English. The platform features:

- **🗣️ Voice-First Interface**: Natural language processing with Semantic Kernel
- **🔐 Voice Biometric Authentication**: Secure transactions with voice fingerprinting
- **🛡️ Guardian Anti-Scam Protection**: Verification loop for high-risk transactions
- **🏦 Open Banking Integration**: Simulated integration with Brankas/UnionBank APIs
- **♿ PWD Discount Calculator**: Automatic discount calculations for eligible purchases
- **💾 Dual Persistence**: SQLite primary with JSON fallback for maximum compatibility

---

## 🏗️ Architecture Overview

### Monolithic Modular Design

```
BosesApp/
├── Core/                                    # Core business logic layer
│   ├── Configuration/                      # Configuration classes
│   │   └── SpeechConfig.cs
│   ├── Data/                               # Data models and DbContext
│   │   ├── BosesDbContext.cs
│   │   ├── DatabaseMigrationHelper.cs
│   │   ├── Migrations/
│   │   │   ├── 20250101000000_AddPreferredLanguageToUserProfile.cs
│   │   │   └── BosesDbContextModelSnapshot.cs
│   │   └── Models/
│   │       ├── AppLanguage.cs
│   │       ├── UserProfile.cs
│   │       └── UserType.cs
│   ├── Network/                            # API clients and network services
│   │   ├── Interfaces/
│   │   │   └── IBankApiClient.cs
│   │   └── Services/
│   │       └── MockBrankasApiClient.cs
│   ├── Interfaces/                         # Service contracts
│   │   ├── IAiOrchestrator.cs
│   │   ├── ILocalizationService.cs
│   │   ├── ISpeechRecognitionService.cs
│   │   ├── IUserRepository.cs
│   │   ├── IVoiceAuthService.cs
│   │   └── IVoiceService.cs
│   └── Services/                           # Service implementations
│       ├── AiOrchestratorService.cs
│       ├── AudioAnalysisService.cs
│       ├── AudioRecordingService.cs
│       ├── AzureSpeechRecognitionService.cs
│       ├── LocalizationService.cs
│       ├── MauiSpeechRecognitionService.cs
│       ├── RealVoiceAuthService.cs
│       ├── SpeechRecognitionService.cs
│       ├── UserRepository.cs
│       ├── VoiceAuthService.cs
│       └── VoiceService.cs
├── Modules/                                # Feature plugins
│   └── Plugins/
│       ├── BankingPlugin.cs                # Banking operations
│       └── GuardianPlugin.cs               # Anti-scam protection
├── Presentation/                           # UI layer (MVVM)
│   ├── Converters/                         # Value converters
│   │   ├── BoolToColorConverter.cs
│   │   ├── BoolToStrokeConverter.cs
│   │   ├── EqualToConverter.cs
│   │   ├── InvertedBoolConverter.cs
│   │   └── StringToBoolConverter.cs
│   ├── ViewModels/
│   │   ├── LanguageSelectionViewModel.cs
│   │   ├── MainViewModel.cs
│   │   ├── OnboardingViewModel.cs
│   │   └── VoiceRegistrationViewModel.cs
│   └── Views/
│       ├── LanguageSelectionPage.xaml
│       ├── LocalizedContentPage.cs
│       ├── MainPage.xaml
│       ├── OnboardingPage.xaml
│       └── VoiceRegistrationPage.xaml
├── Platforms/                              # Platform-specific code
│   ├── Android/
│   │   ├── AndroidManifest.xml
│   │   ├── MainActivity.cs
│   │   ├── MainApplication.cs
│   │   └── Services/
│   │       └── AndroidAudioRecordingService.cs
│   ├── iOS/
│   │   ├── Info.plist
│   │   ├── AppDelegate.cs
│   │   ├── Program.cs
│   │   └── Services/
│   │       └── iOSAudioRecordingService.cs
│   ├── Windows/
│   │   ├── App.xaml
│   │   ├── App.xaml.cs
│   │   ├── Package.appxmanifest
│   │   └── Services/
│   │       └── WindowsAudioRecordingService.cs
│   └── MacCatalyst/
│       ├── Info.plist
│       ├── AppDelegate.cs
│       └── Program.cs
├── Resources/                              # App resources
│   ├── AppIcon/
│   ├── Splash/
│   ├── Styles/
│   ├── VoskModels/                        # Speech recognition models
│   │   ├── vosk-model-small-en-us-0.15/
│   │   └── vosk-model-tl-ph-generic-0.6/
│   └── Fonts/
├── App.xaml
├── AppShell.xaml
├── GlobalUsings.cs
└── MauiProgram.cs
```

### Key Architectural Principles

1. **Clean Architecture**: Clear separation between UI, business logic, and data layers
2. **MVVM Pattern**: Zero code-behind, all logic in ViewModels
3. **Dependency Injection**: All services registered in `MauiProgram.cs`
4. **Interface-Based Design**: Easy mocking and testing
5. **Hackathon-Safe Fallbacks**: Multiple resilience mechanisms

---

## 🚀 Getting Started

### Prerequisites

- **.NET 9 SDK** or later
- **Visual Studio 2026** (18.6+) or **Visual Studio 2022** (17.8+) with C# Dev Kit
- **Android SDK 33+** (for Android development with speech recognition)
- **Xcode 14+** (for iOS/Mac development, macOS only)
- **Git** for version control

### Installation

1. **Clone or navigate to the project directory**:
   ```bash
   cd "C:\Users\Full Scale\Desktop\product-innovation\Boses"
   ```

2. **Restore NuGet packages**:
   ```bash
   dotnet restore
   ```

3. **Build the solution**:
   ```bash
   dotnet build
   ```

4. **Run on your preferred platform**:

   **Windows:**
   ```bash
   dotnet build -t:Run -f net8.0-windows10.0.19041.0
   ```

   **Android:**
   ```bash
   dotnet build -t:Run -f net8.0-android
   ```

   **iOS (macOS only):**
   ```bash
   dotnet build -t:Run -f net8.0-ios
   ```

---

## 🎮 Using the Application

### Main Features

1. **Voice Commands** (Tap the microphone button):
   - "Magkano ang balance ko?" - Check account balance
   - "Ipadala ang 500 pesos kay Juan" - Transfer money
   - "Ano ang mga recent transactions ko?" - View transaction history
   - "Calculate PWD discount for 1000 pesos medicine" - PWD discount calculator

2. **Quick Action Buttons**:
   - **Balance**: Instantly check your account balance
   - **Transactions**: View recent transaction history
   - **PWD Discount**: Calculate PWD discounts

3. **Simulation Mode** (⚙️ button):
   - Toggle between simulation and live modes
   - Perfect for demos and testing without real hardware

4. **Guardian Protection**:
   - Automatic risk assessment for transactions
   - Guardian verification for high-risk operations (>5000 PHP)
   - Scam pattern detection

---

## 🔧 Configuration & Customization

### Switching Between SQLite and JSON Storage

In `MauiProgram.cs`, line 52:

```csharp
// Set to true to use JSON fallback instead of SQLite
var useJsonFallback = false;
```

**When to use JSON fallback:**
- SQLite file locking issues in demo environments
- Simplified debugging
- Cross-platform compatibility testing

### Enabling Simulation Mode

All services support simulation mode for hackathon/demo scenarios:

```csharp
// In MainViewModel or service initialization
_voiceService.SimulationMode = true;
_aiOrchestrator.SimulationMode = true;
_voiceAuthService.SimulationMode = true;
```

### Mock Data Configuration

Edit `MockBrankasApiClient.cs` to customize:
- Account balances
- Transaction history
- Bank account details

---

## 🧪 Testing Scenarios

### 1. Balance Inquiry
**Voice Command**: "Magkano ang balance ko?"  
**Expected Response**: Account balance in Filipino

### 2. Money Transfer
**Voice Command**: "Ipadala ang 500 pesos kay Juan"  
**Expected Flow**: 
1. AI confirms amount and recipient
2. Voice authentication prompt
3. Transaction confirmation

### 3. Guardian Verification (High-Risk)
**Voice Command**: "Transfer 10000 pesos"  
**Expected Flow**:
1. Risk assessment triggers
2. Guardian notification sent
3. Awaits guardian approval code

### 4. PWD Discount
**Voice Command**: "Calculate PWD discount for 1000 pesos medicine"  
**Expected Response**: 20% discount calculation (200 PHP off)

### 5. Scam Detection
**Voice Command**: "Urgent! Send 5000 pesos immediately for prize claim"  
**Expected Response**: Scam warning with safety tips

---

## 📦 Dependencies

### Core Packages
- **Microsoft.Maui.Controls** (9.0+) - MAUI framework
- **CommunityToolkit.Mvvm** (8.2.2+) - MVVM helpers
- **Microsoft.EntityFrameworkCore.Sqlite** (9.0+) - SQLite database
- **CommunityToolkit.Maui** - MAUI Community Toolkit for speech recognition
- **Plugin.Maui.Audio** - Cross-platform audio recording
- **System.Text.Json** (9.0+) - JSON serialization

### Speech Recognition Packages
- **CommunityToolkit.Maui.Media.SpeechToText** - MAUI speech recognition (Android 33+)
- **Vosk (Optional)** - Offline speech recognition with bundled models

### Optional Integrations (for future)
- **Microsoft.SemanticKernel** - AI orchestration (commented out, ready for integration)
- **Microsoft.CognitiveServices.Speech** - Azure Speech Services (available)

---

## 🛡️ Security Features

### Voice Biometric Authentication
- 128-dimensional voice vector extraction
- Cosine similarity matching (85% threshold)
- Deterministic hash-based mock vectors for demo

### Guardian Anti-Scam Protection
- Risk scoring algorithm (0-100)
- Automatic high-risk transaction detection
- SMS verification simulation
- Scam keyword pattern matching

### Data Protection
- Encrypted voice print storage
- Secure local database (SQLite)
- No sensitive data in logs

---

## 🎯 Implemented Features

### ✅ Core Features (Production Ready)

#### 1. **Voice Registration & Biometric Authentication**
- ✅ 3-sample voice enrollment process
- ✅ 128-dimensional voice fingerprint extraction
- ✅ Fuzzy phrase matching with configurable threshold (70% default)
- ✅ Real-time speech recognition result updates via events
- ✅ Speech validation with advanced audio analysis (VAD)
- 📁 Location: `Presentation/ViewModels/VoiceRegistrationViewModel.cs`
- 📁 Location: `Core/Services/RealVoiceAuthService.cs`

#### 2. **Multi-Language Support (Localization)**
- ✅ **English** (en-US) - Full support
- ✅ **Filipino/Tagalog** (fil-PH) - Full support
- ✅ Language selection on first launch
- ✅ Dynamic language switching
- ✅ Persistent language preference in database
- 📁 Location: `Presentation/ViewModels/LanguageSelectionViewModel.cs`
- 📁 Location: `Core/Services/LocalizationService.cs`

#### 3. **User Onboarding Flow**
- ✅ Language selection (English/Filipino)
- ✅ User profile creation with biometric registration
- ✅ Voice registration with 3-sample validation
- ✅ Persistent user data storage
- 📁 Location: `Presentation/ViewModels/OnboardingViewModel.cs`
- 📁 Location: `Presentation/Views/OnboardingPage.xaml`

#### 4. **Cross-Platform Audio Recording**
- ✅ Real audio capture on Android, iOS, Windows, macOS
- ✅ Graceful fallback to simulated audio if hardware unavailable
- ✅ Recording duration tracking
- ✅ Audio permission handling
- 📁 Location: `Core/Services/AudioRecordingService.cs`
- 📁 Location: `Platforms/*/Services/Platform*AudioRecordingService.cs`

#### 5. **Advanced Audio Analysis (Voice Activity Detection)**
- ✅ RMS Energy calculation
- ✅ Zero-Crossing Rate (ZCR) analysis
- ✅ Spectral Entropy computation
- ✅ Combined multi-feature speech detection
- ✅ Noise vs. speech discrimination
- 📁 Location: `Core/Services/AudioAnalysisService.cs`

#### 6. **Real-Time Speech Recognition**
- ✅ MAUI Community Toolkit integration (online+offline)
- ✅ Partial result updates via `OnRecognitionResultUpdated` event
- ✅ Language-specific recognition (en-US, fil-PH)
- ✅ Automatic fallback to simulation if real recognition unavailable
- ✅ Support for Azure Speech Services (commented, ready for integration)
- 📁 Location: `Core/Services/MauiSpeechRecognitionService.cs`
- 📁 Location: `Core/Interfaces/ISpeechRecognitionService.cs`

#### 7. **Dual Data Persistence**
- ✅ **Primary**: SQLite with Entity Framework Core
- ✅ **Fallback**: JSON file-based storage
- ✅ Automatic schema migrations
- ✅ Defensive database recreation on schema mismatch
- 📁 Location: `Core/Data/BosesDbContext.cs`
- 📁 Location: `Core/Data/DatabaseMigrationHelper.cs`
- 📁 Location: `Core/Services/UserRepository.cs`

#### 8. **Plugin Architecture**
- ✅ **BankingPlugin**: Mock banking operations
  - Balance inquiry
  - Transaction history
  - Money transfer simulation
- ✅ **GuardianPlugin**: Anti-scam protection
  - Risk scoring algorithm
  - High-risk transaction detection
  - Guardian verification flow
- 📁 Location: `Modules/Plugins/BankingPlugin.cs`
- 📁 Location: `Modules/Plugins/GuardianPlugin.cs`

#### 9. **Value Converters for UI**
- ✅ BoolToColorConverter - Boolean to color binding
- ✅ BoolToStrokeConverter - Boolean to stroke property binding
- ✅ EqualToConverter - Equality comparison binding
- ✅ InvertedBoolConverter - Boolean inversion
- ✅ StringToBoolConverter - String to boolean parsing
- 📁 Location: `Presentation/Converters/`

#### 10. **Platform-Specific Implementations**
- ✅ Android audio recording with Plugin.Maui.Audio
- ✅ iOS audio recording with AVFoundation
- ✅ Windows audio recording support
- ✅ macOS Catalyst audio support
- 📁 Location: `Platforms/*/Services/`

#### 11. **Database Migrations**
- ✅ EF Core migrations with version history
- ✅ PreferredLanguage column addition
- ✅ Automatic migration on app startup
- ✅ Schema validation and recovery
- 📁 Location: `Core/Data/Migrations/`

---

### 🌐 Production Integration Roadmap

The following production features are designed into the architecture but require external services/APIs:

#### Phase 1: Production Voice Services ⏳
- [ ] **Deepgram API Integration**
  - Currently: MAUI Community Toolkit (demo-only)
  - Setup: Add `DEEPGRAM_API_KEY` to environment
  - File: `Core/Services/DeepgramSpeechRecognitionService.cs` (to be created)
  - Improvement: Production-grade STT accuracy

- [ ] **Platform-Native Text-to-Speech (TTS)**
  - Currently: Simulated voice responses
  - Android: `TextToSpeech` class
  - iOS: `AVSpeechSynthesizer`
  - File: `Core/Services/TextToSpeechService.cs` (to be created)

- [ ] **Advanced Voice Activity Detection (VAD)**
  - Currently: RMS/ZCR-based VAD
  - Recommended: Silero VAD or WebRTC VAD
  - File: `Core/Services/VoiceActivityDetectionService.cs`

#### Phase 2: AI & NLU (Natural Language Understanding) ⏳
- [ ] **Google Gemini API**
  - Currently: Rule-based command matching
  - Setup: Add `GOOGLE_AI_API_KEY` to configuration
  - File: `Core/Services/GeminiAiOrchestrator.cs` (to be created)
  - Benefit: Context-aware conversation

- [ ] **Custom Intent Classification**
  - Recommended: Rasa NLU or Hugging Face
  - Improve: Tagalog + English support

- [ ] **Conversation Memory**
  - Currently: Stateless command processing
  - Improvement: Track conversation context

#### Phase 3: Banking Integration ⏳
- [ ] **Brankas Open Banking API**
  - Currently: `MockBrankasApiClient.cs` (simulated)
  - File: `Core/Network/Services/BrankasApiClient.cs`
  - Setup: Register at https://developer.brankas.com/
  - Features:
    - Real account balance retrieval
    - Live transaction history
    - Secure money transfer
    - Bill payment services

- [ ] **UnionBank Sandbox**
  - File: `Core/Network/Services/UnionBankApiClient.cs` (to be created)
  - Setup: Get credentials from UnionBank developer portal
  - Priority: Staging/UAT environment first

- [ ] **OAuth 2.0 Authentication**
  - File: `Core/Security/OAuth2Service.cs` (to be created)
  - Tokens with refresh flow

- [ ] **PCI-DSS Compliance**
  - Tokenization of sensitive data
  - End-to-end encryption
  - Comprehensive audit logging

#### Phase 4: Voice Biometrics Enhancement ⏳
- [ ] **Neural Voice Biometric Service**
  - Currently: 128-dimensional fuzzy matching (demo-only)
  - Options: Pindrop, Nuance VoiceGem, OneID
  - File: `Core/Services/NeuralVoiceBiometricService.cs` (to be created)
  - Improvement: 512+ dimensional embeddings

- [ ] **Liveness Detection**
  - Challenge-response authentication
  - Replay attack prevention

- [ ] **Anti-Spoofing Measures**
  - Frequency analysis
  - Spectral validation
  - Microphone type detection

#### Phase 5: Guardian System (Anti-Scam) ⏳
- [ ] **SMS Gateway Integration**
  - Currently: `ImmediateSmsNotification()` simulated
  - Options: Twilio, Vonage, Globe Labs
  - File: `Core/Services/SmsGatewayService.cs` (to be created)
  - Features:
    - Send OTP to guardian
    - Risk alerts
    - High-transaction notifications

- [ ] **Push Notification Support**
  - Firebase Cloud Messaging (Android)
  - Apple Push Notifications (iOS)
  - Web push for guardians

- [ ] **Guardian Dashboard (Web Portal)**
  - Separate project: `BosesGuardianDashboard`
  - Monitor user activities
  - Approve/reject transactions
  - Scam pattern management

#### Phase 6: Accessibility Enhancements ⏳
- [ ] Screen reader optimization (TalkBack, VoiceOver)
- [ ] Large text support
- [ ] High contrast mode
- [ ] Haptic feedback for confirmations
- [ ] PWD-specific customizations

#### Phase 7: Analytics & Monitoring ⏳
- [ ] Application Insights integration
- [ ] Usage analytics
- [ ] Error tracking and reporting
- [ ] Performance monitoring

---

### 📋 Implementation Guide for Contributors

When implementing production features, follow this checklist:

**For Banking Integration:**
```markdown
- [ ] Create IBankApiClient interface
- [ ] Implement BankApiClient using HttpClient with retry policy
- [ ] Add API credentials to configuration (never hardcode)
- [ ] Implement token refresh logic
- [ ] Add error handling for network failures
- [ ] Create unit tests with mock responses
- [ ] Document API endpoints and response formats
- [ ] Setup staging environment first
- [ ] Test with production data (after security review)
```

**For Voice/AI Services:**
```markdown
- [ ] Create IServiceInterface
- [ ] Implement with real service
- [ ] Add fallback to simulation
- [ ] Implement timeout handling
- [ ] Add diagnostic logging
- [ ] Create integration tests
- [ ] Document API keys and setup
```

**For Guardian/SMS Services:**
```markdown
- [ ] Implement ISmsGateway interface
- [ ] Setup SMS provider account
- [ ] Add rate limiting
- [ ] Implement delivery tracking
- [ ] Add retry logic for failed messages
- [ ] Document SMS template format
```

---

#### 11. **Database Migrations**
- ✅ EF Core migrations with version history
- ✅ PreferredLanguage column addition
- ✅ Automatic migration on app startup
- ✅ Schema validation and recovery
- 📁 Location: `Core/Data/Migrations/`

---

### 🔄 Service Architecture

All major services follow Interface-Based Design for easy mocking and testing:

```csharp
// Core Interfaces
IAudioRecordingService        → AudioRecordingService
IAudioAnalysisService         → AudioAnalysisService
ISpeechRecognitionService     → MauiSpeechRecognitionService
IVoiceService                 → VoiceService (TTS)
IVoiceAuthService             → RealVoiceAuthService
IUserRepository               → UserRepository
ILocalizationService          → LocalizationService
IAiOrchestrator               → AiOrchestratorService (comment out by default)
IBankApiClient                → MockBrankasApiClient
```

### 🎯 Testing Scenarios (All Implemented)

| Scenario | Voice Command | Status |
|----------|---------------|--------|
| Balance Inquiry | "Magkano ang balance ko?" | ✅ Working |
| Money Transfer | "Ipadala ang 500 pesos kay Juan" | ✅ Working |
| Transaction History | "Ano ang mga recent transactions ko?" | ✅ Working |
| PWD Discount | "Calculate PWD discount for 1000 pesos" | ✅ Working |
| Scam Detection | "Send 5000 pesos for prize claim" | ✅ Working |
| Voice Registration | 3-sample enrollment process | ✅ Working |
| Language Switch | Switch between English and Filipino | ✅ Working |

---

### 🌐 Production Integration Readiness

### Phase 1: Voice Services
- [ ] Integrate Deepgram for real-time STT
- [ ] Implement platform-specific TTS (AVSpeechSynthesizer, TextToSpeech)
- [ ] Add noise cancellation and voice activity detection

### Phase 2: AI & NLU
- [ ] Connect Google Gemini API for production NLU
- [ ] Train custom intent classification models
- [ ] Implement context-aware conversation memory

### Phase 3: Banking Integration
- [ ] Integrate Brankas Open Banking API
- [ ] Connect UnionBank Sandbox for testing
- [ ] Implement OAuth 2.0 authentication flow
- [ ] Add PCI-DSS compliant transaction handling

### Phase 4: Voice Biometrics
- [ ] Integrate specialized voice biometric service (e.g., Pindrop, Nuance)
- [ ] Implement liveness detection
- [ ] Add anti-spoofing measures

### Phase 5: Guardian System
- [ ] Implement SMS gateway integration (Twilio, Vonage)
- [ ] Add push notification support
- [ ] Build guardian dashboard web portal

---

## 🐛 Troubleshooting

### Speech Recognition Not Triggering
**Problem**: OnRecognitionResultUpdated event never fires  
**Causes**:
1. Android API level < 33 (requires Android 13 Tiramisu+)
2. Microphone permission not granted
3. MAUI Community Toolkit not initialized

**Solutions**:
```bash
# Check Android API level on device/emulator
adb shell getprop ro.build.version.release

# Should be 13 or higher
# If lower, create new emulator with Android 33+ API
```

Refer to `SPEECH_RECOGNITION_DIAGNOSTICS.md` for detailed troubleshooting.

### SQLite Database Issues
**Problem**: "Database is locked" or file access errors  
**Solution**: Set `useJsonFallback = true` in `MauiProgram.cs`

### Microphone Not Working
**Problem**: Voice input not captured  
**Solution**: 
1. Check platform permissions:
   - Android: `AndroidManifest.xml` must include `RECORD_AUDIO` permission
   - iOS: `Info.plist` must include microphone usage description
2. Verify device microphone is not in use by another app
3. Enable simulation mode for testing without hardware

**Android Manifest Permissions**:
```xml
<uses-permission android:name="android.permission.RECORD_AUDIO" />
<uses-permission android:name="android.permission.INTERNET" />
```

### Build Errors
**Problem**: NuGet package restore failures  
**Solution**:
```bash
dotnet nuget locals all --clear
dotnet restore --force
dotnet build
```

### Android Deployment Issues
**Problem**: App crashes on startup  
**Solutions**:
1. Ensure Android SDK 21+ is installed
2. Verify emulator is running with API 33+ for speech recognition
3. Check that all required permissions are in AndroidManifest.xml
4. Clear app data: `adb shell pm clear com.boses.accessibility`

**Clear App Data Command**:
```powershell
& "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" shell pm clear com.boses.accessibility
```

### Language Not Displaying Correctly
**Problem**: Text showing in wrong language or default language  
**Solution**:
1. Check `LocalizationService.cs` for language mapping
2. Verify language resources are loaded in `InitializeAsync()`
3. Ensure `PreferredLanguage` column exists in database
4. Try deleting and recreating user profile

---

## 🔧 Advanced Configuration

### Voice Recognition Configuration

**File**: `Core/Configuration/SpeechConfig.cs`

```csharp
public static class SpeechConfig
{
    // Check if Azure Speech is configured
    public static bool IsAzureSpeechConfigured { get; }

    // Get Azure region
    public static string AzureSpeechRegion { get; }
}
```

### Speech Recognition Service Selection

In `MauiProgram.cs`, you can switch between implementations:

```csharp
// Current: MAUI Community Toolkit (Recommended for this app)
builder.Services.AddSingleton<ISpeechRecognitionService, MauiSpeechRecognitionService>();

// Alternative: Basic simulation
// builder.Services.AddSingleton<ISpeechRecognitionService, SpeechRecognitionService>();

// Alternative: Azure Speech (requires credentials)
// builder.Services.AddSingleton<ISpeechRecognitionService, AzureSpeechRecognitionService>();
```

### Voice Biometric Threshold

In `VoiceAuthService.cs` or `RealVoiceAuthService.cs`:

```csharp
private const double VOICE_SIMILARITY_THRESHOLD = 0.85; // 85% match required
private const double PHRASE_SIMILARITY_THRESHOLD = 0.7;  // 70% phrase match
```

Adjust these values based on your accuracy requirements.

---

## 📚 Project Documentation

Additional detailed guides are available:

| Document | Purpose |
|----------|---------|
| **`FEATURE_IMPLEMENTATION_STATUS.md`** | ⭐ **START HERE** - Complete feature checklist (implemented vs. roadmap) |
| **`README_ALIGNMENT_REPORT.md`** | 📋 Verification that README matches codebase implementation |
| `SPEECH_RECOGNITION_DIAGNOSTICS.md` | Speech recognition troubleshooting and debugging |
| `SPEECH_RECOGNITION_FIX_SUMMARY.md` | Details on OnRecognitionResultUpdated event implementation |
| `ARCHITECTURE.md` | Detailed architecture documentation |
| `FEATURES.md` | Complete feature list |
| `ONBOARDING_GUIDE.md` | User onboarding flow documentation |
| `LOCALIZATION_IMPLEMENTATION_SUMMARY.md` | Language support details |
| `QUICKSTART.md` | Quick start guide for developers |

---

## 🚀 Quick Debug Commands

### Check Emulator Android Version
```powershell
adb shell getprop ro.build.version.release
adb shell getprop ro.build.version.sdk
```

### View Real-Time Logs
```powershell
adb logcat | Select-String "SpeechRecognition|VoiceRegistration|Audio"
```

### Clear App and Rebuild
```powershell
adb uninstall com.boses.accessibility
dotnet build -t:Run -f net9.0-android
```

### Delete Local Database
```powershell
Remove-Item -Path "$env:LOCALAPPDATA\Boses\boses.db" -Force -ErrorAction SilentlyContinue
```

---

## 📄 License

This project is developed for hackathon/educational purposes. For production use, ensure compliance with:
- Banking regulations (BSP, PCI-DSS)
- Data privacy laws (Data Privacy Act of 2012)
- Accessibility standards (WCAG 2.1)

---

## 👥 Contributing

This is a hackathon project demonstrating enterprise .NET MAUI architecture. Contributions welcome for:
- Additional language support (Cebuano, Ilocano, etc.)
- Enhanced accessibility features
- Production API integrations
- UI/UX improvements

---

## 🎓 Learning Resources

- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/)
- [Open Banking Philippines](https://www.bsp.gov.ph/Pages/InclusiveFinance/Open-Finance.aspx)
- [Voice Biometrics Overview](https://www.nist.gov/programs-projects/speaker-recognition)

---

## 📞 Support

For questions or issues:
1. Check the troubleshooting section above
2. Review the inline code documentation
3. Examine the simulation mode outputs for debugging

---

**Built with ❤️ for Filipino accessibility and financial inclusion**
