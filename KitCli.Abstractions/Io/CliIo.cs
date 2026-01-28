namespace KitCli.Abstractions.Io;

public class CliIo : ICliIo
{
    public string? Ask()
        => Console.ReadLine();

    public void Pause()
        => Console.WriteLine();
    
    public void Say(string something)
        => Console.WriteLine(something);
}