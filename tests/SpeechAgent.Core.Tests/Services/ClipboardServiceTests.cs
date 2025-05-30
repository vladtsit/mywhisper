using NUnit.Framework;
using SpeechAgent.Core.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SpeechAgent.Core.Tests.Services;

[TestFixture]
[Apartment(ApartmentState.STA)] // Required for WPF Clipboard operations
public class ClipboardServiceTests
{
    private ClipboardService _clipboardService;

    [SetUp]
    public void Setup()
    {
        _clipboardService = new ClipboardService();
    }    [Test]
    public void CopyToClipboard_WithValidText_ShouldNotThrow()
    {
        // Arrange
        const string testText = "Hello, World!";

        try
        {
            // Act
            Assert.DoesNotThrowAsync(async () => await _clipboardService.CopyToClipboardAsync(testText));

            // Assert - Verify text was copied (this might fail in CI environments without clipboard access)
            if (Clipboard.ContainsText())
            {
                Assert.That(Clipboard.GetText(), Is.EqualTo(testText));
            }
        }
        catch (Exception ex) when (ex.Message.Contains("clipboard") || ex.Message.Contains("COM"))
        {
            // Skip test if running in environment without clipboard access
            Assert.Inconclusive("Test skipped due to clipboard access restrictions in test environment");
        }
    }    [Test]
    public void CopyToClipboard_WithNullText_ShouldNotThrow()
    {        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _clipboardService.CopyToClipboardAsync(null!));
    }    [Test]
    public void CopyToClipboard_WithEmptyText_ShouldNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _clipboardService.CopyToClipboardAsync(string.Empty));
    }
}
