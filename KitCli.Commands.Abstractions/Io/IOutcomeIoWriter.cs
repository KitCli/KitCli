using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// Displays a specific <see cref="Outcome"/> type to the user.
/// </summary>
public interface IOutcomeIoWriter
{
    // TODO: Follow similar pattern to ArtefactFactory.
    /// <summary>
    /// Determines whether this writer can display the given outcome.
    /// </summary>
    /// <param name="outcome">The outcome to check.</param>
    /// <returns><see langword="true"/> if this writer can display <paramref name="outcome"/>; otherwise <see langword="false"/>.</returns>
    bool CanWriteFor(Outcome outcome);

    /// <summary>
    /// Displays the given outcome.
    /// </summary>
    /// <param name="outcome">The outcome to display.</param>
    void Write(Outcome outcome);
}