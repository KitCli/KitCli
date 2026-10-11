using KitCli.Abstractions.Io;
using NUnit.Framework;
using Spectre.Console.Testing;

namespace KitCli.Abstractions.Tests.Io;

[TestFixture]
public class CliIoTests
{
    private TestConsole _console;
    private CliIo _io;

    [SetUp]
    public void SetUp()
    {
        _console = new TestConsole();
        _console.Profile.Capabilities.Interactive = true;
        _io = new CliIo(_console);
    }

    [TearDown]
    public void TearDown()
        => _console.Dispose();

    [Test]
    public async Task GivenChoices_WhenArrowDownThenEnter_ThenChoosesTheSecond()
    {
        // Arrange
        _console.Input.PushKey(ConsoleKey.DownArrow);
        _console.Input.PushKey(ConsoleKey.Enter);

        // Act
        var chosen = await _io.ChooseAsync("Which?", Choices("a", "b", "c"), CancellationToken.None);

        // Assert
        Assert.That(chosen, Is.EqualTo(new CliChoice("b", "b")));
    }

    [Test]
    public async Task GivenChoicesWithSquareBrackets_WhenChoose_ThenDoesNotThrow()
    {
        // Arrange
        _console.Input.PushKey(ConsoleKey.Enter);

        // Act
        var chosen = await _io.ChooseAsync("List`1[System.String]?", Choices("List`1[System.String]"), CancellationToken.None);

        // Assert
        Assert.That(chosen, Is.EqualTo(new CliChoice("List`1[System.String]", "List`1[System.String]")));
    }

    [Test]
    public async Task GivenChoices_WhenCancelled_ThenChooseReturnsDefault()
    {
        // Arrange
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        // Act
        var chosen = await _io.ChooseAsync("Which?", Choices("a", "b"), cancelled.Token);

        // Assert
        Assert.That(chosen, Is.Null);
    }

    [Test]
    public async Task GivenChoices_WhenSpaceOnFirstAndThirdThenEnter_ThenChoosesBoth()
    {
        // Arrange
        _console.Input.PushKey(ConsoleKey.Spacebar);
        _console.Input.PushKey(ConsoleKey.DownArrow);
        _console.Input.PushKey(ConsoleKey.DownArrow);
        _console.Input.PushKey(ConsoleKey.Spacebar);
        _console.Input.PushKey(ConsoleKey.Enter);

        // Act
        var chosen = await _io.ChooseManyAsync("Which?", Choices("a", "b", "c"), CancellationToken.None);

        // Assert
        Assert.That(chosen, Is.EqualTo(Choices("a", "c")));
    }

    [Test]
    public async Task GivenChoices_WhenEnterWithNothingTicked_ThenChoosesNone()
    {
        // Arrange
        _console.Input.PushKey(ConsoleKey.Enter);

        // Act
        var chosen = await _io.ChooseManyAsync("Which?", Choices("a", "b"), CancellationToken.None);

        // Assert
        Assert.That(chosen, Is.Empty);
    }

    [Test]
    public async Task GivenChoices_WhenCancelled_ThenChooseManyReturnsEmpty()
    {
        // Arrange
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        // Act
        var chosen = await _io.ChooseManyAsync("Which?", Choices("a", "b"), cancelled.Token);

        // Assert
        Assert.That(chosen, Is.Empty);
    }

    [Test]
    public void GivenNoChoices_WhenChoose_ThenThrows()
    {
        // Arrange
        _console.Input.PushKey(ConsoleKey.Enter);

        // Act & Assert
        Assert.That(async () => await _io.ChooseAsync("Which?", [], CancellationToken.None), Throws.InstanceOf<InvalidOperationException>());
    }

    private static CliChoice[] Choices(params string[] labels)
        => labels.Select(label => new CliChoice(label, label)).ToArray();
}
