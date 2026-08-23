using KitCli.Abstractions.Exceptions;

namespace KitCli.Instructions.Abstractions;

// TODO: Write unit test.
/// <summary>
/// The exception thrown when an instruction cannot be parsed or validated.
/// </summary>
public class InstructionException : CliException
{
    /// <summary>
    /// The specific reason the instruction failed.
    /// </summary>
    public new readonly InstructionExceptionCode Code;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionException"/> class.
    /// </summary>
    /// <param name="code">The specific reason the instruction failed.</param>
    /// <param name="message">A message describing the error.</param>
    public InstructionException(InstructionExceptionCode code, string message)
        : base(CliExceptionCode.Instruction, message)
    {
        Code = code;
    }
}