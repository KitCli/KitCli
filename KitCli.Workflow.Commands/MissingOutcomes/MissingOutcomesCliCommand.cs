using KitCli.Commands.Abstractions;

namespace KitCli.Workflow.Commands.MissingOutcomes;

// TODO: Revisit strategy for reporting missing outcomes.
internal record MissingOutcomesCliCommand(string[] MissingOutcomeNames) : CliCommand;