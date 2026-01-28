using KitCli.Abstractions.Exceptions;

namespace KitCli.Instructions.Abstractions;

// TODO: Write unit test.
public class CliInstructionException : CliException
{
    public new readonly CliInstructionExceptionCode Code;

    public CliInstructionException(CliInstructionExceptionCode code, string message)
        : base(CliExceptionCode.Instruction, message)
    {
        Code = code;
    }
}