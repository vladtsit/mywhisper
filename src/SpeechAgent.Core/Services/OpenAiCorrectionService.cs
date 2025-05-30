using System;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure;
using SpeechAgent.Core.Models;

namespace SpeechAgent.Core.Services;

/// <summary>
/// Service for correcting and improving text using Azure OpenAI
/// </summary>
public class OpenAiCorrectionService : ICorrectionService
{
    private readonly ISettingsService _settingsService;
    private readonly ILogService _logger;
    private OpenAIClient? _openAIClient;

    /// <summary>
    /// Initializes a new instance of the correction service using settings service
    /// </summary>
    public OpenAiCorrectionService(ISettingsService settingsService, ILogService logger)
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
            _logger.LogDebug($"OpenAI client refreshed with endpoint: {settings.Endpoint}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to refresh OpenAI client: {ex.Message}", nameof(OpenAiCorrectionService), ex);
            _openAIClient = null;
        }
    }    /// <summary>
    /// Corrects and improves the provided raw text using Azure OpenAI
    /// </summary>
    /// <param name="rawText">The text to correct</param>
    /// <returns>Corrected text</returns>
    public async Task<string> CorrectAsync(string rawText)
    {
        if (rawText == null)
        {
            _logger.LogError("OpenAiCorrectionService: rawText is null");
            throw new ArgumentNullException(nameof(rawText));
        }

        if (string.IsNullOrEmpty(rawText))
        {
            _logger.LogWarning("OpenAiCorrectionService: rawText is empty, returning empty string");
            return string.Empty;
        }

        // Get settings
        var settings = _settingsService.CurrentSettings;
        var deploymentName = settings.CorrectionDeployment;
        var systemMessage = settings.CorrectionPrompt;

        if (string.IsNullOrEmpty(deploymentName))
            throw new InvalidOperationException("Azure OpenAI correction deployment not configured");

        // Ensure client is initialized
        if (_openAIClient == null)
            RefreshClient();

        // Check client again after refresh attempt
        if (_openAIClient == null)
            throw new InvalidOperationException("Could not initialize OpenAI client");

        _logger.LogInfo("OpenAiCorrectionService: Starting text correction...");
        _logger.LogDebug($"Input text length: {rawText.Length} characters");
        _logger.LogDebug($"Input text: '{rawText}'");
        _logger.LogDebug($"Deployment name: {deploymentName}");
        _logger.LogDebug($"System message: {systemMessage}");

        try
        {
            // Use current deployment and prompt from settings
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = deploymentName,
                Messages =
                {
                    new ChatRequestSystemMessage(systemMessage),
                    new ChatRequestUserMessage($"Please correct and punctuate the following text, fixing any grammar, spelling, or clarity issues: {rawText}")
                },
                MaxTokens = 16000,
                Temperature = 0.1f
            };
            
            _logger.LogDebug("OpenAiCorrectionService: Sending request to Azure OpenAI...");
            var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);

            var result = response.Value.Choices[0].Message.Content ?? rawText;
            _logger.LogInfo($"Correction completed. Result length: {result.Length} characters");
            _logger.LogDebug($"Corrected text: '{result}'");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during correction: {ex.Message}", "OpenAiCorrectionService", ex);
            throw new InvalidOperationException($"Failed to correct text: {ex.Message}", ex);
        }
    }
}
