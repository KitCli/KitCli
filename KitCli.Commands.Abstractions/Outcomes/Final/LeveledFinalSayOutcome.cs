using KitCli.Abstractions.Io;

namespace KitCli.Commands.Abstractions.Outcomes.Final;

/// <summary>
/// A <see cref="FinalSayOutcome"/> carrying an explicit <see cref="MessageLevel"/>, so a configured
/// minimum level can decide whether it is shown.
/// </summary>
/// <param name="Something">The final message to display.</param>
/// <param name="Level">How important this message is.</param>
public record LeveledFinalSayOutcome(string Something, MessageLevel Level) : FinalSayOutcome(Something);
