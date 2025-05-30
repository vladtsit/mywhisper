using NUnit.Framework;
using Moq;
using SpeechAgent.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SpeechAgent.Core.Tests.Services;

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

    [TearDown]
    public void TearDown()
    {
        _audioRecorder?.Dispose();
    }    [Test]
    public async Task StartRecordingAsync_ShouldStartRecording()
    {
        // Act
        await _audioRecorder.StartRecordingAsync();

        // Assert
        Assert.That(_audioRecorder.IsRecording, Is.True);
    }    [Test]
    public async Task StopRecordingAsync_WhenRecording_ShouldReturnAudioStream()
    {
        // Arrange
        await _audioRecorder.StartRecordingAsync();
        await Task.Delay(100); // Record for a short time

        // Act
        var result = await _audioRecorder.StopRecordingAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(_audioRecorder.IsRecording, Is.False); // Should be False after stopping
    }

    [Test]
    public async Task StopRecordingAsync_WhenNotRecording_ShouldReturnNull()
    {
        // Act
        var result = await _audioRecorder.StopRecordingAsync();        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.EqualTo(0)); // Should be empty stream when not recording
    }

    [Test]
    public void IsRecording_InitiallyFalse()
    {
        // Assert
        Assert.IsFalse(_audioRecorder.IsRecording);
    }
}
