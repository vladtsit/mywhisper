using System;
using System.IO;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure;
using SpeechAgent.Core.Models;

namespace SpeechAgent.Core.Services;

/// <summary>
/// Service for transcribing audio using Azure OpenAI Whisper
/// </summary>
public class WhisperService : ITranscriptionService
{
    private readonly ISettingsService _settingsService;
    private readonly ILogService _logger;
    private OpenAIClient? _openAIClient;

    /// <summary>
    /// Initializes a new instance of the Whisper service using settings service
    /// </summary>
    public WhisperService(ISettingsService settingsService, ILogService logger)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Subscribe to settings changes to refresh the OpenAI client
        _settingsService.SettingsChanged += (_, _) => RefreshClient();

        // Initialize the client with current settings
        RefreshClient();
    }

    /// <summary>
    /// Refresh the OpenAI client with the latest settings
    /// </summary>
    private void RefreshClient()
    {
        try
        {
            var settings = _settingsService.CurrentSettings;

            if (string.IsNullOrEmpty(settings.Endpoint))
                throw new InvalidOperationException("Azure OpenAI endpoint not configured");

            if (string.IsNullOrEmpty(settings.Key))
                throw new InvalidOperationException("Azure OpenAI key not configured");

            _openAIClient = new OpenAIClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.Key));
            _logger.LogDebug($"WhisperService: OpenAI client refreshed with endpoint: {settings.Endpoint}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"WhisperService: Failed to refresh OpenAI client: {ex.Message}", nameof(WhisperService), ex);
            _openAIClient = null;
        }
    }    /// <summary>
         /// Transcribes audio stream using Azure OpenAI Whisper
         /// </summary>
         /// <param name="audioStream">Audio stream to transcribe</param>
         /// <returns>Transcribed text</returns>
    public async Task<string> TranscribeAsync(Stream audioStream)
    {
        if (audioStream == null)
        {
            _logger.LogDebug("WhisperService: audioStream is null");
            throw new ArgumentNullException(nameof(audioStream));
        }

        // Get settings
        var settings = _settingsService.CurrentSettings;
        var deploymentName = settings.WhisperDeployment;

        if (string.IsNullOrEmpty(deploymentName))
            throw new InvalidOperationException("Whisper deployment name not configured");

        // Ensure client is initialized
        if (_openAIClient == null)
            RefreshClient();

        // Check client again after refresh attempt
        if (_openAIClient == null)
            throw new InvalidOperationException("Could not initialize OpenAI client");

        _logger.LogInfo("WhisperService: Starting transcription...");
        _logger.LogDebug($"WhisperService: Stream length: {audioStream.Length} bytes");
        _logger.LogDebug($"WhisperService: Stream position: {audioStream.Position}");
        _logger.LogDebug($"WhisperService: Deployment name: {deploymentName}");

        try
        {
            audioStream.Position = 0; // Reset stream position
            _logger.LogDebug("WhisperService: Reset stream position to 0");

            var audioTranscriptionOptions = new AudioTranscriptionOptions
            {
                DeploymentName = deploymentName,
                AudioData = BinaryData.FromStream(audioStream),
                ResponseFormat = AudioTranscriptionFormat.Simple
            };

            _logger.LogDebug("WhisperService: Sending request to Azure OpenAI...");
            var response = await _openAIClient.GetAudioTranscriptionAsync(audioTranscriptionOptions);

            var result = response.Value.Text ?? string.Empty;
            _logger.LogInfo($"WhisperService: Transcription completed. Result: '{result}'");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"WhisperService: ERROR during transcription: {ex.Message}");
            _logger.LogDebug($"WhisperService: Exception type: {ex.GetType().Name}");
            _logger.LogDebug($"WhisperService: Stack trace: {ex.StackTrace}");
            throw new InvalidOperationException($"Failed to transcribe audio: {ex.Message}", ex);
        }
    }
}
