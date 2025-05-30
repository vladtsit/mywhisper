using System.Threading.Tasks;

namespace SpeechAgent.Core.Services;

public interface ICorrectionService
{
    Task<string> CorrectAsync(string rawText);
}
