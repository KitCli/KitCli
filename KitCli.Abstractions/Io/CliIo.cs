namespace KitCli.Abstractions.Io;

public class CliIo : ICliIo
{
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

    public void Pause()
        => Console.WriteLine();

    public void Say(string something)
        => Console.WriteLine(something);

    public void SetTitle(string title)
        => Console.Title = title;

    public void OnCancel(Action cancel)
        => Console.CancelKeyPress += (_, e) =>
        {
            SuppressDefaultAbruptTermination(e);
            cancel();
        };

    private static void SuppressDefaultAbruptTermination(ConsoleCancelEventArgs e)
        => e.Cancel = true;
}