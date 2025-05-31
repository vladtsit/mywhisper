using NUnit.Framework;
using Moq;
using SpeechAgent.Core.Services;
using SpeechAgent.Core.Tests.Mocks;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SpeechAgent.Core.Tests.Services;

[TestFixture]
public class AudioRecorderTests
{
    private IAudioRecorder _audioRecorder;
    private Mock<ILogService> _mockLogService;

    [SetUp]
    public void Setup()
    {
        _mockLogService = new Mock<ILogService>();
        
        // Use mock implementation for CI-safe testing
        _audioRecorder = new MockAudioRecorder();
    }

    [TearDown]
    public void TearDown()
    {
        _audioRecorder?.Dispose();
    }    [Test]
    public void IsRecording_InitialState_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.That(_audioRecorder.IsRecording, Is.False);
    }

    [Test]
    public async Task StartRecordingAsync_ShouldStartRecording()
    {
        // Act
        await _audioRecorder.StartRecordingAsync();

        // Assert
        Assert.That(_audioRecorder.IsRecording, Is.True);
    }

    [Test]
    public async Task StartRecordingAsync_WhenAlreadyRecording_ShouldRemainTrue()
    {
        // Arrange
        await _audioRecorder.StartRecordingAsync();
        
        // Act
        await _audioRecorder.StartRecordingAsync(); // Start again
        
        // Assert
        Assert.That(_audioRecorder.IsRecording, Is.True);
    }

    [Test]
    public async Task StopRecordingAsync_WhenRecording_ShouldReturnAudioStream()
    {
        // Arrange
        await _audioRecorder.StartRecordingAsync();
        await Task.Delay(50); // Brief delay to simulate recording

        // Act
        var result = await _audioRecorder.StopRecordingAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0), "Mock should return audio data");
        Assert.That(_audioRecorder.IsRecording, Is.False);
    }

    [Test]
    public async Task StopRecordingAsync_WhenNotRecording_ShouldReturnEmptyStream()
    {
        // Act
        var result = await _audioRecorder.StopRecordingAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.EqualTo(0));
        Assert.That(_audioRecorder.IsRecording, Is.False);
    }

    [Test]
    public async Task RecordingStateChanged_ShouldFireWhenStarting()
    {
        // Arrange
        bool eventFired = false;
        bool eventValue = false;
        _audioRecorder.RecordingStateChanged += (sender, isRecording) =>
        {
            eventFired = true;
            eventValue = isRecording;
        };

        // Act
        await _audioRecorder.StartRecordingAsync();

        // Assert
        Assert.That(eventFired, Is.True);
        Assert.That(eventValue, Is.True);
    }

    [Test]
    public async Task RecordingStateChanged_ShouldFireWhenStopping()
    {
        // Arrange
        await _audioRecorder.StartRecordingAsync();
        
        bool eventFired = false;
        bool eventValue = true;
        _audioRecorder.RecordingStateChanged += (sender, isRecording) =>
        {
            eventFired = true;
            eventValue = isRecording;
        };

        // Act
        await _audioRecorder.StopRecordingAsync();        // Assert
        Assert.That(eventFired, Is.True);
        Assert.That(eventValue, Is.False);
    }
}
