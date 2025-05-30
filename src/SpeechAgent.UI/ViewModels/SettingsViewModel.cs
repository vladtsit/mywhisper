using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SpeechAgent.Core.Models;
using SpeechAgent.Core.Services;

namespace SpeechAgent.UI.ViewModels;

/// <summary>
/// ViewModel for the Settings window
/// </summary>
public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly ISettingsService _settingsService;
    private AppSettings _settings;
    private bool _isSaving;
    private string _statusMessage = string.Empty;    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _settings = _settingsService.CurrentSettings.Clone();

        // Ensure settings are loaded properly
        if (string.IsNullOrEmpty(_settings.Endpoint) || 
            string.IsNullOrEmpty(_settings.WhisperDeployment) || 
            string.IsNullOrEmpty(_settings.CorrectionDeployment))
        {
            // Load settings asynchronously but wait for result
            _settings = _settingsService.LoadSettingsAsync().GetAwaiter().GetResult();
        }

        SaveCommand = new RelayCommand(async () => await SaveSettingsAsync(), () => !IsSaving);
        CancelCommand = new RelayCommand(() => Cancel());
        ResetToDefaultsCommand = new RelayCommand(() => ResetToDefaults());
    }

    /// <summary>
    /// Azure OpenAI endpoint URL
    /// </summary>
    public string Endpoint
    {
        get => _settings.Endpoint;
        set
        {
            if (_settings.Endpoint != value)
            {
                _settings.Endpoint = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Azure OpenAI API key
    /// </summary>
    public string Key
    {
        get => _settings.Key;
        set
        {
            if (_settings.Key != value)
            {
                _settings.Key = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Whisper deployment name
    /// </summary>
    public string WhisperDeployment
    {
        get => _settings.WhisperDeployment;
        set
        {
            if (_settings.WhisperDeployment != value)
            {
                _settings.WhisperDeployment = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// GPT deployment name for text correction
    /// </summary>
    public string CorrectionDeployment
    {
        get => _settings.CorrectionDeployment;
        set
        {
            if (_settings.CorrectionDeployment != value)
            {
                _settings.CorrectionDeployment = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Custom prompt for text correction
    /// </summary>
    public string CorrectionPrompt
    {
        get => _settings.CorrectionPrompt;
        set
        {
            if (_settings.CorrectionPrompt != value)
            {
                _settings.CorrectionPrompt = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Indicates if settings are currently being saved
    /// </summary>
    public bool IsSaving
    {
        get => _isSaving;
        set
        {
            if (_isSaving != value)
            {
                _isSaving = value;
                OnPropertyChanged();
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Status message to display to user
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ResetToDefaultsCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? CloseRequested;
    public event EventHandler? SettingsSaved;    private async Task SaveSettingsAsync()
    {
        try
        {
            IsSaving = true;
            StatusMessage = "Validating settings...";

            // Validate that all required fields are filled
            if (string.IsNullOrWhiteSpace(Endpoint))
            {
                StatusMessage = "Error: Endpoint URL is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(Key))
            {
                StatusMessage = "Error: API Key is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(WhisperDeployment))
            {
                StatusMessage = "Error: Whisper Deployment Name is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(CorrectionDeployment))
            {
                StatusMessage = "Error: Correction Deployment Name is required";
                return;
            }

            StatusMessage = "Saving settings...";

            await _settingsService.SaveSettingsAsync(_settings);

            StatusMessage = "Settings saved successfully!";
            SettingsSaved?.Invoke(this, EventArgs.Empty);

            // Close the window after a brief delay
            await Task.Delay(1000);
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving settings: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void Cancel()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }    private void ResetToDefaults()
    {
        // Reset to default empty settings since we no longer use appsettings.json
        var defaultSettings = _settingsService.LoadDefaultSettings();
        
        // Apply the default settings to the UI
        Endpoint = defaultSettings.Endpoint;
        Key = defaultSettings.Key;
        WhisperDeployment = defaultSettings.WhisperDeployment;
        CorrectionDeployment = defaultSettings.CorrectionDeployment;
        CorrectionPrompt = defaultSettings.CorrectionPrompt;

        StatusMessage = "Settings reset to defaults";
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Simple implementation of ICommand for MVVM pattern
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Func<Task> _executeAsync;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute;
    }

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _executeAsync = () =>
        {
            execute();
            return Task.CompletedTask;
        };
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    public async void Execute(object? parameter)
    {
        await _executeAsync();
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
