# 🏗️ Boses Architecture Documentation

## Overview

Boses implements a **Monolithic Modular Architecture** with clean separation of concerns, designed for both hackathon resilience and production scalability.

---

## 🎯 Architectural Principles

### 1. Clean Architecture Layers

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│    (Views, ViewModels, UI Logic)        │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│         Application Layer               │
│   (AI Orchestration, Use Cases)         │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│          Domain Layer                   │
│    (Business Logic, Plugins)            │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      Infrastructure Layer               │
│  (Data Access, External Services)       │
└─────────────────────────────────────────┘
```

### 2. MVVM Pattern (Zero Code-Behind)

**Rule**: Views contain ONLY UI markup. All logic resides in ViewModels.

**Benefits**:
- Testable business logic
- Clear separation of concerns
- Easy to mock dependencies
- Platform-agnostic logic

**Example**:
```csharp
// ❌ BAD: Logic in code-behind
public partial class MainPage : ContentPage
{
    private void OnButtonClicked(object sender, EventArgs e)
    {
        // Business logic here - WRONG!
    }
}

// ✅ GOOD: Logic in ViewModel
public partial class MainViewModel : ObservableObject
{
    [RelayCommand]
    private async Task ProcessCommandAsync()
    {
        // Business logic here - CORRECT!
    }
}
```

### 3. Dependency Injection

All services are registered in `MauiProgram.cs` and injected via constructors.

**Service Lifetimes**:
- **Singleton**: Services that maintain state across the app (VoiceService, AIOrchestrator)
- **Transient**: ViewModels and Views (new instance per navigation)
- **Scoped**: Not used in MAUI (no request scope)

---

## 📦 Module Breakdown

### Core Module

**Purpose**: Business logic, data access, and external service integrations

**Components**:

#### Data Layer
- `BosesDbContext`: EF Core SQLite context
- `UserProfile`: User entity model
- `UserRepository`: Dual persistence (SQLite + JSON fallback)

#### Services Layer
- `VoiceService`: Speech-to-text and text-to-speech
- `VoiceAuthService`: Voice biometric authentication
- `AiOrchestratorService`: Semantic Kernel AI orchestration

#### Network Layer
- `IBankApiClient`: Open Banking API contract
- `MockBrankasApiClient`: Simulated Brankas integration

### Modules (Plugins)

**Purpose**: Domain-specific feature implementations

**Semantic Kernel Plugins**:

#### BankingPlugin
- `get_account_balance`: Retrieve account balance
- `get_recent_transactions`: Fetch transaction history
- `transfer_funds`: Execute money transfers
- `calculate_pwd_discount`: PWD discount calculator

#### GuardianPlugin
- `check_transaction_risk`: Risk assessment algorithm
- `request_guardian_verification`: Guardian approval flow
- `verify_guardian_code`: Code verification
- `detect_scam_patterns`: Scam detection engine

### Presentation Module

**Purpose**: UI rendering and user interaction

**Components**:
- `MainViewModel`: Primary application state and commands
- `MainPage.xaml`: UI markup (zero code-behind)
- Value converters for XAML bindings

---

## 🔄 Data Flow Architecture

### Voice Command Processing Flow

```
User Voice Input
      │
      ▼
┌─────────────────┐
│  VoiceService   │ ◄── Deepgram (Production) / Simulation (Demo)
└────────┬────────┘
         │ Transcribed Text
         ▼
┌─────────────────┐
│ MainViewModel   │
└────────┬────────┘
         │ Command String
         ▼
┌─────────────────┐
│ AIOrchestrator  │ ◄── Semantic Kernel + Google Gemini
└────────┬────────┘
         │ Intent Extraction
         ▼
┌─────────────────┐
│ Plugin Router   │
└────────┬────────┘
         │
    ┌────┴────┐
    ▼         ▼
┌─────────┐ ┌──────────┐
│ Banking │ │ Guardian │
│ Plugin  │ │ Plugin   │
└────┬────┘ └────┬─────┘
     │           │
     ▼           ▼
