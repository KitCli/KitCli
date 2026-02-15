using KitCli.Abstractions.Exceptions;

namespace KitCli.Instructions.Abstractions;

// TODO: Write unit test.
public class InstructionException : CliException
{
    public new readonly InstructionExceptionCode Code;

    public InstructionException(InstructionExceptionCode code, string message)
        : base(CliExceptionCode.Instruction, message)
    {
        Code = code;
    }
}