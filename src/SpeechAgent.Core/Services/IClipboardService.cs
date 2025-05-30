using System.Threading.Tasks;

namespace SpeechAgent.Core.Services;

public interface IClipboardService
{
    Task CopyToClipboardAsync(string text);
}
