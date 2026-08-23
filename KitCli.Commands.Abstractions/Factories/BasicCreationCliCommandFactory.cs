namespace KitCli.Commands.Abstractions.Factories;

/// <summary>
/// Base class for a command factory that always creates its command — <see cref="CanCreateWhen"/> is fixed
/// to <see langword="true"/> — but still needs custom <see cref="CliCommandFactory{TCliCommand}.Create"/>
/// logic, e.g. to read arguments or artefacts.
/// </summary>
/// <typeparam name="TCliCommand">The command type this factory creates.</typeparam>
public abstract class BasicCreationCliCommandFactory<TCliCommand> : CliCommandFactory<TCliCommand> where TCliCommand : CliCommand
{
    /// <inheritdoc/>
    public sealed override bool CanCreateWhen() => true;
}