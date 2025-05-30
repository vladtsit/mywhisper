using NUnit.Framework;
using Moq;
using SpeechAgent.Core.Services;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SpeechAgent.Core.Tests;

[TestFixture]
public class AudioRecorderTests
{
    private AudioRecorder _audioRecorder;
    private Mock<ILogService> _mockLogService;

    [SetUp]
    public void Setup()
    {
        _mockLogService = new Mock<ILogService>();
        _audioRecorder = new AudioRecorder(_mockLogService.Object);
    }

    [Test]
    public void IsRecording_InitialState_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.That(_audioRecorder.IsRecording, Is.False);
    }

    [Test]
    public async Task StartRecordingAsync_SetsIsRecordingToTrue()
    {
        // Act
        await _audioRecorder.StartRecordingAsync();

        // Assert
        Assert.That(_audioRecorder.IsRecording, Is.True);

        // Cleanup
        await _audioRecorder.StopRecordingAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _audioRecorder?.Dispose();
    }
}

// OpenAiCorrectionServiceTests moved to its own file in Services/OpenAiCorrectionServiceTests.cs

[TestFixture]
public class ClipboardServiceTests
{
    private ClipboardService _clipboardService;

    [SetUp]
    public void Setup()
    {
        _clipboardService = new ClipboardService();
    }

    [Test]
    public void CopyToClipboardAsync_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _clipboardService.CopyToClipboardAsync("test text"));
    }
}