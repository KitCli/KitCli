using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Instructions.Abstractions;

namespace KitCli.Tests.TestCli.Commands;

public record BasicDecisionTestCliCommand : CliCommand;

public class BasicDecisionTestClICommandFactory : BasicDecisionCliCommandFactory<BasicDecisionTestCliCommand>
{
    public override bool CanCreateWhen(CliInstruction instruction, List<CliCommandArtefact> artefacts)
        => instruction.SubInstructionName == "test";
}

public class BasicDecisionTestClICommandHandler : CliCommandHandler<BasicDecisionTestCliCommand>
{
    public override Task<Outcome[]> HandleCommand(BasicDecisionTestCliCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(new Outcome[]
        {
            new FinalMessageOutcome("Basic Decision Test Command Ran")
        });
    }
}