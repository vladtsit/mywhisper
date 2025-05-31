using NUnit.Framework;
using Moq;
using SpeechAgent.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SpeechAgent.Core.Tests.Services;

/// <summary>
/// Hardware-dependent tests for AudioRecorder that require actual audio devices.
/// These tests are categorized as "Hardware" and should be skipped in CI environments.
/// </summary>
[TestFixture]
[Category("Hardware")]
public class AudioRecorderHardwareTests
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
    }

    [Test]
    [Category("Hardware")]
    public void IsRecording_InitialState_ReturnsFalse()
    {
        // Arrange & Act & Assert
        Assert.That(_audioRecorder.IsRecording, Is.False);
    }

    [Test]
    [Category("Hardware")]
    public async Task StartRecordingAsync_WithRealHardware_SetsIsRecordingToTrue()
    {
        // Note: This test requires actual audio hardware and will fail in CI
        try
        {
            // Act
            await _audioRecorder.StartRecordingAsync();

            // Assert
            Assert.That(_audioRecorder.IsRecording, Is.True);

            // Cleanup
            await _audioRecorder.StopRecordingAsync();
        }
        catch (NAudio.MmException ex) when (ex.Message.Contains("BadDeviceId"))
        {
            Assert.Ignore("Test requires audio hardware - skipping in headless environment");
        }
    }

    [Test]
    [Category("Hardware")]
    public async Task StopRecordingAsync_WithRealHardware_WhenRecording_ShouldReturnAudioStream()
    {
        // Note: This test requires actual audio hardware and will fail in CI
        try
        {
            // Arrange
            await _audioRecorder.StartRecordingAsync();
            await Task.Delay(100); // Record for a short time

            // Act
            var result = await _audioRecorder.StopRecordingAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(_audioRecorder.IsRecording, Is.False);
        }
        catch (NAudio.MmException ex) when (ex.Message.Contains("BadDeviceId"))
        {
            Assert.Ignore("Test requires audio hardware - skipping in headless environment");
        }
    }

    [Test]
    [Category("Hardware")]
    public async Task StopRecordingAsync_WithRealHardware_WhenNotRecording_ShouldReturnEmptyStream()
    {
        // Act
        var result = await _audioRecorder.StopRecordingAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.EqualTo(0));
        Assert.That(_audioRecorder.IsRecording, Is.False);
    }
}
