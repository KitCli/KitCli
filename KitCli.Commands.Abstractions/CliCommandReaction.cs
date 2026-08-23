using MediatR;

namespace KitCli.Commands.Abstractions;

// TODO: Can I add a reaction factory too?
/// <summary>
/// A notification published as a side effect of a command running, handled by any registered
/// <see cref="Handlers.CliCommandReactionHandler{TReaction}"/> for its concrete type.
/// </summary>
public abstract record CliCommandReaction : INotification;