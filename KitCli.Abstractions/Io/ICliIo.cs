namespace KitCli.Abstractions.Io;

public interface ICliIo
{
    Task<string?> AskAsync(CancellationToken cancellationToken);
    void Pause();
    void Say(string something);
    void SetTitle(string title);
    void OnCancel(Action cancel);
}