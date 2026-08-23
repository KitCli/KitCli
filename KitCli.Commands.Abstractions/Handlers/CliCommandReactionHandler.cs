using MediatR;

namespace KitCli.Commands.Abstractions.Handlers;

/// <summary>
/// Base class for a MediatR notification handler that reacts to a <typeparamref name="TReaction"/> published
/// as a side effect of a command running.
/// </summary>
/// <typeparam name="TReaction">The reaction type this handler responds to.</typeparam>
public abstract class CliCommandReactionHandler<TReaction> : INotificationHandler<TReaction> where TReaction : CliCommandReaction
{
    /// <summary>
    /// Handles the reaction by delegating to <see cref="HandleReaction"/>.
    /// </summary>
    /// <param name="reaction">The published reaction.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task that completes when the reaction has been handled.</returns>
    public Task Handle(TReaction reaction, CancellationToken cancellationToken)
        => HandleReaction(reaction, cancellationToken);

    /// <summary>
    /// Handles the reaction.
    /// </summary>
    /// <param name="notification">The published reaction.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A task that completes when the reaction has been handled.</returns>
    public abstract Task HandleReaction(TReaction notification, CancellationToken cancellationToken);
}