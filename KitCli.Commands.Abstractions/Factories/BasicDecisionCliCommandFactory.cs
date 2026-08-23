namespace KitCli.Commands.Abstractions.Factories;

/// <summary>
/// Base class for a command factory that always creates its command the same way — via
/// <see langword="new"/> on its parameterless constructor — but needs custom
/// <see cref="CliCommandFactory{TCliCommand}.CanCreateWhen"/> logic to decide, at runtime, whether it
/// is the right variant to handle the current instruction (e.g. picking between command variants keyed
/// under the same name).
/// </summary>
/// <typeparam name="TCliCommand">The command type this factory creates.</typeparam>
public abstract class BasicDecisionCliCommandFactory<TCliCommand> : CliCommandFactory<TCliCommand> where TCliCommand : CliCommand, new()
{
    /// <inheritdoc/>
    public sealed override CliCommand Create() => new TCliCommand();
}