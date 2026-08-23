namespace KitCli.Commands.Abstractions.Factories;

/// <summary>
/// The default factory used for a command type that has no dedicated <see cref="CliCommandFactory{TCliCommand}"/>
/// and a public parameterless constructor — it always creates the command via <see langword="new"/>.
/// </summary>
/// <typeparam name="TCliCommand">The command type this factory creates.</typeparam>
public class BasicCliCommandFactory<TCliCommand> : CliCommandFactory<TCliCommand> where TCliCommand : CliCommand, new()
{
    /// <inheritdoc/>
    public sealed override bool CanCreateWhen() => true;

    /// <inheritdoc/>
    public sealed override CliCommand Create() => new TCliCommand();
}