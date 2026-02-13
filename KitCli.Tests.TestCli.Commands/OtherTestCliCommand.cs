using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Instructions.Abstractions;

namespace KitCli.Tests.TestCli.Commands;

public record OtherTestCliCommand : CliCommand;

public class OtherTetCliCommandFactory : ICliCommandFactory<OtherTestCliCommand>
{
    public CliCommand Create(CliInstruction instruction, List<CliCommandArtefact> artefacts)
    {
        return new OtherTestCliCommand();
    }
}

public class OtherTestCliCommandHandler : CliCommandHandler<OtherTestCliCommand>
{
    public override Task<CliCommandOutcome[]> HandleCommand(OtherTestCliCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new CliCommandOutcome[]
        {
            new OutputCliCommandOutcome("Other Outcome Ran")
        });
    }
}