using KitCli.Abstractions.Tables;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

public class TestCliCommandHandler : ICliCommandHandler<TestCliCommand>
{
    public Task<CliCommandOutcome[]> Handle(TestCliCommand request, CancellationToken cancellationToken)
    {
        var commandNotFoundOutcome = new CliCommandNotFoundOutcome();
        
        var outputOutcome = new OutputCliCommandOutcome("Output: TestCliCommand executed successfully.");
        
        var messageOutcome = new MessageCliCommandOutcome("This is a message outcome for TestCliCommand.");
        
        var table = new Table
        {
            Columns = ["ID", "Name", "Age"],
            Rows =
            [
                new List<object> { 1, "Alice", 30 },
                new List<object> { 2, "Bob", 25 },
                new List<object> { 3, "Charlie", 35 }
            ]
        };
        
        var tableOutcome = new TableCliCommandOutcome(table);

        var pageSizeOutcome = new PageSizeCliCommandOutcome(20);
        
        var pageNumberOutcome = new PageNumberCliCommandOutcome(2);

        var outcomes = new CliCommandOutcome[]
        {
            commandNotFoundOutcome,
            outputOutcome,
            messageOutcome,
            tableOutcome,
            pageSizeOutcome,
            pageNumberOutcome
        };
        
        return Task.FromResult(outcomes);
    }
}