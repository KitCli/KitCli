namespace KitCli.Abstractions.Exceptions;

public enum CliExceptionCode
{
    Instruction,
    Command,
    Custom,
    
    NoCommandGenerator,
    NoInstruction
}