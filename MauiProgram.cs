using BosesApp.Core.Data;
using BosesApp.Core.Interfaces;
using BosesApp.Core.Network.Interfaces;
using BosesApp.Core.Network.Services;
using BosesApp.Core.Services;
using BosesApp.Modules.Plugins;
using BosesApp.Presentation.ViewModels;
using BosesApp.Presentation.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Configure data persistence with fallback
        ConfigureDataServices(builder.Services);

        // Register core services
        builder.Services.AddSingleton<IVoiceService, VoiceService>();
        builder.Services.AddSingleton<IVoiceAuthService, VoiceAuthService>();
        builder.Services.AddSingleton<IAiOrchestrator, AiOrchestratorService>();
        builder.Services.AddSingleton<IBankApiClient, MockBrankasApiClient>();

        // Register plugins
        builder.Services.AddSingleton<BankingPlugin>();
        builder.Services.AddSingleton<GuardianPlugin>();

        // Register ViewModels and Views
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();

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

                // Ensure database is created
                try
                {
                    dbContext.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SQLite initialization failed: {ex.Message}");
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
