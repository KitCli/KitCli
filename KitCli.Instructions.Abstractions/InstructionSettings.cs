namespace KitCli.Instructions.Abstractions;

public class InstructionSettings
{
    public char Prefix { get; set; } = '/';
    public string ArgumentPrefix { get; set; } = "--";
}
