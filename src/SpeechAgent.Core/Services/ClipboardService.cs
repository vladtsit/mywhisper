using System;
using System.Threading.Tasks;
using System.Windows;

namespace SpeechAgent.Core.Services;

public class ClipboardService : IClipboardService
{
    public Task CopyToClipboardAsync(string text)
    {
        if (text == null)
        {
            return Task.CompletedTask; // Handle null gracefully
        }

        try
        {
            if (Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Clipboard.SetText(text);
                });
            }
            else
            {
                // Fallback for when Application.Current is null (like in tests)
                Clipboard.SetText(text);
            }
        }
        catch (Exception)
        {
            // Silently handle clipboard access issues (common in CI environments)
        }

        return Task.CompletedTask;
    }
}
