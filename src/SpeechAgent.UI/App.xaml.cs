using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpeechAgent.Core.Services;
using SpeechAgent.UI.Views;

namespace SpeechAgent.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost? _host;
    private ILogger<App>? _logger;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e); var builder = new HostBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
            })
            .ConfigureServices((context, services) =>
            {
                // Register services
                services.AddSingleton<ILogService, LogService>();
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<IAudioRecorder, AudioRecorder>();
                services.AddSingleton<IClipboardService, ClipboardService>();
                services.AddSingleton<ICorrectionService, OpenAiCorrectionService>();
                services.AddSingleton<ITranscriptionService, WhisperService>();

                // Register main window and other windows
                services.AddSingleton<MainWindow>();
                services.AddTransient<SpeechAgent.UI.Views.RuntimeLogsWindow>();
                services.AddTransient<SettingsWindow>();
            }); _host = builder.Build();
        _logger = _host.Services.GetRequiredService<ILogger<App>>();

        _logger.LogInformation("SpeechAgent starting up...");

        // Initialize settings service
        var settingsService = _host.Services.GetRequiredService<ISettingsService>();
        await settingsService.LoadSettingsAsync();
        _logger.LogInformation("Settings loaded");

        // Check if settings are valid, if not force user to settings page
        if (!settingsService.AreSettingsValid())
        {
            _logger.LogWarning("Settings are not configured properly, opening settings window");
            ShowSettingsWindowFirst(settingsService);
        }
        else
        {
            _logger.LogDebug("Creating main window...");
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            _logger.LogInformation("Main window created and shown");
        }
    }
    private void ShowSettingsWindowFirst(ISettingsService settingsService)
    {
        bool settingsConfigured = false;

        while (!settingsConfigured)
        {
            var settingsWindow = _host?.Services.GetRequiredService<SettingsWindow>();
            if (settingsWindow == null)
            {
                _logger?.LogError("Failed to get SettingsWindow from DI container");
                Shutdown();
                return;
            }

            // Subscribe to the SettingsSaved event to open main window immediately when settings are saved
            bool mainWindowOpened = false;
            settingsWindow.SettingsSaved += (s, e) =>
            {
                if (!mainWindowOpened)
                {
                    mainWindowOpened = true;
                    settingsConfigured = true;
                    _logger?.LogInformation("Settings saved during first startup, opening main window");
                    var mainWindow = _host?.Services.GetRequiredService<MainWindow>();
                    mainWindow?.Show();
                }
            };

            // Show a message that settings must be configured
            MessageBox.Show("Welcome to Speech Agent!\n\nBefore you can use the application, you need to configure your Azure OpenAI settings.\n\nThe settings window will open next.",
                          "Initial Setup Required",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);

            var result = settingsWindow.ShowDialog();

            // Check if settings are now valid (in case user closed window without saving)
            if (!settingsConfigured && settingsService.AreSettingsValid())
            {
                settingsConfigured = true;
                _logger?.LogInformation("Settings configured successfully, creating main window");
                var mainWindow = _host?.Services.GetRequiredService<MainWindow>();
                mainWindow?.Show();
            }
            else if (!settingsConfigured)
            {
                var exitResult = MessageBox.Show("Settings are still not properly configured.\n\nThe application requires valid Azure OpenAI settings to function.\n\nWould you like to try again or exit the application?",
                                                "Settings Required",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Warning);

                if (exitResult == MessageBoxResult.No)
                {
                    _logger?.LogInformation("User chose to exit application due to missing settings");
                    Shutdown();
                    return;
                }
            }
        }
    }
    protected override void OnExit(ExitEventArgs e)
    {
        _logger?.LogInformation("Application exiting...");
        _host?.Dispose();
        base.OnExit(e);
    }
}

