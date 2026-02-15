namespace KitCli.Commands.Abstractions.Factories;

public class BasicCliCommandFactory<TCliCommand> : CliCommandFactory<TCliCommand> where TCliCommand : CliCommand, new()
{
    public sealed override bool CanCreateWhen() => true;

    public sealed override CliCommand Create() => new TCliCommand();
}