┌─────────────────┐
│  Bank API       │
│  Client         │
└────────┬────────┘
         │ Response
         ▼
┌─────────────────┐
│  VoiceService   │ ◄── Text-to-Speech
└─────────────────┘
         │
         ▼
    User Audio Output
```

### Guardian Verification Flow

```
High-Risk Transaction Detected
         │
         ▼
┌─────────────────────┐
│ GuardianPlugin      │
│ check_transaction   │
│ _risk()             │
└─────────┬───────────┘
          │ Risk Score > 50
          ▼
┌─────────────────────┐
│ request_guardian    │
│ _verification()     │
└─────────┬───────────┘
          │ SMS Sent (Production)
          │ Notification (Demo)
          ▼
┌─────────────────────┐
│ Guardian Receives   │
│ Verification Code   │
└─────────┬───────────┘
          │ Guardian Approves
          ▼
┌─────────────────────┐
│ verify_guardian     │
│ _code()             │
└─────────┬───────────┘
          │ Approved
          ▼
┌─────────────────────┐
│ Transaction         │
│ Proceeds            │
└─────────────────────┘
```

---

## 💾 Persistence Strategy

### Dual Persistence Layer

**Primary**: SQLite via Entity Framework Core  
**Fallback**: JSON flat files via System.Text.Json

**Why Dual Persistence?**
1. **SQLite Issues**: File locking in some demo environments
2. **Portability**: JSON works everywhere
3. **Debugging**: Easy to inspect JSON files
4. **Resilience**: Automatic fallback on SQLite failure

**Implementation**:
```csharp
// MauiProgram.cs
var useJsonFallback = false; // Toggle here

if (useJsonFallback)
{
    services.AddSingleton<IUserRepository>(sp =>
        new UserRepository(null, dataPath, useJsonFallback: true));
}
else
{
    // SQLite with automatic fallback
    services.AddSingleton<IUserRepository>(sp =>
    {
        try
        {
            var dbContext = sp.GetRequiredService<BosesDbContext>();
            dbContext.Database.EnsureCreated();
            return new UserRepository(dbContext, dataPath, false);
        }
        catch
        {
            // Auto-fallback to JSON
            return new UserRepository(null, dataPath, true);
        }
    });
}
```

---

## 🎭 Simulation Mode Architecture

### Purpose
Enable demo/testing without real hardware or API dependencies.

### Simulation Flags

All services expose a `SimulationMode` property:

```csharp
public interface IVoiceService
{
    bool SimulationMode { get; set; }
    void SetSimulatedInput(string input);
}

public interface IAiOrchestrator
{
    bool SimulationMode { get; set; }
}

public interface IVoiceAuthService
{
    bool SimulationMode { get; set; }
    void SetSimulatedAuthResult(bool shouldPass);
}
```

### Simulation Behaviors

| Service | Simulation Mode Behavior |
|---------|-------------------------|
| **VoiceService** | Returns pre-defined responses or simulated input |
| **VoiceAuthService** | Generates deterministic voice vectors, controllable auth results |
| **AIOrchestrator** | Pattern-matching instead of real AI inference |
| **BankApiClient** | Mock data with realistic delays |

### Controlled Simulation Matrix

For hackathon presentations, you can force specific scenarios:

```csharp
// Force voice auth failure
_voiceAuthService.SetSimulatedAuthResult(false);

// Simulate specific voice input
_voiceService.SetSimulatedInput("Transfer 10000 pesos");

// All services respect simulation mode
_voiceService.SimulationMode = true;
_aiOrchestrator.SimulationMode = true;
_voiceAuthService.SimulationMode = true;
```

---

## 🔐 Security Architecture

### Voice Biometric Authentication

**Algorithm** (Simulated):
1. Extract 128-dimensional voice vector from audio
2. Store as serialized JSON in database
3. On verification, compare vectors using cosine similarity
4. Threshold: 85% similarity required

**Production Integration Points**:
- Replace mock vector generation with MFCC/i-vector extraction
- Integrate with specialized voice biometric APIs (Pindrop, Nuance)
- Add liveness detection and anti-spoofing

### Guardian Anti-Scam System

**Risk Scoring Algorithm**:
```
Risk Score = Amount Risk + Recipient Risk + Urgency Risk

