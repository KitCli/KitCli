namespace KitCli.Commands.Abstractions.Factories;

public abstract class BasicDecisionCliCommandFactory<TCliCommand> : CliCommandFactory<TCliCommand> where TCliCommand : CliCommand, new()
{
    public sealed override CliCommand Create() => new TCliCommand();
}