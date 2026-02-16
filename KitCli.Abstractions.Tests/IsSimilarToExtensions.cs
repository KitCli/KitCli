using NUnit.Framework;

namespace KitCli.Abstractions.Tests;

[TestFixture]
public class IsSimilarToExtensions
{
    [Test]
    [TestCase("a", "a", true)]
    [TestCase("ab", "a", true)]
    public void GivenString_WhenSimilarTO_ThenReturnsTrue(
        string value, string match, bool expected)
    {
        // Arrange & Act
        var isSimilarTo = value.IsSimilarTo(match);

        // Assert
        Assert.That(isSimilarTo, Is.EqualTo(expected));
    }
}