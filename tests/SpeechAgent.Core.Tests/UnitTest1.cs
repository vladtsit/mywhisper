using NUnit.Framework;
using SpeechAgent.Core.Services;
using System.Threading.Tasks;

namespace SpeechAgent.Core.Tests;

[TestFixture]
public class ClipboardServiceTests
{
    private ClipboardService _clipboardService;

    [SetUp]
    public void Setup()
    {
        _clipboardService = new ClipboardService();
    }

    [Test]
    public void CopyToClipboardAsync_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _clipboardService.CopyToClipboardAsync("test text"));
    }
}