Amount Risk:
  > 50,000 PHP: +40 points
  > 10,000 PHP: +25 points
  > 5,000 PHP:  +15 points

Recipient Risk:
  Unknown recipient: +30 points

Urgency Risk:
  Contains "urgent", "emergency", "immediately": +20 points

Total Score > 50: Guardian verification required
```

**Scam Pattern Detection**:
- Keyword matching (urgent, prize, winner, etc.)
- Pressure tactics detection
- Unusual transaction patterns

---

## 🚀 Production Migration Path

### Phase 1: Voice Services
```csharp
// Replace VoiceService simulation with Deepgram
public class DeepgramVoiceService : IVoiceService
{
    private readonly DeepgramClient _client;
    
    public async Task<string> StopListeningAsync()
    {
        // Real Deepgram WebSocket integration
        return await _client.GetTranscriptionAsync();
    }
}
```

### Phase 2: AI Integration
```csharp
// Add Google Gemini to Semantic Kernel
var builder = Kernel.CreateBuilder();
builder.AddGoogleGeminiChatCompletion(
    "gemini-pro",
    Environment.GetEnvironmentVariable("GEMINI_API_KEY")
);
```

### Phase 3: Banking APIs
```csharp
// Replace MockBrankasApiClient with real implementation
public class BrankasApiClient : IBankApiClient
{
    private readonly HttpClient _httpClient;
    
    public async Task<BankAccountBalance> GetBalanceAsync(string accountId)
    {
        var response = await _httpClient.GetAsync(
            $"https://api.brankas.com/v1/accounts/{accountId}/balance"
        );
        // Real API integration
    }
}
```

---

## 📊 Performance Considerations

### Async/Await Best Practices
- All I/O operations are async
- No blocking calls on UI thread
- CancellationToken support for long operations

### Memory Management
- Dispose DbContext properly (handled by DI)
- Clear conversation history periodically
- Limit voice recording buffer size

### Network Resilience
- Realistic delays in mock services (500-4000ms)
- Timeout handling
- Retry logic for transient failures

---

## 🧪 Testing Strategy

### Unit Testing
```csharp
[Fact]
public async Task ProcessCommand_BalanceInquiry_ReturnsBalance()
{
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var orchestrator = new AiOrchestratorService(mockRepo.Object);
    orchestrator.SimulationMode = true;
    
    // Act
    var result = await orchestrator.ProcessCommandAsync(
        "Magkano ang balance ko?", 
        userId: 1
    );
    
    // Assert
    Assert.Contains("balanse", result.ToLower());
}
```

### Integration Testing
- Test SQLite → JSON fallback
- Verify plugin registration
- Test voice auth flow end-to-end

---

## 📝 Code Quality Standards

### Naming Conventions
- Interfaces: `IServiceName`
- Async methods: `MethodNameAsync`
- Private fields: `_camelCase`
- Public properties: `PascalCase`

### Documentation
- XML comments on all public APIs
- Inline comments for complex logic
- Architecture decision records (this document)

### Error Handling
- Try-catch at service boundaries
- Meaningful error messages
- Graceful degradation (fallbacks)

---

## 🔄 Future Enhancements

### Planned Features
1. **Multi-language Support**: Cebuano, Ilocano, Hiligaynon
2. **Offline Mode**: Cached responses for common queries
3. **Voice Training**: Personalized voice models
4. **Advanced Analytics**: Transaction insights and spending patterns
5. **Integration Hub**: Connect more banks and services

### Scalability Considerations
- Microservices migration path
- Cloud deployment (Azure, AWS)
- Distributed caching (Redis)
- Message queue integration (RabbitMQ, Azure Service Bus)

---

**This architecture balances hackathon speed with production-ready design patterns.**
