namespace KitCli.Commands.Abstractions.Factories;

public interface ICliCommandFactory<TCliCommand> : IUnidentifiedCliCommandFactory where TCliCommand : class
{
    // This is helping us with reflection for DI.
}