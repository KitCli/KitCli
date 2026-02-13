using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Tests.TestCli.Commands;

public record TestNextCliCommand : CliCommand;

public class TestNextCliCommandHandler : CliCommandHandler<TestNextCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestNextCliCommand request, CancellationToken cancellationToken)
    {
        var nextCommand = new TestNextOneCliCommand("I'm first (1)");

        return FinishThisCommand()
            .BySaying("Initial Command Ran (0)")
            .ByMovingToCommand(nextCommand)
            .EndAsync();
    }
}

public record TestNextOneCliCommand(string Text) : CliCommand;

public class NextTestOneCliCommandHandler : CliCommandHandler<TestNextOneCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestNextOneCliCommand request, CancellationToken cancellationToken)
    {
        var nextCommand = new NextTestTwoCliCommand("I'm second (2)");
        
        return FinishThisCommand()
            .BySaying("Next Test One Command Ran (1)")
            .ByMovingToCommand(nextCommand)
            .EndAsync();
    }
}

public record NextTestTwoCliCommand(string Text) : CliCommand;

public class NextTestTwoCliCommandHandler : CliCommandHandler<NextTestTwoCliCommand>
{
    public override Task<Outcome[]> HandleCommand(NextTestTwoCliCommand request, CancellationToken cancellationToken)
    {
        var nextCommand = new NextTestThreeCliCommand("I'm third (3)");
        
        return FinishThisCommand()
            .BySaying("Next Test Two Command Ran (2)")
            .ByMovingToCommand(nextCommand)
            .EndAsync();
    }
}


public record NextTestThreeCliCommand(string Text) : CliCommand;

public class NextTestThreeCliCommandHandler : CliCommandHandler<NextTestThreeCliCommand>
{
    public override Task<Outcome[]> HandleCommand(NextTestThreeCliCommand request, CancellationToken cancellationToken)
    {
        var nextCommand = new NextTestFourCliCommand("I'm fourth (4)");
        
        return FinishThisCommand()
            .BySaying("Next Test Three Command Ran (3)")
            .ByMovingToCommand(nextCommand)
            .EndAsync();
    }
}

public record NextTestFourCliCommand(string Text) : CliCommand;

public class NextTestFourCliCommandHandler : CliCommandHandler<NextTestFourCliCommand>
{
    public override Task<Outcome[]> HandleCommand(NextTestFourCliCommand request, CancellationToken cancellationToken)
    {
        return FinishThisCommand()
            .BySaying("Next Test Four Command Ran (4)")
            .ByFinallySaying("All Done!")
            .EndAsync();
    }
}
