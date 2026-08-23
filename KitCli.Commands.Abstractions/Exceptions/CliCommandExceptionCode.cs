namespace KitCli.Commands.Abstractions.Exceptions;

/// <summary>
/// Narrows the reason behind a <see cref="CliCommandException"/>.
/// </summary>
public enum CliCommandExceptionCode
{
    /// <summary>
    /// A command has no functionality available to run it.
    /// </summary>
    NoCommandFunctionality,

    /// <summary>
    /// A command returned or encountered an outcome type that isn't recognized.
    /// </summary>
    UnkownCliCommandOutcome
}