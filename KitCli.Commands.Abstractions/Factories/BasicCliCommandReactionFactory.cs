namespace KitCli.Commands.Abstractions.Factories;

/// <summary>
/// The default factory used for a reaction type that has no dedicated <see cref="CliCommandReactionFactory{TReaction}"/>
/// and a public parameterless constructor — it always creates the reaction via <see langword="new"/>.
/// </summary>
/// <typeparam name="TReaction">The reaction type this factory creates.</typeparam>
public class BasicCliCommandReactionFactory<TReaction> : CliCommandReactionFactory<TReaction> where TReaction : CliCommandReaction, new()
{
    /// <inheritdoc/>
    public sealed override bool CanCreateWhen() => true;

    /// <inheritdoc/>
    public sealed override CliCommandReaction Create() => new TReaction();
}
