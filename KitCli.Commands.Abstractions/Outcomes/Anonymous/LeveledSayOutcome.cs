namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

/// <summary>
/// A <see cref="SayOutcome"/> carrying an explicit <see cref="MessageLevel"/>, so a configured
/// minimum level can decide whether it is shown.
/// </summary>
/// <param name="Something">The message to display.</param>
/// <param name="Level">How important this message is.</param>
public record LeveledSayOutcome(string Something, MessageLevel Level) : SayOutcome(Something);
