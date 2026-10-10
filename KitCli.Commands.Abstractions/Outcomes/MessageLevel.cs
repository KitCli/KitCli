namespace KitCli.Commands.Abstractions.Outcomes;

/// <summary>
/// How important a message is, from quietest to loudest.
/// </summary>
public enum MessageLevel
{
    /// <summary>
    /// The most detailed diagnostic information, typically only valuable when actively debugging.
    /// </summary>
    Trace,

    /// <summary>
    /// Information useful during development or troubleshooting.
    /// </summary>
    Debug,

    /// <summary>
    /// Describes the normal flow of the application. The default level.
    /// </summary>
    Information,

    /// <summary>
    /// Highlights an abnormal or unexpected event that does not stop the current operation.
    /// </summary>
    Warning,

    /// <summary>
    /// Highlights an operation that failed.
    /// </summary>
    Error
}
