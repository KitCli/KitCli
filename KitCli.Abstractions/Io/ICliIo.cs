namespace KitCli.Abstractions.Io;

public interface ICliIo 
{
    string? Ask();
    void Pause();
    void Say(string something);
    void SetTitle(string title);
    void OnCancel(Action cancel);
}