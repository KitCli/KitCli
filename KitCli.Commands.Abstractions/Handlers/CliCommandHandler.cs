using KitCli.Abstractions.Aggregators.Filters;
using KitCli.Commands.Abstractions.Outcomes;
using MediatR;

namespace KitCli.Commands.Abstractions.Handlers;

public abstract class CliCommandHandler<TCliCommand> : IRequestHandler<TCliCommand, Outcome[]> where TCliCommand : CliCommand
{
    public Task<Outcome[]> Handle(TCliCommand command, CancellationToken cancellationToken)
        => HandleCommand(command, cancellationToken);

    public virtual Task<Outcome[]> HandleCommand(TCliCommand command, CancellationToken cancellationToken) 
        => FinishThisCommand().ByFinallySaying($"No functionality for {command.GetSpecificCommandName()} base command").EndAsync();

    protected static OutcomeList FinishThisCommand() => [];
}