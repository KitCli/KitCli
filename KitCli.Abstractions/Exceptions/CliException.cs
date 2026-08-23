namespace KitCli.Abstractions.Exceptions;

/// <summary>
/// The exception type thrown for known, categorized CLI failures.
/// </summary>
public class CliException : Exception
{
    /// <summary>
    /// The category of failure that caused this exception.
    /// </summary>
    public CliExceptionCode Code { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CliException"/> class with no code or message.
    /// </summary>
    public CliException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CliException"/> class with a specific failure category and message.
    /// </summary>
    /// <param name="code">The category of failure that caused this exception.</param>
    /// <param name="message">The message that describes the error.</param>
    public CliException(CliExceptionCode code, string message) : base(message)
    {
        Code = code;
    }
}