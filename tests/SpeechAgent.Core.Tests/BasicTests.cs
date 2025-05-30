using NUnit.Framework;

namespace SpeechAgent.Core.Tests;

[TestFixture]
public class BasicTests
{
    [Test]
    public void BasicTest_ShouldPass()
    {
        // Arrange & Act & Assert
        Assert.Pass("Basic test working");
    }

    [Test]
    public void Addition_ShouldWork()
    {
        // Arrange
        int a = 2;
        int b = 3;

        // Act
        int result = a + b;        // Assert
        Assert.That(result, Is.EqualTo(5));
    }
}
