using KitCli.Abstractions.Exceptions;

namespace KitCli.Commands.Abstractions.Exceptions;

/// <summary>
/// Thrown when an app is run without a single <see cref="CliCommand"/> registered besides the built-in
/// workflow commands — neither the entry assembly nor any registry added one.
/// </summary>
public class NoCommandsException : CliException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoCommandsException"/> class with an empty message.
    /// </summary>
    public NoCommandsException() : base(CliExceptionCode.NoCommands, string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoCommandsException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public NoCommandsException(string message) : base(CliExceptionCode.NoCommands, message)
    {
    }
}
