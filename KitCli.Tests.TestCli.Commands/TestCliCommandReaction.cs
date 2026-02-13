using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Skippable;

public record TestReactionCliCommand : CliCommand;

public class TestReactionCliCommandHandler : CliCommandHandler<TestReactionCliCommand>
{
    public override Task<CliCommandOutcome[]> HandleCommand(TestReactionCliCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new CliCommandOutcome[]
        {
            new ReactionCliCommandOutcome
            {
                Reaction = new TestCliCommandReaction()
            }
        });
    }
}

public record TestCliCommandReaction : CliCommandReaction;

public class TestCliCommandReactionHandlerOne : CliCommandReactionHandler<TestCliCommandReaction>
{
    public override Task HandleReaction(TestCliCommandReaction notification, CancellationToken cancellationToken)
    {
        Console.WriteLine("Test Reaction Handled - Handler One");
        return Task.CompletedTask;
    }
}

public class TestCliCommandReactionHandlerTwo : CliCommandReactionHandler<TestCliCommandReaction>
{
    public override Task HandleReaction(TestCliCommandReaction notification, CancellationToken cancellationToken)
    {
        Console.WriteLine("Test Reaction Handled - Handler Two");
        return Task.CompletedTask;
    }
}