namespace KitCli.Commands.Abstractions.Exceptions;

/// <summary>
/// Thrown when an outcome of an unrecognized type is encountered where a known outcome type was expected.
/// </summary>
/// <param name="message">The exception message.</param>
public class UnknownOutcomeException(string message)
    : CliCommandException(CliCommandExceptionCode.UnkownCliCommandOutcome, message)
{
}