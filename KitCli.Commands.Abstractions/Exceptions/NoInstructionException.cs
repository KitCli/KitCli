using KitCli.Abstractions.Exceptions;

namespace KitCli.Commands.Abstractions.Exceptions;

public class NoInstructionException : CliException
{
    public NoInstructionException() : base(CliExceptionCode.NoInstruction, string.Empty)
    {
    }
    
    public NoInstructionException(string message) : base(CliExceptionCode.NoInstruction, message)
    {
    }
}