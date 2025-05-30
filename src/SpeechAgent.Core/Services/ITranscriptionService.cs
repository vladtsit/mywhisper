using System.IO;
using System.Threading.Tasks;

namespace SpeechAgent.Core.Services;

public interface ITranscriptionService
{
    Task<string> TranscribeAsync(Stream audioStream);
}
