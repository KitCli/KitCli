using KitCli.Commands.Abstractions.Outcomes;
using MediatR;

namespace KitCli.Commands.Abstractions.Handlers;

/// <summary>
/// Base class for a MediatR handler that runs a <typeparamref name="TCliCommand"/> and returns the
/// <see cref="Outcome"/>s it produced.
/// </summary>
/// <typeparam name="TCliCommand">The command type this handler runs.</typeparam>
public abstract class CliCommandHandler<TCliCommand> : IRequestHandler<TCliCommand, Outcome[]> where TCliCommand : CliCommand
{
    /// <summary>
    /// Runs the command by delegating to <see cref="HandleCommand"/>.
    /// </summary>
    /// <param name="command">The command to run.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The outcomes produced by running the command.</returns>
    public Task<Outcome[]> Handle(TCliCommand command, CancellationToken cancellationToken)
        => HandleCommand(command, cancellationToken);

    /// <summary>
    /// Runs the command. The default implementation reports that the base command has no functionality;
    /// override to implement the command's actual behavior.
    /// </summary>
    /// <param name="command">The command to run.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The outcomes produced by running the command.</returns>
    public virtual Task<Outcome[]> HandleCommand(TCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand().ByFinallySaying($"No functionality for {command.GetSpecificCommandName()} base command").EndAsync();

    /// <summary>
    /// Starts an empty <see cref="OutcomeList"/> to build the outcomes this handler returns.
    /// </summary>
    /// <returns>A new, empty outcome list.</returns>
    protected static OutcomeList FinishThisCommand() => [];
}