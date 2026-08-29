using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

public record TestSpecifiedReactionCliCommand : CliCommand;

public class TestSpecifiedReactionCliCommandHandler : CliCommandHandler<TestSpecifiedReactionCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestSpecifiedReactionCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByResultingIn(new TestReactionSourceOutcome("This text was created in the publishing command"))
            .ByReacting<TestBasicSpecifiedCliCommandReaction>()
            .ByReacting<TestFactoryBuiltSpecifiedCliCommandReaction>()
            .ByFinallySaying("Specified reactions published.")
            .EndAsync();
}

public record TestReactionSourceOutcome(string Text) : Outcome(OutcomeKind.Reusable);

public record TestReactionSourceArtefact(string Text) : Artefact<string>(nameof(TestReactionSourceArtefact), Text);

public class TestReactionSourceArtefactFactory : ArtefactFactory<TestReactionSourceOutcome>
{
    protected override AnonymousArtefact CreateArtefact(TestReactionSourceOutcome outcome)
        => new TestReactionSourceArtefact(outcome.Text);
}

public record TestBasicSpecifiedCliCommandReaction : CliCommandReaction;

public class TestBasicSpecifiedCliCommandReactionHandler : CliCommandReactionHandler<TestBasicSpecifiedCliCommandReaction>
{
    public override Task HandleReaction(TestBasicSpecifiedCliCommandReaction notification, CancellationToken cancellationToken)
    {
        Console.WriteLine("Test Specified Reaction Handled - Built By Basic Factory");
        return Task.CompletedTask;
    }
}

public record TestFactoryBuiltSpecifiedCliCommandReaction(string Text) : CliCommandReaction;

public class TestFactoryBuiltSpecifiedCliCommandReactionFactory : CliCommandReactionFactory<TestFactoryBuiltSpecifiedCliCommandReaction>
{
    public override bool CanCreateWhen() => AnyArtefact<string>(nameof(TestReactionSourceArtefact));

    public override CliCommandReaction Create()
        => new TestFactoryBuiltSpecifiedCliCommandReaction(
            GetRequiredArtefact<string>(nameof(TestReactionSourceArtefact)).Value);
}

public class TestFactoryBuiltSpecifiedCliCommandReactionHandler : CliCommandReactionHandler<TestFactoryBuiltSpecifiedCliCommandReaction>
{
    public override Task HandleReaction(TestFactoryBuiltSpecifiedCliCommandReaction notification, CancellationToken cancellationToken)
    {
        Console.WriteLine("Test Specified Reaction Handled - " + notification.Text);
        return Task.CompletedTask;
    }
}
