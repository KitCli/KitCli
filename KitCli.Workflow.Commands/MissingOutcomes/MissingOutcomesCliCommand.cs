using KitCli.Commands.Abstractions;

namespace KitCli.Workflow.Commands.MissingOutcomes;

public record MissingOutcomesCliCommand(string[] MissingOutcomeNames) : CliCommand;