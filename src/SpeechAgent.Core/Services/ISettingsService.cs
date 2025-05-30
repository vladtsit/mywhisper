using SpeechAgent.Core.Models;

namespace SpeechAgent.Core.Services;

/// <summary>
/// Service interface for managing application settings
/// </summary>
public interface ISettingsService
{    /// <summary>
    /// Loads settings from encrypted storage or returns default empty settings
    /// </summary>
    Task<AppSettings> LoadSettingsAsync();
    
    /// <summary>
    /// Loads default settings (empty/minimal settings that require user configuration)
    /// </summary>
    AppSettings LoadDefaultSettings();

    /// <summary>
    /// Checks if settings are properly configured for the application to work
    /// </summary>
    bool AreSettingsValid(AppSettings? settings = null);

    /// <summary>
    /// Saves settings to encrypted storage in user profile
    /// </summary>
    Task SaveSettingsAsync(AppSettings settings);

    /// <summary>
    /// Checks if user settings exist in encrypted storage
    /// </summary>
    bool UserSettingsExist();

    /// <summary>
    /// Gets the current settings instance
    /// </summary>
    AppSettings CurrentSettings { get; }

    /// <summary>
    /// Event fired when settings are updated
    /// </summary>
    event EventHandler<AppSettings>? SettingsChanged;
}
