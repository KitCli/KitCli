using KitCli.Abstractions.Exceptions;
using Spectre.Console;

namespace KitCli.Abstractions.Io;

/// <summary>
/// The I/O a headless app runs with: the console for output and Ctrl+C, with nothing attached to its
/// input. <see cref="CanAsk"/> says so ahead of time, and asking or offering a choice anyway is a
/// programming error that throws <see cref="NoInputException"/>, so a command can decline to prompt where
/// nobody would answer. Re-implements <see cref="ICliIo"/> to replace the interface's default answer to
/// <see cref="CanAsk"/>.
/// </summary>
public class HeadlessCliIo(IAnsiConsole? console = null, KitCliSettings? kitCliSettings = null)
    : CliIo(console, kitCliSettings), ICliIo
{
    /// <inheritdoc/>
    public bool CanAsk => false;

    /// <inheritdoc/>
    /// <exception cref="NoInputException">Always: nothing is attached to the input to answer.</exception>
    public override Task<string?> AskAsync(CancellationToken cancellationToken)
        => throw new NoInputException();

    /// <inheritdoc/>
    /// <exception cref="NoInputException">Always: nothing is attached to the input to choose.</exception>
    public override Task<CliChoice?> ChooseAsync(string title, IEnumerable<CliChoice> choices, CancellationToken cancellationToken)
        => throw new NoInputException();

    /// <inheritdoc/>
    /// <exception cref="NoInputException">Always: nothing is attached to the input to choose.</exception>
    public override Task<IReadOnlyList<CliChoice>> ChooseManyAsync(string title, IEnumerable<CliChoice> choices, CancellationToken cancellationToken)
        => throw new NoInputException();
}
