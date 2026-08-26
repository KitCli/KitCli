using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

/// <summary>
/// Declared next commands. This command parks the run at a reusable outcome; while it is parked, an
/// ask resolving to no command prints the moves declared here instead of failing silently. Type
/// <c>/test-suggesting</c>, then anything that isn't a command.
/// </summary>
[CliNextCommandIs("test-follow-up", "Pick up where /test-suggesting left off.")]
[CliNextCommandIs("tfu", "The same command, by its shorthand name.")]
public record TestSuggestingCliCommand : CliCommand;

public record TestSuggestingOutcome() : Outcome(OutcomeKind.Reusable);

public class TestSuggestingCliCommandHandler : CliCommandHandler<TestSuggestingCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestSuggestingCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .BySaying("Ask for something that isn't a command to see what this suggests.")
            .ByResultingIn(new TestSuggestingOutcome())
            .EndAsync();
}

public record TestFollowUpCliCommand : CliCommand;

public class TestFollowUpCliCommandHandler : CliCommandHandler<TestFollowUpCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestFollowUpCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByFinallySaying("Reached via a suggested next command!")
            .EndAsync();
}
