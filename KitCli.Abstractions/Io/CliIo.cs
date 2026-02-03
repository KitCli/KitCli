namespace KitCli.Abstractions.Io;

public class CliIo : ICliIo
{
    public string? Ask()
        => Console.ReadLine();

    public void Pause()
        => Console.WriteLine();
    
    public void Say(string something)
        => Console.WriteLine(something);
    
    public void SetTitle(string title)
        => Console.Title = title;

    public void OnCancel(Action cancel) 
        => Console.CancelKeyPress += (sender, e) => cancel();
}