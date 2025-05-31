using System;
using System.IO;
using System.Threading.Tasks;
using SpeechAgent.Core.Services;

namespace SpeechAgent.Core.Tests.Mocks;

/// <summary>
/// Mock implementation of IAudioRecorder for testing purposes.
/// This allows tests to run without requiring actual audio hardware.
/// </summary>
public class MockAudioRecorder : IAudioRecorder, IDisposable
{
    private bool _isRecording;
    private bool _shouldSimulateAudioData;

    public MockAudioRecorder(bool shouldSimulateAudioData = true)
    {
        _shouldSimulateAudioData = shouldSimulateAudioData;
    }

    public event EventHandler<bool>? RecordingStateChanged;
    public bool IsRecording => _isRecording;

    public Task StartRecordingAsync()
    {
        if (_isRecording)
            return Task.CompletedTask;

        _isRecording = true;
        RecordingStateChanged?.Invoke(this, true);
        return Task.CompletedTask;
    }

    public Task<MemoryStream> StopRecordingAsync()
    {
        if (!_isRecording)
            return Task.FromResult(new MemoryStream());

        _isRecording = false;
        RecordingStateChanged?.Invoke(this, false);

        // Return a mock audio stream with some dummy WAV data if requested
        var stream = new MemoryStream();
        if (_shouldSimulateAudioData)
        {
            // Create a minimal valid WAV file header + some dummy audio data
            var wavHeader = CreateWavHeader(1000); // 1000 bytes of audio data
            stream.Write(wavHeader);

            // Add some dummy audio data
            var dummyAudioData = new byte[1000];
            new Random().NextBytes(dummyAudioData);
            stream.Write(dummyAudioData);

            stream.Position = 0;
        }

        return Task.FromResult(stream);
    }

    private static byte[] CreateWavHeader(int audioDataLength)
    {
        var header = new byte[44];
        var sampleRate = 16000;
        var channels = 1;
        var bitsPerSample = 16;
        var byteRate = sampleRate * channels * bitsPerSample / 8;
        var blockAlign = (short)(channels * bitsPerSample / 8);

        // RIFF header
        header[0] = (byte)'R'; header[1] = (byte)'I'; header[2] = (byte)'F'; header[3] = (byte)'F';
        BitConverter.GetBytes(36 + audioDataLength).CopyTo(header, 4);
        header[8] = (byte)'W'; header[9] = (byte)'A'; header[10] = (byte)'V'; header[11] = (byte)'E';

        // fmt chunk
        header[12] = (byte)'f'; header[13] = (byte)'m'; header[14] = (byte)'t'; header[15] = (byte)' ';
        BitConverter.GetBytes(16).CopyTo(header, 16); // chunk size
        BitConverter.GetBytes((short)1).CopyTo(header, 20); // audio format (PCM)
        BitConverter.GetBytes((short)channels).CopyTo(header, 22);
        BitConverter.GetBytes(sampleRate).CopyTo(header, 24);
        BitConverter.GetBytes(byteRate).CopyTo(header, 28);
        BitConverter.GetBytes(blockAlign).CopyTo(header, 32);
        BitConverter.GetBytes((short)bitsPerSample).CopyTo(header, 34);

        // data chunk
        header[36] = (byte)'d'; header[37] = (byte)'a'; header[38] = (byte)'t'; header[39] = (byte)'a';
        BitConverter.GetBytes(audioDataLength).CopyTo(header, 40);

        return header;
    }

    public void Dispose()
    {
        _isRecording = false;
    }
}
