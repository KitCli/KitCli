using KitCli.Abstractions.Exceptions;

namespace KitCli.Commands.Abstractions.Exceptions;

/// <summary>
/// Thrown when a command-related operation requires an instruction that isn't available.
/// </summary>
public class NoInstructionException : CliException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoInstructionException"/> class with an empty message.
    /// </summary>
    public NoInstructionException() : base(CliExceptionCode.NoInstruction, string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoInstructionException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public NoInstructionException(string message) : base(CliExceptionCode.NoInstruction, message)
    {
    }
}