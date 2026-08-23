using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// Writes a <see cref="SuggestionOutcome"/> by displaying a blank line, then its instruction name and
/// description, so a suggestion is always visually separated from whatever preceded it.
/// </summary>
/// <param name="cliIo">The IO surface to write to.</param>
public class SuggestionOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    /// <inheritdoc/>
    public bool CanWriteFor(Outcome outcome) => outcome is SuggestionOutcome;

    /// <inheritdoc/>
    public void Write(Outcome outcome)
    {
        var suggestionOutcome = (SuggestionOutcome)outcome;
        cliIo.Pause();
        cliIo.Say(suggestionOutcome.Name);
        cliIo.Say(suggestionOutcome.Description);
    }
}
