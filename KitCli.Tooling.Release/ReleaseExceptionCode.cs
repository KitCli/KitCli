namespace KitCli.Tooling.Release;

public enum ReleaseExceptionCode
{
    SolutionNotFound,
    NoPackableProjectsFound,
    CircularProjectReference,
    ProcessFailedToStart,
    DotnetCommandFailed,
    GitCommandFailed,
}
