using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Outcomes;

/// <summary>
/// Covers how long an <see cref="Outcome"/> lasts: one run unless its type passes another lifetime.
/// </summary>
[TestFixture]
public class OutcomeLifetimeTests
{
    [Test]
    public void GivenOutcomePassingNoLifetime_WhenLifetimeIsRead_ThenItLastsTheRun()
    {
        // Arrange
        var outcome = new NothingOutcome();

        // Act
        var lifetime = outcome.Lifetime;

        // Assert
        Assert.That(lifetime, Is.EqualTo(OutcomeLifetime.Run));
    }

    [Test]
    public void GivenOutcomePassingSession_WhenLifetimeIsRead_ThenItLastsTheSession()
    {
        // Arrange
        var outcome = new TestSessionOutcome();

        // Act
        var lifetime = outcome.Lifetime;

        // Assert
        Assert.That(lifetime, Is.EqualTo(OutcomeLifetime.Session));
    }

    [Test]
    public void GivenOutcomePassingSession_WhenKindIsRead_ThenTheKindItPassedIsKept()
    {
        // Arrange
        var outcome = new TestSessionOutcome();

        // Act
        var kind = outcome.Kind;

        // Assert
        Assert.That(kind, Is.EqualTo(OutcomeKind.Anonymous));
    }
}
