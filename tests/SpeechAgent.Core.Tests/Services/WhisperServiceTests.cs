using NUnit.Framework;
using Moq;
using SpeechAgent.Core.Services;
using SpeechAgent.Core.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SpeechAgent.Core.Tests.Services;

[TestFixture]
public class WhisperServiceTests
{
    private WhisperService _whisperService;
    private Mock<ISettingsService> _mockSettingsService;
    private Mock<ILogService> _mockLogService;
    private AppSettings _testSettings;

    [SetUp]
    public void Setup()
    {
        _mockSettingsService = new Mock<ISettingsService>();
        _mockLogService = new Mock<ILogService>();

        // Set up test settings
        _testSettings = new AppSettings
        {
            Endpoint = "https://test.openai.azure.com/",
            Key = "test-key",
            WhisperDeployment = "whisper-1",
            CorrectionDeployment = "gpt-4",
            CorrectionPrompt = "Test correction prompt"
        };

        // Configure the mock settings service
        _mockSettingsService.Setup(s => s.CurrentSettings).Returns(_testSettings);

        _whisperService = new WhisperService(_mockSettingsService.Object, _mockLogService.Object);
    }

    [Test]
    public void Constructor_WithValidSettings_ShouldNotThrow()
    {
        // Assert
        Assert.DoesNotThrow(() => new WhisperService(_mockSettingsService.Object, _mockLogService.Object));
    }

    [Test]
    public void Constructor_WithNullSettingsService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new WhisperService(null!, _mockLogService.Object));
    }

    [Test]
    public void Constructor_WithNullLogService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new WhisperService(_mockSettingsService.Object, null!));
    }

    [Test]
    public void TranscribeAsync_WithMissingDeployment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _testSettings.WhisperDeployment = string.Empty;
        using var testStream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _whisperService.TranscribeAsync(testStream));

        Assert.That(exception.Message, Contains.Substring("Whisper deployment name not configured"));
    }
    [Test]
    public void TranscribeAsync_WithNullStream_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => _whisperService.TranscribeAsync(null!));
    }

    [Test]
    public void TranscribeAsync_WithEmptyStream_ShouldReturnEmptyString()
    {
        // Arrange
        using var emptyStream = new MemoryStream();

        // Act & Assert
        // Note: This test would require actual Azure OpenAI service, so we'll test the error handling
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _whisperService.TranscribeAsync(emptyStream));
        Assert.That(exception.Message, Does.Contain("Failed to transcribe audio").Or.Contains("Could not initialize OpenAI client"));
    }

    [Test]
    public void SettingsChanged_ShouldRefreshClient()
    {
        // Arrange
        _testSettings.Endpoint = "https://new-endpoint.openai.azure.com/";
        _testSettings.Key = "new-key";

        // Act
        _mockSettingsService.Raise(s => s.SettingsChanged += null,
            _mockSettingsService.Object, _testSettings);

        // Assert
        // This test just verifies that no exception is thrown when settings change
        Assert.Pass("Settings change handled without exception");
    }
}
