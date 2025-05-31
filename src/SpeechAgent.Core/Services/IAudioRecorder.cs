using System;
using System.IO;
using System.Threading.Tasks;

namespace SpeechAgent.Core.Services;

public interface IAudioRecorder : IDisposable
{
    event EventHandler<bool> RecordingStateChanged;
    bool IsRecording { get; }
    Task StartRecordingAsync();
    Task<MemoryStream> StopRecordingAsync();
}
