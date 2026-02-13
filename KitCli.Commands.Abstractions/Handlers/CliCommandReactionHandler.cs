using MediatR;

namespace KitCli.Commands.Abstractions.Handlers;

public abstract class CliCommandReactionHandler<TReaction> : INotificationHandler<TReaction> where TReaction : CliCommandReaction
{
    public Task Handle(TReaction reaction, CancellationToken cancellationToken)
        => HandleReaction(reaction, cancellationToken);

    public abstract Task HandleReaction(TReaction notification, CancellationToken cancellationToken);
}