using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpeechAgent.Core.Models;

/// <summary>
/// Represents the application settings for Azure OpenAI configuration
/// </summary>
public class AppSettings : INotifyPropertyChanged
{
    private string _endpoint = string.Empty;
    private string _key = string.Empty;
    private string _whisperDeployment = string.Empty;
    private string _correctionDeployment = string.Empty;
    private string _correctionPrompt = string.Empty;

    /// <summary>
    /// Azure OpenAI endpoint URL
    /// </summary>
    public string Endpoint
    {
        get => _endpoint;
        set => SetProperty(ref _endpoint, value);
    }

    /// <summary>
    /// Azure OpenAI API key
    /// </summary>
    public string Key
    {
        get => _key;
        set => SetProperty(ref _key, value);
    }

    /// <summary>
    /// Whisper deployment name for transcription
    /// </summary>
    public string WhisperDeployment
    {
        get => _whisperDeployment;
        set => SetProperty(ref _whisperDeployment, value);
    }

    /// <summary>
    /// GPT deployment name for text correction
    /// </summary>
    public string CorrectionDeployment
    {
        get => _correctionDeployment;
        set => SetProperty(ref _correctionDeployment, value);
    }

    /// <summary>
    /// Custom prompt for text correction
    /// </summary>
    public string CorrectionPrompt
    {
        get => _correctionPrompt;
        set => SetProperty(ref _correctionPrompt, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Creates a copy of the current settings
    /// </summary>
    public AppSettings Clone()
    {
        return new AppSettings
        {
            Endpoint = Endpoint,
            Key = Key,
            WhisperDeployment = WhisperDeployment,
            CorrectionDeployment = CorrectionDeployment,
            CorrectionPrompt = CorrectionPrompt
        };
    }
}
