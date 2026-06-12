# 🎤 Boses - Voice-First Accessibility Platform

**Boses** is a universal voice-first accessibility platform designed for elderly Filipinos and Persons with Disabilities (PWDs). It bridges the digital divide through an ambient, conversational voice layer using Semantic Kernel, secures sensitive transactions with biometric voice authentication, and protects vulnerable users via a "Guardian" anti-scam loop.
---

## 🎯 Product Mission

Boses empowers elderly Filipinos and PWDs to access digital banking and services through natural voice commands in Tagalog and English. The platform features:

- **🗣️ Voice-First Interface**: Natural language processing powered by **Google Gemini** via Semantic Kernel
- **🤖 Google Gemini AI**: Live NLU with `gemini-1.5-flash` — context-aware, bilingual responses (Tagalog/English)
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
│   │       └── MockBankingApiClient.cs
│   ├── Interfaces/                         # Service contracts
│   │   ├── IAiOrchestrator.cs
│   │   ├── IAccessibilityService.cs
│   │   ├── IAnalyticsService.cs
│   │   ├── IGuardianNotificationService.cs
│   │   ├── ILocalizationService.cs
│   │   ├── ISmsGateway.cs
│   │   ├── ISpeechRecognitionService.cs
│   │   ├── IUserRepository.cs
│   │   ├── IVoiceAuthService.cs
│   │   └── IVoiceService.cs
│   └── Services/                           # Service implementations
│       ├── AccessibilityService.cs
│       ├── AiOrchestratorService.cs         # Includes ScamDetectionResult + SimulateScamDetectionAsync
│       ├── AnalyticsService.cs
│       ├── AudioAnalysisService.cs
│       ├── AudioRecordingService.cs
│       ├── DeepgramSpeechRecognitionService.cs
│       ├── GuardianNotificationService.cs
│       ├── HybridSpeechRecognitionService.cs
│       ├── LocalizationService.cs
│       ├── MauiSpeechRecognitionService.cs
│       ├── RealVoiceAuthService.cs
│       ├── SimulatedSmsGateway.cs
│       ├── SpeechRecognitionService.cs
│       ├── TelegramNotificationGateway.cs
│       ├── TextBeltSmsGateway.cs
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

