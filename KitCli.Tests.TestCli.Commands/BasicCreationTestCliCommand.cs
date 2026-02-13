using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Instructions.Abstractions;

namespace KitCli.Tests.TestCli.Commands;

public record BasicCreationTestCliCommand(string Test) : CliCommand;

public class BasicCreationTestClICommandFactory : BasicCreationCliCommandFactory<BasicCreationTestCliCommand>
{
    public override CliCommand Create(CliInstruction instruction, List<CliCommandArtefact> artefacts)
        => new BasicCreationTestCliCommand("Basic Creation Test Command Ran");
}

public class BasicCreationTestClICommandHandler : CliCommandHandler<BasicCreationTestCliCommand>
{
    public override Task<CliCommandOutcome[]> HandleCommand(BasicCreationTestCliCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new CliCommandOutcome[]
        {
            new OutputCliCommandOutcome(request.Test)
        });
    }
}