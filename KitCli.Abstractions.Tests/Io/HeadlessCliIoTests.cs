using KitCli.Abstractions.Exceptions;
using KitCli.Abstractions.Io;
using NUnit.Framework;

namespace KitCli.Abstractions.Tests.Io;

[TestFixture]
public class HeadlessCliIoTests
{
    [Test]
    public void GivenHeadlessCliIo_WhenAskedWhetherItCanAsk_SaysNo()
    {
        // Arrange
        var classUnderTest = new HeadlessCliIo();

        // Assert
        Assert.That(classUnderTest.CanAsk, Is.False);
    }

    [Test]
    public void GivenHeadlessCliIo_WhenAsked_ThrowsNoInput()
    {
        // Arrange
        var classUnderTest = new HeadlessCliIo();

        // Act
        var ask = () => classUnderTest.AskAsync(CancellationToken.None);

        // Assert
        Assert.That(ask, Throws.InstanceOf<NoInputException>());
    }
}
