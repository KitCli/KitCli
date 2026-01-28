namespace KitCli.Abstractions.Exceptions;

public class CliException : Exception
{
    public CliExceptionCode Code { get; }

    public CliException()
    {
    }

    public CliException(CliExceptionCode code, string message) : base(message)
    {
        Code = code;
    }
}