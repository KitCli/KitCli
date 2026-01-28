using KitCli.Abstractions.Exceptions;

namespace KitCli.Commands.Abstractions.Exceptions;

public class CliCommandException : CliException
{
    public new CliCommandExceptionCode Code { get; }

    public CliCommandException(CliCommandExceptionCode code, string message)
        : base(CliExceptionCode.Command, message)
    {
        Code = code;
    }
}