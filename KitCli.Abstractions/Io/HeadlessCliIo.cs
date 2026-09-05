using KitCli.Abstractions.Exceptions;

namespace KitCli.Abstractions.Io;

/// <summary>
/// The I/O a headless app runs with: the console for output and Ctrl+C, with nothing attached to its
/// input. <see cref="CanAsk"/> says so ahead of time, and asking anyway is a programming error that
/// throws <see cref="NoInputException"/>, so a command can decline to prompt where nobody would answer.
/// Re-implements <see cref="ICliIo"/> to replace the interface's default answer to <see cref="CanAsk"/>.
/// </summary>
public class HeadlessCliIo : CliIo, ICliIo
{
    /// <inheritdoc/>
    public bool CanAsk => false;

    /// <inheritdoc/>
    /// <exception cref="NoInputException">Always: nothing is attached to the input to answer.</exception>
    public override Task<string?> AskAsync(CancellationToken cancellationToken)
        => throw new NoInputException();
}
