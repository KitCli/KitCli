namespace KitCli.Instructions.Abstractions;

public class CliInstructionArgument(string argumentName)
{
    public string ArgumentName { get; } = argumentName;
}