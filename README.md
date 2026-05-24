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
├── Core/                          # Core business logic layer
│   ├── Data/                      # Data models and DbContext
│   │   ├── BosesDbContext.cs
│   │   └── Models/
│   │       └── UserProfile.cs
│   ├── Network/                   # API clients and network services
│   │   ├── Interfaces/
│   │   │   └── IBankApiClient.cs
│   │   └── Services/
│   │       └── MockBrankasApiClient.cs
│   ├── Interfaces/                # Service contracts
│   │   ├── IAiOrchestrator.cs
│   │   ├── IVoiceService.cs
│   │   ├── IVoiceAuthService.cs
│   │   └── IUserRepository.cs
│   └── Services/                  # Service implementations
│       ├── AiOrchestratorService.cs
│       ├── VoiceService.cs
│       ├── VoiceAuthService.cs
│       └── UserRepository.cs
├── Modules/                       # Feature plugins
│   └── Plugins/
│       ├── BankingPlugin.cs       # Banking operations
│       └── GuardianPlugin.cs      # Anti-scam protection
├── Presentation/                  # UI layer (MVVM)
│   ├── ViewModels/
│   │   └── MainViewModel.cs
│   └── Views/
│       ├── MainPage.xaml
│       └── MainPage.xaml.cs
├── Platforms/                     # Platform-specific code
│   ├── Android/
│   ├── iOS/
│   ├── Windows/
│   └── MacCatalyst/
└── Resources/                     # App resources
    ├── Styles/
    ├── Fonts/
    └── Images/
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

- **.NET 8 SDK** or later
- **Visual Studio 2022** (17.8+) or **Visual Studio Code** with C# Dev Kit
- **Android SDK** (for Android development)
- **Xcode** (for iOS/Mac development, macOS only)

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
- **Microsoft.Maui.Controls** (8.0.90) - MAUI framework
- **CommunityToolkit.Mvvm** (8.2.2) - MVVM helpers
- **Microsoft.EntityFrameworkCore.Sqlite** (8.0.8) - SQLite database
- **Microsoft.SemanticKernel** (1.20.0) - AI orchestration
- **System.Text.Json** (8.0.4) - JSON serialization

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

## 🌐 Production Integration Roadmap

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

### SQLite Database Issues
**Problem**: "Database is locked" or file access errors  
**Solution**: Set `useJsonFallback = true` in `MauiProgram.cs`

### Microphone Not Working
**Problem**: Voice input not captured  
**Solution**: 
1. Check platform permissions (AndroidManifest.xml, Info.plist)
2. Enable simulation mode for testing
3. Use quick action buttons as alternative

### Build Errors
**Problem**: NuGet package restore failures  
**Solution**:
```bash
dotnet nuget locals all --clear
dotnet restore --force
```

### Android Deployment Issues
**Problem**: App crashes on startup  
**Solution**: Ensure Android SDK 21+ is installed and emulator is running

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
