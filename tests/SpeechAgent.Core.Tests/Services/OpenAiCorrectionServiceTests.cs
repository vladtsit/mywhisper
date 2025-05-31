using NUnit.Framework;
using Moq;
using SpeechAgent.Core.Services;
using SpeechAgent.Core.Models;
using System;
using System.Threading.Tasks;

namespace SpeechAgent.Core.Tests.Services;

[TestFixture]
public class OpenAiCorrectionServiceTests
{
    private OpenAiCorrectionService _correctionService;
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
            WhisperDeployment = "whisper",
            CorrectionDeployment = "gpt-4",
            CorrectionPrompt = "Test correction prompt"
        };

        // Configure the mock settings service
        _mockSettingsService.Setup(s => s.CurrentSettings).Returns(_testSettings);

        _correctionService = new OpenAiCorrectionService(_mockSettingsService.Object, _mockLogService.Object);
    }

    [Test]
    public void Constructor_WithValidSettings_ShouldNotThrow()
    {
        // Assert
        Assert.DoesNotThrow(() => new OpenAiCorrectionService(_mockSettingsService.Object, _mockLogService.Object));
    }

    [Test]
    public void Constructor_WithNullSettingsService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OpenAiCorrectionService(null!, _mockLogService.Object));
    }

    [Test]
    public void Constructor_WithNullLogService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OpenAiCorrectionService(_mockSettingsService.Object, null!));
    }
    [Test]
    public void CorrectAsync_WithNullText_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => _correctionService.CorrectAsync(null!));
    }

    [Test]
    public async Task CorrectAsync_WithEmptyText_ShouldReturnEmptyString()
    {
        // Act
        var result = await _correctionService.CorrectAsync(string.Empty);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void CorrectAsync_WithMissingDeployment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _testSettings.CorrectionDeployment = string.Empty;
        const string inputText = "hello world how are you today";

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _correctionService.CorrectAsync(inputText));

        Assert.That(exception.Message, Contains.Substring("Azure OpenAI correction deployment not configured"));
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