- **.NET 9 SDK** or later — [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Visual Studio 2022** (17.8+) with the **.NET Multi-platform App UI** workload — [Download](https://visualstudio.microsoft.com/)
- **Windows 10 version 1903 (build 19041) or later** — recommended demo platform ✅
- **Git** for version control

> **Android / iOS** are also supported but require additional setup (Android SDK 33+, Android emulator or physical device, Xcode on macOS). For a quick demo, **Windows is strongly recommended** — no emulator setup needed.

---

### 🗣️ Filipino Text-to-Speech

Boses uses **Google Translate TTS** for Filipino (Tagalog) voice output — completely free with **zero setup required**.

| Property | Detail |
|---|---|
| Provider | Google Translate TTS (public endpoint) |
| Filipino support | ✅ `tl` (Tagalog) |
| English support | ✅ `en` |
| Setup required | ❌ None — works out of the box |
| API key needed | ❌ None |
| Downloads needed | ❌ None |
| Internet required | ✅ Yes (for TTS calls) |
| Fallback when offline | OS built-in English voice |

#### TTS Fallback Chain

```
SpeakAsync("text")
   │
   ├─ Internet available?
   │    └─ Google Translate TTS ──▶ Filipino (tl) or English (en)  ✅
   │
   └─ Offline / request failed?
        └─ OS TTS ──▶ Windows built-in English voice  ✅
```

No configuration needed — just run the app.

---


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

4. **Run on Windows** (recommended — no emulator needed):

```bash
dotnet build -t:Run -f net9.0-windows10.0.19041.0

<details>
<summary>Other platforms (click to expand)</summary>

**Android** (requires Android SDK 33+ and a running emulator or physical device):
```bash
dotnet build -t:Run -f net9.0-android
```

**iOS** (macOS + Xcode 14+ required):
```bash
dotnet build -t:Run -f net9.0-ios
```
</details>

---

## 🎮 Using the Application

### Main Features

1. **Voice Commands** (Tap the microphone button):

**💰 Balance & Account**
- "Magkano ang balance ko?" — Check savings + checking + e-wallet balances
- "Ano ang laman ng aking account?" — Same, alternative phrasing
- "Ipakita ang account information ko" — Account details (masked for security)

**💸 Transfer & Send Money**
- "Mag-transfer ng 500 pesos kay Juan" — Bank-to-bank transfer
- "Ipadala ang 1000 pesos kay Maria" — Same in Tagalog
- "Magpadala ng pera kay Pedro" — Transfer without amount (prompts for amount)
- "Mag-send ng 300 pesos sa GCash" — GCash e-wallet send
- "I-send ang 500 pesos sa Maya" — Maya e-wallet send

**🏧 Withdraw / ATM**
- "Mag-withdraw ng 2000 pesos" — ATM withdrawal guide + balance check
- "Gusto kong mag-withdraw mula sa ATM" — Same without amount
- "Cash out ng 1500 pesos" — Alternative phrasing

**📜 Transaction History**
- "Ano ang mga recent transactions ko?" — Last 5 transactions
- "Ipakita ang kasaysayan ng transaksyon" — Same in Tagalog
- "Mga nakaraang bayad ko" — Past payments

**🧾 Bill Payment**
- "Bayaran ang Meralco bill ng 850 pesos" — Electricity bill
- "Mag-bayad ng Maynilad tubig" — Water bill
- "Bayad ng PLDT internet" — Internet bill
- "Bayaran ang SSS / PhilHealth / Pag-IBIG" — Government contributions
- "Mag-bayad ng kuryente" — Generic electricity payment

**♿ PWD Discount**
- "Kalkulahin ang PWD discount para sa 1000 pesos na gamot" — 20% medicine discount
- "PWD discount para sa 500 pesos" — 5% general discount
- "Magkano ang diskwento ko bilang PWD?" — Discount info

**👴 Senior Citizen Discount**
- "Senior discount para sa 400 pesos na pagkain" — 20% food discount
- "Magkano ang senior citizen discount sa 800 pesos na gamot?" — 20% medicine discount
- "Senior discount" — General discount info

**🏦 Loan Inquiry**
- "Magkano ang pwede ko pang i-loan?" — Loan eligibility overview
- "Gusto kong mag-apply ng personal loan" — Personal loan info
- "Pautang naman" — Tagalog loan request

**🆘 Emergency / Card Block**
- "I-block ang aking card" — Emergency card block instructions
- "Nawala ang aking ATM card" — Lost card guidance
- "Ninakaw ang aking account" — Stolen account response

**ℹ️ Help & Navigation**
- "Tulong" / "Help" — Full command menu
- "Ano ang kaya mong gawin?" — List of capabilities
- "Kumusta Boses!" / "Hello" — Greeting
- "Kausapin ang agent" — Connect to live support

2. **Quick Action Buttons** (one tap — no voice needed):

**🏦 Banking Row**
| Button | Simulated Command |
|---|---|
| 💰 Balance | "Magkano ang balance ko?" |
| 📜 Transactions | "Ano ang mga recent transactions ko?" |
| 💸 Transfer | "Mag-transfer ng 1000 pesos kay Maria" |
| 🏧 Withdraw | "Mag-withdraw ng 2000 pesos" |
| 📱 GCash | "Mag-send ng 500 pesos sa GCash" |
| 🧾 Bills | "Bayaran ang Meralco bill ng 850 pesos" |
| 🏦 Loan | "Magkano ang pwede ko pang i-loan?" |

**🎯 Discounts & Emergency Row**
| Button | Simulated Command |
|---|---|
| ♿ PWD Discount *(PWD users only)* | "PWD discount para sa 1000 pesos na gamot" |
| 👴 Senior Discount | "Senior discount para sa 500 pesos na pagkain" |
| 🆘 Block Card | "Nawala ang aking ATM card" |
| ℹ️ Help | "Tulong" |

**⚡ Special Features Row**
| Button | Action |
|---|---|
| 🎤 Register Voice | Opens voice biometric enrollment |
| 🚨 Scam Detection Demo | Runs scripted scam detection |
| 🔁 Hands-Free (ON/OFF) | Toggles zero-tap continuous voice loop |

3. **🔁 Hands-Free Mode** (🔁 Hands-Free button):
   - Toggle continuous listen → process → speak loop — no screen taps needed
   - Mic auto-restarts after each response (up to 8-second listening window)
   - Exit by voice: say **"stop"**, **"itigil"**, or **"hinto"**
   - Status indicator turns green when active

4. **Simulation Mode** (⚙️ button):
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

Edit `MockBankingApiClient.cs` to customize:
- Account balances
- Transaction history
- Bank account details

---

### 🤖 Google Gemini AI Configuration

Boses uses **Google Gemini** (`gemini-1.5-flash`) for live NLU when an API key is present. Without a key it falls back to rule-based simulation automatically.

**File**: `Core/Configuration/GeminiConfig.cs`

#### Option 1 — Environment variable (recommended)
```powershell
# Windows PowerShell — set before launching the app
$env:GEMINI_API_KEY = "AIza..."
```

#### Option 2 — Hardcoded key (quick demo only, never commit)
```csharp
// In GeminiConfig.cs
private const string _hardcodedKey = "AIza..."; // ← paste key here
```

Get a free key at [https://aistudio.google.com/app/apikey](https://aistudio.google.com/app/apikey) — free tier is **15 RPM / 1 M tokens per day**, sufficient for demos.

#### Gemini settings

| Setting | Value | Notes |
|---|---|---|
| Model | `gemini-1.5-flash` | Fast, free-tier friendly |
| MaxTokens | `1024` | Prevents truncated responses |
| Temperature | `0.7` | Natural, varied replies |

When `SimulationMode = true` (default), Gemini is bypassed even if a key is configured — ideal for offline demos.

---

### 📬 Telegram Guardian Notifications (Free, Unlimited)

Telegram is the recommended notification provider for demos — completely free with no daily limits.

#### One-Time Setup (5 minutes)

**Step 1 — Create a bot**
1. Open Telegram → search **@BotFather** → tap **Start**
2. Send `/newbot` and follow the prompts
3. Copy the **token** BotFather gives you (format: `7123456789:AAExxxxxxxx`)

**Step 2 — Get the guardian's chat ID**
1. Have the guardian open Telegram, search for your bot by name, and send any message (e.g. `/start`)
2. Open this URL in a browser (replace `TOKEN`):
   ```
   https://api.telegram.org/bot{TOKEN}/getUpdates
   ```
3. In the JSON response, find `"chat":{"id": 123456789}` — that number is the **chat ID**

**Step 3 — Enable in MauiProgram.cs**

In `MauiProgram.cs`, comment out OPTION A and uncomment OPTION B:
```csharp
// Comment out:
// builder.Services.AddSingleton<ISmsGateway, SimulatedSmsGateway>();

// Uncomment and fill in your values:
builder.Services.AddHttpClient<TelegramNotificationGateway>();
builder.Services.AddSingleton<ISmsGateway>(sp =>
    new TelegramNotificationGateway(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
        botToken: "7123456789:AAExxxxxxxx",  // ← from @BotFather
        chatId:   "123456789"               // ← from getUpdates
    ));
```

> Guardian will now receive Boses alerts as Telegram messages in real time — including high-risk transaction approvals and scam detection alerts.

---

## 🧪 Testing Scenarios

### 1. Balance Inquiry
**Voice**: "Magkano ang balance ko?" / "Ano ang laman ng aking account?"  
**Response**: Savings, checking, and GCash balances in a single reply

### 2. Money Transfer (Bank)
**Voice**: "Mag-transfer ng 1000 pesos kay Maria"  
**Flow**: Amount + recipient confirmed → voice auth → guardian check if >₱5,000

### 3. Send to GCash / Maya
**Voice**: "Mag-send ng 500 pesos sa GCash"  
**Flow**: Wallet type detected → amount confirmed → identity verification

### 4. ATM Withdrawal
**Voice**: "Mag-withdraw ng 2000 pesos"  
**Response**: Available balance shown + ATM fee reminder

### 5. Bill Payment
**Voice**: "Bayaran ang Meralco bill ng 850 pesos"  
**Flow**: Biller auto-detected → amount confirmed → guardian check if >₱5,000

### 6. Transaction History
**Voice**: "Ano ang mga recent transactions ko?"  
**Response**: Last 5 transactions with date, description, and amount

### 7. PWD Discount Calculator
**Voice**: "PWD discount para sa 1000 pesos na gamot"  
**Response**: 20% discount → shows original, discount amount, and final price

### 8. Senior Citizen Discount
**Voice**: "Senior discount para sa 500 pesos na pagkain"  
**Response**: 20% food/medicine discount → breakdown shown

### 9. Loan Inquiry
**Voice**: "Magkano ang pwede ko pang i-loan?"  
**Response**: Eligible loan products with amounts and terms

### 10. Guardian Verification (High-Risk)
**Voice**: "Mag-transfer ng 10000 pesos"  
**Flow**: Risk assessment triggers → guardian SMS sent → awaits approval code

### 11. Emergency Card Block
**Voice**: "Nawala ang aking ATM card"  
**Response**: Immediate hotline + steps to block + security alert logged

### 12. Help / Menu
**Voice**: "Tulong" / "Help"  
**Response**: Full numbered command menu in Tagalog

### 13. Scam Detection Demo
**Action**: Tap 🚨 Scam Detection Demo button  
**Flow**: Scripted scam call → AI analysis → category + red flags + recommended action displayed

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

### AI / NLU Packages
- **Microsoft.SemanticKernel** - AI orchestration framework
- **Microsoft.SemanticKernel.Connectors.Google** - Google Gemini connector (experimental `SKEXP0070`)

### Optional Integrations (for future)
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
- ✅ BoolToColorConverter — Boolean to configurable color (`TrueColor`/`FalseColor` properties)
- ✅ BoolToTextConverter — Boolean to configurable text (`TrueText`/`FalseText` properties), drives mic icon toggle
- ✅ BoolToStrokeConverter — Boolean to stroke property binding
- ✅ EqualToConverter — Equality comparison binding
- ✅ InvertedBoolConverter — Boolean inversion
- ✅ StringToBoolConverter — String to boolean parsing
- ✅ IntGreaterThanZeroConverter — Drives guardian alert badge visibility
- 📁 Location: `Presentation/Converters/`

#### 10. **Platform-Specific Implementations**
- ✅ Android audio recording with Plugin.Maui.Audio
- ✅ iOS audio recording with AVFoundation
- ✅ Windows audio recording support
- ✅ macOS Catalyst audio support
- 📁 Location: `Platforms/*/Services/`

#### 11. **Guardian Notification System** (Phase 5)
- ✅ Guardian event logging to database
- ✅ SMS verification simulation with code generation
- ✅ High-risk transaction detection (>5,000 PHP threshold)
- ✅ Guardian alert badge (count of today's alerts)
- ✅ `SCAM_BLOCKED` event type for scam-detection demo
- 📁 Location: `Core/Services/GuardianNotificationService.cs`

#### 12. **Scam Detection Demo** (Phase 5)
- ✅ 6 rule-based scam detectors (OTP harvesting, bank impersonation, urgency threats, prize scams, remote access, credential phishing)
- ✅ Confidence scoring (0–99%)
- ✅ Scripted scenario rotation (4 realistic Filipino scam scripts)
- ✅ Full analysis card in conversation history
- ✅ TTS warning playback
- 📁 Location: `Core/Services/AiOrchestratorService.cs` → `SimulateScamDetectionAsync`
- 📁 Location: `Presentation/ViewModels/MainViewModel.cs` → `SimulateScamDemoCommand`

#### 13. **Accessibility Service** (Phase 6)
- ✅ User accessibility profile load & apply
- ✅ Screen-reader `AnnounceAsync` for all AI responses
- 📁 Location: `Core/Services/AccessibilityService.cs`

#### 14. **Hands-Free Mode** (Phase 6)
- ✅ Continuous listen → process → speak loop (no screen taps required)
- ✅ Auto mic restart after each response
- ✅ 8-second per-cycle listening window
- ✅ Voice exit commands: "stop", "itigil", "tigilan", "hinto"
- ✅ Re-entrancy guard prevents overlapping loops
- 📁 Location: `Presentation/ViewModels/MainViewModel.cs` → `ToggleHandsFreeModeCommand`

#### 15. **Analytics Service** (Phase 7)
- ✅ Screen view tracking
- ✅ Voice command tracking with latency (ms)
- ✅ Feature usage tracking per intent
- ✅ Guardian/scam event tracking
- ✅ Error event reporting
- 📁 Location: `Core/Services/AnalyticsService.cs`

#### 16. **Database Migrations**
- ✅ EF Core migrations with version history
- ✅ PreferredLanguage column addition
- ✅ Automatic migration on app startup
- ✅ Schema validation and recovery
- 📁 Location: `Core/Data/Migrations/`

---

### 🌐 Production Integration Roadmap

The following production features are designed into the architecture but require external services/APIs:

#### Phase 1: Production Voice Services ⏳
- [x] **Deepgram API Integration**
  - Status: Implemented — `Core/Services/DeepgramSpeechRecognitionService.cs`
  - Setup: Add `DEEPGRAM_API_KEY` to environment / `SpeechConfig.cs`
  - Features: Real-time streaming STT with WebSocket, language detection

- [x] **Hybrid Speech Recognition**
  - Status: Implemented — `Core/Services/HybridSpeechRecognitionService.cs`
  - Automatically selects Deepgram (online) or MAUI Community Toolkit (offline)

- [ ] **Advanced Voice Activity Detection (VAD)**
  - Currently: RMS/ZCR-based VAD
  - Recommended: Silero VAD or WebRTC VAD
  - File: `Core/Services/VoiceActivityDetectionService.cs`

#### Phase 2: AI & NLU (Natural Language Understanding) ✅
- [x] **Google Gemini API**
  - Status: **Implemented** — `Core/Services/AiOrchestratorService.cs` + `Core/Configuration/GeminiConfig.cs`
  - Model: `gemini-1.5-flash` via `Microsoft.SemanticKernel.Connectors.Google`
  - Setup: Set `GEMINI_API_KEY` env var or fill `_hardcodedKey` in `GeminiConfig.cs`
  - Settings: `MaxTokens = 1024`, `Temperature = 0.7` via `GeminiPromptExecutionSettings`
  - Fallback: Rule-based simulation when no key is set or `SimulationMode = true`

- [ ] **Custom Intent Classification**
  - Recommended: Rasa NLU or Hugging Face
  - Improve: Tagalog + English support

- [ ] **Conversation Memory**
  - Currently: Stateless per-request `ChatHistory`
  - Improvement: Persist history across turns for multi-turn dialogue

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
- [x] **SMS / Notification Gateway — Multi-Provider**
  - `ISmsGateway` abstraction allows zero-code provider swaps in `MauiProgram.cs`
  - **`SimulatedSmsGateway`** — default, prints to debug console, zero config
  - **`TelegramNotificationGateway`** ⭐ recommended free option:
    - 100% free, no credit card, no daily limits
    - Guardian receives real Telegram push notifications instantly
    - 5-minute setup via @BotFather
    - File: `Core/Services/TelegramNotificationGateway.cs`
  - **`TextBeltSmsGateway`** — real SMS to phone, free tier 1 SMS/day (key=`"textbelt"`)
  - TextBelt paid: ~$0.01/SMS
  - Files: `Core/Interfaces/ISmsGateway.cs`, `Core/Services/TelegramNotificationGateway.cs`, `Core/Services/TextBeltSmsGateway.cs`, `Core/Services/SimulatedSmsGateway.cs`

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
ISpeechRecognitionService     → HybridSpeechRecognitionService
                                  ├── DeepgramSpeechRecognitionService (online)
                                  └── MauiSpeechRecognitionService (offline fallback)
IVoiceService                 → VoiceService (TTS + mic orchestration)
IVoiceAuthService             → RealVoiceAuthService
IUserRepository               → UserRepository
ILocalizationService          → LocalizationService
IAiOrchestrator               → AiOrchestratorService (Gemini live | rule-based fallback)
IGuardianNotificationService  → GuardianNotificationService
IAccessibilityService         → AccessibilityService
IAnalyticsService             → AnalyticsService
IBankApiClient                → MockBankingApiClient
```

### 🎯 Testing Scenarios (All Implemented)

| Scenario | Voice Command / Action | Status |
|----------|------------------------|--------|
| Balance Inquiry | "Magkano ang balance ko?" | ✅ Working |
| Money Transfer | "Mag-transfer ng 1000 pesos kay Maria" | ✅ Working |
| GCash / Maya Send | "Mag-send ng 500 pesos sa GCash" | ✅ Working |
| ATM Withdrawal | "Mag-withdraw ng 2000 pesos" | ✅ Working |
| Bill Payment | "Bayaran ang Meralco bill ng 850 pesos" | ✅ Working |
| Transaction History | "Ano ang mga recent transactions ko?" | ✅ Working |
| PWD Discount | "PWD discount para sa 1000 pesos na gamot" | ✅ Working |
| Senior Discount | "Senior discount para sa 500 pesos na pagkain" | ✅ Working |
| Loan Inquiry | "Magkano ang pwede ko pang i-loan?" | ✅ Working |
| Guardian Verification | "Mag-transfer ng 10000 pesos" (>₱5,000) | ✅ Working |
| Emergency Card Block | "Nawala ang aking ATM card" | ✅ Working |
| Help / Menu | "Tulong" / "Help" | ✅ Working |
| Greeting | "Kumusta Boses!" | ✅ Working |
| Scam Detection Demo | 🚨 Scam Detection Demo button | ✅ Working |
| Hands-Free Loop | 🔁 Hands-Free button; say "itigil" to stop | ✅ Working |
| Voice Registration | 3-sample enrollment process | ✅ Working |
| Language Switch | Switch between English and Filipino | ✅ Working |

---

### 🌐 Production Integration Readiness

### Phase 1: Voice Services
- [ ] Integrate Deepgram for real-time STT
- [ ] Implement platform-specific TTS (AVSpeechSynthesizer, TextToSpeech)
- [ ] Add noise cancellation and voice activity detection

### Phase 2: AI & NLU
- [x] Google Gemini API integrated (`gemini-1.5-flash`, Semantic Kernel connector)
- [x] Bilingual system prompt (Tagalog/English banking context)
- [x] `GeminiPromptExecutionSettings` — `MaxTokens = 1024`, `Temperature = 0.7`
- [x] Automatic fallback to rule-based simulation when offline or `SimulationMode = true`
- [ ] Train custom intent classification models
- [ ] Persistent multi-turn conversation memory

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
- [x] Guardian event logging (`GuardianNotificationService.cs`)
- [x] SMS/notification gateway abstraction (`ISmsGateway`) with pluggable providers
- [x] `SimulatedSmsGateway` — zero-config debug logging (default)
- [x] `TelegramNotificationGateway` — ⭐ free, unlimited, instant push to guardian
- [x] `TextBeltSmsGateway` — real SMS, free 1/day or paid ~$0.01/SMS
- [x] SMS verification simulation with code generation
- [x] High-risk transaction detection (>5,000 PHP threshold)
- [x] Scam detection simulation demo with 6 rule-based detectors
- [ ] Real SMS gateway integration (Twilio, Vonage, Globe Labs)
- [ ] Push notification support (FCM / APNs)
- [ ] Guardian dashboard web portal

### Phase 6: Accessibility Enhancements
- [x] Accessibility profile load & apply (`AccessibilityService.cs`)
- [x] Screen-reader announcement via `AnnounceAsync`
- [x] Hands-Free continuous voice loop for users who cannot tap
- [ ] TalkBack / VoiceOver optimisation
- [ ] High contrast mode
- [ ] Haptic feedback for confirmations

### Phase 7: Analytics & Monitoring
- [x] Screen view tracking (`AnalyticsService.cs`)
- [x] Voice command tracking with latency
- [x] Feature usage tracking
- [x] Guardian/scam event tracking
- [x] Error reporting
- [ ] Application Insights cloud integration
- [ ] Performance monitoring dashboard

## 🐛 Troubleshooting

### Speech Recognition Not Working on Windows
**Problem**: Microphone button does nothing or transcription is always empty  
**Causes**:
1. Microphone permission not granted to the app
2. Filipino language pack not installed (falls back to English)
3. Default microphone not set correctly

**Solutions**:
```
1. Windows Settings → Privacy & Security → Microphone
   → Ensure "Let apps access your microphone" is ON
   → Ensure "Let desktop apps access your microphone" is ON

2. Install Filipino language pack (see "Filipino Language Pack" section above)

3. Windows Settings → System → Sound
   → Set correct input device as Default
   → Test microphone recording level
```

### Windows App Microphone Permission Denied
**Problem**: `PermissionException` or mic stays silent  
**Solution**:
```powershell
# Check if microphone is accessible
Get-PnpDevice -Class "AudioEndpoint" | Where-Object { $_.Status -eq "OK" }
```
Also check `Package.appxmanifest` includes the `microphone` device capability.

### Filipino TTS Voice Not Speaking Tagalog
**Problem**: TTS speaks in English even when Filipino commands are used  
**Solution**: Install the Filipino language pack and speech features (see above).
After install, sign out and back in to Windows — then relaunch the app.

### SQLite Database Issues
**Problem**: "Database is locked" or file access errors  
**Solution**: Set `useJsonFallback = true` in `MauiProgram.cs`

### Build Errors
**Problem**: NuGet package restore failures  
**Solution**:
```bash
dotnet nuget locals all --clear
dotnet restore --force
dotnet build
```

### Windows Developer Mode Required
**Problem**: `DEP0700` or sideload errors when deploying  
**Solution**:
```
Windows Settings → Privacy & Security → For developers
→ Turn on Developer Mode
```

### Language Not Displaying Correctly
**Problem**: Text showing in wrong language  
**Solution**:
1. Check `LocalizationService.cs` for language mapping
2. Verify language resources are loaded in `InitializeAsync()`
3. Ensure `PreferredLanguage` column exists in database
4. Try deleting the local database:
```powershell
Remove-Item -Path "$env:LOCALAPPDATA\Boses\boses.db" -Force -ErrorAction SilentlyContinue
```

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
|----------|--------|
| `QUICKSTART.md` | Quick start guide for developers |
| `ARCHITECTURE.md` | Detailed architecture documentation |
| `FEATURES.md` | Complete feature showcase |
| `project-report.md` | AI-generated project report |

---

## 🚀 Quick Commands

### Build and Run (Windows — recommended)
```powershell
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

### Check Installed TTS Voices (verify Filipino voice)
```powershell
Add-Type -AssemblyName System.Speech
$synth = New-Object System.Speech.Synthesis.SpeechSynthesizer
$synth.GetInstalledVoices() | ForEach-Object { $_.VoiceInfo.Name }
```

### View Real-Time App Logs (Visual Studio Output window)
```
Debug → Windows → Output → Show output from: Debug
Filter by: [Audio] [VoiceService] [Guardian] [TTS]
```

### Delete Local Database (reset app state)
```powershell
Remove-Item -Path "$env:LOCALAPPDATA\Boses\boses.db" -Force -ErrorAction SilentlyContinue
```

### Force NuGet Restore
```powershell
dotnet nuget locals all --clear
dotnet restore --force
```

---

## 🖥️ Windows Deployment Guide (No Visual Studio Required)

This guide lets judges or reviewers run Boses on any Windows 10/11 PC **without installing Visual Studio**.

---

### Option A — Run from Source (Developer Machine → Judge PC via USB/share)

> Best when you have access to the judge's PC briefly before the demo.

#### On your developer machine — build the self-contained EXE

```powershell
dotnet publish BosesApp.csproj `
  -f net9.0-windows10.0.19041.0 `
  -c Release `
  -r win10-x64 `
  --self-contained true `
  -p:WindowsPackageType=None `
  -o .\publish\windows
```

This produces a `publish\windows\` folder containing `BosesApp.exe` and all dependencies — **no .NET runtime installation needed** on the judge's PC.

#### Transfer to the judge's PC

Copy the entire `publish\windows\` folder via USB drive, network share, or OneDrive.

#### On the judge's PC — enable Developer Mode (one-time, 30 seconds)

```
Settings → Privacy & Security → For Developers → Developer Mode → ON
```

> Without Developer Mode, Windows blocks unsigned sideloaded desktop apps.

#### Launch the app

Double-click **`BosesApp.exe`** inside the copied folder.

> If Windows SmartScreen appears, click **More info → Run anyway**. This is expected for unsigned apps.

---

### Option B — Publish a Single-File EXE (easiest to share)

Produces a **single `.exe` file** — no folder needed.

```powershell
dotnet publish -f net9.0-windows10.0.19041.0 -c Release -p:WindowsPackageType=None -p:WindowsAppSDKSelfContained=true -p:RuntimeIdentifierOverride=win10-x64
```

Share `publish\single\BosesApp.exe` — that one file is the entire app.

> **Note**: First launch extracts files to `%TEMP%` and takes ~5 seconds. Subsequent launches are instant.

---

### Option C — MSIX Package (cleanest install experience)

Produces a proper Windows installer the judge double-clicks to install like any normal app.

#### Prerequisites (on your machine only)

1. Visual Studio 2022 with **Windows App SDK** workload, **or** install the Windows SDK:
   ```powershell
   winget install Microsoft.WindowsSDK.10.0.22621
   ```

2. Create a self-signed certificate for packaging:
   ```powershell
   $cert = New-SelfSignedCertificate `
     -Type CodeSigningCert `
     -Subject "CN=BosesDemo" `
     -KeyUsage DigitalSignature `
     -FriendlyName "Boses Demo Cert" `
     -CertStoreLocation "Cert:\CurrentUser\My" `
     -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

   $pwd = ConvertTo-SecureString -String "boses123" -Force -AsPlainText
   Export-PfxCertificate -Cert $cert -FilePath BosesDemo.pfx -Password $pwd
   Write-Host "Thumbprint: $($cert.Thumbprint)"
   ```

3. Update `Package.appxmanifest` — set `Publisher` to `CN=BosesDemo`.

#### Build the MSIX

```powershell
dotnet publish BosesApp.csproj `
  -f net9.0-windows10.0.19041.0 `
  -c Release `
  -p:GenerateAppxPackageOnBuild=true `
  -p:AppxPackageSigningEnabled=true `
  -p:PackageCertificateKeyFile=BosesDemo.pfx `
  -p:PackageCertificatePassword=boses123
```

The `.msix` file is output to `bin\Release\net9.0-windows10.0.19041.0\win10-x64\AppPackages\`.

#### On the judge's PC — install the certificate then the app

```powershell
# 1. Install the signing certificate (run as Administrator once)
Import-PfxCertificate -FilePath BosesDemo.pfx `
  -CertStoreLocation Cert:\LocalMachine\Root `
  -Password (ConvertTo-SecureString "boses123" -AsPlainText -Force)

# 2. Install the MSIX
Add-AppxPackage -Path ".\BosesApp_1.0.0.0_x64.msix"
```

Then launch **Boses** from the Start menu like any installed app.

---

### Minimum Judge PC Requirements

| Requirement | Value |
|---|---|
| OS | Windows 10 version 1903 (build 19041) or Windows 11 |
| Architecture | x64 |
| RAM | 4 GB minimum, 8 GB recommended |
| Disk space | ~200 MB (self-contained) |
| .NET runtime | **Not required** (self-contained publish bundles it) |
| Visual Studio | **Not required** |
| Internet | Optional — app runs fully offline in simulation mode |
| Microphone | Required for live voice input; simulation mode works without it |

---

### Quick Demo Checklist (day-of)

```
□ Developer Mode ON  (Settings → For Developers)
□ Microphone permission ON  (Settings → Privacy → Microphone)
□ Default microphone set  (Settings → Sound → Input device)
□ Filipino language pack installed  (optional — see Language Pack section)
□ BosesApp.exe copied to desktop
□ App launched and welcome message displayed
□ Simulation Mode pill visible in header (orange "SIM MODE")
□ Test: tap 💰 Balance button → response appears in chat
□ Test: tap 🚨 Scam Detection Demo → analysis card appears
□ Test: tap 🔁 Hands-Free → green mode activates
```

---

## 📄 License

This project is developed for passion/educational purposes. For production use, ensure compliance with:
- Banking regulations (BSP, PCI-DSS)
- Data privacy laws (Data Privacy Act of 2012)
- Accessibility standards (WCAG 2.1)

---

## 👥 Contributing

This is a passion project demonstrating enterprise .NET MAUI architecture. Contributions welcome for:
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

This project was built with the help of Anthropic Claude and Microsoft Copilot, whose AI assistance guided development and design

**Built with ❤️ for Filipino accessibility and financial inclusion**
