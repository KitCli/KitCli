using KitCli.Abstractions.Io;
using NUnit.Framework;

namespace KitCli.Abstractions.Tests.Io;

[TestFixture]
public class CliIoTests
{
    [Test]
    public void GivenCliIo_WhenAskedWhetherItCanAsk_SaysYes()
    {
        // Arrange
        ICliIo classUnderTest = new CliIo();

        // Assert
        Assert.That(classUnderTest.CanAsk, Is.True);
    }
}
