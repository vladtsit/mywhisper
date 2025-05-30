using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;

namespace SpeechAgent.Core.Services;

public class AudioRecorder : IAudioRecorder, IDisposable
{
    private readonly ILogService _logger;
    private WaveInEvent? _waveIn;
    private MemoryStream? _recordingStream;
    private WaveFileWriter? _waveFileWriter;
    private bool _isRecording;
    private DateTime _recordingStartTime;
    private readonly List<byte> _audioBuffer = new();

    public AudioRecorder(ILogService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public event EventHandler<bool>? RecordingStateChanged;
    public bool IsRecording => _isRecording;    public Task StartRecordingAsync()
    {
        if (_isRecording)
        {
            _logger.LogDebug("Recording already in progress, ignoring start request");
            return Task.CompletedTask;
        }

        _logger.LogDebug("Starting audio recording...");
        _recordingStartTime = DateTime.Now;
        
        // Clear the audio buffer for new recording
        _audioBuffer.Clear();
        
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 1) // 16kHz, mono for Whisper
        };

        _logger.LogDebug($"Audio format: {_waveIn.WaveFormat.SampleRate}Hz, {_waveIn.WaveFormat.Channels} channel(s), {_waveIn.WaveFormat.BitsPerSample} bits");

        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.RecordingStopped += OnRecordingStopped;

        _waveIn.StartRecording();
        _isRecording = true;
        RecordingStateChanged?.Invoke(this, true);
        
        _logger.LogInfo("Recording started successfully");

        return Task.CompletedTask;
    }public Task<MemoryStream> StopRecordingAsync()
    {        if (!_isRecording || _waveIn == null)
        {
            _logger.LogDebug("Stop recording called but not currently recording");
            return Task.FromResult(new MemoryStream());
        }

        _logger.LogDebug("Stopping audio recording...");
        var recordingDuration = DateTime.Now - _recordingStartTime;
        
        try
        {
            _waveIn.StopRecording();
            
            // Wait a moment for the recording to fully stop
            System.Threading.Thread.Sleep(100);
            
            // Immediately update the recording state
            _isRecording = false;
            RecordingStateChanged?.Invoke(this, false);            // Create a WAV file from the raw audio data manually
            _logger.LogDebug($"Creating WAV stream from {_audioBuffer.Count} bytes of raw audio data");
            
            var audioStream = new MemoryStream();
            
            if (_audioBuffer.Count > 0)
            {
                _logger.LogDebug("Writing WAV header and audio data manually...");
                
                // Create WAV file manually to avoid stream disposal issues
                var audioData = _audioBuffer.ToArray();
                var format = _waveIn.WaveFormat;
                
                using (var writer = new BinaryWriter(audioStream, System.Text.Encoding.UTF8, true))
                {
                    // WAV Header
                    writer.Write("RIFF".ToCharArray());
                    writer.Write((uint)(36 + audioData.Length)); // File size - 8
                    writer.Write("WAVE".ToCharArray());
                    
                    // Format chunk
                    writer.Write("fmt ".ToCharArray());
                    writer.Write((uint)16); // Format chunk size
                    writer.Write((ushort)1); // Audio format (PCM)
                    writer.Write((ushort)format.Channels);
                    writer.Write((uint)format.SampleRate);
                    writer.Write((uint)format.AverageBytesPerSecond);
                    writer.Write((ushort)format.BlockAlign);
                    writer.Write((ushort)format.BitsPerSample);
                    
                    // Data chunk
                    writer.Write("data".ToCharArray());
                    writer.Write((uint)audioData.Length);
                    writer.Write(audioData);
                    
                    writer.Flush();                }
                
                _logger.LogDebug($"WAV file created manually, total length: {audioStream.Length} bytes");
            }
            else
            {
                _logger.LogWarning("No audio data to write to WAV stream");
            }
            
            // Reset position to beginning
            audioStream.Position = 0;
              // Get audio data info
            var audioDataLength = audioStream.Length;
            var estimatedDurationSeconds = audioDataLength > 44 ? (audioDataLength - 44) / (double)(_waveIn.WaveFormat.AverageBytesPerSecond) : 0;
            
            _logger.LogInfo($"Recording stopped - Duration: {recordingDuration.TotalSeconds:F2}s, Buffer: {_audioBuffer.Count:N0} bytes, WAV: {audioDataLength:N0} bytes ({audioDataLength / 1024:N0} KB)");
            _logger.LogDebug($"Audio details - Estimated duration: {estimatedDurationSeconds:F2}s, Sample rate: {_waveIn.WaveFormat.SampleRate}Hz, Channels: {_waveIn.WaveFormat.Channels}, Bits per sample: {_waveIn.WaveFormat.BitsPerSample}");
            
            if (audioDataLength == 0)
            {
                _logger.LogWarning("No audio data recorded!");
            }
            else if (audioDataLength < 1000)
            {
                _logger.LogWarning("Very small audio file, may indicate recording issue");
            }
            
            return Task.FromResult(audioStream);        }        catch (Exception ex)
        {
            _logger.LogError($"Error in StopRecordingAsync: {ex.Message}", "AudioRecorder", ex);
            
            // Still update the recording state
            _isRecording = false;
            RecordingStateChanged?.Invoke(this, false);
            
            throw; // Re-throw the exception so the UI can handle it
        }
    }    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        // Store raw audio data in our buffer instead of using WaveFileWriter directly
        if (e.BytesRecorded > 0)
        {
            var audioData = new byte[e.BytesRecorded];
            Array.Copy(e.Buffer, audioData, e.BytesRecorded);
            _audioBuffer.AddRange(audioData);
        }
    }    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        _logger.LogDebug("OnRecordingStopped event fired");
        
        // Only update state if it hasn't been updated already
        if (_isRecording)
        {
            _logger.LogDebug("Recording state still active, updating to stopped");
            _isRecording = false;
            RecordingStateChanged?.Invoke(this, false);
        }
    }    public void Dispose()
    {
        _logger.LogDebug("AudioRecorder.Dispose() called");
        
        if (_isRecording && _waveIn != null)
        {
            _logger.LogDebug("Stopping recording during disposal");
            _waveIn.StopRecording();
        }
        
        _waveFileWriter?.Dispose();
        _waveFileWriter = null;
        
        if (_waveIn != null)
        {
            _waveIn.DataAvailable -= OnDataAvailable;
            _waveIn.RecordingStopped -= OnRecordingStopped;
            _waveIn.Dispose();
            _waveIn = null;
        }
        
        _recordingStream?.Dispose();
        _recordingStream = null;
        
        _audioBuffer.Clear();
        
        _logger.LogDebug("AudioRecorder disposal completed");
    }
}
