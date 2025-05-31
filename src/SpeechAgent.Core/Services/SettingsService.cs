using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpeechAgent.Core.Models;

namespace SpeechAgent.Core.Services;

/// <summary>
/// Service for managing application settings with encryption support
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly string _settingsDirectory;
    private readonly string _settingsFilePath;
    private AppSettings _currentSettings;

    public AppSettings CurrentSettings => _currentSettings;

    public event EventHandler<AppSettings>? SettingsChanged;

    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;

        _settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpeechAgent");
        _settingsFilePath = Path.Combine(_settingsDirectory, "settings.dat");

        _currentSettings = new AppSettings();

        // Ensure settings directory exists
        Directory.CreateDirectory(_settingsDirectory);
    }    /// <summary>
         /// Loads settings from encrypted storage or returns default empty settings
         /// </summary>
    public async Task<AppSettings> LoadSettingsAsync()
    {
        try
        {
            if (UserSettingsExist())
            {
                _logger.LogInformation("Loading settings from encrypted user storage");
                _currentSettings = await LoadFromEncryptedFileAsync();
            }
            else
            {
                _logger.LogInformation("No user settings found, creating default empty settings");
                _currentSettings = CreateDefaultSettings();
            }

            // Ensure correction prompt is never null
            if (_currentSettings.CorrectionPrompt == null)
            {
                _currentSettings.CorrectionPrompt = GetDefaultCorrectionPrompt();
            }

            return _currentSettings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings, creating default empty settings");
            _currentSettings = CreateDefaultSettings();
            return _currentSettings;
        }
    }

    /// <summary>
    /// Saves settings to encrypted storage in user profile
    /// </summary>
    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            await SaveToEncryptedFileAsync(settings);
            _currentSettings = settings.Clone();
            SettingsChanged?.Invoke(this, _currentSettings);
            _logger.LogInformation("Settings saved successfully to encrypted storage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings");
            throw;
        }
    }    /// <summary>
         /// Checks if user settings exist in encrypted storage
         /// </summary>
    public bool UserSettingsExist()
    {
        return File.Exists(_settingsFilePath);
    }
    /// <summary>
    /// Checks if settings are properly configured for the application to work
    /// </summary>
    public bool AreSettingsValid(AppSettings? settings = null)
    {
        var settingsToCheck = settings ?? _currentSettings;

        return !string.IsNullOrWhiteSpace(settingsToCheck.Endpoint) &&
               !string.IsNullOrWhiteSpace(settingsToCheck.Key) &&
               !string.IsNullOrWhiteSpace(settingsToCheck.WhisperDeployment) &&
               !string.IsNullOrWhiteSpace(settingsToCheck.CorrectionDeployment);
    }

    /// <summary>
    /// Loads default settings (empty/minimal settings that require user configuration)
    /// </summary>
    public AppSettings LoadDefaultSettings()
    {
        return CreateDefaultSettings();
    }

    private AppSettings CreateDefaultSettings()
    {
        return new AppSettings
        {
            Endpoint = string.Empty,
            Key = string.Empty,
            WhisperDeployment = string.Empty,
            CorrectionDeployment = string.Empty,
            CorrectionPrompt = GetDefaultCorrectionPrompt()
        };
    }

    private string GetDefaultCorrectionPrompt()
    {
        return "Please correct any spelling, grammar, and punctuation errors in the following text while preserving its original meaning and tone. Only make necessary corrections and maintain the natural flow of speech.";
    }

    private async Task<AppSettings> LoadFromEncryptedFileAsync()
    {
        var encryptedData = await File.ReadAllBytesAsync(_settingsFilePath);
        var decryptedJson = DecryptData(encryptedData);
        _logger.LogDebug($"Loaded encrypted settings from '{_settingsFilePath}'. JSON length: {decryptedJson.Length} characters");
        return JsonSerializer.Deserialize<AppSettings>(decryptedJson) ?? new AppSettings();
    }

    private async Task SaveToEncryptedFileAsync(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var encryptedData = EncryptData(json);
        _logger.LogDebug($"Saving encrypted settings to '{_settingsFilePath}'. JSON length: {json.Length} characters, encrypted size: {encryptedData.Length} bytes");
        await File.WriteAllBytesAsync(_settingsFilePath, encryptedData);
    }

    private byte[] EncryptData(string data)
    {
        var entropy = GetMachineEntropy();
        var dataBytes = Encoding.UTF8.GetBytes(data);
        return ProtectedData.Protect(dataBytes, entropy, DataProtectionScope.CurrentUser);
    }

    private string DecryptData(byte[] encryptedData)
    {
        var entropy = GetMachineEntropy();
        var decryptedBytes = ProtectedData.Unprotect(encryptedData, entropy, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    private byte[] GetMachineEntropy()
    {
        // Create entropy based on machine name and user name for additional security
        var entropyString = $"{Environment.MachineName}:{Environment.UserName}:SpeechAgent";
        return Encoding.UTF8.GetBytes(entropyString);
    }
}
