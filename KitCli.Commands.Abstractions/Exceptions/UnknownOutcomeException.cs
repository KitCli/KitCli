namespace KitCli.Commands.Abstractions.Exceptions;

public class UnknownOutcomeException(string message)
    : CliCommandException(CliCommandExceptionCode.UnkownCliCommandOutcome, message)
{
}