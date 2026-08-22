using KitCli.Abstractions.Exceptions;

namespace KitCli.Tooling.Release;

// TODO: Write unit test.
public class ReleaseException : CliException
{
    public new readonly ReleaseExceptionCode Code;

    public ReleaseException(ReleaseExceptionCode code, string message)
        : base(CliExceptionCode.Custom, message)
    {
        Code = code;
    }
}
