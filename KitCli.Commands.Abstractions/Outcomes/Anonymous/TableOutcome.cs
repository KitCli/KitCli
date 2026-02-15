using KitCli.Abstractions.Tables;

namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

public record TableOutcome(Table Table) : Outcome(OutcomeKind.Anonymous);