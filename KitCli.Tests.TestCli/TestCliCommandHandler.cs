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

        return FinishThisCommand()
            .ByNotFindingCommand()
            .ByFinallySaying("Final Say Outcome Used")
            .BySaying("Message Outcome Used")
            .ByShowingTable(table)
            .ByRememberingPageSize(20)
            .ByRememberingPageNumber(2)
            .EndAsync();
    }
}