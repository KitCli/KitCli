using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// Writes a <see cref="SuggestionOutcome"/> by displaying a blank line, then its instruction name
/// indented two spaces, with its description indented four — so a suggestion is always visually
/// separated from whatever preceded it, and reads as one indented block rather than two flush-left
/// lines.
/// </summary>
/// <param name="cliIo">The IO surface to write to.</param>
public class SuggestionOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    internal const string NameSpacing = "  ";
    internal const string DescriptionSpacing = "    ";
    
    /// <inheritdoc/>
    public bool CanWriteFor(Outcome outcome) => outcome is SuggestionOutcome;

    /// <inheritdoc/>
    public void Write(Outcome outcome)
    {
        var suggestionOutcome = (SuggestionOutcome)outcome;

        cliIo.SayCommandSuggestion(suggestionOutcome.Name, suggestionOutcome.Description);
    }
}
