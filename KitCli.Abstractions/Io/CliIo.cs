namespace KitCli.Abstractions.Io;

/// <summary>
/// Default <see cref="ICliIo"/> implementation backed by the standard <see cref="Console"/>.
/// </summary>
public class CliIo : ICliIo
{
    /// <inheritdoc/>
    public async Task<string?> AskAsync(CancellationToken cancellationToken)
    {
        var abandonableBackgroundConsoleRead = Task.Run(Console.ReadLine, CancellationToken.None);

        try
        {
            return await abandonableBackgroundConsoleRead.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public void Pause()
        => Console.WriteLine();

    /// <inheritdoc/>
    public void Say(string something)
        => Console.WriteLine(something);

    /// <inheritdoc/>
    public void SetTitle(string title)
        => Console.Title = title;

    /// <inheritdoc/>
    public void OnCancel(Action cancel)
        => Console.CancelKeyPress += (_, e) =>
        {
            SuppressDefaultAbruptTermination(e);
            cancel();
        };

    private static void SuppressDefaultAbruptTermination(ConsoleCancelEventArgs e)
        => e.Cancel = true;
}