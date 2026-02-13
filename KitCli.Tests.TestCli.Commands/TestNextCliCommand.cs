using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Tests.TestCli.Commands;

public record TestNextCliCommand : CliCommand;

public class TestNextCliCommandHandler : CliCommandHandler<TestNextCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestNextCliCommand request, CancellationToken cancellationToken)
    {
        var outcome = new NextCliCommandOutcome(new  NextTestOneCliCommand("Send from original command"));
        
        return Task.FromResult(new Outcome[]
        {
            outcome
        });
    }
}

public record NextTestOneCliCommand(string Text) : CliCommand;

public class NextTestOneCliCommandHandler : CliCommandHandler<NextTestOneCliCommand>
{
    public override Task<Outcome[]> HandleCommand(NextTestOneCliCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Next Test One Command Ran with Text: {request.Text}");
        
        var outcome = new NextCliCommandOutcome(new  NextTestTwoCliCommand("Send from next test one command"));
        return Task.FromResult(new Outcome[]
        {
            outcome
        });
    }
}


public record NextTestTwoCliCommand(string Text) : CliCommand;

public class NextTestTwoCliCommandHandler : CliCommandHandler<NextTestTwoCliCommand>
{
    public override Task<Outcome[]> HandleCommand(NextTestTwoCliCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Next Test Two Command Ran with Text: {request.Text}");
        var outcome = new NextCliCommandOutcome(new  NextTestThreeCliCommand("Send from next test two command"));
        return Task.FromResult(new Outcome[]
        {
            outcome
        });
    }
}


public record NextTestThreeCliCommand(string Text) : CliCommand;

public class NextTestThreeCliCommandHandler : CliCommandHandler<NextTestThreeCliCommand>
{
    public override Task<Outcome[]> HandleCommand(NextTestThreeCliCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Next Test Three Command Ran with Text: {request.Text}");
        var outcome = new NextCliCommandOutcome(new  NextTestFourCliCommand("Send from next test three command"));
        return Task.FromResult(new Outcome[]
        {
            outcome
        });
    }
}

public record NextTestFourCliCommand(string Text) : CliCommand;

public class NextTestFourCliCommandHandler : CliCommandHandler<NextTestFourCliCommand>
{
    public override Task<Outcome[]> HandleCommand(NextTestFourCliCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Next Test Four Command Ran with Text: {request.Text}");
        return Task.FromResult(new Outcome[]
        {
            new FinalMessageOutcome("Final Outcome from Next Test Four Command")
        });
    }
}
