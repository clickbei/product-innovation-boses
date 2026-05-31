using BosesApp.Core.Data.Models;
using BosesApp.Core.Interfaces;
using BosesApp.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BosesApp.Presentation.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IVoiceService _voiceService;
    private readonly IAiOrchestrator _aiOrchestrator;
    private readonly IVoiceAuthService _voiceAuthService;
    private readonly IUserRepository _userRepository;
    private readonly IGuardianNotificationService _guardianNotification;
    private readonly IAccessibilityService _accessibilityService;
    private readonly IAnalyticsService _analytics;
    private readonly ILocalizationService _localization;
    private readonly GoogleTranslationService _translation;

    [ObservableProperty] private string _statusMessage = "Tap the microphone to start";
    [ObservableProperty] private bool   _isListening;
    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private string _currentUserName = "Guest";
    [ObservableProperty] private bool   _isVoiceAuthEnabled;
    [ObservableProperty] private string _lastTranscription = string.Empty;
    [ObservableProperty] private string _aiResponse = string.Empty;
    [ObservableProperty] private bool   _isPwd;
    [ObservableProperty] private bool   _isSimulationMode = true;
    [ObservableProperty] private int    _guardianAlertsToday;

    // ── Language toggle ───────────────────────────────────────────────────────
    private AppLanguage _activeLanguage = AppLanguage.Tagalog;
    public AppLanguage ActiveLanguage
    {
        get => _activeLanguage;
        private set
        {
            if (SetProperty(ref _activeLanguage, value))
                RefreshUiStrings();
        }
    }

    public bool IsEnglish  => _activeLanguage == AppLanguage.English;
    public bool IsTagalog  => _activeLanguage == AppLanguage.Tagalog;
    public string LanguageButtonLabel => _activeLanguage == AppLanguage.English ? "🌐 Filipino" : "🌐 English";

    // ── Localized UI strings (bound from XAML) ────────────────────────────────
    [ObservableProperty] private string _labelBalance      = "💰 Balance";
    [ObservableProperty] private string _labelTransactions = "📜 Transactions";
    [ObservableProperty] private string _labelTransfer     = "💸 Transfer";
    [ObservableProperty] private string _labelWithdraw     = "🏧 Withdraw";
    [ObservableProperty] private string _labelGCash        = "📱 GCash";
    [ObservableProperty] private string _labelBills        = "🧾 Bills";
    [ObservableProperty] private string _labelLoan         = "🏦 Loan";
    [ObservableProperty] private string _labelPwdDiscount  = "♿ PWD Discount";
    [ObservableProperty] private string _labelSeniorDiscount = "👴 Senior Discount";
    [ObservableProperty] private string _labelBlockCard    = "🆘 Block Card";
    [ObservableProperty] private string _labelHelp         = "ℹ️ Help";
    [ObservableProperty] private string _labelRegisterVoice   = "🎤 Register Voice";
    [ObservableProperty] private string _labelScamDemo     = "🚨 Scam Detection Demo";
    [ObservableProperty] private string _labelClear        = "🗑️ Clear";
    [ObservableProperty] private string _labelMode         = "⚙️ Mode";
    [ObservableProperty] private string _labelQuickActions = "Quick Actions";
    [ObservableProperty] private string _emptyHint         = "Tap a Quick Action or the mic to start";
    [ObservableProperty] private string _footerHint        = "🎤 Tap mic to record  •  ⚙️ toggle Sim/Live  •  🔁 Hands-Free: no taps needed";


    /// <summary>
    /// When true, the mic restarts automatically after every response
    /// so the user never needs to touch the screen.
    /// Say "stop", "tigilan", or "itigil" to exit by voice.
    /// </summary>
    private bool _isHandsFreeMode;
    public bool IsHandsFreeMode
    {
        get => _isHandsFreeMode;
        set
        {
            if (SetProperty(ref _isHandsFreeMode, value))
            {
                OnPropertyChanged(nameof(HandsFreeModeLabel));
                OnPropertyChanged(nameof(HandsFreeButtonColor));
            }
        }
    }

    public string HandsFreeModeLabel   => _isHandsFreeMode ? "🔁 Hands-Free: ON"  : "🔁 Hands-Free: OFF";
    public Color  HandsFreeButtonColor => _isHandsFreeMode ? Color.FromArgb("#27AE60") : Color.FromArgb("#7F8C8D");

    // Prevents re-entrant hands-free loops
    private bool _handsFreeLoopRunning;

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
        IAnalyticsService analytics,
        ILocalizationService localization,
        GoogleTranslationService translation)
    {
        _voiceService       = voiceService;
        _aiOrchestrator     = aiOrchestrator;
        _voiceAuthService   = voiceAuthService;
        _userRepository     = userRepository;
        _guardianNotification = guardianNotification;
        _accessibilityService = accessibilityService;
        _analytics          = analytics;
        _localization       = localization;
        _translation        = translation;
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
                    GuardianPhoneNumber = "+639187654321",
                    PreferredLanguage = AppLanguage.English
                });
                _currentUserId = user.Id;
            }

            CurrentUserName = user.FullName;
            IsVoiceAuthEnabled = user.IsVoiceAuthEnabled;
            IsPwd = user.UserType == UserType.PWD;
            PreferredLanguage = user.PreferredLanguage;
            ActiveLanguage = user.PreferredLanguage;   // apply to UI labels
            _localization.SetLanguage(user.PreferredLanguage);
            _voiceService.SetActiveLanguage(user.PreferredLanguage == AppLanguage.Tagalog ? "fil-PH" : "en-US");
            RefreshUiStrings();

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

            string welcomeMessage = "Kumusta, " + CurrentUserName + "! Ako si Boses, ang iyong voice assistant. Magtanong ka tungkol sa iyong bank account, PWD discounts, o iba pang tulong.";
            // Add welcome message
            AddMessage("System", welcomeMessage, false);
            await _voiceService.SpeakAsync(welcomeMessage);
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

    [RelayCommand]
    private async Task ToggleHandsFreeModeAsync()
    {
        IsHandsFreeMode = !IsHandsFreeMode;

        await _analytics.TrackEventAsync("hands_free_toggled",
            new Dictionary<string, string> { ["enabled"] = IsHandsFreeMode.ToString() });

        if (IsHandsFreeMode)
        {
            StatusMessage = "🔁 Hands-Free ON — listening automatically";
            await _voiceService.SpeakAsync("Hands-free mode on. I'm listening.");
            await RunHandsFreeCycleAsync();
        }
        else
        {
            StatusMessage = "🔁 Hands-Free OFF";
            await _voiceService.SpeakAsync("Hands-free mode off.");
            if (IsListening)
            {
                IsListening = false;
                await _voiceService.StopListeningAsync();
            }
        }
    }

    /// <summary>
    /// Continuous listen → process → speak loop.
    /// Keeps running until <see cref="IsHandsFreeMode"/> is set to false.
    /// </summary>
    private async Task RunHandsFreeCycleAsync()
    {
        if (_handsFreeLoopRunning) return;
        _handsFreeLoopRunning = true;

        try
        {
            while (IsHandsFreeMode)
            {
                // Start mic
                await StartListeningAsync();

                if (!IsListening)
                {
                    await Task.Delay(2000); // mic failed — wait before retry
                    continue;
                }

                // Hold mic open up to 8 seconds then auto-stop
                var timeout = Task.Delay(TimeSpan.FromSeconds(8));
                while (IsListening && !timeout.IsCompleted)
                    await Task.Delay(200);

                if (!IsHandsFreeMode) break;

                // Stop and transcribe
                var transcription = await StopListeningForHandsFreeAsync();

                if (!IsHandsFreeMode) break;

                if (string.IsNullOrWhiteSpace(transcription))
                {
                    StatusMessage = "🔁 Listening again...";
                    await Task.Delay(600);
                    continue;
                }

                // Check for voice exit commands
                if (IsStopCommand(transcription))
                {
                    IsHandsFreeMode = false;
                    StatusMessage   = "🔁 Hands-Free OFF";
                    await _voiceService.SpeakAsync("Hands-free mode off.");
                    break;
                }

                AddMessage("You", transcription, true);
                await ProcessVoiceCommandAsync(transcription);

                if (!IsHandsFreeMode) break;

                await Task.Delay(600); // brief gap between cycles
            }
        }
        finally
        {
            _handsFreeLoopRunning = false;
        }
    }

    private static bool IsStopCommand(string text)
    {
        var t = text.Trim().ToLowerInvariant();
        return t is "stop" or "exit" or "quit" or "cancel" or "tigilan" or "itigil" or "hinto";
    }

    private async Task<string?> StopListeningForHandsFreeAsync()
    {
        IsBusy        = true;
        StatusMessage = "Processing your voice...";
        var transcription = await _voiceService.StopListeningAsync();
        IsListening       = false;
        IsBusy            = false;
        if (!string.IsNullOrWhiteSpace(transcription))
            LastTranscription = transcription;
        return transcription;
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
            StatusMessage = IsHandsFreeMode ? "🔁 Listening again..." : "Tap to speak again";
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

    // ── Language toggle ───────────────────────────────────────────────────────

    [RelayCommand]
    private async Task ToggleLanguageAsync()
    {
        var next = ActiveLanguage == AppLanguage.English
            ? AppLanguage.Tagalog
            : AppLanguage.English;

        _localization.SetLanguage(next);
        ActiveLanguage = next;
        PreferredLanguage = next;

        // Notify VoiceService so STT locale and demo phrase language both update
        var ietf = next == AppLanguage.Tagalog ? "fil-PH" : "en-US";
        _voiceService.SetActiveLanguage(ietf);

        var confirmMsg = next == AppLanguage.Tagalog
            ? "Napili ang Filipino."
            : "English selected.";
        StatusMessage = confirmMsg;
        await _voiceService.SpeakAsync(confirmMsg);

        await _analytics.TrackEventAsync("language_switched",
            new Dictionary<string, string> { ["language"] = next.ToString() });
    }

    /// <summary>
    /// Re-translates all bound UI label strings to match the active language.
    /// Called once on init and every time the language toggles.
    /// </summary>
    private void RefreshUiStrings()
    {
        bool fil = ActiveLanguage == AppLanguage.Tagalog;

        _labelBalance        = fil ? "💰 Balanse"                : "💰 Balance";
        _labelTransactions   = fil ? "📜 Mga Transaksyon"        : "📜 Transactions";
        _labelTransfer       = fil ? "💸 Transfer"                : "💸 Transfer";
        _labelWithdraw       = fil ? "🏧 Mag-withdraw"            : "🏧 Withdraw";
        _labelGCash          = fil ? "📱 GCash"                   : "📱 GCash";
        _labelBills          = fil ? "🧾 Mga Bayarin"             : "🧾 Bills";
        _labelLoan           = fil ? "🏦 Loan"                    : "🏦 Loan";
        _labelPwdDiscount    = fil ? "♿ PWD Diskwento"           : "♿ PWD Discount";
        _labelSeniorDiscount = fil ? "👴 Senior Diskwento"         : "👴 Senior Discount";
        _labelBlockCard      = fil ? "🆘 I-block ang Card"        : "🆘 Block Card";
        _labelHelp           = fil ? "ℹ️ Tulong"                  : "ℹ️ Help";
        _labelRegisterVoice  = fil ? "🎤 Irehistro ang Boses"     : "🎤 Register Voice";
        _labelScamDemo       = fil ? "🚨 Demo ng Scam Detection"  : "🚨 Scam Detection Demo";
        _labelClear          = fil ? "🗑️ Burahin"               : "🗑️ Clear";
        _labelMode           = fil ? "⚙️ Mode"                   : "⚙️ Mode";
        _labelQuickActions   = fil ? "Mabilis na Aksyon"          : "Quick Actions";
        _emptyHint           = fil ? "I-tap ang isang aksyon o ang mikropono"
                                   : "Tap a Quick Action or the mic to start";
        _footerHint          = fil ? "🎤 I-tap ang mic  •  ⚙️ Sim/Live  •  🔁 Hands-Free"
                                   : "🎤 Tap mic to record  •  ⚙️ toggle Sim/Live  •  🔁 Hands-Free: no taps needed";

        OnPropertyChanged(nameof(LabelBalance));
        OnPropertyChanged(nameof(LabelTransactions));
        OnPropertyChanged(nameof(LabelTransfer));
        OnPropertyChanged(nameof(LabelWithdraw));
        OnPropertyChanged(nameof(LabelGCash));
        OnPropertyChanged(nameof(LabelBills));
        OnPropertyChanged(nameof(LabelLoan));
        OnPropertyChanged(nameof(LabelPwdDiscount));
        OnPropertyChanged(nameof(LabelSeniorDiscount));
        OnPropertyChanged(nameof(LabelBlockCard));
        OnPropertyChanged(nameof(LabelHelp));
        OnPropertyChanged(nameof(LabelRegisterVoice));
        OnPropertyChanged(nameof(LabelScamDemo));
        OnPropertyChanged(nameof(LabelClear));
        OnPropertyChanged(nameof(LabelMode));
        OnPropertyChanged(nameof(LabelQuickActions));
        OnPropertyChanged(nameof(EmptyHint));
        OnPropertyChanged(nameof(FooterHint));
        OnPropertyChanged(nameof(LanguageButtonLabel));
        OnPropertyChanged(nameof(IsEnglish));
        OnPropertyChanged(nameof(IsTagalog));
    }

    private static AnalyticsFeature MapIntentToFeature(string action) => action switch
    {
        "BALANCE_INQUIRY"    => AnalyticsFeature.BankBalance,
        "TRANSFER"           => AnalyticsFeature.BankTransfer,
        "WITHDRAW"           => AnalyticsFeature.BankTransfer,
        "EWALLET_SEND"       => AnalyticsFeature.BankTransfer,
        "TRANSACTION_HISTORY"=> AnalyticsFeature.BankTransactions,
        "BILL_PAYMENT"       => AnalyticsFeature.BankTransfer,
        "PWD_DISCOUNT"       => AnalyticsFeature.PwdDiscount,
        "SENIOR_DISCOUNT"    => AnalyticsFeature.PwdDiscount,
        "LOAN_INQUIRY"       => AnalyticsFeature.VoiceCommand,
        "EMERGENCY_BLOCK"    => AnalyticsFeature.VoiceCommand,
        "HELP"               => AnalyticsFeature.VoiceCommand,
        "GREETING"           => AnalyticsFeature.VoiceCommand,
        _                    => AnalyticsFeature.VoiceCommand
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

    /// <summary>
    /// Scripted scam-detection demo.
    /// Picks a random realistic scam script, "plays" it as an incoming message,
    /// runs it through scam detection, and displays the full analysis.
    /// </summary>
    [RelayCommand]
    private async Task SimulateScamDemoAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        StatusMessage = "🚨 Running scam detection demo...";

        try
        {
            // Rotate through several realistic scam scenarios
            var scenarios = new[]
            {
                new {
                    Label = "OTP Harvesting",
                    Script = "Hello po, ito ay si Mark mula sa BDO Customer Service. " +
                             "Na-detect namin na may suspicious activity sa inyong account. " +
                             "Para ma-secure ang inyong account, kailangan naming i-verify ang inyong identity. " +
                             "Paki-share ang OTP na naka-send sa inyong cellphone."
                },
                new {
                    Label = "Fake Prize Scam",
                    Script = "Congratulations! Nanalo kayo ng 50,000 pesos sa aming raffle promo. " +
                             "Para ma-claim ang inyong prize, kailangan munang mag-bayad ng 500 pesos " +
                             "processing fee sa aming account. Ibigay po ang inyong GCash number."
                },
                new {
                    Label = "Account Suspension Threat",
                    Script = "URGENT: Ang inyong BPI account ay naka-suspend dahil sa hindi verified na impormasyon. " +
                             "Kailangan ninyong i-verify ang inyong account number at password ngayon " +
                             "bago mabura ang inyong account sa loob ng 24 hours."
                },
                new {
                    Label = "Remote Access Scam",
                    Script = "Hi, this is Maya technical support. We detected malware on your phone. " +
                             "Please download AnyDesk immediately so our technician can remove it remotely " +
                             "and secure your account."
                }
            };

            var scenario = scenarios[DateTime.Now.Second % scenarios.Length];

            // Step 1 — show the incoming scam message
            await Task.Delay(500);
            AddMessage("📞 Incoming Call", scenario.Script, false);
            StatusMessage = "🔍 Analyzing for scam signals...";
            await _voiceService.SpeakAsync("Nakatanggap ng mensahe. Ini-analyze para sa scam signals.");

            // Step 2 — run detection
            var result = await _aiOrchestrator.SimulateScamDetectionAsync(scenario.Script);

            // Step 3 — show analysis result
            if (result.IsScam)
            {
                var redFlagList = result.RedFlags.Count > 0
                    ? "\n\n🚩 Red Flags:\n• " + string.Join("\n• ", result.RedFlags)
                    : string.Empty;

                var analysisMessage =
                    $"🚨 SCAM DETECTED — {result.Category}\n" +
                    $"Confidence: {result.ConfidencePercent}%\n\n" +
                    $"📋 {result.Explanation}" +
                    redFlagList +
                    $"\n\n✅ What to do: {result.RecommendedAction}";

                AddMessage("🛡️ Boses Shield", analysisMessage, false);
                await _voiceService.SpeakAsync(
                    $"Babala! Scam detected. {result.Category}. {result.Explanation} {result.RecommendedAction}");

                // Log as scam-prevented guardian event
                await _guardianNotification.LogGuardianEventAsync(new GuardianEvent
                {
                    UserId             = _currentUserId,
                    EventType          = "SCAM_BLOCKED",
                    Description        = $"Scam call blocked: {result.Category}",
                    TransactionDetails = scenario.Script,
                    Status             = "BLOCKED"
                });

                await _analytics.TrackGuardianEventAsync("SCAM_BLOCKED", wasScamPrevented: true);
                GuardianAlertsToday++;

                StatusMessage = $"🚨 Scam blocked — {result.Category}";
            }
            else
            {
                var safeMessage =
                    $"✅ Message appears safe (confidence: {result.ConfidencePercent}% scam likelihood).\n" +
                    $"{result.Explanation}";

                AddMessage("🛡️ Boses Shield", safeMessage, false);
                await _voiceService.SpeakAsync("Mukhang ligtas ang mensaheng ito.");
                StatusMessage = "✅ No scam signals detected";
            }

            await _analytics.TrackEventAsync("scam_demo_run", new Dictionary<string, string>
            {
                ["scenario"] = scenario.Label,
                ["detected"] = result.IsScam.ToString(),
                ["category"] = result.Category
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ScamDemo] Error: {ex.Message}");
            StatusMessage = "Scam demo error";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RegisterVoiceAsync()
    {
        try
        {
            var registrationPage = Application.Current?.Handler?.MauiContext?.Services
                .GetService<Presentation.Views.VoiceRegistrationPage>();

            if (registrationPage != null && Application.Current?.Windows.FirstOrDefault()?.Page is NavigationPage navPage)
            {
                registrationPage.SetUserId(_currentUserId);
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
