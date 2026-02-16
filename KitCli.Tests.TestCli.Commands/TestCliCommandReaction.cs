using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Tests.TestCli.Commands;

public record TestReactionCliCommand : CliCommand;

public class TestReactionCliCommandHandler : CliCommandHandler<TestReactionCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestReactionCliCommand request, CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByReacting(new TestCliCommandReaction())
            .EndAsync();
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