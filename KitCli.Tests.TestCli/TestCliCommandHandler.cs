using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

public class TestCliCommandHandler : ICliCommandHandler<TestCliCommand>
{
    public Task<CliCommandOutcome[]> Handle(TestCliCommand request, CancellationToken cancellationToken)
    {
        var outcome = new CliCommandOutputOutcome("Output: TestCliCommand executed successfully.");
        
        return Task.FromResult(new CliCommandOutcome[] { outcome });
    }
}