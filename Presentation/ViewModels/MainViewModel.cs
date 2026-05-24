using BosesApp.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BosesApp.Presentation.ViewModels;

/// <summary>
/// Main view model for Boses voice interface
/// Handles voice interaction, AI orchestration, and UI state
/// Zero code-behind - all logic encapsulated here
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IVoiceService _voiceService;
    private readonly IAiOrchestrator _aiOrchestrator;
    private readonly IVoiceAuthService _voiceAuthService;
    private readonly IUserRepository _userRepository;

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

    public ObservableCollection<ConversationMessage> ConversationHistory { get; } = new();

    private int _currentUserId = 1; // Default user for demo

    public MainViewModel(
        IVoiceService voiceService,
        IAiOrchestrator aiOrchestrator,
        IVoiceAuthService voiceAuthService,
        IUserRepository userRepository)
    {
        _voiceService = voiceService;
        _aiOrchestrator = aiOrchestrator;
        _voiceAuthService = voiceAuthService;
        _userRepository = userRepository;
    }

    public async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Initializing Boses...";

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
        StatusMessage = "Starting microphone...";

        var started = await _voiceService.StartListeningAsync();
        if (started)
        {
            IsListening = true;
            StatusMessage = "🎤 Listening... Speak now";
        }
        else
        {
            StatusMessage = "Failed to start microphone";
        }

        IsBusy = false;
    }

    private async Task StopListeningAsync()
    {
        IsBusy = true;
        StatusMessage = "Processing your voice...";

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
        try
        {
            StatusMessage = "Thinking...";

            // Check if guardian verification is needed
            var needsGuardian = await _aiOrchestrator.RequiresGuardianVerificationAsync(command);
            if (needsGuardian)
            {
                var guardianMessage = "⚠️ Para sa iyong seguridad, kailangan ng guardian verification para sa transaksyon na ito. Gusto mo bang magpatuloy?";
                AddMessage("Boses", guardianMessage, false);
                await _voiceService.SpeakAsync(guardianMessage);
                StatusMessage = "Guardian verification required";
                IsBusy = false;
                return;
            }

            // Process command with AI
            var response = await _aiOrchestrator.ProcessCommandAsync(command, _currentUserId);
            AiResponse = response;

            AddMessage("Boses", response, false);

            // Speak response
            StatusMessage = "Speaking response...";
            await _voiceService.SpeakAsync(response);

            StatusMessage = "Tap to speak again";
        }
        catch (Exception ex)
        {
            var errorMsg = $"Sorry, may problema. Error: {ex.Message}";
            AddMessage("System", errorMsg, false);
            StatusMessage = "Error occurred";
        }
        finally
        {
            IsBusy = false;
        }
    }

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

        var mode = _voiceService.SimulationMode ? "Simulation" : "Live";
        StatusMessage = $"Mode: {mode}";

        await Task.CompletedTask;
    }

    [RelayCommand]
    private void ClearConversation()
    {
        ConversationHistory.Clear();
        LastTranscription = string.Empty;
        AiResponse = string.Empty;
        StatusMessage = "Conversation cleared. Tap to speak.";
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
