using KitCli.Commands.Abstractions.Factories;

namespace KitCli.Workflow.Commands.Exit;

/// <summary>
/// Creates <see cref="ExitCliCommand"/> instances using the default, unconditional
/// creation behavior provided by <see cref="BasicCliCommandFactory{TCliCommand}"/>.
/// </summary>
public class ExitCliCommandFactory : BasicCliCommandFactory<ExitCliCommand>;