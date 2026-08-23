using KitCli.Abstractions.Exceptions;

namespace KitCli.Commands.Abstractions.Exceptions;

/// <summary>
/// An exception raised for a command-level failure, carrying a <see cref="CliCommandExceptionCode"/> that
/// narrows the reason beyond the base <see cref="CliException"/>'s <see cref="CliExceptionCode.Command"/>.
/// </summary>
public class CliCommandException : CliException
{
    /// <summary>
    /// The specific command-level reason for this exception.
    /// </summary>
    public new CliCommandExceptionCode Code { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CliCommandException"/> class.
    /// </summary>
    /// <param name="code">The specific command-level reason for this exception.</param>
    /// <param name="message">The exception message.</param>
    public CliCommandException(CliCommandExceptionCode code, string message)
        : base(CliExceptionCode.Command, message)
    {
        Code = code;
    }
}