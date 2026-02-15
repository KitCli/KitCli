using KitCli.Abstractions.Tables;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

public class TestCliCommandHandler : CliCommandHandler<TestCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestCliCommand command, CancellationToken cancellationToken)
    {
        var commandNotFoundOutcome = new NotFoundOutcome();
        
        var outputOutcome = new FinalSayOutcome("Output: TestCliCommand executed successfully.");
        
        var messageOutcome = new SayOutcome("This is a message outcome for TestCliCommand.");
        
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
        
        var tableOutcome = new TableOutcome(table);

        var pageSizeOutcome = new PageSizeOutcome(20);
        
        var pageNumberOutcome = new PageNumberOutcome(2);

        var outcomes = new Outcome[]
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