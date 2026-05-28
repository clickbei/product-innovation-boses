using BosesApp.Core.Data.Models;
using BosesApp.Core.Interfaces;
using BosesApp.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BosesApp.Presentation.ViewModels;

/// <summary>
/// Main view model for Boses voice interface.
/// Handles voice interaction, AI orchestration, accessibility, analytics, and guardian protection.
/// Zero code-behind — all logic encapsulated here.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IVoiceService _voiceService;
    private readonly IAiOrchestrator _aiOrchestrator;
    private readonly IVoiceAuthService _voiceAuthService;
    private readonly IUserRepository _userRepository;
    private readonly IGuardianNotificationService _guardianNotification;
    private readonly IAccessibilityService _accessibilityService;
    private readonly IAnalyticsService _analytics;

    [ObservableProperty]
    private string _statusMessage = "Tap the microphone to start";

    [ObservableProperty]
    private bool _isListening;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _currentUserName = "Guest";

    [ObservableProperty]
    private bool _isVoiceAuthEnabled;

    [ObservableProperty]
    private string _lastTranscription = string.Empty;

    [ObservableProperty]
    private string _aiResponse = string.Empty;

    [ObservableProperty]
    private bool _isPwd;

    [ObservableProperty]
    private bool _isSimulationMode = true;

    [ObservableProperty]
    private int _guardianAlertsToday;

    public ObservableCollection<ConversationMessage> ConversationHistory { get; } = new();

    private int _currentUserId = 1; // Default user for demo
    private AppLanguage PreferredLanguage { get; set; }// Default to Tagalog for demo

    public MainViewModel(
        IVoiceService voiceService,
        IAiOrchestrator aiOrchestrator,
        IVoiceAuthService voiceAuthService,
        IUserRepository userRepository,
        IGuardianNotificationService guardianNotification,
        IAccessibilityService accessibilityService,
        IAnalyticsService analytics)
    {
        _voiceService = voiceService;
        _aiOrchestrator = aiOrchestrator;
        _voiceAuthService = voiceAuthService;
        _userRepository = userRepository;
        _guardianNotification = guardianNotification;
        _accessibilityService = accessibilityService;
        _analytics = analytics;
    }

    public async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
           
            StatusMessage = "Initializing Boses...";

            // Track screen view (Phase 7 — Analytics)
            await _analytics.TrackScreenViewAsync("MainPage");

            // Initialize AI orchestrator
            await _aiOrchestrator.InitializeAsync();

            // Load or create default user
            var user = await _userRepository.GetUserByIdAsync(_currentUserId);
           
            if (user == null)
            {
                user = await _userRepository.CreateUserAsync(new Core.Data.Models.UserProfile
                {
                    FullName = "Demo User",
                    PhoneNumber = "+639171234567",
                    IsVoiceAuthEnabled = false,
                    GuardianName = "Maria Santos",
                    GuardianPhoneNumber = "+639187654321"
                });
                _currentUserId = user.Id;
            }

            CurrentUserName = user.FullName;
            IsVoiceAuthEnabled = user.IsVoiceAuthEnabled;
            IsPwd = user.UserType == UserType.PWD;
            PreferredLanguage = user.PreferredLanguage;

            // Phase 6 — Accessibility: load and apply profile
            await _accessibilityService.LoadProfileAsync(user.Id);
            _accessibilityService.ApplyProfile();

            // Phase 7 — Analytics: track session feature flags
            await _analytics.TrackEventAsync("user_session_start", new Dictionary<string, string>
            {
                ["user_id"]       = user.Id.ToString(),
                ["is_pwd"]        = IsPwd.ToString(),
                ["voice_auth"]    = IsVoiceAuthEnabled.ToString(),
                ["sim_mode"]      = IsSimulationMode.ToString()
            });

            // Phase 5 — Guardian: count today's alerts for badge
            var events = await _guardianNotification.GetGuardianEventsAsync(user.Id, limit: 100);
            GuardianAlertsToday = events.Count(e => e.CreatedAt.Date == DateTime.Today);

            StatusMessage = $"Welcome, {CurrentUserName}! Tap to speak."; 

            // Add welcome message
            AddMessage("System", "Kumusta! Ako si Boses, ang iyong voice assistant. Magtanong ka tungkol sa iyong bank account, PWD discounts, o iba pang tulong.", false);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Initialization error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleListeningAsync()
    {
        if (IsBusy) return;

        try
        {
            if (IsListening)
            {
                // Stop listening and process
                await StopListeningAsync();
            }
            else
            {
                // Start listening
                await StartListeningAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            IsListening = false;
        }
    }

    private async Task StartListeningAsync()
    {
        IsBusy = true;
        StatusMessage = PreferredLanguage == AppLanguage.Tagalog ? "Simulan ang Mic" :  "Starting microphone...";
        await _voiceService.SpeakAsync(StatusMessage);

        var started = await _voiceService.StartListeningAsync();

        if (started)
        {
            IsListening = true;
            StatusMessage = PreferredLanguage == AppLanguage.Tagalog ? "Nakikinig... Magsalita"  :"Listening... Speak now";
            await _voiceService.SpeakAsync(StatusMessage);
        }
        else
        {
            StatusMessage = PreferredLanguage == AppLanguage.Tagalog ? "Bigo ang pag start ng microphone"  : "Failed to start microphone";
            await _voiceService.SpeakAsync(StatusMessage);
        }

        IsBusy = false;
    }

    private async Task StopListeningAsync()
    {
        IsBusy = true;
        StatusMessage = "Processing your voice...";
        await _voiceService.SpeakAsync(StatusMessage);

        var transcription = await _voiceService.StopListeningAsync();
        IsListening = false;

        
        if (string.IsNullOrWhiteSpace(transcription))
        {
            StatusMessage = "No speech detected. Try again.";
            IsBusy = false;
            return;
        }

        LastTranscription = transcription;
        AddMessage("You", transcription, true);

        // Process with AI
        await ProcessVoiceCommandAsync(transcription);
    }

    private async Task ProcessVoiceCommandAsync(string command)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            StatusMessage = "Thinking...";

            // Phase 7 — Analytics: extract intent category before AI call
            var intent = await _aiOrchestrator.ExtractTransactionIntentAsync(command);
            var intentCategory = intent?.Action ?? "UNKNOWN";

            // Phase 5 — Guardian: check for high-risk transactions
            var needsGuardian = await _aiOrchestrator.RequiresGuardianVerificationAsync(command);
            if (needsGuardian)
            {
                var user = await _userRepository.GetUserByIdAsync(_currentUserId);
                var code = Random.Shared.Next(1000, 9999).ToString();

                // Send guardian SMS/push notification
                if (user?.GuardianPhoneNumber is { Length: > 0 } guardianPhone)
                {
                    await _guardianNotification.SendVerificationSmsAsync(
                        guardianPhone,
                        user.GuardianName ?? "Guardian",
                        user.FullName,
                        command,
                        code);

                    GuardianAlertsToday++;
                }

                await _guardianNotification.LogGuardianEventAsync(new GuardianEvent
                {
                    UserId             = _currentUserId,
                    EventType          = "VERIFICATION_REQUESTED",
                    Description        = "High-risk voice command detected",
                    TransactionDetails = command,
                    Status             = "PENDING"
                });

                await _analytics.TrackGuardianEventAsync("VERIFICATION_REQUESTED", wasScamPrevented: false);

                var guardianMessage =
                    $"⚠️ Para sa iyong seguridad, ipinadala na ang verification request sa iyong guardian. " +
                    $"Hintayin ang kanilang approval bago magpatuloy. (Code: {code})";
                AddMessage("Boses", guardianMessage, false);
                await _voiceService.SpeakAsync(guardianMessage);
                StatusMessage = "Guardian verification required";
                IsBusy = false;
                return;
            }

            // Process command with AI
            var response = await _aiOrchestrator.ProcessCommandAsync(command, _currentUserId);
            AiResponse = response;
            sw.Stop();

            // Phase 7 — Analytics: track voice command
            await _analytics.TrackVoiceCommandAsync(intentCategory, wasSuccessful: true, (int)sw.ElapsedMilliseconds);
            if (intent?.Action is { Length: > 0 })
                await _analytics.TrackFeatureUsageAsync(MapIntentToFeature(intent.Action));

            AddMessage("Boses", response, false);

            // Phase 6 — Accessibility: announce for screen-reader mode
            await _accessibilityService.AnnounceAsync(response);

            StatusMessage = "Speaking response...";
            await _voiceService.SpeakAsync(response);
            StatusMessage = "Tap to speak again";
        }
        catch (Exception ex)
        {
            sw.Stop();
            await _analytics.TrackErrorAsync("voice_command_error", ex.Message, command);
            await _analytics.TrackVoiceCommandAsync("UNKNOWN", wasSuccessful: false, (int)sw.ElapsedMilliseconds);
            var errorMsg = $"Sorry, may problema. Error: {ex.Message}";
            AddMessage("System", errorMsg, false);
            StatusMessage = "Error occurred";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static AnalyticsFeature MapIntentToFeature(string action) => action switch
    {
        "BALANCE_INQUIRY"   => AnalyticsFeature.BankBalance,
        "TRANSFER"          => AnalyticsFeature.BankTransfer,
        "WITHDRAW"          => AnalyticsFeature.BankTransfer,
        "TRANSACTION_HISTORY" => AnalyticsFeature.BankTransactions,
        "PWD_DISCOUNT"      => AnalyticsFeature.PwdDiscount,
        "BILL_PAYMENT"      => AnalyticsFeature.BankTransfer,
        _                   => AnalyticsFeature.VoiceCommand
    };

    [RelayCommand]
    private async Task SimulateVoiceInputAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        _voiceService.SetSimulatedInput(input);
        LastTranscription = input;
        AddMessage("You", input, true);

        await ProcessVoiceCommandAsync(input);
    }

    [RelayCommand]
    private async Task ToggleSimulationModeAsync()
    {
        _voiceService.SimulationMode = !_voiceService.SimulationMode;
        _aiOrchestrator.SimulationMode = _voiceService.SimulationMode;
        _voiceAuthService.SimulationMode = _voiceService.SimulationMode;
        IsSimulationMode = _voiceService.SimulationMode;

        var mode = _voiceService.SimulationMode ? "Simulation" : "Live";
        StatusMessage = $"Mode: {mode}";

        await _analytics.TrackEventAsync("simulation_mode_toggled",
            new Dictionary<string, string> { ["mode"] = mode });
    }

    [RelayCommand]
    private void ClearConversation()
    {
        ConversationHistory.Clear();
        LastTranscription = string.Empty;
        AiResponse = string.Empty;
        StatusMessage = "Conversation cleared. Tap to speak.";
    }

    [RelayCommand]
    private async Task RegisterVoiceAsync()
    {
        try
        {
            // Navigate to voice registration page
            var registrationPage = Application.Current?.Handler?.MauiContext?.Services
                .GetService<Presentation.Views.VoiceRegistrationPage>();

            if (registrationPage != null && Application.Current?.Windows.FirstOrDefault()?.Page is NavigationPage navPage)
            {
                await navPage.PushAsync(registrationPage);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void AddMessage(string sender, string message, bool isUser)
    {
        ConversationHistory.Add(new ConversationMessage
        {
            Sender = sender,
            Message = message,
            Timestamp = DateTime.Now,
            IsUser = isUser
        });
    }
}

public class ConversationMessage
{
    public string Sender { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsUser { get; set; }
}
