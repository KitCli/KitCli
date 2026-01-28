using KitCli.Commands.Abstractions.Outcomes;
using MediatR;

namespace KitCli.Commands.Abstractions.Handlers;

// TODO: Make MediatR dependency not inheritable to other packages.
public interface ICliCommandHandler<in TCommand> : IRequestHandler<TCommand, CliCommandOutcome[]>
    where TCommand : CliCommand
{
}