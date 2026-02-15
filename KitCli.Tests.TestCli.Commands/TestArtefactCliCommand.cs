using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Tests.TestCli.Commands;

public record TestArtefactCliCommand : CliCommand;

// No command handler, automatically registered.

public record TestOutcome(string Text) : Outcome(OutcomeKind.Reusable);

public record TestArtefact(string Text) : Artefact<string>(nameof(TestArtefact), Text);

public class TestArtefactFactory : ArtefactFactory<TestOutcome>
{
    protected override AnonymousArtefact CreateArtefact(TestOutcome outcome)
        => new TestArtefact(outcome.Text);
}

public class TestArtefactCliCommandHandler : CliCommandHandler<TestArtefactCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestArtefactCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByResultingIn(new TestOutcome("This text was created in original command"))
            .EndAsync();
}

public record TestArtefactResultCliCommand(string Text) : CliCommand;

public class TestArtefactResultCliCommandFactory : BasicCreationCliCommandFactory<TestArtefactResultCliCommand>
{
    public override CliCommand Create()
    {
        var testArtefact = GetRequiredArtefact<string>(nameof(TestArtefact));

        return new TestArtefactResultCliCommand(testArtefact.Value);
    }
}

public class TestArtefactResultCliCommandHandler : CliCommandHandler<TestArtefactResultCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestArtefactResultCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByFinallySaying(command.Text)
            .EndAsync();
}