namespace KitCli.Abstractions.Io;

/// <summary>
/// Abstracts console input/output so CLI applications can prompt for input, write output, and react to
/// cancellation without depending directly on <see cref="Console"/>.
/// </summary>
public interface ICliIo
{
    /// <summary>
    /// Asynchronously reads a line of input from the console.
    /// </summary>
    /// <param name="cancellationToken">A token used to abandon the read.</param>
    /// <returns>The line read from the console, or <see langword="null"/> if the read was cancelled.</returns>
    Task<string?> AskAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Writes a blank line to the console.
    /// </summary>
    void Pause();

    /// <summary>
    /// Writes a line of output to the console.
    /// </summary>
    /// <param name="something">The text to write.</param>
    void Say(string something);

    /// <summary>
    /// Sets the console window title.
    /// </summary>
    /// <param name="title">The title to display.</param>
    void SetTitle(string title);

    /// <summary>
    /// Registers a callback to run when the user requests cancellation (e.g. Ctrl+C), suppressing the
    /// default abrupt termination behavior.
    /// </summary>
    /// <param name="cancel">The callback to invoke on cancellation.</param>
    void OnCancel(Action cancel);
}