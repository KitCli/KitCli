namespace KitCli.Commands.Abstractions.Outcomes.Final;

public record FinalSayOutcome(string Something) : Outcome(OutcomeKind.Final);