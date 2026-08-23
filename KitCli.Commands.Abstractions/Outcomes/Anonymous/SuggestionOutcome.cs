namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

/// <summary>
/// A suggested instruction name and what it does, with no effect on the workflow run.
/// </summary>
/// <param name="Name">The suggested instruction name, including its prefix.</param>
/// <param name="Description">A short description of what that instruction does.</param>
public record SuggestionOutcome(string Name, string Description) : Outcome(OutcomeKind.Anonymous);
