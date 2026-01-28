namespace KitCli.Abstractions.Io;

public interface ICliIo 
{
    string? Ask();
    void Pause();
    void Say(string something);
}