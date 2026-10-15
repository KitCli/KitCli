namespace KitCli.Abstractions.Io;

/// <summary>
/// Abstracts console input/output so CLI applications can prompt for input, write output, and react to
/// cancellation without depending directly on <see cref="Console"/>.
/// </summary>
public interface ICliIo
{
    /// <summary>
    /// Whether anything is attached to the input to answer an ask. <see langword="false"/> under a headless
    /// app, whose only ask is the process args, so a command that would confirm or prompt can fail with
    /// what to pass instead.
    /// </summary>
    bool CanAsk => true;

    /// <summary>
    /// Asynchronously reads a line of input from the console.
    /// </summary>
    /// <param name="cancellationToken">A token used to abandon the read.</param>
    /// <returns>The line read from the console, or <see langword="null"/> if the read was cancelled.</returns>
    Task<string?> AskAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Shows a list the person moves through with the arrow keys and picks one item from with enter.
    /// </summary>
    /// <param name="title">The question shown above the list.</param>
    /// <param name="choices">The choices to pick from.</param>
    /// <param name="cancellationToken">A token used to abandon the prompt.</param>
    /// <returns>The picked choice, or <see langword="null"/> if the prompt was cancelled.</returns>
    Task<CliChoice?> ChooseAsync(string title, IEnumerable<CliChoice> choices, CancellationToken cancellationToken);

    /// <summary>
    /// Shows a list the person moves through with the arrow keys, ticks choices in with the space bar and
    /// confirms with enter.
    /// </summary>
    /// <param name="title">The question shown above the list.</param>
    /// <param name="choices">The choices to pick from.</param>
    /// <param name="cancellationToken">A token used to abandon the prompt.</param>
    /// <returns>The ticked choices, or an empty list if nothing was ticked or the prompt was cancelled.</returns>
    Task<IReadOnlyList<CliChoice>> ChooseManyAsync(string title, IEnumerable<CliChoice> choices, CancellationToken cancellationToken);

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
    /// Writes a line of output to the console, unless <paramref name="level"/> is below the configured
    /// <see cref="KitCliSettings.MinimumMessageLevel"/>.
    /// </summary>
    /// <param name="something">The text to write.</param>
    /// <param name="level">How important the text is.</param>
    void Say(string something, MessageLevel level);

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