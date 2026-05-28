using BosesApp.Core.Data;
using BosesApp.Core.Interfaces;
using BosesApp.Core.Network.Interfaces;
using BosesApp.Core.Network.Services;
using BosesApp.Core.Services;
using BosesApp.Modules.Plugins;
using BosesApp.Presentation.ViewModels;
using BosesApp.Presentation.Views;
using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#if WINDOWS || ANDROID || IOS || MACCATALYST
using Plugin.Maui.Audio;
#endif

namespace BosesApp
{

/// <summary>
/// MAUI application entry point and dependency injection configuration
/// Implements hackathon-safe fallback mechanisms
/// </summary>
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Configure data persistence with fallback
        ConfigureDataServices(builder.Services);

#if WINDOWS || ANDROID || IOS || MACCATALYST
        // Register Plugin.Maui.Audio for real audio recording
        builder.Services.AddSingleton(AudioManager.Current);
#endif

        // Register CommunityToolkit ISpeechToText — routes to native platform APIs:
        //   Windows  → Windows.Media.SpeechRecognition
        //   Android  → SpeechRecognizer (API 33+)
        //   iOS/macOS → SFSpeechRecognizer
        builder.Services.AddSingleton<CommunityToolkit.Maui.Media.ISpeechToText>(CommunityToolkit.Maui.Media.SpeechToText.Default);

        // Register core services
        builder.Services.AddSingleton<IVoiceService, VoiceService>();
        builder.Services.AddSingleton<IAudioRecordingService, AudioRecordingService>();
        builder.Services.AddSingleton<IAudioAnalysisService, AudioAnalysisService>();
        builder.Services.AddSingleton<IVoiceAuthService, RealVoiceAuthService>();
        builder.Services.AddSingleton<IAiOrchestrator, AiOrchestratorService>();
        builder.Services.AddSingleton<IBankApiClient, MockBrankasApiClient>();
        // Phase 3: Production banking API client (mock for demo)
        builder.Services.AddSingleton<IBankingApiClient, MockBankingApiClient>();
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
        builder.Services.AddSingleton<IGuardianNotificationService, GuardianNotificationService>();
        builder.Services.AddSingleton<IAccessibilityService, AccessibilityService>();
        builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();

        // Register Deepgram as a concrete service (Tier 2 — cloud STT for Tagalog + English fallback)
        // Free tier: 12,000 minutes/year — https://console.deepgram.com
        // 🔑 Regenerate your key at console.deepgram.com and paste the new one below.
        builder.Services.AddSingleton<DeepgramSpeechRecognitionService>(sp =>
            new DeepgramSpeechRecognitionService(
                sp.GetRequiredService<IAudioRecordingService>(),
                apiKey: DeepgramSpeechRecognitionService.DefaultApiKey
            ));

        // Register HybridSpeechRecognitionService as ISpeechRecognitionService
        // Tier 1: Native platform STT via CommunityToolkit (Windows / Android API 33+ / iOS)
        // Tier 2: Deepgram cloud (Tagalog + English fallback, or when Tier 1 unavailable)
        // Tier 3: Simulation (canned phrases, when both tiers are unavailable)
        builder.Services.AddSingleton<ISpeechRecognitionService>(sp =>
            new HybridSpeechRecognitionService(
                sp.GetRequiredService<CommunityToolkit.Maui.Media.ISpeechToText>(),
                sp.GetRequiredService<DeepgramSpeechRecognitionService>()
            ));

        // Register plugins
        builder.Services.AddSingleton<BankingPlugin>();
        builder.Services.AddSingleton<GuardianPlugin>();

        // Register ViewModels and Views
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<VoiceRegistrationViewModel>();
        builder.Services.AddTransient<VoiceRegistrationPage>();
        builder.Services.AddTransient<OnboardingViewModel>();
        builder.Services.AddTransient<OnboardingPage>();
        builder.Services.AddTransient<LanguageSelectionViewModel>();
        builder.Services.AddTransient<LanguageSelectionPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void ConfigureDataServices(IServiceCollection services)
    {
        // Determine data path based on platform
        var dataPath = GetDataPath();

        // Configuration flag: Set to true to use JSON fallback instead of SQLite
        // Useful for environments with SQLite file locking issues
        var useJsonFallback = false;

        if (useJsonFallback)
        {
            // JSON fallback mode - no SQLite dependency
            services.AddSingleton<IUserRepository>(sp =>
                new UserRepository(null, dataPath, useJsonFallback: true));
        }
        else
        {
            // Primary SQLite mode with EF Core
            var dbPath = Path.Combine(dataPath, "boses.db");

            services.AddDbContext<BosesDbContext>(options =>
            {
                options.UseSqlite($"Data Source={dbPath}");
                options.EnableSensitiveDataLogging();
            });

            services.AddSingleton<IUserRepository>(sp =>
            {
                var dbContext = sp.GetRequiredService<BosesDbContext>();

                // Apply migrations to ensure database schema is up-to-date
                try
                {
                    // Check if database needs recreation (old schema)
                    bool needsRecreation = false;
                    try
                    {
                        dbContext.Database.OpenConnection();
                        using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                        {
                            // Check if UserProfiles table exists
                            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='UserProfiles'";
                            var tableExists = (long)(command.ExecuteScalar() ?? 0L) > 0;

                            if (tableExists)
                            {
                                // Check if PreferredLanguage column exists
                                command.CommandText = "PRAGMA table_info(UserProfiles)";
                                using (var reader = command.ExecuteReader())
                                {
                                    bool hasPreferredLanguage = false;
                                    while (reader.Read())
                                    {
                                        var columnName = reader[1].ToString();
                                        if (columnName == "PreferredLanguage")
                                        {
                                            hasPreferredLanguage = true;
                                            break;
                                        }
                                    }

                                    if (!hasPreferredLanguage)
                                    {
                                        Console.WriteLine("Old database schema detected (missing PreferredLanguage column). Will recreate database...");
                                        needsRecreation = true;
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        dbContext.Database.CloseConnection();
                    }

                    //dbContext.Database.EnsureDeleted();
                    // If we need to recreate, do it BEFORE accessing schema again
                    if (needsRecreation)
                    {
                        Console.WriteLine("Deleting old database file...");
                        dbContext.Database.EnsureDeleted();
                        Console.WriteLine("Database deleted. Will recreate with new schema...");
                    }

                    // Create database with current schema
                    Console.WriteLine("Ensuring database is created...");
                    dbContext.Database.EnsureCreated();
                    Console.WriteLine("Database ready.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SQLite migration/database setup failed: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    Console.WriteLine("Falling back to JSON storage...");

                    // Automatic fallback to JSON if SQLite fails
                    return new UserRepository(null, dataPath, useJsonFallback: true);
                }

                return new UserRepository(dbContext, dataPath, useJsonFallback: false);
            });
        }
    }

    private static string GetDataPath()
    {
        // Platform-specific data paths
#if ANDROID
        return FileSystem.AppDataDirectory;
#elif IOS || MACCATALYST
        return FileSystem.AppDataDirectory;
#elif WINDOWS
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appPath = Path.Combine(localAppData, "Boses");
        Directory.CreateDirectory(appPath);
        return appPath;
#else
        return FileSystem.AppDataDirectory;
#endif
    }
}
}
