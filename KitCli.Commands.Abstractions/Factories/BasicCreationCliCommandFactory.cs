namespace KitCli.Commands.Abstractions.Factories;

public abstract class BasicCreationCliCommandFactory<TCliCommand> : CliCommandFactory<TCliCommand> where TCliCommand : CliCommand
{
    public sealed override bool CanCreateWhen() => true;
}